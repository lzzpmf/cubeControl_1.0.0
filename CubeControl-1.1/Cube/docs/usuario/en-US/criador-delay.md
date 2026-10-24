# Delay creator

## Prepare preview
Select PRESET mode and the desired A/B/C physically on the pedal. On the Pedalboard, open the effect’s **Advanced options**: this already attempts to prepare and start preview. The app selects the detected physical preset in the interface. Preparation enables the delay/modulation section and raises the mix if it is zero; disabling chorus/phaser is not required. This preparation may change live parameters without saving the bank. An incompatible profile or context prevents preview; see [Using effect creators](criadores-efeitos.md).

## Controls
The graph offers temporal modulation depth and speed, time in internal samples and fine feedback/mix resolution. Depth and speed are percentages of the original: zero depth removes oscillation; zero speed freezes its phase. Effective speed depends on time. Internal mix 50 keeps dry and echo signals at full level; it is not a complementary wet/dry sum. Millisecond equivalence has not been verified by audio measurement.

## Files and restoration
Import `.delay.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write.

Restore delay, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

The recovery snapshot exists only in memory for this app session. If the connection is lost, reconnect the same device and use Restore delay, subject to the context checks performed by the app. Closing or restarting the app discards this snapshot; old recovery files are not loaded automatically. Export any adjustments you want to keep.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
