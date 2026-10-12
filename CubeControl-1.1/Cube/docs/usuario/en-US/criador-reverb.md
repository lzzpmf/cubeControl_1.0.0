# Reverb creator

## Prepare preview
On the Pedalboard, open Reverb → Reverb creator. Enable reverb and the reverb/cabinet section, turn off modulation, select the same physical preset in the app and use Read active reverb. An incompatible profile or context prevents preview; do not bypass this check.

## Controls
Dry signal, reverberated signal, feedback, line return and damping. Adjust gradually within the editor ranges.

## Files and restoration
Import `.reverb.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write or preview for this effect on the simulated device.

Restore reverb, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

The recovery snapshot exists only in memory for this app session. If the connection is lost, reconnect the same device and use Restore reverb, subject to the context checks performed by the app. Closing or restarting the app discards this snapshot; old recovery files are not loaded automatically. Export any adjustments you want to keep.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
