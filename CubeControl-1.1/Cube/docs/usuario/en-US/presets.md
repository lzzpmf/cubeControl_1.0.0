# Presets, saving and files

## Edit and listen
A/B/C select the preset being edited. Select the matching preset on the physical pedal to hear it. Available adjustments are sent live; saving is still required for persistence.

| Action | Result |
| --- | --- |
| Save A/B/C | Writes the complete three-preset bank and checks it by reading back |
| Export bank | Saves local editing as a JSON file on the computer |
| Import bank | After confirmation, applies known fields; saving is still required |
| Read again or reconnect | Loads pedal state; observe replacement and backup messages |
| Undo | Reverts preset edit history; it does not undo IR or amp writes |

## Keep and recover your work
Export the bank before major changes. Reading back does not prove retention after power-off. Use the correct files: bank JSON for A/B/C, WAV for IR, BIN for amp and a dedicated JSON format for each effect creator.

See [Backups and recovery](recuperacao.md) and [Pedalboard](pedaleira.md).
