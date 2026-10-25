# Backing Tracks — Cube Studio

Cube Studio helps you practise with accompaniment. Two compact tracks, **Backing** and **My guitar**, have volume, pan, mute and solo controls in their headers and click-to-seek waveforms. Search stays on the right. Play, Pause, Stop, Rec and the position bar are at the top; Rec records guitar directly. The interface fills the Backing tracks tab with normal CubeControl navigation. Search stacks below the tracks in narrower windows. Knobs follow the app theme and custom finishes.

## Choose a backing

Use **Import audio**, Ctrl+I or drop an audio file onto the timeline. Enter at least three characters or paste a link in search. Choose accompaniment search or free search. **Add to session** reuses a valid download or downloads and loads the audio directly into the backing track.

A new backing loads in full from the beginning, replaces the previous one, clears A/B and resets speed to 100% and transposition to zero. Loading from search discards previous guitar attempts without confirmation only after the new audio is prepared successfully. Local imports retain the discard confirmation.

Results show title, author, duration and **Add to session**. Hovering starts up to 30 seconds of preview and pauses study sounds; leaving the result stops the preview. Author and source also appear in the tooltip. The search field context menu switches between accompaniment search and free search.

Search and downloads remain available during playback and capture. Previews and backing replacement are blocked during recording. Outdated preparations are canceled. Leaving Studio stops previews while study playback continues.

When no folder is configured, the first download automatically uses Music/CubeControl/BackingTracks. The folder button in search changes the destination. Audio downloads in its available original format without an intermediate MP3 conversion. Completed downloads are reused. Previews share the session ASIO or WASAPI device; their waveform and cursor identify the excerpt being heard. Results show progress and the footer offers cancellation.

## Play and repeat sections

Click the timeline to seek. Space toggles playback and pause. **Stop** ends playback, capture and the metronome and returns to the beginning.

**Transpose** ranges from -12 to +12 semitones using specialized asset buttons; the value is read-only. **Speed** ranges from 25% to 200%, using the standard app knob. These controls affect only the backing. Guitar playback keeps its original pitch and speed, including audio exports.

Mark the start with **A** and the end with **B**, or Shift-drag on the ruler. Drag markers to adjust the section. **Repeat A/B** or L toggles repetition. Shift+wheel zooms the timeline; the wheel scrolls when zoomed. Ctrl+0 fits the study when the timeline has focus; elsewhere it resets Cube zoom.

## Independent metronome

On/off, BPM, **TAP**, volume, time signature and sound remain visible in the metronome card. Defaults are off, 100 BPM and 4/4. TAP sets click tempo and phase.

Backing speed never changes metronome BPM; BPM and TAP never change backing speed. The click works without a file, with the backing paused and after audio ends. Play, pause, seek and loop do not reset its phase. **Stop** turns everything off. The click does not automatically align with a file's beats.

## Configure and record guitar

Open audio options in the footer. Choose WASAPI for Windows devices or ASIO for your interface driver. WASAPI has separate input/output selection; ASIO uses a duplex device. **ASIO panel** opens the manufacturer's options. Choose the guitar input channel, output channel pair, available rate and buffer, then **Apply**.

Software monitoring starts off each run. Enable it to hear input through the app. Leave it off if you already hear direct monitoring through your interface or pedal, to avoid duplicate sound. Device settings are locked during capture.

Seek and use **Record** or R. No track arming is required. The previous attempt stays silent during new capture. Finish with Record, pause or Stop. Without loop, the timeline grows; each loop pass produces an independent attempt.

The **My guitar** card context menu lets you select and audition an attempt, delete, undo or redo. Each attempt retains its capture context and plays its original audio. Drag the guitar waveform edges to trim or use its cursor context menu. Ctrl+Z undoes trim/deletion and Ctrl+Y redoes. Original audio files are never modified.

Playback, capture and the metronome continue when navigating through Cube. The miniplayer shows status and offers **Stop** and **Open Studio**.

## Save audio and finish

**Save audio** exports only the selected, trimmed and aligned attempt. Choose **Guitar with backing** or **Guitar only**. Stereo PCM 24-bit WAV is the default; 320 kbps MP3 is the alternative. The metronome is excluded. Progress/cancellation is available and files are only published after success.

Importing a local backing, clearing the study or closing prompts for discard if attempts have not been exported. In search, **Add to session** loads the new backing and discards previous attempts without confirmation after successfully preparing the audio. Trimming an exported attempt requires saving the new version to avoid the prompt. Changing pitch, speed or A/B does not request a save.

Studio starts empty and does not open, save or restore projects. Existing `.cubestudio.json`, `.ministudio.json`, `.cubeproject` files and their media remain untouched. Downloads and exports stay in chosen folders. Temporary recordings use `PracticeRecordings` in app data, in a folder unique to each run, and are cleaned after confirmed discard.

## Troubleshooting

Check output, volumes and mute on both tracks. For recording, check input and channel in audio options. ASIO requires its installed driver; WASAPI uses available Windows devices.

Device or writing failures stop the operation and preserve recoverable audio. In audio options, use **Recover recordings** and choose a journal. Recovery creates an attempt from an audio copy without opening a project. Review it and use Save audio to keep it. Recovery sources remain intact.

Search, preview and download require internet. Check the result's message and chosen folder after failure. Missing tools require reinstalling the complete package. Cancellation preserves completed files.
