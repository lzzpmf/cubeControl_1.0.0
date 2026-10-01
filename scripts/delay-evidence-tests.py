"""Exercise the delay evidence verifier with corrupted copies of a complete archive.

No USB access. Originals are read only; temporary archives are private to this run.
"""
import argparse
import base64
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import zipfile

spec = importlib.util.spec_from_file_location('delay_evidence', Path(__file__).with_name('delay-evidence.py'))
evidence = importlib.util.module_from_spec(spec)
spec.loader.exec_module(evidence)


def envelope(command, payload):
    raw = b'\x00\x59' + bytes([command]) + len(payload).to_bytes(3, 'little') + payload + bytes([~sum(payload) & 255])
    wire = bytearray([0xF0])
    bits = count = 0
    for value in raw:
        bits |= value << count
        count += 8
        while count >= 7:
            wire.append(bits & 127)
            bits >>= 7
            count -= 7
    if count:
        wire.append(bits & 127)
    wire.append(0xF7)
    return wire.hex().upper()


def run(archive):
    original = evidence.summarize(archive)
    assert original['completeInvestigation']
    with zipfile.ZipFile(archive) as source:
        entries = {name: source.read(name) for name in source.namelist()}
    cases = []
    snapshot_name = 'controls-01/0-depth-zero-checked.bin'
    corrupted = bytearray(entries[snapshot_name])
    corrupted[0x69C] ^= 1
    cases.append(('snapshot-binary', {snapshot_name: bytes(corrupted)}, 'snapshot JSON/binary mismatch'))
    metadata_name = snapshot_name[:-4] + '.json'
    metadata = json.loads(entries[metadata_name])
    metadata['snapshot']['Dsp'] = base64.b64encode(corrupted).decode()
    cases.append(('snapshot-and-json', {snapshot_name: bytes(corrupted), metadata_name: json.dumps(metadata).encode()}, 'snapshot not evidenced by USB'))

    intent_name = 'controls-01/intent-0000.json'
    intent = json.loads(entries[intent_name])
    intent['after'] = base64.b64encode(b'\x00').decode()
    cases.append(('intent', {intent_name: json.dumps(intent).encode()}, 'intent/read/write mismatch'))

    log_name = 'controls-01/transport.log'
    lines = entries[log_name].decode('utf-8-sig').splitlines()
    for index, line in enumerate(lines):
        if ' TX ' not in line:
            continue
        stamp, direction, encoded = line.split(' ', 2)
        command, payload = evidence.protocol.packet(encoded)
        if command != 0x22:
            continue
        forbidden = bytearray(payload)
        forbidden[0] = 4
        forbidden[1:5] = (0x700).to_bytes(4, 'little')
        changed = lines.copy()
        changed[index] = f'{stamp} {direction} {envelope(command, forbidden)}'
        cases.append(('outside-allowlist', {log_name: ('\n'.join(changed) + '\n').encode()}, 'write outside allowlist'))
        changed = lines.copy()
        changed[index] = f'{stamp} {direction} {envelope(0x24, payload)}'
        cases.append(('unknown-command', {log_name: ('\n'.join(changed) + '\n').encode()}, 'unexpected command'))
        changed = lines.copy()
        changed[index] = f'{stamp} {direction} {encoded[:-4]}F7'
        cases.append(('truncated-usb', {log_name: ('\n'.join(changed) + '\n').encode()}, 'Truncated envelope'))
        break
    assert len(cases) == 6

    runtime_name = 'runtime-01/runtime-frozen-0-external.json'
    runtime = json.loads(entries[runtime_name])
    state = bytearray(base64.b64decode(runtime['Runtime']))
    state[0x26] ^= 1
    runtime['Runtime'] = base64.b64encode(state).decode()
    cases.append(('runtime-state', {runtime_name: json.dumps(runtime).encode()}, 'runtime sample not evidenced by USB'))
    outcomes = []
    with tempfile.TemporaryDirectory(prefix='cube-delay-evidence-') as folder:
        for name, replacements, expected in cases:
            target = Path(folder) / (name + '.zip')
            with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED, compresslevel=1) as output:
                for entry, data in entries.items():
                    output.writestr(entry, replacements.get(entry, data))
            try:
                evidence.summarize(target)
            except ValueError as error:
                if expected not in str(error):
                    raise AssertionError(f'{name}: unexpected rejection: {error}') from error
                outcomes.append({'case': name, 'rejected': True, 'reason': str(error)})
            else:
                raise AssertionError(f'Corrupted archive accepted: {name}')
    return {'format': 'cubecontrol-delay-evidence-tests-v1',
            'originalArchiveSha256': hashlib.sha256(Path(archive).read_bytes()).hexdigest(),
            'originalAccepted': True, 'negativeCases': outcomes}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive')
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    Path(args.output).write_text(json.dumps(run(args.archive), indent=2) + '\n', encoding='utf-8')
