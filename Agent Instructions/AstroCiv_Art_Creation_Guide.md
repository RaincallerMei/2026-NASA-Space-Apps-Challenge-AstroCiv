# AstroCiv — Art Creation & Free Asset Guide

Version 1.0 · 14 November 2026 · Production brief for artists and Unity developers

## Purpose and authority

Create a welcoming, beautiful 2D space-colony game whose world is readable, whose interface explains decisions, and whose story inspires children to explore astronomy. Use this file for asset selection, drawing, adaptation, animation, cutscene creation, and visual acceptance.

This consolidates the seven-cutscene decision and the attached `AstroCiv_CivVI_UI_UX_Visual_Design_Guide(1).md`. The attached guide remains the detailed interaction reference; this document translates it into art deliverables. No current gameplay screenshot was available for inspection, so this is a creation specification, not an audit of the existing build. No packages were downloaded or tested in Unity during this research. Availability and license notes below reflect publisher pages checked on 14 November 2026; inspect the downloaded license before importing.

**Established direction:** cute, authentic 16-bit-inspired pixel art; bright azure/cyan/white; hopeful Ontario-to-Moon story; modular lunar colony; seven essential cutscenes; map-first HUD; contextual panels; NASA Knowledge Base.

**New production defaults proposed here:** palette tokens, exact asset footprints, animation budgets, and export conventions. Preserve an existing working camera/grid where possible; do not rebuild gameplay just to fit a downloaded tileset.

## 1. Visual identity

**One-sentence brief:** A small, living white-and-cyan civilization on another world, built from deliberate pixels, friendly characters, distinct buildings, and clearly visible scientific choices.

“16-bit” describes an aesthetic, not a color-depth requirement. Authenticity comes from consistent pixel size, controlled shading, clean silhouettes, repeated construction rules, and selective detail. Downloading more art alone will not fix unclear hierarchy or inconsistent composition.

| Property | Production rule |
| --- | --- |
| Pixel construction | Deliberate square clusters; no anti-aliased outlines, blurred texture, or random single-pixel noise |
| Shading | Usually 2–3 shades per material; shared upper-left light; dark navy/slate outlines |
| Mood | Cute, capable, optimistic; hazards create urgency without horror |
| Detail | Concentrate on interactive objects; leave quieter terrain around them |
| Earth | Azure sky, leafy greens, warm peach light, welcoming rooms |
| Moon | Cool gray regolith, blue-violet shadows, white hulls, cyan trim, black sky in horizon scenes |
| Mars | Rust/ochre ground, dusty peach atmosphere, same recognizable habitat family |
| Selection | Thin cyan/white outline and footprint marker, supported by text in context panel |
| Warning | Amber icon plus actionable label; red reserved for critical states |
| Scientific images | Separate sourced NASA figures from stylized game illustrations; do not pixelate away important data |

### Suggested palette tokens

Use these as a starting palette, not a promise that every scene contains precisely the same colors.

| Token | Hex | Role |
| --- | --- | --- |
| Ink | `#203653` | Outlines, body text |
| Deep space | `#10182D` | Space backgrounds |
| Hull white | `#F4F8F5` | Habitats, panel surfaces |
| Ice blue | `#CDEAF4` | Hull shade, UI background |
| Azure | `#3889D7` | Main action and identity |
| Cyan | `#60D5E5` | Technology trim, selection |
| Mint | `#8DD6A1` | Plants, healthy production |
| Peach | `#F3BD91` | Warm accents and portraits |
| Amber | `#F3B84D` | Attention |
| Danger | `#D95C62` | Critical status only |
| Lunar gray | `#B9C3CF` | Regolith |
| Lunar shadow | `#707F9C` | Ground/hull shadow |
| Mars sand | `#D99A73` | Sunlit ground |
| Mars rust | `#AA6257` | Rocks and ground shadows |

## 2. Perspective, resolution, and scale

The visual target for the colony is a **2D isometric/dimetric presentation with 2:1 ground diamonds**. A 64×32-pixel ground diamond is the proposed new-art standard. It is not interchangeable with a 32×32 orthogonal tile.

