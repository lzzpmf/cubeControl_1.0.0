"""Recheck preserved chorus USB evidence. Offline, Python standard library only."""
import argparse
import base64
import hashlib
import importlib.util
import json
import struct
import zipfile
from collections import Counter
from pathlib import Path

spec = importlib.util.spec_from_file_location('phaser_evidence', Path(__file__).with_name('phaser-evidence.py'))
protocol = importlib.util.module_from_spec(spec)
spec.loader.exec_module(protocol)


def f32(b, offset):
    return struct.unpack_from('<f', b, offset)[0]


def rounded(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]


def dynamic(i):
    return 0x68C <= i < 0x694 or 0xF6C <= i < 0xF70


def stable(a, b):
    return len(a) == len(b) == 0xF70 and all(x == y or dynamic(i) for i, (x, y) in enumerate(zip(a, b)))


def require(condition, label):
    if not condition:
        raise ValueError(label)


def summarize(archive):
    with zipfile.ZipFile(archive) as z:
        names = z.namelist()
        def read(name):
            return z.read(name)
        def meta(name):
            return json.loads(read(name))['snapshot']
        result = {'format': 'cubecontrol-chorus-evidence-v1',
                  'archiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(),
                  'sha256': {n: hashlib.sha256(read(n)).hexdigest() for n in sorted(names)},
                  'restoration': {}, 'traffic': {}, 'speedTable': [], 'trials': []}
        if 'map-02/mapping.json' in names:
            mapping = json.loads(read('map-02/mapping.json'))['mapping']
            blocks = [('dispatch', 0x80C374), ('oscillators', 0x80C3DC), ('worker', 0x8326B4),
                ('delay-and-phase', 0x832188), ('preset', 0x831AC0), ('initial-filters', 0xC487AC),
                ('mod-table', 0xC487E4), ('waveforms', 0x833540), ('initialize', 0xC13B04), ('filter-helper', 0x80A91E)]
            replies = {}
            for line in read('map-02/transport.log').decode('utf-8-sig').splitlines():
                if ' TX ' not in line and ' RX ' not in line: continue
                _, direction, encoded = line.split(' ', 2)
                command, payload = protocol.packet(encoded)
                if direction == 'TX': require(command in (0x11, 0x23), 'mapping must be read only')
                if direction == 'RX' and command == 0x23 and payload[0] == 4:
                    replies[int.from_bytes(payload[1:5], 'little')] = payload[8:]
            for name, address in blocks:
                actual = read('map-02/' + name + '.bin')
                require(actual.hex().upper() == mapping[name], 'mapping JSON/binary mismatch')
                chunks = [replies[((address - 0x8347F8) + i) & 0xFFFFFFFF] for i in range(0, len(actual), 113)]
                require(b''.join(chunks) == actual, 'mapping does not match USB')
            signatures = [('worker', 0x110, '4b3f852a0f15934beacea107471de2877f89052d3eaef4652bead312a28369aa'),
                ('delay-and-phase', 0xCC, '1eb86dcfd8f7d15dfc4bee9fde2641522a88b315d3ddd26040552b19547cd5fb'),
                ('waveforms', 0x54, '78adc5ec3bc6558618428322d2d5d2394a0e9d3d091a32e297bc01572b0697bb'),
                ('preset', 0x49C, 'cf811d5442a4ecd134f315d7c89f408efcbf363c926ab029db7a790c48d94484'),
                ('initialize', 0x30, '31fa77b842bbc324aa73a921d1a5986d9f8f06de57b4a898adb49db2c4584bd7'),
                ('filter-helper', 0x2A, '465961c91038ea88c1490bc3e4cf0dbf9acc3b550e6f096398d37e25bf685b83')]
            for name, size, expected in signatures:
                require(hashlib.sha256(read('map-02/' + name + '.bin')[:size]).hexdigest() == expected, 'incompatible captured firmware')
            require(read('map-02/dispatch.bin')[20:24] == bytes.fromhex('B5268300'), 'dispatcher entry')
            require(read('map-02/oscillators.bin')[:12] == bytes.fromhex('633583004135830061358300'), 'oscillator entries')
            result['mapping'] = {'matchedToUsb': True, 'codeSignatures': {n: h for n, _, h in signatures}}
        series_names = ['sweep-01', 'controls-01']
        if 'transitions-01/original.bin' in names: series_names.append('transitions-01')
        for series in series_names:
            original = read(f'{series}/original.bin')
            restored = read(f'{series}/restored.bin')
            first, last = (meta(f'{series}/{s}.json') for s in ('original', 'restored'))
            require(stable(original, restored), 'stable DSP restoration: ' + series)
            require(first['Bank'] == last['Bank'] and first['PhysicalState'] == last['PhysicalState'], 'context restoration')
            result['restoration'][series] = {'bankExact': True, 'physicalStateExact': True,
                'stableDspExact': True, 'dspExact': original == restored,
                'dynamicDifferences': [hex(i) for i, (a, b) in enumerate(zip(original, restored)) if a != b]}
            slot = base64.b64decode(first['PhysicalState'])[0]
            writes = []
            counts = {}
            received_snapshots = []
            assembled = bytearray()
            current_bank = current_state = None
            assembling = False
            for line in read(f'{series}/transport.log').decode('utf-8-sig').splitlines():
                if ' TX ' not in line and ' RX ' not in line:
                    continue
                _, direction, encoded = line.split(' ', 2)
                command, payload = protocol.packet(encoded)
                key = f'{direction}-{command:02X}'
                counts[key] = counts.get(key, 0) + 1
                if direction == 'RX' and command == 0x23:
                    region = payload[0]
                    address = int.from_bytes(payload[1:5], 'little')
                    length = int.from_bytes(payload[5:8], 'little')
                    require(len(payload) == length + 8, 'read response length')
                    if region == 5 and address == 0 and length == 48:
                        current_bank = payload[8:]
                    if region == 4 and address == 0xFFFD8202:
                        current_state = payload[8:]
                    if region == 4 and address == 0 and length == 113:
                        assembled = bytearray()
                        assembling = True
                    if assembling and region == 4 and address == len(assembled):
                        assembled.extend(payload[8:])
                        if len(assembled) == 0xF70:
                            require(current_bank is not None and current_state is not None, 'snapshot context missing')
                            received_snapshots.append((hashlib.sha256(assembled).hexdigest(), current_bank.hex(), current_state.hex()))
                            assembling = False
                if direction != 'TX':
                    continue
                require(command in (0x11, 0x22, 0x23), 'unexpected command')
                if command != 0x22:
                    continue
                region = payload[0]
                address = int.from_bytes(payload[1:5], 'little')
                length = int.from_bytes(payload[5:8], 'little')
                require(len(payload) == 8 + length, 'write size')
                require((region == 4 and 0x63C <= address and address + length <= 0x6E0) or
                        (region == 5 and length == 1 and address in [0x80000000 + slot * 16 + n for n in (5, 8, 11)]),
                        'write outside research allowlist')
                writes.append({'region': region, 'address': address, 'data': payload[8:].hex()})
            intentions = [json.loads(read(n)) for n in sorted(names) if n.startswith(series + '/intent-')]
            stored_snapshots = []
            for n in names:
                if n.startswith(series + '/') and n.endswith('.bin'):
                    context = meta(n[:-4] + '.json')
                    require(base64.b64decode(context['Dsp']) == read(n), 'JSON/binary DSP mismatch')
                    stored_snapshots.append((hashlib.sha256(read(n)).hexdigest(), base64.b64decode(context['Bank']).hex(), base64.b64decode(context['PhysicalState']).hex()))
            require(Counter(stored_snapshots) == Counter(received_snapshots), 'snapshots must match raw USB responses including bank/state')
            require(len(intentions) == len(writes), 'one durable intent per write')
            for intent, write in zip(intentions, writes):
                require(intent['address'] == write['address'] and base64.b64decode(intent['after']).hex() == write['data'], 'intent/write mismatch')
                require(write['region'] == (5 if intent['kind'] == 'live' else 4), 'intent region')
            result['traffic'][series] = {'counts': counts, 'writes': writes, 'intentCount': len(intentions),
                'snapshotsMatchedToUsb': len(received_snapshots), 'flashWrites': 0}
        for mod in range(7):
            matches = [n for n in names if n.startswith(f'sweep-01/mod-{mod:02}-') and n.endswith('.bin')]
            require(len(matches) >= 3, 'missing MOD samples')
            samples = [read(n) for n in matches]
            require(all(b[0x63C] == 3 and b[0x63D] == 5 for b in samples), 'chorus dispatcher state')
            require(all(stable(samples[0], b) for b in samples), 'stable MOD snapshots')
            b = samples[0]
            result['speedTable'].append({'mod': mod, 'depth': f32(b, 0x644), 'step': f32(b, 0x648),
                'estimatedHzAt48000': f32(b, 0x648) * 48000, 'mix': f32(b, 0x688), 'feedback': f32(b, 0x64C)})
        baseline = read('controls-01/mod-03-2.bin')
        fields = {'depth-half': (0x644, 0.2), 'depth-zero': (0x644, 0),
            'rate-low': (0x648, 0.000008), 'rate-high': (0x648, 0.000063),
            'mix-zero': (0x688, 0), 'mix-half': (0x688, 0.425), 'feedback-small': (0x64C, 0.1),
            'wave-triangle': (0x640, 1), 'wet-filter-gain': (0x650, 0.5), 'combined-filter-gain': (0x66C, 0.5)}
        for cycle in range(2):
            for label, (offset, value) in fields.items():
                prefix = f'controls-01/{cycle}-{label}'
                target = bytearray(baseline)
                if offset == 0x640:
                    target[offset] = value
                else:
                    struct.pack_into('<f', target, offset, value)
                for phase in ('before', 'immediate', 'delayed', 'mod4', 'restored'):
                    wanted = baseline if phase in ('before', 'restored') else target
                    if phase == 'mod4':
                        wanted = bytearray(target)
                        struct.pack_into('<ff', wanted, 0x644, 0.4, 0.000023)
                    require(stable(read(prefix + '-' + phase + '.bin'), wanted), f'{prefix}-{phase}: unexpected stable changes')
                    context = meta(prefix + '-' + phase + '.json')
                    reference = meta('controls-01/mod-03-2.json')
                    bank = bytearray(base64.b64decode(reference['Bank']))
                    slot = base64.b64decode(reference['PhysicalState'])[0]
                    if phase == 'mod4': bank[slot * 16 + 8] = 4
                    require(base64.b64decode(context['Bank']) == bank and context['PhysicalState'] == reference['PhysicalState'], 'trial context')
                result['trials'].append({'cycle': cycle, 'control': label, 'offset': hex(offset), 'value': value,
                    'isolatedPersistedRestored': True, 'survivesDifferentChorusMod': offset not in (0x644, 0x648)})
        if 'transitions-01' in series_names:
            initial = read('transitions-01/start-chorus.bin')
            for label in ('12', '7', '8', 'section'):
                require(stable(read(f'transitions-01/before-transition-{label}.bin'), initial), 'transition baseline')
                custom = bytearray(initial)
                struct.pack_into('<f', custom, 0x688, 0.425)
                require(stable(read(f'transitions-01/custom-mix-{label}.bin'), custom), 'transition custom mix')
                returning = 'section-on' if label == 'section' else 'return-' + label
                require(stable(read(f'transitions-01/{returning}.bin'), initial), 'transition must reset custom mix')
            phaser = read('transitions-01/transition-12.bin')
            require(phaser[0x63C] == 3 and phaser[0x63D] == 2, 'direct phaser transition')
            for label in ('transition-7', 'transition-8', 'section-off'):
                require(read('transitions-01/' + label + '.bin')[0x63C] == 0, 'modulation off')
            result['transitions'] = {'phaserThenChorusResets': True, 'off7ThenChorusResets': True,
                'off8ThenChorusResets': True, 'sectionOffOnResets': True}
        return result


