# AstroCiv — pixel asset pack v1

Actual native-resolution assets reconstructed from the supplied white-and-cyan AstroCiv reference. This is an art and technical handoff, not a playable Unity project. Start with `Previews/buildings_contact.png`, then `Previews/moon_assembly.png` and `Previews/mars_assembly.png`.

## Contents

- **11 building sprites + one six-wheeled rover:** command hub, habitat, greenhouse, battery, communications dish, solar array, regolith shelter, airlock, laboratory, life support, rover garage.
- **19 corridor pieces:** two straight axes, four corners, four T-junctions, cross, four end caps, four building adapters. All share exact socket geometry.
- **21 astronaut frames:** idle, wave, tablet, repair, one extra walk pose, and 16 directional walk frames (SE/SW/NW/NE). Four strip sheets, four animated GIF previews, one portrait.
- **Moon and Mars:** repeating ground textures, 64×32 ground diamonds, three crater sizes and three rock-cluster sizes per environment, tracks and footprints, separate distant ridges. Space background and Earth are separate; Mars has a separate peach sky.
- **UI:** dark navy stepped panels, four cyan button states, 14 resource/action icons, 11 thumbnails taken from the actual buildings, wordmark, original uppercase bitmap font.
- **Layers:** eleven separate shadow sprites and eleven equipment dust overlays plus a warm Mars regolith surface overlay.
- **Technical:** three packed atlases with rectangle metadata, per-asset manifest, palette, Unity texture importer, validation report, editable raster masters and Python rebuild scripts.

There are 142 exported PNGs under `Assets/AstroCiv`, excluding packed atlases, masters and previews. The ZIP contains the complete organized folder.

## Quick Unity import

1. Copy `Assets/AstroCiv/Editor` into your project's `Assets/AstroCiv/Editor` first. Let Unity compile the importer.
2. Copy the remaining `Assets/AstroCiv` folders into that location. If images were imported first, select them and choose Reimport.
3. Use an orthographic camera. The importer selects Point filtering, no mipmaps, uncompressed RGBA, Full Rect sprite mesh and **64 pixels per unit**. Use integer camera/UI scaling. Do not bilinearly scale the sprite art.
4. Build your grid with screen-pixel axes **u=(32,16), v=(-32,16)**, equivalent to world-space u=(0.5,-0.25), v=(-0.5,-0.25) when screen-down is world-negative-Y. Each ground diamond is 64×32 pixels.
5. Place buildings by their footprint centre. The default sprite pivot is centred horizontally and 8 pixels above the bottom. Canvas size is padding, not physical size. Footprint occupancy comes from `asset_manifest.json`.
6. Corridors use a 96×64 padded canvas and ground centre at pixel **(48,40)** from the top left; their Unity pivot is (0.5,0.375). Position neighbouring centres exactly 32px horizontally and 16px vertically apart along the appropriate diagonal.
7. Use the individual 32×48 character frames for Animation clips, in numeric order, at **6 FPS**, looped. Their common foot pivot is (16,44) from the top left. Sheets are also supplied as plain textures, so they do not import as giant characters.
8. For UI panels, use Unity UI Image Type **Sliced**, border 8px on all sides, and preserve integer edge scaling. Button and panel PNGs are deliberately unlabeled. Render text separately.

The Editor helper is supplied as source but has **not been compiled or run in Unity here**. If your Unity version changes importer APIs, apply the settings above manually. No prefabs, scenes, colliders, Animator Controllers or runtime UI logic are claimed to be included.

## World scale and footprints

| Sprite | Occupancy in cells | Canvas |
|---|---:|---:|
| Command hub | 3×3 | 192×192 |
| Habitat | 2×2 | 128×128 |
| Greenhouse | 3×2 | 192×192 |
| Battery | 2×1 | 128×128 |
| Communications dish | 2×2 | 192×192 |
| Solar array | 2×2 | 128×128 |
| Regolith shelter | 3×3 | 192×192 |
| Airlock | 1×1 | 128×128 |
| Laboratory | 2×2 | 192×192 |
| Life support | 2×2 | 192×192 |
| Rover garage | 3×2 | 192×192 |
| Rover | 2×1 reservation | 128×128 |

