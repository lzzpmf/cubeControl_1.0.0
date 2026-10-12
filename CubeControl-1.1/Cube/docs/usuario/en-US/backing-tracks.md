# Backing Tracks

## Play
Add MP3/WAV files and choose a Windows audio output. The list references original files; keep them available. Playing a track does not require a USB pedal connection. Bluetooth uses a Windows audio output and does not edit the pedal.

## Practice a section
Use A/B to mark and loop a section. Adjust speed and pitch with the available controls. Automatically detected BPM and key are estimates; you can correct them. Guitar monitoring uses the chosen audio input and does not simulate the pedal DSP.

The backward and forward buttons skip five seconds. They display icons only; hover over them to read their tooltips.

Under **Tempo & pitch**, scroll over the percentage to change speed in 1% steps, from 50% to 150%. Scroll over the semitone value to change pitch in steps of 1 semitone, from −6 to +6. Scrolling up increases the value; scrolling down decreases it. A selected track is required. Click either value to open its options and restore speed or pitch. Scrolling over these controls changes the value without scrolling the page.

## Mixer

Open the mixer with the settings icon beside the metronome. Each channel — **Track**, **Guitar**, and **Metronome** — has a horizontal fader, an editable percentage, and a mute button beside the value. The button itself shows the muted state, without a separate duplicate indicator. Channel mutes are independent and preserve their volume settings.

**Compact** mode keeps volumes and mutes accessible. **Expanded** also shows guitar monitoring and metronome commands and signal meters. The guitar input meter is before its volume control; the output meter combines the track and synchronized click. **Reset volumes** restores track to 30%, guitar to 0%, and metronome to 30%, preserving mutes. Double-click a fader to restore only that channel's default volume.

## Record
Under **Recording source**, choose **Guitar only**, **Backing track + guitar**, or a specific Windows input/output. The guitar preset uses the input selected above. The combined preset requires a selected track, input and output; it records the output mix, including other sounds playing on that device.

Use **Start recording**, choose the WAV destination and play the track when ready. Combined mode starts guitar monitoring automatically; if its volume is 0%, it changes to 50% during recording. Use headphones to avoid feedback and adjust the guitar and backing volumes. The previous monitor state is restored when recording ends.

Use **Stop and save** to finalize the WAV. You can show the file or add it to the playlist. Choose a different source before starting another recording. If a failure occurs, the app shows the recoverable WAV path. Recording does not require USB; audio comes from the chosen audio device.

## Export
Export creates a new file at the chosen destination without changing the original. Check the section and settings before exporting. Removing a track from the list does not delete its audio file.

## YouTube
Search and download require internet and use tools included in the package. Choose your own or authorized content. A network failure should not replace an existing file. Importing does not start playback automatically.

Playlist and YouTube use separate cards below the player, with independent searches. In narrow windows, both cards remain open and stack vertically.

The headphones button inside **Search tracks** appends “backing track” to the search term. When on, it has an accent-colored background and a checkmark; when off, it is neutral with no checkmark. This changes the query and does not guarantee that every result is an instrumental backing track.

Hover over a video for 800 ms to preview up to 30 seconds. If a local track is playing, it pauses when preview audio is ready. Leaving the card or finishing the preview keeps the track paused at the same position; use Play to resume. If preparation fails, the track keeps playing. With a keyboard, use Preview audio and Escape to stop.

Choose a folder on your first download. It is remembered for later downloads and across app restarts. Use **Change folder** in the YouTube card to select another destination. Duplicate filenames receive a numeric suffix without overwriting existing files. The card shows the current stage, an indeterminate progress bar and a cancel action during download, conversion and validation.

## Metronome

The metronome is integrated into the player: the metronome icon toggles it, and the hand button accepts repeated clicks for **Tap tempo**. Scroll over the BPM value to increase or decrease it in steps of 1, from 20 to 300, without scrolling the page. Click the value to edit it. The arrow beside it opens synchronization, meter, accent, count-in and alignment options. Its separate volume and mute controls are in the mixer.

Choose Click, Bell, Ping, Woodblock or Hi-hat from the sound menu, or import a mono/stereo PCM or float WAV up to two seconds long. The app copies the file into its local data and remembers the selection; unavailable files fall back to Click.
Use **Start metronome** with **Follow track** off to practice even with an empty playlist. Set its BPM, Tap tempo, time signature, accent and volume. Track mute does not silence the metronome; use the Metronome channel mute. In 6/8, BPM represents quarter notes and each click represents an eighth note.

With **Follow track** on, the metronome uses the detected or corrected BPM and follows the local track's speed, pause, seek and loop. Use **Mark first beat here** or **First beat (s)** to align the grid. **Count-in (bars)** accepts 0, 1 or 2 and prepares playback; resuming a pause and repeating a loop do not repeat the count-in. YouTube preview does not support synchronization; independent mode remains available.

Preferences, manual BPM and alignment per file are remembered, but the metronome always opens stopped. Changing mode or output stops clicks. WAV export excludes the metronome; loopback output recording can include the clicks you hear.

## Session and files
The playlist, selection, output and playback adjustments remain in the current session only. Reopening the app starts an empty playlist; the old `backing-tracks-v1.json` is neither loaded nor updated. Metronome preferences, manual BPM and beat alignment per file, plus the YouTube download folder and completed-download records, are stored separately. Downloaded audio remains in your chosen folder; the list is not a backup of files. See [Privacy](privacidade.md) for storage and external services.
