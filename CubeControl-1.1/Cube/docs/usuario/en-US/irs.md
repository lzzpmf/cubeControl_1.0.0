# IRs, cabinets and associations

## Choose a cabinet
The pedal has eight IR slots. Their contents may have been replaced: a slot number does not identify a cabinet. The included library works offline.

## Import and try an IR
Choose an impulse-response WAV in the interface and check the reported compatibility. An IR WAV is neither an amp file nor a Backing Tracks song. IR preview temporarily uses a pedal slot and preserves a recovery reference; follow the stop/restore actions before other writes.

Importing to a slot and writing are device operations. Export or preserve the backup of the previous contents before replacement. Read the messages and the destination slot in the confirmation.

## Build and apply packs
In **IR packs**, select a **Predefined** pack or use **Create pack** to build one with one to eight IRs. Use **Create a copy** to change a predefined pack. Select a library file and choose **Use in this slot** on the destination card. Empty slots preserve the pedal's current contents. Building and searching do not start a preview or write to the device.

Use **3 · Review and apply** to select the IRs to send and adjust each distance from 0 to 99. These adjustments apply only to this operation; they do not change the saved pack. The app compares destinations with the pedal and skips identical contents. The final confirmation lists the slots to replace. Canceling preserves your draft.

**Save pack** stores your draft for reuse; saving is optional before applying. When switching a changed pack, choose **Save**, **Discard**, or **Cancel**.

**Export** creates a `.cubeirpack.zip` containing the WAVs, which can be imported on another computer. **Import** opens a draft for review without automatically saving or applying it. Imported WAVs are stored in user data and also appear in the individual library. Older `.cubeirpack.json` files remain supported but require their referenced WAVs. Missing references are shown on cards and cannot be selected for application.

Binary BFI/BFIN packs are available through the separate **Restore original IRs** action when original resources are available. See [Backups and recovery](recuperacao.md).

## Associate with an amp
Associate cabinet applies the configured amp association to the edited preset. Automatic association starts disabled and is stored locally. When enabled, it attempts to apply the association to the current amp and subsequent choices. Without a valid association, the cabinet is preserved.

## Identity and pictures
Manufacturer names and photos help selection but do not prove that an IR exactly reproduces that equipment. Visual customization does not change audio.

See [Backups and recovery](recuperacao.md).
