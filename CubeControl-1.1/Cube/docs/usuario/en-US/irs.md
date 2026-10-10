# IRs, cabinets and associations

## Choose a cabinet
The pedal has eight IR slots. Their contents may have been replaced: a slot number does not identify a cabinet. The included library works offline.

## Import and try an IR
Choose an impulse-response WAV in the interface and check the reported compatibility. An IR WAV is neither an amp file nor a Backing Tracks song. IR preview temporarily uses a pedal slot and preserves a recovery reference; follow the stop/restore actions before other writes.

Importing to a slot and writing are device operations. Export or preserve the backup of the previous contents before replacement. Read the messages and the destination slot in the confirmation.

## Build and apply packs
In **IR packs**, choose any pack and edit it directly. The first change to a predefined pack creates an independent draft and shows the yellow **Creating pack** notice; the original stays intact. Remove all eight IRs to start empty. Drag a library file onto the destination card. Dragging between cards swaps their IRs and distances. The trash icon appears on hover or keyboard focus; Delete also removes an IR. The context menu provides keyboard alternatives. Empty slots preserve the pedal's current contents. Building and searching do not start a preview or write to the device.

Use **Review and apply** to select the IRs to send and adjust each distance from 0 to 99. These adjustments apply only to this operation; they do not change the saved pack. The app compares destinations with the pedal and skips identical contents. The final confirmation lists the slots to replace. Canceling preserves your draft.

The **Review and apply** and **Restore original IRs** actions appear above the grid. **Save pack** asks for a name and description in a popup and stores your draft for reuse; saving is optional before applying. Saving a draft based on a predefined pack creates a personal pack; saving changes to a personal pack updates it. Discarding restores the source pack. When switching a changed pack, choose **Save**, **Discard**, or **Cancel**.

**Export** creates a `.cubeirpack.zip` containing the WAVs, which can be imported on another computer. **Import** opens a draft for review without automatically saving or applying it. Imported WAVs are stored in user data and also appear in the individual library. Older `.cubeirpack.json` files remain supported but require their referenced WAVs. Missing references are shown on cards and cannot be selected for application.

Binary BFI/BFIN packs are available through the separate **Restore original IRs** action when original resources are available. See [Backups and recovery](recuperacao.md).

## Associate with an amp
Associate cabinet applies the configured amp association to the edited preset. Automatic association starts disabled and is stored locally. When enabled, it attempts to apply the association to the current amp and subsequent choices. Without a valid association, the cabinet is preserved.

## Identity and pictures
Manufacturer names and photos help selection but do not prove that an IR exactly reproduces that equipment. Visual customization does not change audio.

See [Backups and recovery](recuperacao.md).

## Overdriven.fr collections

The Overdriven.fr library contains 13 collections and 682 IRs. Combine collection,
cabinet, speaker, microphone, position and capture distance filters with search.
**Clear filters** restores the whole library. **Not provided** selects files
without metadata. Filtering does not send audio to the pedal.

The tree follows collection → microphone → IR. Open **Capture information** for
original format, source, position, variants and filename. Microphone models
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
