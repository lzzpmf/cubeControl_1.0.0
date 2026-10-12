# Delay creator

## Prepare preview
On the Pedalboard, open Delay → Delay creator. Enable the delay/modulation section, turn off chorus/phaser, select the same physical preset in the app and use Read active delay. An incompatible profile or context prevents preview; do not bypass this check.

## Controls
The graph offers temporal modulation depth and speed, time in internal samples and fine feedback/mix resolution. Depth and speed are percentages of the original: zero depth removes oscillation; zero speed freezes its phase. Effective speed depends on time. Internal mix 50 keeps dry and echo signals at full level; it is not a complementary wet/dry sum. Millisecond equivalence has not been verified by audio measurement.

## Files and restoration
Import `.delay.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write or preview for this effect on the simulated device.

Restore delay, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

The recovery snapshot exists only in memory for this app session. If the connection is lost, reconnect the same device and use Restore delay, subject to the context checks performed by the app. Closing or restarting the app discards this snapshot; old recovery files are not loaded automatically. Export any adjustments you want to keep.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