If the current game is already orthogonal top-down, keep its functional grid and draw matching top-down buildings. Isometric cutscenes can still depict the same base; do not paste isometric structures into an orthogonal gameplay scene. Inspect the existing renderer before choosing the integration branch.

| Asset | Proposed authoring size / rule |
| --- | --- |
| Cutscene | 640×360 logical pixels, displayed at integer scale; 1920×1080 is 3× |
| World reference view | Start at 640×360; confirm map visibility in the existing game |
| Isometric ground | 64×32 diamond; building footprints in whole cells |
| Orthogonal alternative | 32×32 tiles; do not mix with isometric assets |
| Astronaut | Approximately 32×48 source pixels; feet at consistent pivot |
| Small rover | Approximately 48×32 to 64×48, adjusted to established perspective |
| Resource icon | 16×16; optional 24×24 illustration variants kept separate |
| Crew portrait | 64×64, one shared framing and outline treatment |
| UI | Crisp pixel borders, independently scalable readable text |

A 16×16 source pack enlarged 2× has a visibly coarser grid than newly drawn 32×32 work. Integer enlargement is crisp but does not automatically harmonize the styles. Redraw/adapt important pieces or isolate them in a separate interior view. Never stretch a side-view building into an isometric one.

AI-generated compositions are concepts until checked and cleaned on the target grid. Nearest-neighbor enlargement preserves existing pixels; it does not repair inconsistent pixel art. Downsampling alone is not a substitute for palette and silhouette cleanup.

## 3. Lunar base design and asset inventory

Build a legible settlement rather than scattering unrelated space props. Place a central command hub with short enclosed corridor branches to crew quarters, laboratory, and life support. Keep a compact shelter near the occupied cluster. Put solar generation and battery storage in a readable utility zone. Put the rover garage and airlock at the outward-facing exploration edge. A distant landing area can appear in backgrounds without forcing an extra playable system.

These are illustrative game-layout choices, not an engineering plan or NASA-approved habitat design. Terrain and system connectivity must remain consistent with gameplay.

| Asset ID | Silhouette and identifying detail | Suggested footprint | Required states | Source approach |
| --- | --- | --- | --- | --- |
| `command_hub` | Low rounded white hub, cyan roof band, small antenna | 3×3 | Normal, selected, unpowered | Original signature building |
| `crew_habitat` | Horizontal cylinder, repeating small windows | 3×2 | Normal, occupied, unpowered | Original modular family |
| `laboratory` | Rectangular hull with compact instrument mast | 3×2 | Idle, researching, fault | Original; interior props may be adapted |
| `life_support` | Twin tanks and recycling enclosure | 2×2 | Working, warning, repair | Original readable silhouette |
| `greenhouse` | Enclosed hull with small cutaway showing mint plant rows | 3×2 | Seedlings, growth, harvest, low power | Original; avoid enormous unshielded glass bubble as a safety claim |
| `solar_array` | Two sapphire panel wings on white supports | 2×2 | Working, offline, damaged | Original to match projection |
| `battery_bank` | Squat white cabinet, cyan charge strips | 1×2 | Charging, low, offline | Original |
| `storm_shelter` | Low regolith-covered mound with reinforced entry | 2×2 | Available, occupied, sealed | Original |
| `comms_dish` | White dish and compact electronics box | 2×2 | Linked, intermittent, offline | Original or redraw permitted source |
| `airlock` | Small capsule with two visible door markers | 1×2 | Closed, cycling, occupied | Original |
| `corridor` | Enclosed white tunnel, blue roof seam | 1-cell segments | Connected, disconnected | Straight, corner, T, cross, end variants |
| `rover_garage` | Low canopy/enclosure and charging pad | 2×2 | Empty, parked, charging | Original |
| `rover` | Six readable wheels, instrument mast, cyan body stripe | Mobile | Idle, driving, scanning | Original scout vehicle |
| `terrain` | Craters, boulders, flat regolith, sparse footprints | Tile family | Ordinary, surveyed, placement feedback | Free source where perspective/license fit |

