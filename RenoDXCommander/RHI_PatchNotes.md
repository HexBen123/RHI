## v2.5.0

### New

- Added a search bar to the shader pack picker — filter by pack name or individual shader filename.
- **DLSS Tool (ShortFuse)** — ShortFuse's DLSS5 addon is now in the addon picker as a second option alongside DLSS5 Tool. Supports DX12, DX11 and DX9 with HDR scaling. On install, RHI automatically downloads and deploys the newest DLSS SR, RR, FG, NR and Streamline files to the game folder. Supports RTX 20-50 Series. Still WIP — fall back to DLSS5 Tool if you have issues.
- **Updated nvngx_dlssnr.dll** to ShortFuse's latest build, now supporting RTX 20, 30, 40 and 50 Series GPUs with identical performance to the original NVIDIA build on RTX 50 Series.

### Changes

- Moved the Neural Rendering column to the far right of the Nvidia Profile section, after Streamline.
- Renamed RenoDX DLSS5 addon to DLSS5 Tool. The current version is now shown next to the name in the addon picker.

---

## v2.4.9

### New

- **nvngx_dlssnr.dll 310.8.SF** — a modified Neural Rendering DLL by ShortFuse that extends support to RTX 20, 30, 40 and 50 Series GPUs. This is now the default version RHI deploys. Shown as `310.8.1` in Windows Explorer, `310.8.SF` in RHI.

### Changes

- The Neural Rendering Deploy DLL button now also deploys `nvngx_dlss.dll` to the game folder alongside `nvngx_dlssnr.dll`. Any existing `nvngx_dlss.dll` is backed up as `.original` first.
- Added an MOTD button to the status bar next to Patch Notes — click it to re-read the current message at any time.

### Manifest Updates

- Added Reshade Motion Estimation by JakobPCoder to the shader pack library — dense real-time optical flow motion estimation.

---

## v2.4.8

### Bug Fixes

- Fixed "How to use" link not appearing in the per-game addon picker.
- Fixed `renodx-dlss5.addon64` triggering an install prompt when double-clicked or drag-dropped. It is managed by RHI internally and should only be installed via the addon picker or placed in the Custom Addons folder.

### Manifest Updates

- Added DLSS5 DX11 Bridge and DLSS5 Feeder to the addon picker — both enable DLSS 5 Neural Rendering in D3D11 games. Additional setup steps are required; the How To Use button on each addon links to the repo for instructions.
- Added DLSS5 Feeder companion shader to the shader pack library.
- Fixed Metal Gear Solid 4 (Master Collection) showing as Unreal Engine — now correctly shows MGS4 Engine.

---

## v2.4.7

### Bug Fixes

- Fixed the Neural Rendering column not showing `nvngx_dlssnr.dll` as installed after deploying it. It now updates immediately without needing a Refresh.
- The Neural Rendering column now clearly shows "Custom" when a custom DLL is active.

---

## v2.4.6

### Bug Fixes

- Fixed RenoDX DLSS5 not auto-updating to games when a new version is released. The addon now deploys the updated file directly from its own staging folder and no longer creates a redundant copy in the addons folder.

### Manifest Updates

- Added CubeLUT3Ddith by aron7awol to the shader pack library — Cube 3D LUT shader with dithering to reduce banding.

---

## v2.4.5

### Bug Fixes

- Fixed RenoDX DLSS5 not deploying to game folders after the addons staging folder was deleted. The addon now deploys directly from its own staging location.

---

## v2.4.4

### New

- **RenoDX DLSS5 addon** — `renodx-dlss5.addon64` is now a first-class addon in the per-game addon picker, listed above RenoDX Upgrade. Enable it per game from the Addons combo → Select. RHI downloads it automatically, keeps it updated silently alongside other components, and deploys `nvngx_dlssnr.dll` to the game folder alongside it if not already present. For 50 Series GPUs only.
