# Backing Tracks

## Play
Add MP3/WAV files and choose a Windows audio output. The list references original files; keep them available. Playing a track does not require a USB pedal connection. Bluetooth uses a Windows audio output and does not edit the pedal.

## Practice a section
Use A/B to mark and loop a section. Adjust speed and pitch with the available controls. Automatically detected BPM and key are estimates; you can correct them. Guitar monitoring uses the chosen audio input and does not simulate the pedal DSP.

BPM and key identification runs locally and offline, using up to five segments of up to 20 seconds distributed across the track. It compares rhythmic patterns across segments and compensates for small tuning offsets when estimating a major or minor key. Silence, ambiguous rhythm, and tempo or key changes may produce **not identified**. Automatic detection covers 40 to 240 BPM; manual correction still accepts 20 to 300 BPM. Manual corrections are preserved during reanalysis.

Added, downloaded and recorded tracks start with **no tuning or preset association**. In **Correct track data**, leave Tuning blank to remove its association and choose the unassigned preset option to clear that link. An estimated key does not identify string tuning or associate a preset. Manually chosen associations remain in place during the session.

The backward and forward buttons skip five seconds. They display icons only; hover over them to read their tooltips.

Under **Tempo & pitch**, scroll over the percentage to change speed in 1% steps, from 50% to 150%. Scroll over the semitone value to change pitch in steps of 1 semitone, from −6 to +6. Scrolling up increases the value; scrolling down decreases it. A selected track is required. Click either value to open its options and restore speed or pitch. Scrolling over these controls changes the value without scrolling the page.

Selecting another valid output resumes the track at its current position without requiring Play again; opening the device may cause a brief interruption. A paused track stays paused. Changing input does not pause the track, and monitoring is restored if it was enabled. Input selection is locked during REC. A disconnected or failed device may interrupt audio; check the message and selected output.

## Mixer

Open the mixer with the settings icon beside the metronome. Each channel — **Track**, **Guitar**, and **Metronome** — has a horizontal fader, an editable percentage, and a mute button beside the value. The button itself shows the muted state, without a separate duplicate indicator. Channel mutes are independent and preserve their volume settings.

The mixer has a single layout, with monitoring and metronome commands always available. Behind each fader, a meter shows that channel's digital peak after volume and mute, from −60 to 0 dBFS, with a one-second peak hold. Track and click are measured separately; the guitar shows a level only while monitoring is active. The input clipping warning remains independent of guitar volume. These are sample peaks, not true-peak or calibrated analog VU measurements. **Reset volumes** restores track to 30%, guitar to 0%, and metronome to 30%, preserving mutes. Double-click a fader to restore only that channel's default volume.

## Record
Choose **Guitar input** above the player and press **REC**, after Loop. Recording starts immediately without a dialog and captures only that input. It does not automatically include the backing track, metronome or output sounds. The WAV uses the device's WASAPI capture format.

REC does not start track playback or change monitoring, mute or volumes. During capture, the button is active and the player shows **Recording** and elapsed time. Input changes are blocked until completion; playback output can still change. Direct capture is independent of monitor volume and mute. If the device itself provides a mix at its input, that mix will be captured.

Press **REC** again to stop recording only; **Stop** stops playback and recording, even without a selected track. After finalizing the WAV header, the app adds one new track at the end of the playlist. Current selection and playback are preserved; an empty playlist selects the recording without playing it automatically.

Files receive a timestamp and unique identifier in `Recordings` under the current data directory; simulation uses its separate directory. Use **Show file** in the track menu to locate the WAV. Closing the app finalizes capture and registers the track before saving the playlist. Files without samples do not create tracks. On failure, preserve the recoverable WAV shown in the message; it is not reported as a completed recording. If only playlist insertion or persistence fails, a separate message confirms that the WAV was saved.

## Export
Export produces a 16-bit PCM WAV. With valid A and B markers, it exports one pass through the section, even when looping is off; without a valid section, it exports the entire track. The minimum interval between A and B is 0.1 seconds.

The dialog's option to apply speed and pitch without volume/mute starts unchecked. Check it to include speed and pitch adjustments; leave it unchecked to keep the original speed and pitch. Player volume and mute do not affect export. Guitar, metronome and sounds from other apps are also excluded. REC captures only the selected input.

The app prevents choosing an audio file in the playlist as the destination. Another existing file can be replaced after confirmation in the save dialog; choose a new name to preserve it. Replacement happens only when export completes. Canceling preserves the previous destination. Removing a track from the list does not delete its audio file.

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

Preferences, manual BPM and alignment per file are remembered, but the metronome always opens stopped. Changing mode stops clicks. Selecting another valid output restores the metronome if it was enabled. WAV export and REC do not automatically include the metronome.

## Session and files
Playlist order and paths are saved in `backing-playlist-v1.json` after completed recordings and when closing the app. Missing files remain listed so you can use **Locate again**. Selection, output and playback adjustments remain session-only; the old `backing-tracks-v1.json` is neither loaded nor updated. Metronome preferences, manual BPM and beat alignment per file, plus the YouTube download folder and completed-download records, are stored separately. Downloaded audio remains in your chosen folder; the list is not a backup of files. See [Privacy](privacidade.md) for storage and external services.
