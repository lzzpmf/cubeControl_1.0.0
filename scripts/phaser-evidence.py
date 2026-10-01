"""Validate preserved Phaser USB captures and summarize observed controls (Python stdlib only)."""
import argparse
import base64
import hashlib
import json
import struct
import zipfile
from pathlib import Path


def packet(hexadecimal):
    wire = bytes.fromhex(hexadecimal)
    if wire[0] != 0xF0 or wire[-1] != 0xF7:
        raise ValueError('Invalid SysEx boundaries')
    decoded = bytearray()
    bits = count = 0
    for value in wire[1:-1]:
        if value > 127:
            raise ValueError('Non-septet in SysEx')
        bits |= value << count
        count += 7
        while count >= 8:
            decoded.append(bits & 255)
            bits >>= 8
            count -= 8
    if decoded[:2] != b'\x00\x59':
        raise ValueError('Invalid envelope header')
    length = int.from_bytes(decoded[3:6], 'little')
    if len(decoded) < length + 7:
        raise ValueError('Truncated envelope')
    payload = decoded[6:6 + length]
    if decoded[6 + length] != (~sum(payload) & 255):
        raise ValueError('Invalid checksum')
    return decoded[2], payload


def summarize(archive):
    with zipfile.ZipFile(archive) as z:
        hashes = {name: hashlib.sha256(z.read(name)).hexdigest() for name in sorted(z.namelist())}
        traffic = {}
        for name in z.namelist():
            if not name.endswith('transport.log'):
                continue
            counts = {}; writes = []
            for line in z.read(name).decode('utf-8-sig').splitlines():
                if ' TX ' not in line and ' RX ' not in line:
                    continue
                _, direction, encoded = line.split(' ', 2)
                command, payload = packet(encoded)
                key = f'{direction}-{command:02X}'
                counts[key] = counts.get(key, 0) + 1
                if direction != 'TX':
                    continue
                if command not in (0x11, 0x22, 0x23):
                    raise ValueError(f'Unexpected transmitted command {command:02X}')
                if command == 0x22:
                    region = payload[0]; address = int.from_bytes(payload[1:5], 'little'); size = int.from_bytes(payload[5:8], 'little')
                    if not ((region == 4 and 0x63C <= address and address + size <= 0x6E0) or
                            (region == 5 and size == 1 and address in (0x80000005, 0x80000008, 0x8000000B))):
                        raise ValueError(f'Unexpected mutation {region}:{address:X}+{size}')
                    writes.append({'region': region, 'address': f'0x{address:X}', 'length': size})
            traffic[name] = {'counts': counts, 'writes': writes, 'flashWrites': 0}
        speeds = []
        for mod in range(9, 16):
            b = z.read(f'sweep-01/mod-{mod:02}-0.bin')
            speeds.append({'mod': mod, 'step': struct.unpack_from('<f', b, 0x644)[0]})
        restored = {}
        for series in ('sweep-01', 'controls-01'):
            before = z.read(f'{series}/original.bin'); after = z.read(f'{series}/restored.bin')
            different = [i for i in range(len(before)) if before[i] != after[i]]
            first = json.loads(z.read(f'{series}/original.json'))['snapshot']
            last = json.loads(z.read(f'{series}/restored.json'))['snapshot']
            stable = [i for i in different if not (0x658 <= i < 0x65C or 0xF6C <= i < 0xF70)]
            if stable or first['Bank'] != last['Bank'] or first['PhysicalState'] != last['PhysicalState']:
                raise ValueError(f'Restoration unconfirmed: {series}')
            restored[series] = {'bankExact': True, 'physicalStateExact': True, 'stableDspExact': True,
                                'dynamicDifferences': [f'0x{i:X}' for i in different],
                                'bankHex': base64.b64decode(last['Bank']).hex()}
        fields = {'step-low': (0x644, '<f', 0.00001), 'step-high': (0x644, '<f', 0.00013),
                  'lower': (0x64C, '<f', 1000), 'upper': (0x650, '<f', 2000), 'level': (0x654, '<f', 0.32),
                  'sections-one': (0x641, '<B', 0), 'sections-two': (0x641, '<B', 1)}
        trials = []
        reference = z.read('controls-01/mod-12-0.bin')
        for cycle in range(2):
            for label, (offset, fmt, expected) in fields.items():
                samples = [z.read(f'controls-01/{cycle}-{label}-{phase}.bin') for phase in ('immediate', 'delayed', 'restored')]
                actual = [struct.unpack_from(fmt, b, offset)[0] for b in samples]
                for value in actual[:2]:
                    if abs(value - expected) > max(abs(expected) * 1e-6, 1e-10):
                        raise ValueError(f'Control not confirmed: {cycle}-{label}')
                size = struct.calcsize(fmt)
                for phase, b in zip(('immediate', 'delayed', 'restored'), samples):
                    allowed = set(range(offset, offset + size)) if phase != 'restored' else set()
                    allowed.update(range(0x658, 0x65C)); allowed.update(range(0xF6C, 0xF70))
                    if any(a != v and i not in allowed for i, (a, v) in enumerate(zip(reference, b))):
                        raise ValueError(f'Unrelated stable data changed: {cycle}-{label}-{phase}')
                trials.append({'cycle': cycle, 'control': label, 'values': actual, 'isolatedAndRestored': True})
        return {'format': 'cubecontrol-phaser-evidence-v1', 'archiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(),
                'speedTable': speeds, 'restoration': restored, 'trials': trials, 'traffic': traffic, 'sha256': hashes}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive')
    parser.add_argument('--output')
    args = parser.parse_args()
    result = json.dumps(summarize(args.archive), indent=2) + '\n'
    if args.output:
        Path(args.output).write_text(result, encoding='utf-8')
    else:
        print(result)


if __name__ == '__main__':
    main()
