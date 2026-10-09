# Backups and recovery

## Keep your edits
Export the A/B/C bank, BIN models and creator files as appropriate. Keep original WAVs and songs separately. Exporting does not write to the pedal.

## Interrupted preview
A preview may leave a pending record. After restart, the app loads recovery without reapplying adjustments. Reconnect the same device and use the feature's restore action. Firmware profile, bank and physical context must match; do not overwrite unknown state.

## Preserve files
Backups and records generally reside in `%LOCALAPPDATA%\CubeControl`, including `Backups`, `AmpRecovery`, effect recovery folders and `pending-*-preview.json` files. Do not delete them while preview recovery is pending. Simulation editor data resides in `CubeControl-Simulation`.

## Factory recovery
Factory recovery and slot replacement are different from undo. Read the confirmation, check the destination and preserve edits before proceeding. Preset history cannot undo IR or amp sector writes.

## Confirm the result
Watch for read-back confirmation. Lost connection or incompatible context can prevent restoration. Model identification is not a unique serial number; use the same pedal and the correct context.

See [Troubleshooting](problemas.md).
