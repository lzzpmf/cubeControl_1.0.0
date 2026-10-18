# Backing Tracks

## Play
Add MP3/WAV files and choose a Windows audio output. The list references original files; keep them available. Playing a track does not require a USB pedal connection. Bluetooth uses a Windows audio output and does not edit the pedal.

## Practice a section
Use A/B to mark and loop a section. Adjust speed and pitch with the available controls. Automatically detected BPM and key are estimates; you can correct them. Guitar monitoring uses the chosen audio input and does not simulate the pedal DSP.

BPM and key identification runs locally and offline, using up to five segments of up to 20 seconds distributed across the track. It compares rhythmic patterns across segments and compensates for small tuning offsets when estimating a major or minor key. Silence, ambiguous rhythm, and tempo or key changes may produce **not identified**. Automatic detection covers 40 to 240 BPM; manual correction still accepts 20 to 300 BPM. Manual corrections are preserved during reanalysis.

Added, downloaded and recorded tracks start with **no tuning or preset association**. In **Correct track data**, leave Tuning blank to remove its association and choose the unassigned preset option to clear that link. An estimated key does not identify string tuning or associate a preset. Manually chosen associations remain in place during the session.

The backward and forward buttons skip five seconds. They display icons only; hover over them to read their tooltips.

Under **Tempo & pitch**, scroll over the percentage to change speed in 1% steps, from 50% to 150%. Scroll over the semitone value to change pitch in steps of 1 semitone, from −6 to +6. Scrolling up increases the value; scrolling down decreases it. A selected track is required. Click either value to open its options and restore speed or pitch. Scrolling over these controls changes the value without scrolling the page.

Outside recording, selecting another valid output resumes the track at its current position without requiring Play again; opening the device may cause a brief interruption. A paused track stays paused. Changing input does not pause the track, and monitoring is restored if it was enabled. Input and output selection are locked during REC. A disconnected or failed device may interrupt audio; check the message and selected output.

## Mixer

Open the mixer with the settings icon beside the metronome. Each channel — **Track**, **Guitar**, and **Metronome** — has a horizontal fader, an editable percentage, and a mute button beside the value. The button itself shows the muted state, without a separate duplicate indicator. Channel mutes are independent and preserve their volume settings.

The mixer has a single layout, with monitoring and metronome commands always available. Behind each fader, a meter shows that channel's digital peak after volume and mute, from −60 to 0 dBFS, with a one-second peak hold. Track and click are measured separately; the guitar shows a level only while monitoring is active. The input clipping warning remains independent of guitar volume. These are sample peaks, not true-peak or calibrated analog VU measurements. **Reset volumes** restores track to 30%, guitar to 0%, and metronome to 100%, preserving mutes. Double-click a fader to restore only that channel's default volume.

## Sessions and tracks

Each playlist song opens a session with synchronized backing audio and recordings. **Add track**, or dropping files onto session tracks, imports audio into that session; adding files to the playlist creates separate songs. Selecting a track changes the editing target without interrupting other tracks.

Each track has **Mute**, **Solo** and volume. Multiple solos are allowed; when any solo is active, only soloed tracks are heard. Mute takes priority. The Track mixer channel controls the combined session; monitoring and metronome remain separate. Drag songs or tracks to reorder them: the insertion line and label show the destination, with scrolling near the edges. Escape cancels. Move up/down buttons provide an alternative. Vertical order does not change start time.

The playlist saves sessions, order, mute/solo, volume and take parameters. Older playlists migrate to one session per file with a `.v1.bak` copy; old recordings are not automatically associated. Missing files remain listed; use **Locate track audio**. Removing a track does not delete its WAV.

## Record

Choose an input and output, then press **REC**. Playback starts at the cursor and a new track appears immediately, with a growing waveform from captured samples. Capture records only the input, using the device's WASAPI format; it does not automatically include backing tracks, metronome or monitoring. A mix supplied by the device itself is recorded as received.

During REC, input/output switching, seeking, looping, speed, pitch and session switching are locked. Mute, solo and reordering remain available. YouTube preview is blocked to avoid pausing playback. Recording is continuous, without loop takes. Starting with altered speed/pitch is supported; these settings are stored for subsequent aligned and transposed playback.

**Follow recording** keeps the recording end visible when zoomed in. Navigating the overview or changing zoom disables following; enable it again to resume. Positive **Latency compensation (ms)** values shift the take earlier; negative values shift it later. Configure this before recording. Alignment uses capture timestamps and the player clock, but physical accuracy depends on the device and requires listening to a take.

Press REC again to finalize and pause playback; **Stop** finalizes and returns to the beginning. The same track changes from recording to finalizing and saved. Recording without a selected song creates a session. Empty captures do not become valid tracks. Interruptions preserve the reported recoverable WAV and label the track as interrupted. Playlist save failures are reported separately from audio save failures.

Files receive a timestamp and unique identifier in `Recordings` under the data directory. Closing the app finalizes capture. The visual envelope uses bounded memory and nominal 50 ms updates rather than loading the entire recording into RAM.

## Export

Choose **Session mix** or **Selected track**. Both produce 16-bit PCM stereo WAV at 48 kHz. The mix respects alignment, track mute/solo and volumes, and overall volume/mute; it excludes monitoring and the metronome. Isolated export ignores playback mute/solo and volume.

A valid A/B range exports one pass even when looping is disabled; without a valid range, export uses the available duration. Applying speed and pitch uses current session settings; disabling it uses 100% and original pitch while compensating for take settings. Export preserves source files and supports cancellation.

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

`backing-playlist-v1.json` retains its filename for location compatibility and now stores session format version 2: order, files, tracks, mute/solo, volumes, take offsets and parameters, session speed/pitch and latency compensation. Organization and mix changes are persisted, and closing saves again. Selection, output and playback state are not restored. The old `backing-tracks-v1.json` is not loaded or updated. Metronome preferences, per-file BPM/alignment and download records remain separate. The playlist references audio files; it is not their backup. See [Privacy](privacidade.md).

Metronome sounds, including non-silent custom WAVs, are peak-normalized to 0 dBFS before the fader. The default and reset level is 100%; previously saved volumes are preserved. Lowering the fader lowers the output peak. Peak normalization does not equalize perceived loudness across timbres.
