# Backups and recovery

## Keep your edits
Export the A/B/C bank, BIN models and creator files as appropriate. Keep original WAVs and songs separately. Exporting does not write to the pedal.

## Interrupted preview
The previous state is held in memory during the current app session. If the connection is lost, reconnect the same device and use the feature's restore action, subject to profile and context checks. Restarting the app discards the snapshots; a preview from a previous session is not recovered automatically. Normal closing attempts to restore effect, amp and IR previews and reports failures before offering to close anyway.

## Preserve files
Automatic bank, IR and amp copies retained by the editor are temporary and held in memory. To keep data between sessions, explicitly export it to your chosen location. Preferences and images reside in `%LOCALAPPDATA%\CubeControl`. Old recovery files are not loaded automatically.

## Factory recovery
Factory recovery and slot replacement are different from undo. Read the confirmation, check the destination and preserve edits before proceeding. Preset history cannot undo IR or amp sector writes.

Factory preset restoration looks for `%USERPROFILE%\Downloads\CubeSuite\CubeSuite.exe` and `bin\BFI.bin` in that installation. The app validates specific versions by size/hash; missing or unrecognized files make this feature unavailable. This flow has no source-location picker. Restore original IRs checks BFI/BFIN packs separately in the same `bin` folder. These external resources are not bundled with cubeControl; `FactoryAmps.bin` does not replace them.

## Reinstall or update firmware
In Device and help, disconnect the editor’s MIDI session before installing. Select a listed `.up` or choose a file; refreshing the list does not write to the pedal. The app validates the format, but this does not establish that the variant is suitable for your device.

Export any data you want to keep: presets and IRs may be replaced. Close `flash_loader` and enter flash mode by powering on the pedal connected to the PC while holding the third footswitch, as stated in the app’s confirmation. After confirmation, Windows requests administrator permission. Do not power off or disconnect during writing. Progress and errors appear in the editor; this is different from restoring only presets or IRs. If writing is interrupted, follow the error message before retrying; automatic restoration of the previous firmware is not guaranteed.

## Confirm the result
Watch for read-back confirmation. Lost connection or incompatible context can prevent restoration. Model identification is not a unique serial number; use the same pedal and the correct context.

See [Troubleshooting](problemas.md).
