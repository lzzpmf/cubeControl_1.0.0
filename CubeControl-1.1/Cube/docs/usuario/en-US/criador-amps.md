# Amp creator

## Prepare an edit
Choose a base amp or import a valid 4096-byte BIN. This format is not a cabinet WAV, preset JSON or NAM model. Offline import prepares a local edit.

## Adjust and listen
Selecting the source in the interface attempts to start preview when the pedal is available. Adjustments trigger automatic preview. Preview changes shared RAM for the active amp: selecting a source does not prove which amp is physically active. Changing the amp on the pedal may reload its RAM.

Creator gain and volume change internal tables; they differ from the preset GAIN and VOLUME controls. Classic/Modern EQ are presentation choices. Adjust gradually within the displayed limits.

## Precise editing and response profiles
Click a value or press Enter to type it; Enter confirms and Escape or leaving the field discards it. Restore band recovers that EQ's parameters and coefficients. The value marker indicates a difference from the loaded reference, not from factory contents.

Response provides two capacitor panels with profile and intensity. Original and zero intensity preserve the reference. Profiles are listening proposals, not a calibrated electrical simulation. Restore original affects only the chosen panel. Importing, reading or loading sets a new reference without trying to infer profiles from the imported sound.

Double-click a knob or fader to restore the loaded reference; Backspace with the control focused does the same. EQ resets the entire band; Response resets the original profile and intensity.

## Export, restore and write
- Export BIN saves your edit on the computer.
- Restore attempts to return the RAM captured at the start of preview.
- Save to pedal replaces the sector for the slot shown in the confirmation. This is persistent and cannot be undone through preset history.

Keep backups and read [Recovery](recuperacao.md). In simulation, device amp reading, preview and writing are disabled; local files remain available.

After a successful write, select another amp on the pedal and return to reload the slot. Byte read-back confirmation does not prove retention after power-off or physical sound reloading.
