# Troubleshooting

## The pedal does not connect
Check power and a USB data cable. Close other MIDI editors and try connecting again. Check connection details in Device and help. Bluetooth does not replace USB-MIDI.

## I change a preset but cannot hear it
Check that A/B/C in the app matches the pedal. Check switches and physical sections. Selecting a preset in the app does not operate the footswitch. Resolve any pending [recovery](recuperacao.md) before starting another preview.

## Settings disappeared after power-off
Live adjustments are not persistent writes. Save the bank for A/B/C. Chorus/phaser/delay/reverb creators work only in RAM; export their files to keep the edits.

## A track will not play
Check that its MP3/WAV still exists at the original path and that the audio output is available. Check volume and mute. YouTube search requires a network; local playback does not.

## REC unavailable or recording has no audio
Select a valid input above the player and check that it receives a signal in Windows. REC captures only that input; mixer volume, mute and monitoring do not determine the captured audio. Backing track and metronome are not automatically included. Press REC again to save a new track; Stop also stops playback. If the input disconnects or capture fails, preserve the recoverable WAV shown in the message. Files without samples do not create tracks; if the WAV was saved but playlist registration failed, the message shows the preserved path. See [Record](backing-tracks.md#record).

## The exported WAV sounds different from the player
Export ignores volume and mute. Speed and pitch adjustments are included only when the corresponding dialog option is checked. A/B selection defines the exported section even with looping off. See [Export](backing-tracks.md#export).

## A document will not open
Guides are included in the package. If files are missing, extract the complete package again. PDFs require an associated Windows reader. Web links require a browser and connection.

## Before requesting support
Record the app version, displayed message and steps that reproduce the issue. Keep backups and review paths or other personal data before sharing logs.
