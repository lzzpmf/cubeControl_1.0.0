# M-VAVE Cube Baby: User Manual

Revised and expanded user edition · English (US) · October 8, 2026

[Português (Brasil)](../pt-BR/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual.pt-BR.md) · [Original PDF](../../oficial/en-US/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual-Oficial.pdf)

This guide covers the **guitar Cube Baby** and cubeControl for Windows. It combines the included manufacturer's manual, practical advice, and the app behavior implemented in this checkout. It is an independent revision, not a new official manufacturer manual. Cube Baby Bass, AC, Chocolate, and Blackbox are outside its scope.

## Contents

- [Quick start](#quick-start)
- [Power and connections](#power-and-connections)
- [Controls and effects](#controls-and-effects)
- [Modes, footswitches, and saving](#modes-footswitches-and-saving)
- [Tuner and Bluetooth](#tuner-and-bluetooth)
- [Building useful tones](#building-useful-tones)
- [Using cubeControl](#using-cubecontrol)
- [IRs, previews, and backups](#irs-previews-and-backups)
- [Advanced creators](#advanced-creators)
- [Reference tables](#reference-tables)
- [Troubleshooting](#troubleshooting)
- [Specifications and care](#specifications-and-care)
- [Further reading and verification](#further-reading-and-verification)

## Quick start

1. Turn the receiving amplifier/speaker volume down. Start quietly with headphones too.
2. Connect the guitar to **IN** with a mono instrument cable. Connect **OUT** to an amplifier/interface, or use the dedicated headphone output.
3. Turn on the pedal. **PRESET** is the default performance mode described in the original manual.
4. Press A, B, or C to select a stored tone. Raise the listening volume gradually while playing.
5. To edit in cubeControl, connect a USB **data** cable and select the same A/B/C physically on the pedal and in the app.
6. Export your bank before substantial changes. Save ordinary preset changes to the pedal when you want to keep them after power-off.

A preset recalls settings; it does not rearrange processing order. Chorus and Phaser share one modulation block and cannot run together.

## Power and connections

### Battery and charging

The original manual specifies a **780 mAh** built-in battery, up to **6 hours** of operation, and about **2 hours** for a full charge with a **5 V / 2 A** adapter. These are reference figures, not guaranteed results for an aged battery.

Use a suitable 5 V USB supply or power bank. USB also connects to a computer. A cable that charges successfully may lack data conductors. The pedal has a charging indicator; the included manual does not give a complete color/status chart.

### Rear-panel ports

| Port | Purpose |
| --- | --- |
| IN | Guitar input: 1/4-inch (6.35 mm), mono TS. |
| Power switch | Turns the pedal on/off. |
| USB | Charging, computer connection, preset editing, and IR transfer. |
| Phone recording output | Recording with a compatible phone and recording cable. |
| Headphone output | Headphone connection. |
| OUT | Audio output: 1/4-inch (6.35 mm), mono TS. |

### Choose your listening setup

| Setup | Starting point |
| --- | --- |
| Headphones | Use the headphone port. Start with an IR enabled and low volume. |
| Interface, mixer, or full-range speaker | Use OUT into a compatible input. Start with an IR enabled; adjust receiving gain to avoid clipping. |
| Guitar amp front input | Use OUT, a clean channel, and low amp gain. Compare IR OFF with an enabled IR; the amp and speaker also color the sound. |
| Guitar amp effects return | If supported by the amp, start with low receiving volume and compare IR OFF. Follow the amp's connection instructions. |
| Phone recording | Use the recording output and a compatible cable/adapter; test the recording app's input. |
| Computer recording | The original manual describes USB sound-card operation. Check that the pedal appears as an audio device in the operating system and recording software. USB-MIDI editing alone does not confirm USB-audio availability. |

Recording and headphone ports have different purposes. A phone's USB-C/Lightning adapter must support the required audio input; a headphone-only adapter may not work. The manual does not specify every pinout, driver, or phone compatibility. Do not assume any cable or adapter will work.

An **IR** (impulse response) shapes the sound like a captured cabinet/microphone response. It is not an amp model. Sending an IR-processed sound into a guitar speaker adds another layer of speaker coloration; choose by listening to your actual setup.

## Controls and effects

The documented processing order is:

**Tuner → Preamp → Chorus or Phaser → Delay → Reverb → IR CAB**

You select effects and settings, not a freely reorderable chain.

| Control | What it does |
| --- | --- |
| TYPE | Selects one of nine preamp models. Compare at similar listening volume. |
| GAIN | Changes preamp drive. More gain is not simply more output volume. |
| TONE | Adjusts brightness. Reduce it if the sound becomes harsh. |
| MOD | Left half: Chorus; center: modulation off; right half: Phaser. |
| TIME | Sets the interval between delay repeats, not the number of repeats. |
| FB | Sets delay feedback: higher settings make repeats persist longer. |
| MIX | Adjusts delay contribution relative to the direct signal. Fully left removes delay. |
| REVERB | Adjusts reverb amount. Fully left removes reverb. The ordinary preset control combines reverberated level and feedback. |
| IR CAB | Selects OFF or one of eight stored IRs: nine selector positions in total. |
| VOLUME | Controls level. Check it after changing mode, amp, gain, or cabinet. |

**TIME versus FB:** TIME spaces the echoes apart; FB makes them continue longer. Raise MIX only enough to hear echoes without losing note definition. The app's ordinary TIME value uses the pedal's own scale; it is not a documented milliseconds or BPM readout.

**MOD:** Chorus adds movement and thickness; Phaser creates a moving spectral sweep. The original software scale is **0–6 Chorus, 7–8 OFF, 9–15 Phaser**. These are selector values, not percentages.

**REVERB:** A little adds space. A strong wash can hide fast phrases, especially with delay active too.

## Modes, footswitches, and saving

### Quick reference

“Tap” means a short simultaneous press and release. “Hold” means keeping the switches pressed for about 2 seconds.

| Gesture / context | Documented action |
| --- | --- |
| A, B, or C in PRESET | Select the corresponding stored preset. |
| B+C, tap | LIVE/PRESET switching. The text explicitly describes entering LIVE; the panel labels the pair LIVE/PRESET. |
| A+B, tap | Enter EDIT, according to the included manual. |
| B+C, hold for 2 seconds | Enter the tuner. |
| A+B, hold for 2 seconds | Activate Bluetooth. |
| A, B, or C, hold in LIVE | Save current settings to that destination. The worked example specifies A held for 2 seconds. |

A tap and a hold on the same pair do different things. The included manual does not fully describe exiting the tuner, disabling Bluetooth, exiting EDIT, or the exact gesture that activates bypass. Those details are not supplied here as verified instructions.

### PRESET

Use A/B/C to recall the three stored tones. The manual identifies **all preset indicators off** as bypass in this mode. Do not apply that interpretation to LIVE or tuner indicators.

Stored values may differ from physical knob positions. Listen when moving a knob instead of assuming its position already matches the recalled preset.

### LIVE

LIVE is for building a tone directly with the knobs. Panel footswitch groups are:

| Footswitch | Panel group |
| --- | --- |
| A | IR CAB / REVERB |
| B | DELAY / MOD |
| C | TONE / AMP |

These are group controls, not six independent effect switches. In particular, cubeControl's Tone section is not a general amp bypass.

The manual states that final volume is set to **80% in LIVE**, yet its example also tells the user to adjust VOLUME. That interaction is not fully explained. Start quietly and check listening level; 80% is not a guaranteed measured output level.

### Build and save a tone on the pedal

1. Enter LIVE with a short B+C press.
2. Center MOD and turn MIX and REVERB fully left for a simpler starting sound. Select IR OFF or a suitable cabinet for your output.
3. Enable the needed panel groups, choose TYPE, and adjust GAIN/TONE while playing. Start with low VOLUME.
4. Add modulation, delay, and reverb gradually, comparing at similar volume.
5. Hold destination A, B, or C to save, as described for LIVE in the manual. Its example uses A for 2 seconds. This replaces the destination preset's settings.
6. Return to PRESET using the panel's B+C LIVE/PRESET combination and recall the destination to check it.

### EDIT

The manual describes EDIT as retaining previous parameter values, with knob indicators showing effect status. It says to return to PRESET to set output volume but gives no complete EDIT exit/save sequence. Use LIVE above for a documented physical save workflow, or cubeControl's explicit bank save for computer editing.

## Tuner and Bluetooth

### Tuner

Hold B+C for 2 seconds. Play one open string at a time and let the note settle. Panel labels show standard tuning, lowest to highest:

| String | Note | Labeled knob indicator |
| --- | --- | --- |
| 6 | E | IR CAB |
| 5 | A | REVERB |
| 4 | D | MIX |
| 3 | G | FB |
| 2 | B | TIME |
| 1 | E | MOD |

| Indicator | Meaning |
| --- | --- |
| >, above TONE | Flat: raise the pitch. |
| ■, above GAIN | In tune. |
| <, above TYPE | Sharp: lower the pitch. |

These LEDs indicate tuning here, not effect values. The manual does not establish alternate-tuning support, output muting, or a tuner exit gesture.

### Bluetooth accompaniment

1. Hold A+B for 2 seconds to activate Bluetooth.
2. Open your phone's Bluetooth settings and connect to the pedal when available.
3. Start music quietly and balance phone playback against the guitar.

A **flashing blue BT light** means active but not connected; **steady blue** means connected, according to the manual. Bluetooth plays phone audio through the pedal. cubeControl edits through USB-MIDI. Bluetooth playback is not a documented wireless guitar-recording path.

## Building useful tones

These are listening starting points, not calibrated knob recipes or additional factory presets.

| Goal | Starting point | Fine adjustment |
| --- | --- | --- |
| Clean | TYPE 1 or 2, modest GAIN, MOD off, little REVERB, suitable IR. | Add a little Chorus if desired; keep delay MIX low for clear chord attacks. |
| Crunch | Try an overdrive model, raise GAIN gradually, restrain delay/reverb. | Explore guitar volume and picking strength; reduce TONE if harsh. |
| Lead | Choose a driven model, add moderate delay and some reverb. | Raise FB carefully; compare against the rhythm preset before choosing a lead volume boost. |

Adjust in this order: **listening setup → TYPE → GAIN → TONE → IR → modulation → delay → reverb → final volume**. Change one thing at a time. Excess gain reduces note separation; excess FB/reverb masks phrasing. An IR change can alter tone and perceived loudness.

Compare A/B/C through the same output at the intended listening level. Export a bank you like before further experiments.

## Using cubeControl

### Connect and select the audible preset

1. Turn on the guitar Cube Baby and connect a USB data cable to Windows.
2. Close other editors using its MIDI ports.
3. Open cubeControl and wait for identification, A/B/C loading, and the eight-slot IR inventory. Use the connection button to retry if needed.
4. Select A/B/C physically on the pedal, then select the same letter in the app.

**App A/B/C selection chooses the editing destination. It does not press the footswitch or guarantee which preset you hear.** The current connection flow does not start general footswitch monitoring. Follow the advanced creators' additional physical-context instructions.

### Edit, save, import, and export

| Action | Result |
| --- | --- |
| Change an available ordinary preset control | Sends live changes through the communication queue; saving is separate. |
| Save / Gravar A/B/C | Saves the complete bank of all three presets and checks it by reading back. Review A, B, and C first. |
| Export bank | Writes the local bank to a computer JSON file; does not save the pedal. |
| Import bank | After confirmation, applies known preset fields live. Save A/B/C afterward for persistence. |
| Reread / reconnect | Retrieves current pedal state; review warnings before replacing local work. |
| Undo | Reverts ordinary preset history, not IR/amp-slot writes. |

A return read confirms transferred data, not a new power-cycle test. To check retention, finish operations, restore temporary previews, save, then recall the presets after a normal restart.

**Keep a file backup:** export your preferred bank before imports or replacements. In this checkout, automatic bank and IR recovery copies remain in app-session memory. They do not replace exported files or guarantee recovery after closing/crashing.

### Other useful features

Cabinet association can select a cabinet linked to an amp; automatic association starts disabled. Save A/B/C to persist a changed preset cabinet selection.

Backing Tracks plays through a selected Windows audio output independently of USB editing. Choose the actual listening output and balance its volume. A track's preset association is a reminder; change the physical footswitch yourself.

Theme, language, and zoom are in the options menu. Zoom supports 50–200% and Ctrl +, Ctrl -, Ctrl 0. Appearance settings do not alter sound.

## IRs, previews, and backups

### Understand the destinations

| Destination | Meaning |
| --- | --- |
| Local file | A WAV, bank export, or creator file on the computer. Keeping it alone does not change the pedal. |
| RAM preview | Temporary sound on the connected pedal. Restore/end it to return to the captured reference. |
| Stored slot / preset bank | An explicit persistent write. Saving A/B/C and writing IR/amp slots are separate operations. |

### Audition and store an IR

1. Before replacing an IR, read its slot and export the offered **IR backup JSON**. A preset-bank export does not contain the eight IRs.
2. Choose a library IR or external WAV. **Selection automatically starts a preview when the connected pedal is available.**
3. Listen quietly. Microphone distance uses 0–100%, not documented centimeters. The original manual says 100% produces silence; avoid that endpoint when diagnosing no sound. This guide did not measure it acoustically.
4. Choose destination **slot 1–8**, check its current content, then confirm the write. The previous sector stays in session memory; written data is verified.
5. Select the stored cabinet in your preset and save A/B/C if that selection changed. If needed, select another cabinet physically and return to reload the stored slot.
6. When auditioning without writing, use **End preview / Encerrar prévia** and wait for restoration confirmation.

External WAV support: up to **32 MiB**, PCM 16/24/32-bit or float32, mono/stereo. Preparation produces mono **48 kHz / 512 samples**, resampling and truncating/padding as needed, without normalization. Visual waveform enlargement does not increase IR loudness.

IR packs can replace selected slots. Inspect proposed changes and export backups first. Personal `.cubeirpack.json` files hold library references/hashes, not embedded WAVs; resolve missing references before applying.

### Recover and restore

Do not unplug or close during a slot write. If a transfer fails, read the affected slot and restore from an exported backup. This checkout keeps IR preview snapshots and slot recovery data in session memory, without automatic recovery from a previous app process. End previews and restore while the app remains open whenever possible.

Factory restoration and named packs are different resources; availability depends on recognized local resources. BFI/BFIN recognition does not establish every commercial cabinet model or prove a pack is your unit's factory content. Use content recognition, not slot position alone.

## Advanced creators

Chorus, Phaser, Delay, and Reverb creators expose internal adjustments beyond physical knobs. Their graphics explain controls; they are not measured audio responses or acoustic simulations.

Start with matching physical/app presets in **PRESET**, then use **Read active effect**. Follow status messages: the current app prepares/checks context and may enable the effect or disable conflicting modulation. Recheck ordinary preset settings afterward.

| Creator | Required audible context / useful controls |
| --- | --- |
| Chorus | Chorus in the shared modulation block; depth, speed, mix, feedback, waveform, and two gains. |
| Phaser | Phaser instead of Chorus; speed, sweep limits, output level, and sections. |
| Delay | Delay/modulation section active, Chorus/Phaser off; time, fine feedback/mix, temporal modulation depth/rate. |
| Reverb | Reverb and reverb/cabinet section active, modulation off; direct/wet levels, decay selection, return level, damping. |

Select a graph handle for its name, value, unit, and explanation. Drag to adjust; Tab/arrows edit by keyboard, Enter opens numeric entry, Escape cancels the current gesture. Active RAM sessions may apply changes automatically. **Apply adjustments does not save to flash.**

Export `.chorus.json`, `.phaser.json`, `.delay.json`, or `.reverb.json` to keep internal settings locally. Import prepares local values; Apply sends them to an active RAM session. These creators do not persist internal controls in flash, and saving A/B/C does not add that capability. Ordinary controls, section reloads, or physical changes can reset/interrupt custom internal settings.

Use **Restore** before normal preset editing or another preview. Restore returns to the captured reference, not factory defaults. Recovery snapshots currently stay in process memory. Export creator files to keep custom work; those files are not full recovery snapshots.

The **amp creator** can preview in RAM and explicitly write an amp-model slot. That write is separate from A/B/C and IR slots. Restore its preview explicitly before closing; do not rely on window closure to restore it. Read its dedicated guide before replacing a model.

## Reference tables

These preserve names/categories in the included PDF, not a verified inventory of your pedal today. Models/IRs may have been replaced. Commercial names and app images are references, not acoustic certification.

### Nine preamps

| TYPE position | Manual name | Manual category |
| --- | --- | --- |
| 1 | Power-Zone Clean | Clean |
| 2 | US Gold 100 Clean | Clean |
| 3 | Two Stone Coral OD | Overdrive |
| 4 | Doctor3 B | Overdrive |
| 5 | Cali JP A | Overdrive |
| 6 | Day Tripper OD | Distortion |
| 7 | Shittcow Dist | Distortion |
| 8 | Wo Stone Coral OD | Distortion |
| 9 | Mr Smith Dist | Distortion |

Day Tripper OD remains Distortion here as in the PDF; an app visual catalog may classify it differently.

### Cabinet selector and software slots

The PDF numbers selector positions 1–9; software offers storage slots 1–8 plus OFF. **OFF is not a ninth storage slot.**

| PDF selector position | Software IR slot | Manual reference | Format |
| --- | --- | --- | --- |
| 1 | OFF | No IR | — |
| 2 | 1 | Line 6 Vetta | 2×12 |
| 3 | 2 | Marshall 1960AV | 4×12 |
| 4 | 3 | Marshall 1960A T75 LA610 | 4×12 |
| 5 | 4 | VHT Deliverance | 2×12 |
| 6 | 5 | Soldano | 2×12 |
| 7 | 6 | Peavey 5150 + Mesa Oversized | 4×12 |
| 8 | 7 | JSX KT77 + Mesa Stiletto AKG C1000 | 4×12 |
| 9 | 8 | Diezel V30 SM57 (Soldano HR PowerAmp) | 4×12 |

### Presets described in the PDF

| Preset | Described settings | Category |
| --- | --- | --- |
| A | Wo Stone Coral OD + Delay + Reverb + VHT Deliverance 2×12 | Distortion |
| B | Cali JP A + Delay + Reverb + Soldano 2×12 V30 | Overdrive |
| C | US Gold 100 Clean + Chorus + Delay + Reverb + Marshall 1960A T75 LA610 | Clean |

V30 in preset B reproduces the PDF wording; it does not independently identify an IR's speaker. Do not overwrite your settings just to match these historical examples.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| No guitar sound | Power/battery, guitar volume, IN/OUT, receiving input, VOLUME, active mode. End previews; avoid IR distance at 100%. |
| Harsh/thin sound | Try IR enabled on headphones/full-range output; compare IR OFF on a guitar amp. Reduce TONE/GAIN; check clipping. |
| Muddy notes | Reduce GAIN, delay MIX/FB, REVERB; compare dry/processed sound. |
| App changes but audible preset does not | Match physical A/B/C with the app selection. |
| USB charges but editing cannot connect | Data cable, another USB port, power, other MIDI editors. |
| Bluetooth light flashes | Phone not connected; check pairing/playback volume. Bluetooth is not the editing transport. |
| Changes disappear after restart | Distinguish export, live edit, RAM preview, and saving. Internal effect-creator edits are temporary. |
| Editing blocked | Finish operations and restore/end active previews. Follow context/firmware messages instead of competing operations. |
| Bank changed on pedal | Export local work if needed, reread, review before saving. |
| IR name differs from table | Content may be custom/from another pack; identify by content, not position. |
| Computer/phone recording fails | Audio input availability, permissions, cable/adapter, recording app input selection. |
| Slot write failed | Keep exported backups, reread, restore/verify intended content. A partial write is not completion. |

## Specifications and care

Figures below come from the included manual, not new measurements.

| Item | Specification |
| --- | --- |
| Input / output | 1/4-inch (6.35 mm), mono TS |
| Audio specification | 48 kHz / 24-bit as stated; not a guarantee of every USB recording format |
| Battery | 780 mAh |
| Runtime / charging reference | Up to 6 hours / about 2 hours with 5 V / 2 A |
| Dimensions | 161 × 57 × 17.5 mm (length × width × height) |
| Weight | 260 g |
| Listed accessories | Charging and recording cables; check your package |

Keep dry; avoid extreme temperatures and impacts; clean with a dry cloth. Do not disassemble. Store cool and dry. Retire the pedal and its built-in battery through an appropriate electronics collection/recycling service, following local instructions.

## Further reading and verification

- [Original PDF](../../oficial/en-US/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual-Oficial.pdf): hardware labels, shortcuts, reference tables, specifications.
- [cubeControl usage guide](../../../../../../../docs/historico/guia-de-uso.md): connection and workflows (Portuguese).
- [Graphical effect editors](../../../../../../../docs/referencias/editores-graficos-de-efeitos.md): controls/parameter meanings (Portuguese).
- [Amp creator](../../../../../../../docs/referencias/criador-de-amps.md) and [Backing Tracks](../../../../../../../docs/historico/backing-tracks.md): dedicated workflows (Portuguese).
- [Amp and cabinet catalog](../../../../../../../docs/referencias/ampsEirs.md): aliases, pack identity, limits (Portuguese).

App behavior was checked against current preset/IR and creator code on October 8, 2026. Some linked guides describe older behavior, especially disk recovery and creator activation; for those differences, the current behavior stated here takes precedence. This revision performed no new physical, audio, or power-cycle test. Unspecified hardware gestures and compatibility are identified rather than filled in by assumption.