Create a healthy base first. Reuse overlays and a few state frames for failures instead of drawing a completely different building for every condition. Show power failure by dimming equipment lights plus a status symbol, not by making the whole building unreadably black.

### Mars adaptations

Reuse the structural family so players recognize functions. Change terrain, ground contact dust, selected exterior details, and environment-specific equipment; do not simply tint the entire Moon scene orange.

Create rust rock clusters, a crater-edge tile family, dust overlays, a dust-coated solar-panel state, and a compact atmospheric-processing module if supported by gameplay. Keep plants inside controlled habitats. Do not imply that Mars has breathable outdoor air or established native vegetation. A horizon cutscene may show a dusty sky; a close map does not need a permanent strip of sky.

Use **Next Mission Day** on the Moon and **Next Sol** on Mars. Keep any different duration assumptions explicit in the simulation and resource labels.

## 4. UI art: Civ VI principles translated into deliverables

The map is the main workspace. Ornament should help players find decisions, not consume the screen. Use white/ice panels, navy lettering, blue action buttons, small corner notches, and consistent pixel borders. Avoid unrelated medieval frames, glossy sci-fi holograms, or an always-open dashboard wall.

| Component | Art and behavior specification |
| --- | --- |
| Top resource strip | Five consistent icons: power, oxygen, water, food, research; quantity and trend; details on click/focus |
| Mission tracker | Upper-left compact card, one objective and next useful action; collapsible |
| Alerts | Upper-right grouped indicator; icon, consequence, response; no constant feed covering the map |
| Context panel | Bottom-left only on selection; thumbnail, name, state, input/output, one primary action, Learn More |
| Turn button | Bottom-right strong silhouette; label changes by planetary scenario |
| Main window | One shared header/body/close pattern for research, crew, construction and knowledge |
| Mission Control | One friendly 64×64 portrait and short dismissible hints linked to a relevant action |
| Placement | Transparent ghost, footprint, valid/invalid symbol and explanation; color is supplementary |
| Map overlays | Optional power/radiation/resource layers with legend and obvious exit |
| Dialogue | Reusable framed panel; character name and editable text added in Unity; lower scene area stays quiet |

Draw one nine-slice panel family and button states for normal, hover/focus, pressed, disabled, and selected. Include close, back, search, book, alert, repair, build, crew, research, pause, and advance symbols. Share the same icon vocabulary across HUD, tooltips, construction catalog, and Knowledge Base.

Resource bars must distinguish stored amount, capacity, and net rate. Research is a game progression metric. Use example numbers only as mockup values, never as purported NASA operating data. A small animation or numeric change should explain outcomes after advancing time.

### Knowledge Base visual template

Use the same panel family with search, category navigation, a compact illustration, and readable paragraphs. Categories: Astronomy & Space Weather; Planetary Environments; Spacecraft & Habitats; Astronaut Life & Missions; NASA Missions & Discoveries.

Every entry has a plain-language summary, short explanation, gameplay relevance, reality-versus-game note, source links, and review date. Basic science remains accessible without progression locks. Generated planet sprites are decorative illustrations, not observational evidence. Scientific charts and exact diagrams should be built from verified data or explicit geometry, not image generation.

## 5. Characters and animation

Protagonist continuity: messy midnight-navy hair, warm tan skin, orange star pin/emblem; child in sky-blue hoodie, student in blue jacket, adult in white/cyan uniform or pressure suit. Large friendly features should survive simplification. Make a reference sheet before refining the seven scenes.

MVP crew roles: commander/scientist, engineer, botanist, medical officer. Differentiate portraits and small role emblems without requiring four entirely different suit designs. Outdoor astronauts wear sealed suits. A friendly rover can carry personality through antenna movement and a small status display.

