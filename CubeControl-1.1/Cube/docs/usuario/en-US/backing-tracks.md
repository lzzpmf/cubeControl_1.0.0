# Backing Tracks — Cube Studio

Cube Studio helps you play and practise with accompaniment. The study area has two fixed tracks: **Backing** and **My guitar**, each with a waveform, volume and mute. Search stays visible on the right.

## Choose a backing

Use **Import audio**, Ctrl+I or drop an audio file onto the timeline. Enter at least three characters or paste a link in search. Choose accompaniment search or free search. **Download** only saves an MP3 in your selected folder; **Use backing** reuses a valid download or downloads and loads the audio.

A new backing loads in full from the beginning, replaces the previous one, clears A/B and resets speed to 100% and transposition to zero. Studio asks before discarding attempts that have not been saved as audio.

Results show title, duration, an actual waveform and download status. Remaining over a result silently prepares up to 30 seconds of audio. A neutral state appears before analysis. **Listen** explicitly starts the preview and pauses study sounds. Author and source appear in result details. Downloaded audio can show its full waveform.

Search and downloads remain available during playback and capture. Previews and backing replacement are blocked during recording. Outdated preparations are canceled. Leaving Studio stops previews while study playback continues.

Choose a folder on your first download and use **Change folder** later. Progress and cancellation are shown; repeated downloads preserve existing files. Preview output and volume are in search options and use a Windows output, including when the study uses ASIO.

## Play and repeat sections

Click the ruler to seek. Space toggles playback and pause. **Stop** ends playback, capture and the metronome and returns to the beginning. Back and forward move five seconds.

**Transpose** ranges from −12 to +12 semitones, with −/+ buttons and reset to zero. **Speed** is always a percentage, from 25% to 200%, with a slider, editable value and reset to 100%. These controls affect the backing and selected attempt, retaining each capture's context.

Mark the start with **A** and the end with **B**, or Shift-drag on the ruler. Drag markers to adjust the section. **Repeat A/B** or L toggles repetition. Shift+wheel zooms the timeline; the wheel scrolls when zoomed. Ctrl+0 fits the study when the timeline has focus; elsewhere it resets Cube zoom.

## Independent metronome

On/off, BPM, **TAP** and volume remain visible. Defaults are off, 100 BPM and 4/4; time signature and sound are in the options. TAP sets click tempo and phase.

Backing speed never changes metronome BPM; BPM and TAP never change backing speed. The click works without a file, with the backing paused and after audio ends. Play, pause, seek and loop do not reset its phase. **Stop** turns everything off. The click does not automatically align with a file's beats.

## Configure and record guitar

Open audio options in the footer. Choose WASAPI for Windows devices or ASIO for your interface driver. WASAPI has separate input/output selection; ASIO uses a duplex device. **ASIO panel** opens the manufacturer's options. Choose the guitar channel, available rate and buffer, then **Apply**.

Software monitoring starts off each run. Enable it to hear input through the app. Leave it off if you already hear direct monitoring through your interface or pedal, to avoid duplicate sound. Device settings are locked during capture.

Seek and use **Record** or R. No track arming is required. The previous attempt stays silent during new capture. Finish with Record, pause or Stop. Without loop, the timeline grows; each loop pass produces an independent attempt.

Select an attempt to display and hear it. Each keeps its capture position, speed and transposition, even when recorded in a different section. Drag the guitar waveform edges to trim, or use the context menu at the cursor. Delete removes it from the list; Ctrl+Z undoes trim/deletion and Ctrl+Y redoes. Original audio files are never modified.

Playback, capture and the metronome continue when navigating through Cube. The miniplayer shows status and offers **Stop** and **Open Studio**.

## Save audio and finish

**Save audio** exports only the selected, trimmed and aligned attempt. Choose **Guitar with backing** or **Guitar only**. Stereo PCM 24-bit WAV is the default; 320 kbps MP3 is the alternative. The metronome is excluded. Progress/cancellation is available and files are only published after success.

Replacing the backing, clearing the study or closing prompts for discard if attempts have not been exported. Trimming an exported attempt requires saving the new version to avoid the prompt. Changing pitch, speed or A/B does not request a save.

Studio starts empty and does not open, save or restore projects. Existing `.cubestudio.json`, `.ministudio.json`, `.cubeproject` files and their media remain untouched. Downloads and exports stay in chosen folders. Temporary recordings use `PracticeRecordings` in app data, in a folder unique to each run, and are cleaned after confirmed discard.

## Troubleshooting

Check output, volumes and mute on both tracks. For recording, check input and channel in audio options. ASIO requires its installed driver; WASAPI uses available Windows devices.

Device or writing failures stop the operation and preserve recoverable audio. In audio options, use **Recover recordings** and choose a journal. Recovery creates an attempt from an audio copy without opening a project. Review it and use Save audio to keep it. Recovery sources remain intact.

Search, preview and download require internet. Check the result's message and chosen folder after failure. Missing tools require reinstalling the complete package. Cancellation preserves completed files.