Footprints are conservative gameplay occupancy reservations, not extracted physical CAD geometry. Camera is fixed. These are elevated three-quarter illustrations normalized to a common ground-grid convention, not mathematical isometric renders. Do not rotate buildings to create unsupported views. Shadows and dust overlays share the building's canvas and pivot. Apply them at the same transform; render shadows beneath the structure. Use ground-contact Y sorting, with overlays immediately above their parent.

## Corridor contract

Ports are NE, SE, SW, NW in screen space. Each is 0.375 cell wide and 12 pixels tall; neighbouring reciprocal port centres coincide. `asset_manifest.json` gives every active port in top-left pixel coordinates. Unconnected arms are absent. End caps terminate a route; adapters use the same dimensions and are intended to tuck 6px into a building wall. Building doors and walls are illustrated, so final adapter placement/occlusion is a scene-level task.

The generated corridor master is included. Final grid pieces are a simplified deterministic reconstruction of its white roof/cyan-window design, chosen to make all variants join reliably. They are boxier than the arched corridors in the concept. The join preview and all 256 reciprocal socket pair checks are included; placement in a Unity scene still needs an integration check.

## Environment and science distinctions

The buildings are shared between Moon and Mars. Martian props use a warm palette conversion of the lunar geometry; dust is an optional overlay, not baked into structures. Ridges and sky are for horizon scenes and must not be put into placement tiles. The repeat textures use a mirrored periodic construction from the generated ground master; opposite edges match exactly. Repetition can become recognizable over large areas; break it up with props and decals.

Use **Next Day / Next Mission Day on the Moon**, **Next Sol on Mars**. Reference-style previews show ore, water, power, food and crew; oxygen and research icons are additionally supplied for the gameplay guide's operational HUD. Stock, capacity and production rate are separate values. Preview numbers are invented gameplay examples, not NASA performance data. The terrain, Earth, buildings and greenhouse are stylized illustrations, not scientific diagrams or engineering designs. No scientific safety or shielding performance is encoded by the art.

## UI behavior handoff

Follow the existing AstroCiv UI/UX guide for behavior and this pack's dark navy appearance:

- One compact persistent resource bar; selection panel appears only on selection.
- One major management/Knowledge Base window at a time; Escape/Back closes the topmost dismissible layer.
- Use the info icon for Learn More and NASA Knowledge Base access. Sources and reality-versus-game explanations belong in runtime text, not sprite pixels.
- Mission Control hints are optional. Pair urgent state colors with a symbol and explicit text.
- Preview resource impact before committing; advancing time should explain consequences.
- Label quantities and periods. The icon set includes both reference resources and guide resources without forcing all onto one crowded bar.
- Bitmap font is A–Z, digits, `+ / : - .` and space only. It is suitable for headings and counters at 2× or larger. For long encyclopedia copy/localization, use a separately chosen readable font; do not silently drop unsupported characters.

## Masters and reproducibility

`Masters` contains the full-resolution generated RGBA source groups. These are editable raster PNGs, not layered Aseprite/PSD documents. Export cleanup thresholds alpha, removes small disconnected artifacts, samples to target pixels without interpolation, and maps colors to the shared palette. Shadows, overlays, UI and font are separate editable outputs. `Tools/build_pack.py` is the editable deterministic source for exact UI/geometry and export settings. `Tools/finalize_pack.py` packs atlases and validates output.

Rebuild from this folder with Python 3, Pillow, NumPy, SciPy and Shapely:

```sh
python Tools/build_pack.py
python Tools/finalize_pack.py
```

The built-in image-generation tool produced the illustration masters. No external stock art or third-party font file is bundled. `GENERATION_NOTES.md` records the prompt specifications and transformations.

## Validation and limits

See `validation_report.json` for exact checks: dimensions, RGBA, binary standalone alpha, silhouette padding, edge-equal repeated textures, distinct walk frames and reciprocal corridor ports. See `Previews` for contact sheets, assembled examples, animations and repeated terrain.

This is a coherent first production pack, not a claim of a pixel-for-pixel copy of the approved board. Generation introduced differences in the battery, dish, architecture proportions and character poses. Small within-cycle helmet/pose variations remain; the feet are baseline-aligned. The core sprites use one fixed camera and palette, while exact perspective is approximate. Unity import, runtime sorting, final building adapters and animation feel still require an in-project smoke test.
