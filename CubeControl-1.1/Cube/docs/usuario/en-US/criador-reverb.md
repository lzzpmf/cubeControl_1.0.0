# Reverb creator

## Prepare preview
Select PRESET mode and the desired A/B/C physically on the pedal. On the Pedalboard, open the creator and use Read active reverb to start preview. The app selects the detected physical preset in the interface. Preparation enables the reverb/cabinet section and raises reverb if it is zero; disabling modulation is not required. This preparation may change live parameters without saving the bank. An incompatible profile or context prevents preview; see [Using effect creators](criadores-efeitos.md).

## Controls
Dry signal, reverberated signal, feedback, line return and damping. Adjust gradually within the editor ranges.

## Files and restoration
Import `.reverb.json` prepares local settings; Apply settings sends them to an active preview. Export does not write to the pedal. There is no persistent flash write or preview for this effect on the simulated device.

Restore reverb, normal disconnection, reading again and normal closing attempt to return the original parameters. Lost connection or a context change stops writes; an attempted restoration is not confirmation of success.

The recovery snapshot exists only in memory for this app session. If the connection is lost, reconnect the same device and use Restore reverb, subject to the context checks performed by the app. Closing or restarting the app discards this snapshot; old recovery files are not loaded automatically. Export any adjustments you want to keep.

See [Effect creators](criadores-efeitos.md) and [Backups and recovery](recuperacao.md).
