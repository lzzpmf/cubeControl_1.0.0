# Using effect creators

## Open and adjust
On the Pedalboard, open the effect’s **Advanced options**. Opening already attempts to prepare and start preview; returning to simple editing neither ends it nor restores parameters. Use the effect’s restore action to end preview and recover the previous state. Manipulate the graph contours directly. The selected element displays its numeric details. Keyboard focus allows adjustment without dragging.

Drawings are illustrative, not acoustic-response measurements. Normal pedal controls remain separate from creators.

## Select and type a parameter
The panel provides a parameter list and slider as alternatives to graph handles. Click the value or press Enter with a handle selected to type. Enter confirms and Escape cancels; leaving the field attempts to apply a valid value. Invalid or out-of-range values are not sent. The panel sits beside the graph when space allows and above it at narrower widths.

## Preview and persistence
When starting a new preview, the app checks the firmware and requires PRESET mode on the pedal. It reads the physical preset and selects the matching A/B/C in the interface. Preparation may send live adjustments to enable the section and give the effect an audible level before capturing its reference; Read active effect is therefore not a read-only operation. These adjustments may leave unsaved bank changes. The app cannot remotely change the physical pedal mode. Other operations may be blocked during preview to prevent conflicts.

Chorus, phaser, delay and reverb creators modify RAM: **they do not write their adjustments to flash or guarantee persistence after power-off**. Export saves a local file; import prepares an edit, and Apply settings sends it to an active preview.

## Specific guides
- [Chorus](criador-chorus.md)
- [Phaser](criador-phaser.md)
- [Delay](criador-delay.md)
- [Reverb](criador-reverb.md)
- [Backups and recovery](recuperacao.md)
