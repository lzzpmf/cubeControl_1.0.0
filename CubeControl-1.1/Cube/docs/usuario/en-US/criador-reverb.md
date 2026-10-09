# Reverb creator

## Prepare preview
On the Pedalboard, open Reverb → Reverb creator. Enable reverb and the reverb/cabinet section, turn off modulation, select the same physical preset in the app and use Read active reverb. An incompatible profile or context prevents preview; do not bypass this check.

## Controls
Dry signal, reverberated signal, feedback, line return and damping. Adjust gradually within the editor ranges.

## Files and restoration
Import `.reverb.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write or preview for this effect on the simulated device.

Restore reverb, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

After restarting, pending recovery loads without reapplying adjustments. Reconnect the same device, restore the matching context and use Restore reverb. Preserve `pending-reverb-preview.json` and `ReverbRecovery` in the data folder until recovery is resolved.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
