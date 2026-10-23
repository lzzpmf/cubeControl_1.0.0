# VD612 fixtures

Source: user-supplied `CubeBaby_VD612.up`, SHA-256
`1311c2f02c36ae632716c47a72d26cebd95a06042f19d5301199a7c92b44df03`.

`firmware-ranges.bin` concatenates these source file ranges, in order:

| File offset | Length | CPU address | Content |
| --- | --- | --- | --- |
| 0xAA00 | 0x3200 | 0x830C00 | Copied RAM code, effect workers and preset application |
| 0x4A000 | 0x600 | 0xC49F00 | SysEx dispatcher |
| 0x13900 | 0x800 | 0xC13800 | Effect initialization |
| 0x48A00 | 0xA00 | 0xC48900 | Immutable tables and coefficients |

All four ranges matched read-only captures of the connected SINCO-CubeBaby.
The region-4 DSP base is 0x834724; the bank alias is 0x80CBBC and physical
slot/mode are at 0x80CB16. `dsp.bin` and `bank.bin` are read-only snapshots.
Delay and reverb were active in state 2; modulation was inactive. Modulation
test states are synthesized from the existing bounded parameter codecs.

Hardware validation identified the profile and read all four preparation
contexts plus delay/reverb recovery snapshots, with zero device writes.
Apply/restore and interrupted-write tests run against simulated memory only.
