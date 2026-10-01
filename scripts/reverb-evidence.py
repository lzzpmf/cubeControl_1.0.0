"""Verify reverb captures against raw USB, including writes, restoration and firmware.

Offline; standard library only. --disassembly additionally uses capstone==5.0.9.
"""
import argparse
import base64
from collections import Counter
import hashlib
import importlib.util
import json
from pathlib import Path
import struct
import zipfile

spec = importlib.util.spec_from_file_location('protocol', Path(__file__).with_name('phaser-evidence.py'))
protocol = importlib.util.module_from_spec(spec)
spec.loader.exec_module(protocol)

# Captured CPU addresses; never inferred using a uniform firmware delta.
BLOCKS = {'preset': 0x831AC0, 'caller': 0x833724, 'worker': 0x833978,
          'diffusion': 0x833904, 'damping': 0x8338E0, 'initialize': 0xC13D00,
          'filter': 0x80A8BC, 'default': 0xC48C98, 'feedback-table': 0xC4881C,
          'damping-table': 0xC488D0, 'mix-table': 0xC48D38, 'input-filter': 0x8359A0,
          'runtime': 0x8181BC, 'input-coefficients': 0x818228}
FIELDS = [(0x6E1, 1), (0x6E3, 6), (0x6EA, 12), (0x708, 4), (0x70C, 4)]


def require(condition, message):
    if not condition:
        raise ValueError(message)


def stable(a, b):
    return len(a) == len(b) == 0xF70 and all(x == y or 0xF6C <= i < 0xF70 for i, (x, y) in enumerate(zip(a, b)))


def packets(raw):
    for line in raw.decode('utf-8-sig').splitlines():
        if ' TX ' in line or ' RX ' in line:
            _, direction, encoded = line.split(' ', 2)
            command, payload = protocol.packet(encoded)
            yield direction, command, payload


