"""Rebuild metadata from versioned Overdriven WAVs/readmes; never modify audio.

Run with Python 3 from any directory. --check verifies without writing.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import sys

# Keep --check read-only, including Python's import cache.
sys.dont_write_bytecode = True
from overdriven_metadata import MICROPHONES, MICROPHONE_OVERRIDES, classification

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / 'CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/ImpulseResponses'
SOURCE = DEST / 'Overdriven.fr'
EXPECTED = {
    '212-GOV': ('FB / GOV', 82), 'DE-112-T75-G4-CLASSD': ('DE-112 / T75', 32),
    'E112-BVV': ('E112 / BVV', 40), 'E112-CV-CLASSD': ('E112 / CV', 40),
    'E112-GM-G4': ('E112 / GM', 56), 'E112-K100-G4': ('E112 / K100', 86),
    'E112-P50E-CLASSD': ('E112 / P50E', 39), 'E112-VT-CLASSD': ('E112 / VT', 36),
    'FB-VET30-G4': ('FB / VET30', 45), 'M212-GB55-CLASSD': ('M212 / GB55', 44),
    'M212-P50': ('M212 / P50', 86), 'M212-VINT-L-G4-CLASSD': ('M212 / VINT-L', 57),
    'R112-V30-G4': ('R112 / V30', 39),
}
# Explicit capture prefixes: filenames do not consistently separate cabinet/speaker.
PREFIXES = {
    '212-GOV': 'OD-FB-GOV-', 'DE-112-T75-G4-CLASSD': 'OD-E112-T75-',
    'E112-BVV': 'OD-E112-BV30V-', 'E112-CV-CLASSD': 'OD-E112-CV-',
    'E112-GM-G4': 'OD-E112-GM-', 'E112-K100-G4': 'OD-E112-K100-',
    'E112-P50E-CLASSD': 'OD-E112-P50-', 'E112-VT-CLASSD': 'OD-E112-VT-',
    'FB-VET30-G4': 'OD-FBVET30-', 'M212-GB55-CLASSD': 'OD-M212-GB55-',
    'M212-P50': 'OD-M212-P50-', 'M212-VINT-L-G4-CLASSD': 'OD-M212-VINT-',
    'R112-V30-G4': 'OD-R112-',
}


def wav_info(data):
    if data[:4] != b'RIFF' or data[8:12] != b'WAVE':
        raise ValueError('Not a RIFF WAV')
    end = struct.unpack_from('<I', data, 4)[0] + 8
    if end > len(data):
        raise ValueError('Truncated WAV')
    pos, fmt, length = 12, None, None
    while pos + 8 <= end:
        tag, size = struct.unpack_from('<4sI', data, pos)
        if pos + 8 + size > end:
            raise ValueError('Truncated chunk')
        if tag == b'fmt ':
            fmt = struct.unpack_from('<HHIIHH', data, pos + 8)
            code = fmt[0]
            if code == 65534:
                code = struct.unpack_from('<H', data, pos + 32)[0]
        if tag == b'data':
            length = size
        pos += 8 + size + size % 2
    if fmt is None or length is None or code not in (1, 3):
        raise ValueError('Unsupported WAV')
    return dict(SampleRate=fmt[2], Channels=fmt[1], Bits=fmt[5],
                Format='Float' if code == 3 else 'PCM', DurationMs=length / fmt[3] * 1000)


def generate(check):
    entries, seen = [], set()
    for folder in sorted(SOURCE.iterdir()):
        if not folder.is_dir():
            continue
        key = folder.name.removeprefix('Overdriven-').removesuffix('-SSP2-v1.0')
        label, expected = EXPECTED[key]
        seen.add(key)
        readme = folder / 'SSP2/readme.txt'
        text = readme.read_text(encoding='utf-8-sig')
        fields = {}
        for field in ('Cab', 'Speaker', 'Power amp', 'Mic preamp'):
            fields[field] = re.search(r'^' + field + r'\s*:\s*(.+)$', text, re.M)[1].strip()
        waves = sorted(folder.rglob('*.wav'))
        if len(waves) != expected:
            raise ValueError(f'{key}: expected {expected}, found {len(waves)}')
        models = MICROPHONES | MICROPHONE_OVERRIDES.get(key, {})
        naming = classification(key)
        naming['Sources'] = [readme.relative_to(DEST).as_posix()] + naming['Sources']
        for wave in waves:
            preview = 'SP1-PREVIEW' in wave.parts
            # Preview IRs describe other captures: retain their full prefix as provenance.
            prefix = PREFIXES[key]
            if not preview and not wave.stem.startswith(prefix):
                raise ValueError(f'Unexpected capture prefix: {wave}')
            capture_name = wave.stem
            if key == 'DE-112-T75-G4-CLASSD' and wave.parent.name == 'CND-M3':
                capture_name = capture_name.replace('-M3-', '-CND-M3-')
            match = re.fullmatch(r'(.+?)-(?P<mic>(?:DYN|CND|RBN|DN|LD|RB)-.+?)-(?P<pos>P?\d+[A-Z]?)-(?P<mm>\d+)(?P<variants>(?:-.+)?)', capture_name)
            if not match:
                raise ValueError(f'Unrecognized name: {wave}')
            mic, pos = match['mic'], match['pos']
            if not pos.startswith('P'):
                if wave.name not in ('OD-FBVET30-DN-409U-09-40.wav', 'OD-FBVET30-DN-421B-10-40-LC.wav'):
                    raise ValueError(f'Undocumented position exception: {wave}')
                pos = 'P' + pos
            relative = Path('Overdriven.fr') / folder.name / wave.relative_to(folder)
            data = wave.read_bytes()
            entries.append(dict(RelativePath=relative.as_posix(), Sha256=hashlib.sha256(data).hexdigest().upper(),
                CollectionId=folder.name, Collection=label, Cabinet=fields['Cab'], Speaker=fields['Speaker'],
                PowerAmp=fields['Power amp'], MicPreamp=fields['Mic preamp'], Microphone=mic,
                MicrophoneModel=models.get(mic), Position=pos, CaptureDistanceMm=int(match['mm']),
                Variants=match['variants'].lstrip('-').split('-') if match['variants'] else [],
                IsPreview=preview, CapturePrefix=match[1], OriginalName=wave.name, **naming, **wav_info(data)))
    if seen != set(EXPECTED) or len(entries) != 682:
        raise ValueError('Incomplete collection inventory')
    result = json.dumps(dict(Version=1, Provider='Overdriven.fr', Entries=entries), ensure_ascii=False, indent=2) + '\n'
    manifest = DEST / 'Overdriven.catalog.v1.json'
    if check:
        if manifest.read_text(encoding='utf-8') != result:
            raise ValueError('Catalog is stale')
    else:
        manifest.write_text(result, encoding='utf-8')
    print(f'{"Verified" if check else "Generated"} 13 collections, {len(entries)} unchanged WAVs and readmes')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--check', action='store_true')
    generate(parser.parse_args().check)
