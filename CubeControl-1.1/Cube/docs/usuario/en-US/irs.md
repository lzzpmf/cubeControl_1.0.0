# IRs, cabinets and associations

## Choose a cabinet
The pedal has eight IR slots. Their contents may have been replaced: a slot number does not identify a cabinet. The included library works offline.

## Shared library
**Manage IRs** has **Individual analysis and preview** and **IR packs (8 slots)** tabs sharing the same library, search and selection. The individual tab opens initially; the selected tab is retained during the session. In Packs, selecting a file only makes it available for slot assembly. Switching tabs does not apply IRs or discard the draft; an active preview remains on the pedal. Return to the individual tab to end it.

## Import and try an IR
Choose an impulse-response WAV in the interface and check the reported compatibility. An IR WAV is neither an amp file nor a Backing Tracks song. IR preview changes pedal RAM without replacing any of the eight stored slots. Choosing an external WAV or selecting an IR in the individual library starts preview automatically when the pedal is connected and the session is not blocked; without a connection, it only prepares the local file. The previous state remains in memory for this app session; use End preview before closing.

Importing to a slot and writing are device operations. Export or preserve the backup of the previous contents before replacement. Read the messages and the destination slot in the confirmation.

## Audition with recorded guitar
Select one of the five guitar recordings and choose the pedal’s USB audio output. An operational MIDI connection and a confirmed IR RAM preview are required. The computer plays the raw WAV; the pedal processes it with its current effects. Outputs come from Windows: choosing another output does not apply the IR in software. Volume starts at 30% and looping is enabled.

Changing the IR pauses and mutes audition during transfer; playback resumes at the same position only after confirmation. Pause keeps the position; Stop returns to the beginning. Changing the recording during playback restarts with the new recording. Leaving IRs, changing output, disconnecting or ending preview stops playback. Backing Tracks, YouTube previews and IR audition do not play simultaneously.

## Build and apply packs
The catalog includes **34 predefined packs**, grouped by brand, style and guitar, with eight slots and an initial distance of 50%. Descriptions suggest uses; they do not simulate guitar models or imply listening validation. Personal packs appear under **My packs**.

In **IR packs**, choose any pack and edit it directly. The first change to a predefined pack creates an independent draft and shows the yellow **Creating pack** notice; the original stays intact. Remove all eight IRs to start empty. Drag a library file onto the destination card. Dragging between cards swaps their IRs and distances. The trash icon appears on hover or keyboard focus; Delete also removes an IR. The context menu provides keyboard alternatives. Empty slots preserve the pedal's current contents. Building and searching do not start a preview or write to the device.

Use **Review and apply** to select the IRs to send and adjust each distance from 0 to 99. These adjustments apply only to this operation; they do not change the saved pack. The app compares destinations with the pedal and skips identical contents. The final confirmation lists the slots to replace. Canceling preserves your draft.

The **Review and apply** and **Restore original IRs** actions appear below the grid. **Save pack** asks for a name and description in a popup and stores your draft for reuse; saving is optional before applying. Saving a draft based on a predefined pack creates a personal pack; saving changes to a personal pack updates it. Discarding restores the source pack. When switching a changed pack, choose **Save**, **Discard**, or **Cancel**.

**Export** creates a `.cubeirpack.zip` containing the WAVs, which can be imported on another computer. **Import** opens a draft for review without automatically saving or applying it. Imported WAVs are stored in user data and also appear in the individual library. Older `.cubeirpack.json` files remain supported but require their referenced WAVs. Missing references are shown on cards and cannot be selected for application.

Binary BFI/BFIN packs are available through the separate **Restore original IRs** action when original resources are available. See [Backups and recovery](recuperacao.md).

### Limits and partial application
Individual WAVs and WAVs in a portable pack are limited to 32 MiB per file. The pack validates all WAV hashes and formats before copying them to the imported library; changed or invalid files are rejected. Importing a ZIP can already copy WAVs to user data before you save the draft.

Applying a pack writes one slot at a time. If it fails, the message lists completed slots; it does not automatically undo slots already written. Check the result before retrying. Automatic copies of previous contents remain only in session memory; export backups you need to keep.

## Random cabinet IR
In the effect chain, under **Cabinet**, **Randomize and apply** chooses from the entire library, including imports and ignoring search filters, with an integer distance from 40% to 80%. The destination is the current preset cabinet, not the separately selected slot on the IR page. It writes directly without another confirmation and applies in RAM for immediate use; it does not save the preset or change the IR section on/off state. Repeats are possible.

A working connection, loaded bank, valid slot and available library are required. The file is validated before writing. If writing succeeds but immediate application fails, the message distinguishes the two: the slot has already been replaced. Previous contents are retained only in session memory.

## Associate with an amp
Associate cabinet applies the configured amp association to the edited preset. Associations and the automatic-association option remain in the current session only; reopening starts with automatic association disabled and requires configuring associations again. Saving associations does not send commands to the pedal. When enabled, it attempts to apply the association to the current amp and subsequent choices. Without a valid association, the cabinet is preserved.

## Identity and pictures
Manufacturer names and photos help selection but do not prove that an IR exactly reproduces that equipment. Visual customization does not change audio.

See [Backups and recovery](recuperacao.md).

## Overdriven.fr collections

The Overdriven.fr library contains 13 collections and 682 IRs. Search matches every typed term, case-insensitively, against file paths, names and metadata. Clear the text to restore the full library. There are no separate collection, microphone or distance selectors. Searching does not send audio to the pedal; selecting an IR in the individual library can start preview.

Files with metadata follow brand → speaker configuration → model and speaker → capture. Overdriven.fr identifies the source, not a cabinet brand. Files without metadata follow their folders; imported files appear under **Imported IRs**. Expanding one brand collapses the others. Selection details show original format, source, identification, evidence, position, variants and filename. Microphone models
marked “inspired by” come from the provider documentation. Undocumented codes
are preserved without assumed equivalents.

In the provider's visual reference, P00 is the dust cap center, P05 is on the
cap, P09 near its edge, P10 at its edge, P20 on the cone and P30 near the outer
edge. Other codes, such as P12 and P25, have no exact documented location in
that image. The original photo is not bundled with the app.

Filename distances are approximate **millimeters**. They describe the capture
and do not change the pedal's 0–100 **Distance** control. Suffixes such as LC45,
EQ, P0, CUT1 and UA610 identify variants, without triggering processing in the
app. SP1-PREVIEW marks preview files, each identified by its own microphone and
original name.

All files are mono at 44.1 kHz: 82 GOV IRs are PCM 16-bit/50 ms (their readme
states float 32-bit); the rest are PCM 24-bit/100 ms. The app converts to 48 kHz
and retains 512 samples, approximately 10.7 ms, without gain normalization.
It applies no automatic cuts, EQ, gain or phase correction. The provider's
filter and adjustment recommendations are listening suggestions.