def disassemble(archive):
    from capstone import Cs, CS_ARCH_ARM, CS_MODE_THUMB, CS_MODE_LITTLE_ENDIAN
    from capstone.arm import ARM_OP_MEM, ARM_REG_PC
    sections = [('worker', 0x8326B4, 0x110), ('delay-and-phase', 0x832188, 0xCC),
                ('waveforms', 0x833540, 0x54), ('preset', 0x831AC0, 0x49C),
                ('initialize', 0xC13B04, 0x30), ('filter-helper', 0x80A91E, 0x2A)]
    cs = Cs(CS_ARCH_ARM, CS_MODE_THUMB | CS_MODE_LITTLE_ENDIAN)
    cs.detail = True
    output = ['Captured instructions and literal pools; linear decoding may show data as instructions.']
    with zipfile.ZipFile(archive) as z:
        for name, base, length in sections:
            b = z.read('map-02/' + name + '.bin')
            output.append(f'\n{name} CPU={base:08X} captureSha256={hashlib.sha256(b).hexdigest()}')
            for ins in cs.disasm(b[:length], base):
                extra = ''
                for operand in ins.operands:
                    if operand.type == ARM_OP_MEM and operand.mem.base == ARM_REG_PC:
                        literal = ((ins.address + 4) & ~3) + operand.mem.disp
                        if base <= literal <= base + len(b) - 4:
                            word = struct.unpack_from('<I', b, literal - base)[0]
                            extra = f' ; [{literal:08X}]={word:08X} float32={f32(b, literal-base):.9g}'
                output.append(f'{ins.address:08X} {ins.bytes.hex():10s} {ins.mnemonic:10s} {ins.op_str}{extra}')
    return '\n'.join(output) + '\n'


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive')
    parser.add_argument('--output')
    parser.add_argument('--disassembly', help='Optional output path; requires capstone==5.0.9')
    args = parser.parse_args()
    text = json.dumps(summarize(args.archive), indent=2) + '\n'
    if args.output:
        Path(args.output).write_text(text, encoding='utf-8')
    else:
        print(text)
    if args.disassembly:
        Path(args.disassembly).write_text(disassemble(args.archive), encoding='utf-8')
