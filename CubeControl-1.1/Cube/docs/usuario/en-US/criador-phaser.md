# Phaser creator

## Prepare preview
On the Pedalboard, open Phaser → Phaser creator. Enable phaser and the delay/modulation section on the physical preset, select the same A/B/C in the app and use Read active phaser. An incompatible profile or context prevents preview; do not bypass this check.

## Controls
Estimated speed and frequency endpoints, level and section count. The lower endpoint must not exceed the upper one. Estimated values are not acoustic measurements.

## Files and restoration
Import `.phaser.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write or preview for this effect on the simulated device.

Restore phaser, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

After restarting, pending recovery loads without reapplying adjustments. Reconnect the same device, restore the matching context and use Restore phaser. Preserve `pending-phaser-preview.json` and `PhaserRecovery` in the data folder until recovery is resolved.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