Start with four facings and a four-frame walk cycle when compatible with the game's movement. Add idle, work/repair, and shelter/alert poses; reuse neutral frames where useful. Two-frame indicator blinks and short dish/rover animations are enough to make the settlement feel inhabited. Do not animate every UI element. Honor reduced-motion preferences and avoid rapid flashing.

## 6. Seven essential cutscenes and creation prompts

One finished image per scene. No additional full-screen scenes are necessary for the MVP. Narration and dialogue stay editable in Unity. Reserve the lower 25–30% for overlays. All prompts below use this shared style prefix:

> Original AstroCiv 16:9 cutscene; cute, authentic 16-bit-inspired JRPG pixel art; designed for a 640×360 logical grid; deliberate square clusters, consistent apparent pixel size, clear navy outlines, 2–3 shades per material, restrained harmonious palette; brilliant azure, cyan and clean white with mint and peach accents; expressive simple characters; crisp edges, no airbrushing, bloom, photographic texture or noisy microdetail. Keep the lower 25–30% visually quiet. No embedded captions, logos or watermarks. Reuse the approved protagonist reference and palette.

| # | Scene and scene-specific prompt | Story / gameplay purpose |
| --- | --- | --- |
| 01 | **Just One More Turn.** Sunny Mississauga bedroom; 11-year-old protagonist in blue hoodie and orange star pin at a wooden desk, viewed from behind at three-quarter angle; monitor shows an original miniature civilization game with forests, roads and small houses; telescope, astronomy books and toy rocket; blue sky outside. | Establish imagination and introduce building through a brief prologue. |
| 02 | **The Astronaut from Ontario.** Welcoming science classroom; same child watches a simplified depiction of Chris Hadfield on a classroom screen aboard the ISS; friendly teacher, classmates, paper planets, small Canadian flag in the displayed scene, blue sky and trees outside. Do not imply endorsement. | Make the dream feel locally possible. |
| 03 | **Years of Learning.** Graduation at University of Toronto's University College; recognizable Romanesque architecture, lawn and trees; same protagonist now a young adult in gown, holding diploma and astronomy book, supported by friends; bright Ontario sky. | Narration covers study, setbacks, discovery and growth without extra cutscenes. |
| 04 | **The Journey Begins.** White-and-cyan crew rocket lifting from a coastal launch site under a clear blue sky, bright exhaust and simple cloud clusters, distant safe ground viewpoint. The protagonist and crew are aboard, not standing beside their own departing rocket. | Transition from years of preparation to the Moon mission. |
| 05 | **Welcome to Our New Home.** Elevated isometric view of the approved lunar colony: connected white habitats, command hub, lab, greenhouse, sapphire solar arrays, battery, dish, shielded shelter, tiny astronauts and rover; cool gray terrain, black sky and blue Earth where horizon composition permits. | Match the actual building silhouettes and introduce the playable settlement. |
| 06 | **Our First Solar Storm.** One image split into two balanced scenes. Left: two suited astronauts urgently returning to the airlock, one supporting an exhausted teammate. Right: bright white/cyan control room, engineer troubleshooting a console, scientist restoring monitoring, commander allocating backup power, amber alerts and shelter status. Keep it tense and humane. No visible radiation beams, lunar lightning, outdoor windstorm, or instant radiation-mutation imagery; exhaustion comes from exertion, not a claim that every storm immediately causes illness. | Player protects crew and balances power. Equipment faults are a fictional scenario, not an inevitable outcome of every solar event. |
| 07 | **A Message Back to Earth.** Adult protagonist waves to Mississauga schoolchildren on a communications screen; supportive crew, small plant and rover nearby; white/cyan habitat, blue Earth beyond the window; warmth echoes the childhood room. | Complete the story by inspiring the next generation. |

Production order: 01 and 05 establish Earth/Moon style; finalize protagonist reference; then 02, 03, 04, 06, 07. Use simple fades and restrained camera motion. Keep cutscenes skippable/replayable. Stargazing, cardboard base, training, crew introductions, first harvest, rover expedition, and next dreamer become dialogue, small milestones, or later content.

### Reusable individual asset prompt