def summarize(archive):
    with zipfile.ZipFile(archive) as z:
        names = z.namelist()
        def read(n): return z.read(n)
        def meta(n): return json.loads(read(n))['snapshot']
        def key(dsp, bank, physical): return (hashlib.sha256(dsp).hexdigest(), bank.hex(), physical.hex())
        result = {'format': 'cubecontrol-reverb-evidence-v1', 'audioMeasured': False,
                  'archiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(),
                  'sha256': {n: hashlib.sha256(read(n)).hexdigest() for n in sorted(names)},
                  'mapping': {}, 'traffic': {}, 'restoration': {}, 'table': [], 'trials': []}
        for series in ('map-01', 'map-02'):
            mapping = json.loads(read(series + '/mapping.json'))['mapping']
            chunks = {}
            for direction, command, payload in packets(read(series + '/transport.log')):
                if direction == 'TX': require(command in (0x11, 0x23), 'mapping is read only')
                if direction == 'RX' and command == 0x23:
                    chunks[(payload[0], int.from_bytes(payload[1:5], 'little'))] = payload[8:]
            for name, hexdata in mapping.items():
                b = read(series + '/' + name + '.bin')
                require(b.hex().upper() == hexdata, 'mapping JSON/binary mismatch')
                if name in BLOCKS:
                    address, region = (BLOCKS[name] - 0x8347F8) & 0xFFFFFFFF, 4
                else:
                    address, region = {'bank': (0, 5), 'dsp': (0, 4), 'physical': (0xFFFD8202, 4)}[name]
                actual = b''.join(chunks[(region, (address + i) & 0xFFFFFFFF)] for i in range(0, len(b), 113))
                require(actual == b, 'mapping does not match USB: ' + name)
            result['mapping'][series] = {'matchedToUsb': True}
        # Signatures are pinned independently in the protocol fixture shipped with these tests.
        fixture = json.loads((Path(__file__).parents[1] / 'CubeControl-1.1/Cube/windows/Cube.Tests/Fixtures/reverb-live-code.json').read_text())
        for name, cpu in BLOCKS.items():
            if name in ('runtime', 'input-coefficients'): continue
            b = read('map-02/' + name + '.bin')
            require(bytes.fromhex(fixture[str((cpu - 0x8347F8) & 0xFFFFFFFF)]) == b, 'firmware fixture mismatch: ' + name)
        default = read('map-02/default.bin')
        feedback = read('map-02/feedback-table.bin')
        mix = read('map-02/mix-table.bin')
        for series in ('sweep-01', 'controls-01', 'transitions-01'):
            original, restored = read(series + '/original.bin'), read(series + '/restored.bin')
            first, last = meta(series + '/original.json'), meta(series + '/restored.json')
            require(stable(original, restored), 'stable restoration: ' + series)
            require(first['Bank'] == last['Bank'] and first['PhysicalState'] == last['PhysicalState'], 'context restoration')
            slot = base64.b64decode(first['PhysicalState'])[0]
            received, writes, counts = [], [], Counter()
            memory = {}
            assembled = None
            bank = physical = None
            pending = None
            for direction, command, payload in packets(read(series + '/transport.log')):
                counts[f'{direction}-{command:02X}'] += 1
                if direction == 'TX':
                    require(command in (0x11, 0x22, 0x23), 'unexpected command')
                    pending = (command, payload)
                elif command == 0x23:
                    require(pending is not None and pending[0] == 0x23 and payload[:8] == pending[1], 'read response/header mismatch')
                    region = payload[0]; address = int.from_bytes(payload[1:5], 'little')
                    length = int.from_bytes(payload[5:8], 'little')
                    require(len(payload) == length + 8, 'read length')
                    memory.update({(region, (address + i) & 0xFFFFFFFF): v for i, v in enumerate(payload[8:])})
                    if region == 5 and address == 0 and length == 48: bank = bytes(payload[8:])
                    if region == 4 and address == 0xFFFD8202: physical = bytes(payload[8:])
                    if region == 4 and address == 0 and length == 113: assembled = bytearray()
                    if assembled is not None and region == 4 and address == len(assembled):
                        assembled.extend(payload[8:])
                        if len(assembled) == 0xF70:
                            received.append(key(assembled, bank, physical)); assembled = None
                if direction != 'TX' or command != 0x22: continue
                region = payload[0]; address = int.from_bytes(payload[1:5], 'little')
                length = int.from_bytes(payload[5:8], 'little')
                require(len(payload) == 8 + length, 'write length')
                require((region == 4 and 0x6E0 <= address and address + length <= 0x764) or
                        (region == 5 and length == 1 and address in [0x80000000 + slot * 16 + n for n in (3, 5, 10)]), 'write outside allowlist')
                source = address - 0x80000000 if region == 5 else address
                before = bytes(memory[(region, source + i)] for i in range(length))
                writes.append({'region': region, 'address': address, 'data': bytes(payload[8:]).hex(), 'before': before.hex()})
            stored = []
            for n in names:
                if n.startswith(series + '/') and n.endswith('.bin'):
                    context = meta(n[:-4] + '.json'); b = read(n)
                    require(base64.b64decode(context['Dsp']) == b, 'snapshot JSON/binary mismatch')
                    stored.append(key(b, base64.b64decode(context['Bank']), base64.b64decode(context['PhysicalState'])))
            # Guard-only snapshots are also read, but not separately saved. Every saved snapshot
            # must have its own matching occurrence in raw traffic, including its exact context.
            require(not (Counter(stored) - Counter(received)), 'snapshot not evidenced by USB')
            intents = [json.loads(read(n)) for n in sorted(names) if n.startswith(series + '/intent-')]
            require(len(intents) == len(writes), 'one intent per write')
            for intent, write in zip(intents, writes):
                require(intent['region'] == write['region'] and intent['address'] == write['address'] and
                        base64.b64decode(intent['after']).hex() == write['data'] and base64.b64decode(intent['before']).hex() == write['before'], 'intent/read/write mismatch')
                if write['region'] == 4 and intent['kind'] != 'inactive-restore':
                    require((write['address'], len(bytes.fromhex(write['data']))) in FIELDS, 'non-control active write')
            result['traffic'][series] = {'counts': dict(counts), 'writes': writes, 'intentCount': len(intents),
                                         'snapshotsMatchedToUsb': len(stored), 'guardSnapshots': len(received) - len(stored), 'flashWrites': 0}
            result['restoration'][series] = {'bankExact': True, 'physicalExact': True, 'stableDspExact': True,
                                             'dspExact': original == restored, 'differences': [hex(i) for i, (a, b) in enumerate(zip(original, restored)) if a != b]}
        for raw in range(16):
            samples = [read(f'sweep-01/raw-{raw:02}-{i}.bin') for i in range(3)]
            require(all(stable(samples[0], b) for b in samples), 'unstable table samples')
            b = samples[0]
            if raw:
                expected = bytearray(default); expected[0] = 3; expected[2] = raw * 8
                expected[10:22] = feedback[(raw-1)*12:raw*12]; expected[40:48] = mix[(raw-1)*8:raw*8]
                require(b[0x6E0:0x764] == expected, 'table not applied')
                result['table'].append({'raw': raw, 'dry': struct.unpack_from('<f', b, 0x708)[0],
                     'wet': struct.unpack_from('<f', b, 0x70C)[0], 'feedbackQ14': list(struct.unpack_from('<6h', b, 0x6EA)), 'unusedTableByte': b[0x6E2]})
            else: require(b[0x6E0] == 0, 'zero must disable')
        baseline = read('controls-01/baseline.bin')
        trials = {'dry-zero': (0x708, struct.pack('<f', 0)), 'dry-half': (0x708, struct.pack('<f', .5)),
                  'wet-zero': (0x70C, struct.pack('<f', 0)), 'wet-half': (0x70C, struct.pack('<f', .275)),
                  'decay-low': (0x6EA, feedback[:12]), 'decay-high': (0x6EA, feedback[-12:]),
                  'return-zero': (0x6E1, bytes([0])), 'return-half': (0x6E1, bytes([40])),
                  'damping-zero': (0x6E3, bytes(6)), 'damping-half': (0x6E3, bytes(range(20, 26)))}
        for cycle in range(2):
            for name, (offset, data) in trials.items():
                target = bytearray(baseline); target[offset:offset + len(data)] = data
                for phase in ('checked', 'delayed', 'same', 'raw9', 'restore-checked', 'restored'):
                    expected = target
                    if phase in ('restore-checked', 'restored'): expected = baseline
                    elif phase == 'raw9':
                        expected = bytearray(target); expected[0x6EA:0x6F6] = feedback[96:108]
                        expected[0x708:0x710] = mix[64:72]; expected[0x6E2] = 72
                    require(stable(read(f'controls-01/{cycle}-{name}-{phase}.bin'), expected), 'trial mismatch: ' + name + '-' + phase)
                result['trials'].append({'cycle': cycle, 'name': name, 'offset': hex(offset), 'isolatedPersistedRestored': True})
        for transition in ('off', 'section'):
            baseline = read('transitions-01/baseline.bin')
            custom = bytearray(baseline); custom[0x6E1] = 40; custom[0x6E3:0x6E9] = bytes(range(20, 26))
            custom[0x6EA:0x6F6] = feedback[:12]; struct.pack_into('<ff', custom, 0x708, .5, .275)
            require(stable(read(f'transitions-01/custom-{transition}-checked.bin'), custom), 'transition customization')
            require(read(f'transitions-01/{transition}-off.bin')[0x6E0] == 0, 'transition off')
            require(stable(read(f'transitions-01/{transition}-on.bin'), baseline), 'transition reset')
        result['transitions'] = {'offOnResetsAllControls': True, 'sectionResetsAllControls': True}
        return result


