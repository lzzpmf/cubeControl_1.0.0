# Backing Tracks

## Prepare your guitar

Use **Import** to create MP3/WAV tracks and **New track**, in Options, for an empty track. In **Prepare guitar**, select the track, input device and mono/stereo channels. **Arm recording** prepares REC. **Monitor input** starts off when opening a project; input meters work even without listening. Input clipping means you should lower the device's input level.

Choose headphone/speaker output in the same panel. S/M/R/I mean Solo, Mute, Arm recording and Monitor input. Headers and mixer share their state. One capture device is used at a time, with channels assigned to tracks.

## Record and review takes

**Record** starts at the cursor after a count-in of zero, one or two bars, even with metronome off for the recording itself. With A/B Loop enabled, it records one pass from A to B.

**Finish take** or **Stop** closes the recording, pauses and returns to its start. Select a clip to choose its active take, edit its name and use **Listen to take**, Previous or Next. Recording again preserves earlier alternatives in the overlapping region and audio outside it.

Gain, pan, mute and solo remain adjustable during REC. Input, channels, arming, monitoring and structural editing stay locked until finalization. Recorded files contain the signal before gain and pan; accompaniment and metronome are not automatically captured.

After an interruption, restore the project and use **Recover interrupted take** to attempt a usable copy of the selected track's latest interrupted recording. The original remains intact. Ignore interrupted take removes only the reference and supports Undo; it does not delete audio. Physical accuracy depends on devices; latency compensation is under Audio options.

## Projects and recovery

**New**, **Open**, **Save**, **Save as** and **Recent projects** manage `.cubeproject` files. Keep the file together with its **Media** folder. Audio, recordings and alternate takes are copied there when saving. Moving the complete folder preserves references; Save as in another folder creates new copies. Originals are not deleted.

The indicator distinguishes temporary projects, unsaved changes and saved projects. Automatic recovery keeps recent changes without replacing manual saves. After interruption, choose Restore or Discard. Closing or switching a changed project offers Save, Discard or Cancel.

Options lets you select older sessions for import. **Import legacy session**, in the project toolbar overflow, can also open an older playlist. Save the result as a project; previous data is preserved. For missing media, use Locate clip audio or Locate track audio before saving again.

## Timeline and editing

Drag clips to move them and edges to trim. Upper handles adjust fades; numerical fields use seconds. **Crossfade** is available for two overlapping clips on the same track. Edits do not modify original audio.

**Snap** follows the musical division; Alt bypasses alignment while dragging. **Cut** splits selected clips at the cursor, or clips on the selected track. Undo/Redo includes takes, fades and mixing.

BPM defines the grid without stretching audio. **Apply suggested BPM** confirms analysis. Speed changes playback; Pitch transposes from −6 to +6 semitones. Completing analysis preserves zoom.

Set A/B using the ruler menu and drag its markers. Loop repeats the region during playback. The metronome menu provides meter, accent, sound and first-beat settings.

## Mixer and library

**Mixer** opens real track channels and Master; drag its divider to resize. **Library** opens Local files and YouTube. Panels start collapsed on a new installation and remember your preference. In small windows, preparation/library alternate with the timeline; transport stays accessible. Additional commands appear in toolbar overflow menus.

For local audio, Add folder indexes MP3/WAV. Search combines with All, Loops, One Shots and Vocals; assign categories through the file menu. Preview toggles to Stop preview. The session pauses without automatically resuming. Insert adds a clip at the cursor; dragging chooses track and position.

On YouTube, search, preview up to 30 seconds and download to a chosen folder. **Add to project** creates a track after downloading. These features require internet; use content you own or are authorized to use. Hover preview is optional and remembered.

## Export and shortcuts

**Export** offers mix/selected track and full session/A/B. Applying speed and pitch is enabled by default. Mixdown includes gains, pan, mute, solo and Master, excluding metronome and live input. Isolated-track export ignores mute/solo and retains gain, pan and Master. Output is 48 kHz stereo PCM16 WAV. Source audio is protected from overwrite.

Ctrl+S saves; Ctrl+O opens; Ctrl+N creates a project. Space plays/pauses; Home returns to the start or A; L toggles loop. Ctrl+Z/Ctrl+Y undo/redo. In the timeline, Ctrl+C/Ctrl+V copy/paste, Ctrl+D duplicates and Delete removes clips. Alt+arrows moves by 0.01 s; Alt+Shift+arrows by 0.1 s. Shift+F10 opens clip actions. F6 switches navigation/content.
