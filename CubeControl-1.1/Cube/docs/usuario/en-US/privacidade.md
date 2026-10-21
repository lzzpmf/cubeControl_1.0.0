# Privacy

Updated: October 19, 2026. Scope: cubeControl 1.1.0 for Windows, including amp creator and Backing Tracks.

Application controller: Luiz Paulo Martins Ferreira. Privacy contact: [lzpmartins@gmail.com](mailto:lzpmartins@gmail.com).

## Local operation
The HTML manual works offline and searches its content inside the browser without sending queries. When supported, it saves only the theme preference in the `cube-manual-theme` local-storage key. Clearing browser data removes this preference. External and email links open only when selected by the user.

Pedal editing uses USB-MIDI. No account, registration, advertising, developer analytics or automatic upload to developer servers is required. Included IRs and local audio playback work offline. Windows audio output may use Bluetooth; the Bluetooth button opens Windows settings.

## Information and purposes
The app uses MIDI port names and pedal identification to connect; presets, effects, amp data and IRs to edit, preview and restore; language, appearance, zoom and visual preferences to customize the interface; chosen WAVs, images, backups and BIN files for import/export and recovery; MP3/WAV paths, list order, output, volume and mute to organize the list and control playback during the current session. Loop points, position, speed and waveforms support practice and export. Backup dates and integrity checks support recovery. Connection/error messages support diagnosis.

Names, paths and selected content may contain personal information. Avoid unnecessary personal details.

## Storage and retention
Preferences and images generally reside in `%LOCALAPPDATA%\CubeControl`. Simulation editor data uses `%LOCALAPPDATA%\CubeControl-Simulation`; theme, language and zoom remain shared. Exported files go to your chosen location.

The playlist references files without copying them; order and paths are stored in `backing-playlist-v1.json`, while playback settings remain in memory for this session. REC recordings reside in `Recordings` under the active data directory, separate in simulation; the old `backing-tracks-v1.json` is neither loaded nor updated. Images reside in `Images`, with associations in `gear-images-v1.json`. Metronome preferences, manual BPM and per-file beat alignment reside in `backing-metronome-v1.json`; imported sounds are copied to `metronome-sounds`. The download folder and video IDs mapped to completed files reside in `youtube-downloads-v1.json`. Automatic bank, IR, amp and effect recovery snapshots remain in memory for the current session; the editor does not generate or reload pending preview files after restart. Explicitly exported backups and adjustments remain in your chosen location. See [Backups and recovery](recuperacao.md). Local files remain until replaced or deleted; backups have no general automatic expiry. Uninstalling the executable does not remove this data. Pedal settings remain independently on the device. See [Recovery](recuperacao.md).

Personal packs are stored in `IrPacks`, WAVs imported from portable packs in `ImportedIrs`, and custom names in `ir-pack-names-v1.json`, under the editor data directory. Importing can copy WAVs before the draft is saved; discarding the draft does not delete those copies.

## External services
The app does not automatically send your presets, files, device information or diagnostics to its developer or the pedal manufacturer, or sell them for advertising. YouTube search, download and audio preview, including previews triggered by hovering over a result, uses internet tools to contact external services; queries and media requests are subject to those services' policies. Local playback and documentation do not require these requests.

Voluntary sharing through email or another service occurs outside local editing. Review content and the receiving service's terms. Windows, synchronization, backup and distribution services may process data under their own policies.

## Control and deletion
You can change preferences, disconnect the pedal and delete local/exported files with Windows. Before deleting the data folder, finish or restore previews, close the app and preserve needed backups. Defaults may be recreated next time. Removing a track from the list does not delete its audio; exporting creates a separate file. Computer deletion does not erase pedal settings, exported copies or external backups.

## Security
Local storage reduces transmission requirements. Access depends on Windows permissions and computer security; the app does not guarantee its own file encryption. Keep Windows updated, protect your account and control access to backups. No storage method provides absolute protection against loss or unauthorized access.

## Rights and contact
Where the controller processes personal data, applicable rights under Brazil's LGPD include confirmation/access, correction, sharing information and, where legally applicable, anonymization, blocking, deletion, portability and withdrawal of consent. You may petition ANPD. Use the contact above for requests. The controller has no remote access to files held exclusively on your computer; manage those locally.

If support requires sending personal information, its purpose, legal basis, recipients and retention period or criteria must be explained for that support request. App use does not grant generic consent for additional purposes. Legal reference: [Brazilian Law 13,709/2018](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm).

## Changes
This policy may be updated when features or processing practices change. Its revision date will be displayed. New collection or transmission conditions must be disclosed before processing, with consent where legally required.
