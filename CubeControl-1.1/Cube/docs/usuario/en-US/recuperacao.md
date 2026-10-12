# Backups and recovery

## Keep your edits
Export the A/B/C bank, BIN models and creator files as appropriate. Keep original WAVs and songs separately. Exporting does not write to the pedal.

## Interrupted preview
The previous state is held in memory during the current app session. If the connection is lost, reconnect the same device and use the feature's restore action, subject to profile and context checks. Restarting the app discards the snapshots; a preview from a previous session is not recovered automatically. Normal closing attempts to restore effect, amp and IR previews and reports failures before offering to close anyway.

## Preserve files
Automatic bank, IR and amp copies retained by the editor are temporary and held in memory. To keep data between sessions, explicitly export it to your chosen location. Preferences and images reside in `%LOCALAPPDATA%\CubeControl`; simulation editor data uses `CubeControl-Simulation`. Old recovery files are not loaded automatically.

## Factory recovery
Factory recovery and slot replacement are different from undo. Read the confirmation, check the destination and preserve edits before proceeding. Preset history cannot undo IR or amp sector writes.

## Confirm the result
Watch for read-back confirmation. Lost connection or incompatible context can prevent restoration. Model identification is not a unique serial number; use the same pedal and the correct context.

See [Troubleshooting](problemas.md).