> Create one original [ASSET] for AstroCiv, matching the approved [ISOMETRIC 2:1 / ORTHOGONAL] projection, [FOOTPRINT] and [SOURCE SIZE]. White hull, cyan trim, navy outline, upper-left illumination, two or three shades per material. Distinguish its function through [IDENTIFYING SHAPE]. Transparent background, no text, no scene, no decorative platform beyond its footprint. Crisp deliberate pixels. Produce a concept to be cleaned and exported on the target grid; do not claim generation guarantees exact sprite alignment.

Do not use downloaded restricted assets as AI inputs without permission. Original prompts can describe the game's visual rules without supplying third-party pack artwork.

## 7. Ranked free assets usable in Unity

These are **editorial fit rankings**, not store ratings. They weigh pixel style, relevance, integration effort, and license clarity. “Unity-compatible” means importable sprite art, not guaranteed prefabs or tested project compatibility. Name-your-price listings below expose a free download; premium extras are excluded. Rank measures value to the whole production, not just colony exteriors.

| Rank / fit | Pack and source | Free scope and best use | Match / adaptation | License and decision |
| --- | --- | --- | --- | --- |
| 1 · 9/10 | [Kenney — UI Pack: Pixel Adventure](https://kenney.nl/assets/ui-pack-pixel-adventure) | 500 listed files; pixel panels/buttons/interface pieces | Best first HUD upgrade; recolor to white/cyan, establish one border system | CC0; recommended |
| 2 · 9/10 | [Deep-Fold — Pixel Planet Generator](https://deep-fold.itch.io/pixel-planet-generator) | Export planets, stars, asteroids, galaxies and black holes as PNG/GIF/spritesheets | Excellent astronomy illustrations; use our palette; stylized rather than scientifically measured | Creator permits outputs in projects; source MIT. Export PNG/sheets into Unity; no Godot runtime required. Preserve MIT notice if reusing code |
| 3 · 8/10 | [MattWalkden — Free Lunar Battle Pack](https://mattwalkden.itch.io/lunar-battle-pack) | Lunar ground/structures, landing pads, backgrounds, asteroids and UI elements | Strong retro lunar vocabulary; mainly side-view, so use in backgrounds/landing scenes or redraw for map projection | CC0; recommended selectively; omit combat elements |
| 4 · 7.5/10 | [0x72 — 16x16+ Robot Tileset](https://0x72.itch.io/16x16-robot-tileset) | 16×16 tiles and nine taller robot characters | Useful interiors and cute robots; requires palette/projection discipline; not an exterior colony kit | CC0; recommended for interior/robot branch |
| 5 · 7/10 | [Kenney — Pixel UI Pack](https://kenney.nl/assets/pixel-ui-pack) | 750 listed pixel UI files | Alternative to rank 1, not an invitation to mix two entire themes | CC0; choose one UI foundation |
| 6 · 7/10 | [reodesuxz — Free Pixel Sci-Fi Tileset](https://reodesuxz.itch.io/free-pixel-sci-fi-tileset-16x16) | 83 top-down 16×16 tiles/sprites; consoles, tanks, pipes, solar panel, medical props and characters | Good functional interior coverage; redraw/cohere with larger assets | Free personal/commercial use; source-file redistribution prohibited. Keep raw files out of public repository |
| 7 · 6/10 | [Rumblecade — Lunar Surface 1-B](https://rumblecade.itch.io/lunar-surface) | 32×32 lunar platformer tiles and equipment | Moon details useful, but 1-bit and side-view differ markedly from our colored isometric target | Free/commercial game use allowed; standalone resale prohibited; confirm repository redistribution rights before including source |
| 8 · 5.5/10 | [Cainos — Pixel Art Top Down: Basic](https://assetstore.unity.com/packages/2d/environments/pixel-art-top-down-basic-187605) | Free Unity environment pack | Earth greenery/terrain branch; not lunar habitats; preserve its top-down perspective | Standard Unity Asset Store EULA; compiled game use versus source redistribution must be separated |
| 9 · 4/10 | [Lootzifr — Pixel Space Shooter: FREE](https://assetstore.unity.com/packages/2d/pixel-space-shooter-free-216898) | Six ships, space background and effects | Pixel art but combat-oriented; limited use for distant spacecraft/backgrounds | Free Unity listing; standard EULA; low priority |
| 10 · 3/10 | [Brett Gregory — 2D Space Kit](https://assetstore.unity.com/packages/2d/environments/2d-space-kit-27662) | 115 artwork pieces plus demo content | Broad space coverage, but high-resolution backgrounds/space-shooter presentation do not establish our pixel style | Free Unity listing; standard EULA; older package, test scripts separately; reference/fallback only |

### Strong visual candidates with unresolved terms

| Candidate | Why relevant | Why not in the recommended import set |
| --- | --- | --- |
| [Kipperfalcon — Free Space Isometric Pixel Art 32×32](https://kipperfalcon.itch.io/free-space-isometric-pixel-art-32x32-tileset) | Closest terrain perspective; free portion lists 54 tiles and 20 decorations | Page says CC0 but also prohibits redistribution and AI training. Treat terms as conflicting pending clarification. Machines, materials and six buildings shown in premium content are not part of the free terrain offer. Do not count this as a free complete base kit |
| [Penusbmic — Sci-fi Survival Camp: Planet One](https://penusbmic.itch.io/sci-fi-planetone) | 16×16 survival scenery/props could inform a Martian outpost | Free PNG; pictured hero excluded. Creator comment allows commercial release, but full redistribution terms are not clear on the retrieved page. Different mood/projection also need adaptation; hold pending clear license |

**Unity Asset Store-only ranking:** Cainos for Earth scenes, Lootzifr for pixel spacecraft, then 2D Space Kit for limited space imagery. None of these verified listings is a complete white/cyan isometric Moon-and-Mars settlement pack. The stronger matches for UI, astronomy, and lunar scenery are external PNG packs that Unity can import.

### Recommended zero-purchase combination

Start with **one Kenney UI pack + Deep-Fold exports + selected MattWalkden lunar backgrounds + optional 0x72 interiors/robots**, then create the signature colony buildings and protagonist. Keep a smaller coherent selection rather than importing every pack.

For an already top-down game, reodesuxz is a practical interior option if the source-file restriction fits your distribution workflow. For an isometric exterior, author the shared building family and use original terrain until the closest terrain candidate's terms are clarified. No verified complete free Mars colony kit was found in this search; create Mars terrain variants and reuse the habitat family. This is a search finding, not a claim that none exists anywhere.

## 8. License and provenance workflow

Free price does not mean public-domain or open-source permission. Your public GitHub repository is a separate distribution channel from a compiled game. Unity's standard EULA allows incorporation in qualifying products, not unrestricted distribution of standalone asset files. Keep restricted raw files out of public source; provide acquisition/import instructions instead. Do not assume recoloring removes restrictions.

For every imported pack record: asset name, creator, original URL, download date, downloaded version, license text/path, required credit, modifications, source-file redistribution permission, and in-game locations. Keep third-party art license scope separate from the repository's code license. Credit CC0 creators voluntarily as good provenance practice. For unclear/conflicting terms, keep the candidate on hold rather than silently selecting whichever statement is convenient.

Suggested manifest row:

`asset_id | creator | source_url | version | downloaded_on | license | credit | modified | raw_repo_allowed | use_locations`

## 9. Unity handoff

1. Inspect current Unity version, render pipeline, camera projection, grid and asset scale. This brief does not authorize replacing working game systems.
2. Import chosen sprites as Sprite (2D and UI); use Multiple for sheets and slice by actual cell size/margins. Do not slice a tall character using its ground-tile size blindly.
3. Use Point filtering and no texture compression for crisp source art. Normally disable mipmaps for this fixed-scale sprite workflow. Keep texture-size settings from silently shrinking the sheet.
4. Set a common world Pixels Per Unit and matching Pixel Perfect Camera value. A starting proposal is 32 PPU, but preserve the current coherent scale if different. A 64×32 diamond at 32 PPU spans 2×1 world units; grid setup must account for that.
5. Set 640×360 reference resolution only after checking gameplay visibility. Prefer integer display scale and deliberate letterboxing/cropping over uneven resampling. Handle UI scale separately so text stays readable.
6. Use consistent ground-contact pivots, footprint overlays, and sorting settings appropriate to the existing isometric/top-down renderer. Test actors walking in front of and behind buildings.
7. Build nine-slice panels without stretching pixel corners. Verify normal, focus, pressed and disabled states. Use a readable licensed font already in the project or acquire one with a recorded license.
8. Keep editable art masters separate from runtime PNGs; build atlases with adequate padding and check edge bleeding in the actual build.
9. Test camera movement, zoom, window resizing and animation for shimmer, distorted pixels, sorting errors and clipped text.

Suggested folders: `Art/World/Moon`, `Art/World/Mars`, `Art/Buildings`, `Art/Characters`, `Art/UI`, `Art/Cutscenes`, `Art/Knowledge`, `Art/ThirdParty`, `Art/Licenses`. Keep restricted content outside publicly committed paths. Example filename: `moon_command_hub_iso_idle_01.png`.

## 10. Production sequence and completion checklist

**Pass 1 — One beautiful playable view:** lock perspective and palette; finish terrain, command hub, habitat, power and greenhouse silhouettes; introduce one astronaut and rover; skin the resource strip, context panel, mission tracker and turn button. Validate selection, resource explanation and advancing time before expanding the catalog.

**Pass 2 — States and learning:** finish shelter, life support, laboratory and connectivity; create fault/working indicators; add unified management windows and a few fully sourced Knowledge Base entries. Show production, crew work and repairs through restrained animation.

**Pass 3 — Story and second world:** finish the seven cutscenes using approved building/character references; make Mars terrain and dust states; add optional milestones only after the main loop looks and works coherently.

- [ ] Buildings can be distinguished by silhouette without labels.
- [ ] Palette, outline width, light direction and apparent pixel density agree.
- [ ] No mismatched side-view, orthogonal and isometric objects share one map.
- [ ] Healthy, idle, disconnected and critical states use icons/text as well as color.
- [ ] The player can find the current goal and next action within a few seconds.
- [ ] A resource shortage leads to inspection, action, turn resolution and a clear result.
- [ ] The map remains the visual focus; only one major window opens at a time.
- [ ] Text remains readable at small window sizes and with increased UI scale.
- [ ] All seven cutscenes preserve the protagonist and the base's identity.
- [ ] Moon scenes have no atmospheric storm clouds or wind-blown vegetation.
- [ ] NASA facts, illustrative art, fictional events and gameplay approximations are distinguishable.
- [ ] Every external asset has provenance and a compatible distribution path.
- [ ] Imported packs are tested in the actual Unity project; a store listing alone is not validation.

## Reference links

Asset-specific publisher links appear directly in the tables above. Additional implementation and science references:

- [Unity sprite preparation and Pixel Perfect Camera](https://docs.unity.com/en-us/engine/6000.3/manual/unity2d/2d-urp/2d-pixelperfect/prep-sprites)
- [Unity pixel-perfect camera configuration](https://docs.unity.com/en-us/engine/6000.0/manual/unity2d/2d-urp/2d-pixelperfect/configure)
- [Unity Asset Store EULA FAQ](https://assetstore.unity.com/browse/eula-faq)
- [Unity Asset Store terms and EULA](https://unity.com/legal/as-terms)
- [NASA Moon facts](https://science.nasa.gov/moon/facts/)
- [NASA Science](https://science.nasa.gov/)
- [NASA APIs / DONKI entry point](https://api.nasa.gov/)
- [NASA Open Science Data Repository](https://osdr.nasa.gov/)

The separately generated concept board is an original visual target, not a screenshot of the game, a composite of the listed packs, or an import-ready sprite sheet.
