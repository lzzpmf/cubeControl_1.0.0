"""Extract the bounded Cube Baby B effect profile from a hash-pinned firmware.

Addresses were traced through the preset applier, DSP caller, dispatch table,
initializers and their literal pools. This script never accesses a device.
The update's XIP bias is 0xBFFF00; its copied code uses 0x826200 and
0x801E00. Audio parameter offsets match the existing 0x8347F8 DSP layout.
"""
import argparse
import hashlib
import json
from pathlib import Path

SHA256 = "9184852bdebfa1d939ad21a250a579104ec1e3de17f072edbd8311015daad216"
# Effect, legacy CPU address, B CPU address, source file offset, length.
RANGES = [
    ("shared", 0x832108, 0x832150, 0xBF50, 12),
    ("shared", 0x83239C, 0x83239C, 0xC19C, 0x104),
    ("shared", 0x831D84, 0x831DCC, 0xBBCC, 0x46),
    ("shared", 0x831F5C, 0x831FA4, 0xBDA4, 0x1B8),
    ("shared", 0x8327C4, 0x8327C4, 0xC5C4, 0x88),
    ("chorus", 0x8326B4, 0x8326B4, 0xC4B4, 0x110),
    ("chorus", 0x832188, 0x832188, 0xBF88, 0xCC),
    ("chorus", 0x833540, 0x833540, 0xD340, 0x54),
    ("chorus", 0x831AC0, 0x831B08, 0xB908, 0x49C),
    ("chorus", 0xC13B04, 0xC13AF4, 0x13BF4, 0x30),
    ("chorus", 0x80A91E, 0x80A92A, 0x8B2A, 0x2A),
    ("delay/reverb", 0x831AC0, 0x831B08, 0xB908, 0x484),
    ("delay/reverb", 0x833724, 0x833724, 0xD524, 0x150),
    ("delay", 0x832DA8, 0x832DA8, 0xCBA8, 0x338),
    ("delay", 0xC13C3C, 0xC13C2C, 0x13D2C, 0xC4),
    ("delay", 0x833562, 0x833562, 0xD362, 0x32),
    ("delay/reverb", 0x80A8BC, 0x80A8C8, 0x8AC8, 0x62),
    ("delay", 0xC488E0, 0xC4883C, 0x4893C, 0x38),
    ("delay/reverb", 0x8359A0, 0x8359A0, 0xF7A0, 0x6C),
    ("reverb", 0x833978, 0x833978, 0xD778, 0x284),
    ("reverb", 0x833904, 0x833904, 0xD704, 0x74),
    ("reverb", 0x8338E0, 0x8338E0, 0xD6E0, 0x24),
    ("reverb", 0xC13D00, 0xC13CF0, 0x13DF0, 0x200),
    ("reverb", 0xC48C98, 0xC48BF4, 0x48CF4, 0x84),
    ("reverb", 0xC4881C, 0xC48778, 0x48878, 0xB4),
    ("reverb", 0xC488D0, 0xC4882C, 0x4892C, 0x10),
    ("reverb", 0xC48D38, 0xC48C94, 0x48D94, 0x78),
]
POINTERS = [(0x80C380, 0xA580, 4), (0x80C38C, 0xA58C, 4), (0x80C3E0, 0xA5E0, 12)]


def extract(path):
    data = Path(path).read_bytes()
    if hashlib.sha256(data).hexdigest() != SHA256:
        raise ValueError("Firmware B hash differs; do not reuse this mapping")
    return data


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("firmware")
    parser.add_argument("--fixture", type=Path)
    parser.add_argument("--evidence", type=Path, help="Compare read-only USB captures with the update")
    parser.add_argument("--disassembly", type=Path, help="Requires capstone==5.0.9; preserve bounded code disassembly")
    args = parser.parse_args()
    data = extract(args.firmware)
    fixture = {}
    for effect, legacy, cpu, offset, size in RANGES:
        value = data[offset:offset + size]
        key = str((cpu - 0x8347F8) & 0xFFFFFFFF)
        if len(value.hex()) > len(fixture.get(key, "")):
            fixture[key] = value.hex()
        print(f'(0x{legacy:X}, 0x{cpu:X}, 0x{size:X}, "{hashlib.sha256(value).hexdigest()}"), // {effect}')
    for cpu, offset, size in POINTERS:
        fixture[str((cpu - 0x8347F8) & 0xFFFFFFFF)] = data[offset:offset + size].hex()
    if args.fixture:
        args.fixture.write_text(json.dumps(fixture, indent=2) + "\n", encoding="utf-8")
    if args.disassembly:
        from capstone import Cs, CS_ARCH_ARM, CS_MODE_THUMB, CS_MODE_LITTLE_ENDIAN
        from capstone.arm import ARM_OP_MEM, ARM_REG_PC
        import struct
        cs = Cs(CS_ARCH_ARM, CS_MODE_THUMB | CS_MODE_LITTLE_ENDIAN)
        cs.detail = True
        lines = [f'Firmware B SHA-256 {SHA256}',
                 'Bounded disassembly; ranges can include literal pools, which are data, not instructions.']
        for effect, legacy, cpu, offset, size in RANGES:
            if offset >= 0x40000:
                continue  # Coefficient and preset tables are data.
            lines.append(f'\n{effect}: CPU {cpu:08X}, file {offset:X}, size {size:X}')
            bias = cpu - offset
            for ins in cs.disasm(data[offset:offset + size], cpu):
                extra = ''
                for op in ins.operands:
                    if op.type == ARM_OP_MEM and op.mem.base == ARM_REG_PC:
                        literal = ((ins.address + 4) & ~3) + op.mem.disp
                        source = literal - bias
                        if 0 <= source <= len(data) - 4:
                            extra = f' ; [{literal:08X}]={struct.unpack_from("<I", data, source)[0]:08X}'
                lines.append(f'{ins.address:08X} {ins.bytes.hex():10s} {ins.mnemonic:9s} {ins.op_str}{extra}')
        args.disassembly.write_text('\n'.join(lines) + '\n', encoding='utf-8')
    if args.evidence:
        for effect, legacy, cpu, offset, size in RANGES:
            actual = (args.evidence / f'{cpu:08X}-{size:X}.bin').read_bytes()
            if actual != data[offset:offset + size]:
                raise ValueError(f'{effect} USB evidence differs at {cpu:08X}, length {size:X}')
        for cpu, offset, size in POINTERS:
            if (args.evidence / f'{cpu:08X}-{size:X}.bin').read_bytes() != data[offset:offset + size]:
                raise ValueError(f'Dispatch/oscillator evidence differs at {cpu:08X}')
        print("All 27 code/table ranges and 3 dispatch/oscillator ranges match the connected pedal.")


if __name__ == "__main__":
    main()