def disassembly(archive):
    from capstone import Cs, CS_ARCH_ARM, CS_MODE_THUMB
    from capstone.arm import ARM_OP_MEM, ARM_REG_PC
    cs = Cs(CS_ARCH_ARM, CS_MODE_THUMB); cs.detail = True
    # Exclude literal pools from each contiguous routine; preset branches cross several pools.
    segments = {'preset': [(0, 0x270), (0x298, 0x1B0)], 'caller': [(0, 0x124)],
                'worker': [(0, 0x25C)], 'diffusion': [(0, 0x68)], 'damping': [(0, 0x20)],
                'initialize': [(0, 0xA4)], 'filter': [(0, 0x32)], 'input-filter': [(0, 0x64)]}
    lines = ['Reverb live capture. Literal pools omitted; PC-relative constants annotated.']
    with zipfile.ZipFile(archive) as z:
        for name, spans in segments.items():
            b = z.read('map-02/' + name + '.bin'); base = BLOCKS[name]
            lines.append(f'\n{name} CPU={base:08X} SHA256={hashlib.sha256(b).hexdigest()}')
            for offset, length in spans:
                for ins in cs.disasm(b[offset:offset+length], base+offset):
                    extra = ''
                    for op in ins.operands:
                        if op.type == ARM_OP_MEM and op.mem.base == ARM_REG_PC:
                            literal = ((ins.address + 4) & ~3) + op.mem.disp
                            if base <= literal <= base + len(b) - 4:
                                extra = f' ; [{literal:08X}]={struct.unpack_from("<I", b, literal-base)[0]:08X}'
                    lines.append(f'{ins.address:08X} {ins.bytes.hex():10s} {ins.mnemonic:10s} {ins.op_str}{extra}')
    return '\n'.join(lines) + '\n'


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive'); parser.add_argument('--output'); parser.add_argument('--disassembly')
    args = parser.parse_args()
    summary = json.dumps(summarize(args.archive), indent=2) + '\n'
    if args.output:
        Path(args.output).parent.mkdir(parents=True, exist_ok=True); Path(args.output).write_text(summary, encoding='utf-8')
    else: print(summary)
    if args.disassembly: Path(args.disassembly).write_text(disassembly(args.archive), encoding='utf-8')
