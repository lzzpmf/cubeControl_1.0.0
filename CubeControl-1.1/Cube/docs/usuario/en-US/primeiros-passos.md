# Getting started

## Install and open
Extract the complete package to a folder on your computer and open `CubeControl.exe`. Keep the package folders next to the executable. The published package does not require the .NET SDK.

## Connect the Cube Baby
1. Power on the guitar Cube Baby and connect a USB data cable to the computer.
2. Close other editors using the MIDI port.
3. Open cubeControl and wait for identification and loading of A/B/C and all eight IR slots.
4. If needed, use the connection button and check Device and help for connection details.

The editor uses USB-MIDI, not Bluetooth. Cube Baby Bass, AC, Chocolate and Blackbox profiles are not implemented. Manual disconnection pauses automatic reconnection; connecting again resumes it.

## Make your first adjustment
Select the same A/B/C in the app as on the physical pedal. Open a Pedalboard module and adjust a control. Selecting A/B/C in the app chooses the editing destination; it does not operate the footswitch. Read [Presets and files](presets.md) before saving.

## Customize reading
Theme, language and zoom are in the ellipsis menu. Documentation works offline without a pedal. Guides follow the app language; PDFs display their own language.

Zoom ranges from 50% to 200% in 10% steps. Use Ctrl +, Ctrl − and Ctrl 0 to increase, decrease and reset to 100%; this does not change Windows DPI.

At 100%, the interface uses its base size without an additional reduction. The normal minimum window size is 1280 × 720 logical units; when Windows scaling leaves a smaller work area, the window fits that area. Sections rearrange their groups and provide scrolling when needed. Tracks and graphs use additional space while controls retain their size.

For connection issues, see [Troubleshooting](problemas.md).

## Find help
In **Device and help**, **cubeControl manual** opens the HTML manual in your default browser using the app's current language and theme. It includes full-text search, a chapter index, printing and switching between Portuguese and English. You can also open `Assets/Documents/manual/index.html` directly, without internet. The browser's theme selection does not change the app's appearance.

Documentation Center search matches catalog titles, summaries and keywords, not the full guide text. Every space-separated term must match; case and accents are ignored. If the selected language is unavailable, the app uses Brazilian Portuguese or, failing that, the first available version. Preserved manuals describe their source versions; use the Center’s guides for current app workflows.

## Navigation and help
With the pointer over the main navigation bar, the mouse wheel moves between the five tabs and stops at either end. Outside the bar, scrolling continues to affect the page or control. Use Tab and Shift+Tab for keyboard navigation; Alt alone does not activate navigation cues. Tooltips appear after 1.2 seconds and disappear five seconds after appearing; a new hover can show them again. The documentation window follows the app theme, with zoom applied to its reading content.
