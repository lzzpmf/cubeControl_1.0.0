"""Read-only, hash-pinned analysis of the Cube Baby 20220706 firmware.

Install capstone==5.0.9 in a local venv. No device access or firmware upload.
Addresses are CPU virtual addresses, NOT USB region offsets.
"""
import argparse
import hashlib
import struct
from pathlib import Path

from capstone import Cs, CS_ARCH_ARM, CS_MODE_THUMB, CS_MODE_LITTLE_ENDIAN
from capstone.arm import ARM_OP_MEM, ARM_REG_PC

SHA256 = "d99a9bc4ba0636118801b1e3a8d11adadf03831d316c7477af8de015ba5bd743"


class Firmware:
    def __init__(self, path, live=False):
        self.data = Path(path).read_bytes()
        self.live = live
        expected = "91da48cbe86aedca0debc4f08fa538e9193bfde46960af5c550b2ae1963708f6" if live else SHA256
        if hashlib.sha256(self.data).hexdigest() != expected:
            raise ValueError("Firmware hash differs; do not reuse this address map")
        self.cs = Cs(CS_ARCH_ARM, CS_MODE_THUMB | CS_MODE_LITTLE_ENDIAN)
        self.cs.detail = True

    def offset(self, address):
        if self.live:
            if 0x82F3F8 <= address < 0x8347F8:
                return address - 0x82F3F8
            raise ValueError(f"Address outside captured live code {address:08X}")
        if 0x830C00 <= address < 0x836000:
            return address - 0x826200
        if 0xC00000 <= address < 0xBFFF00 + len(self.data):
            return address - 0xBFFF00
        raise ValueError(f"Unmapped address {address:08X}")

    def word(self, address):
        return struct.unpack_from('<I', self.data, self.offset(address))[0]

    def disasm(self, address, size):
        offset = self.offset(address)
        for ins in self.cs.disasm(self.data[offset:offset + size], address):
            extra = ''
            for op in ins.operands:
                if op.type == ARM_OP_MEM and op.mem.base == ARM_REG_PC:
                    literal = ((ins.address + 4) & ~3) + op.mem.disp
                    try:
                        value = self.word(literal)
                        extra = f' ; [{literal:08X}]={value:08X} f32={struct.unpack("<f", struct.pack("<I", value))[0]:.9g}'
                    except ValueError:
                        pass
            print(f'{ins.address:08X} {ins.bytes.hex():10s} {ins.mnemonic:9s} {ins.op_str}{extra}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--live', action='store_true', help='Use the hash-pinned 2026-10-01 region-4 capture')
    parser.add_argument('firmware')
    sub = parser.add_subparsers(dest='command', required=True)
    dis = sub.add_parser('disasm')
    dis.add_argument('address', type=lambda x: int(x, 0))
    dis.add_argument('size', type=lambda x: int(x, 0))
    refs = sub.add_parser('words')
    refs.add_argument('values', nargs='+', type=lambda x: int(x, 0))
    args = parser.parse_args()
    fw = Firmware(args.firmware, args.live)
    if args.command == 'disasm':
        fw.disasm(args.address, args.size)
    else:
        for value in args.values:
            needle = struct.pack('<I', value)
            for offset in range(0x100, len(fw.data) - 3, 2):
                if fw.data[offset:offset + 4] == needle:
                    if fw.live:
                        print(f'{value:08X} file={offset:08X} CPU={offset + 0x82F3F8:08X}')
                        continue
                    ram = f' RAM={offset + 0x826200:08X}' if 0xAA00 <= offset < 0xFE00 else ''
                    print(f'{value:08X} file={offset:08X} XIP={offset + 0xBFFF00:08X}{ram}')


if __name__ == '__main__':
    main()
