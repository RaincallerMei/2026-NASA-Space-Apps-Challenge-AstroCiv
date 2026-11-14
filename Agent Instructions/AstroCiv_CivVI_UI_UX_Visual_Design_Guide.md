# AstroCiv UI/UX & Visual Design Guide
## Lessons from *Sid Meier's Civilization VI*, adapted for a 2D pixel-art space game

**Document type:** Consolidated design reference and implementation guide  
**Applies to:** AstroCiv's gameplay UI, visual art, onboarding, scientific learning, and interaction systems  
**Design target:** A welcoming, readable, strategy-driven game for young players, with authentic 16-bit-inspired pixel art and NASA-grounded educational content

---

## 1. The core lesson from Civilization VI

*Civilization VI* makes an unusually complex strategy game approachable by keeping the **world map as the primary workspace** and organizing information around it. The interface gradually reveals complexity: persistent resource and progress indicators provide situational awareness; selecting a city or unit reveals relevant actions; dedicated windows expose deeper systems; and the Civilopedia explains terminology, game mechanics, and historical context.

Civ VI also relies on **guided turn progression**. A player can explore and make decisions at their own pace, while prompts, notifications, and the Next Turn control communicate what needs attention and what has changed. Its stylized visual design supports recognition and a feeling of building a living world rather than reading spreadsheets.

**AstroCiv's adaptation:** Preserve the clarity, agency, discovery, and educational integration—**not** Civ VI's full density or number of systems. AstroCiv should feel like a smaller, warmer strategy game whose attractive 2D base map is the star, and whose decisions teach authentic astronomy and space-exploration concepts.

### Direct translation of the major patterns

| Civilization VI pattern | AstroCiv interpretation | Why it matters |
| --- | --- | --- |
| Map-first gameplay | Interactive 2D Moon/Mars base and surrounding terrain | Decisions are spatial and visual, not only numerical. |
| Top-level yields/status | Persistent life-support and operations resource bar | Players can assess base health at a glance. |
| City/unit contextual actions | Building, rover, and astronaut selection panel | Show actions only when relevant. |
| Technology/civics and objectives | Compact research progression and mission tracker | Give players a near-term goal and a longer-term aspiration. |
| Advisor, prompts, and tooltips | Friendly Mission Control hints, warnings, and explainers | Tell beginners what to do and why, without taking over. |
| Civilopedia | **NASA Knowledge Base**: real astronomy and space-science entries | Connect play to verifiable NASA knowledge. |
| Map lenses/overlays | Optional power-grid, radiation, and resource overlays | Make invisible relationships visible on demand. |
| Next Turn and event feedback | Next Day/Next Sol, simulation update, and event summaries | Make cause and effect understandable. |
| Discovery/achievement celebrations | Brief pixel-art discovery moments and cutscenes | Give progress an emotional reward. |

---

## 2. Five design principles

1. **World first, interface second.** Most gameplay happens on the base map; panels support it instead of covering it permanently.
2. **Simple by default, detailed on request.** A beginner sees essential numbers and suggested actions; deeper formulas and science appear in expandable windows or tooltips.
3. **Every important change has feedback.** Show what changed, why it changed, and what the player can do next.
4. **Play and learning reinforce each other.** The NASA Knowledge Base explains the real science behind an action but never replaces gameplay with mandatory reading.
5. **Consistent, authentic pixel art.** Map tiles, characters, UI frames, icons, illustrations, and cutscenes share a coherent 16-bit-inspired visual language.

**Non-goals for the initial release:** Re-creating Civ VI's complete menu density, diplomacy systems, vast technology tree, or late-game micromanagement.

---

## 3. Visual direction: cozy 16-bit-inspired pixel art

Civ VI's specific style is bright, approachable, and stylized rather than photorealistic. AstroCiv should adopt that *approachability*, not copy its historical UI ornament or 3D rendering.

### Art rules

- **Style:** Handcrafted-looking 2D pixel art with clear silhouettes, appealing miniature buildings, expressive characters, and gently stylized planetary terrain.
- **Pixels:** Use a consistent apparent pixel size. Prefer crisp nearest-neighbor scaling. Avoid anti-aliased edges, semi-photorealistic texture, inconsistent pixel densities, painterly gradients, and excessive AI-like microdetail.
- **Tile and asset system:** Choose a single base grid (for example, 16×16 or 32×32 source-pixel tiles) and design buildings as clean multiples of that grid. Choose one grid for the game; do not mix unrelated source resolutions without deliberate scaling rules.
- **Palette:** A controlled palette. Mars can favor warm rust and sand; the Moon can favor cool grays and slate. Use turquoise/teal for supportive technology accents, warm amber for attention, and sparing red for danger. Keep HUD colors distinct from terrain.
- **Lighting:** Simple, intentional pixel-art shading with a consistent light direction. No soft photographic bloom or glossy gradients that fight the pixel look.
- **UI frames:** Reusable pixel-perfect borders, 9-slice panels where supported, clear selected-state outlines, and a small family of consistent icons.
- **Text:** Use a genuinely readable bitmap/pixel font at an appropriate on-screen size. Favor readability over mimicking a low-resolution console exactly; do not force tiny text just to look retro.
- **Cutscenes:** The same palette, pixel density, character designs, and lighting conventions as gameplay, but with richer composition when needed.

**Terminology:** “16-bit” is an *art-direction shorthand*, not a requirement to use 16-bit color or obsolete rendering hardware. The measurable requirements are consistent pixels, silhouettes, palette, and readable UI.

### Visual hierarchy

1. **Primary:** The map, selected object, urgent hazards, current actionable decision.
2. **Secondary:** Resources, objective progress, contextual actions, Next Day/Next Sol.
3. **Tertiary:** Detailed statistics, encyclopedic background, historical logs, advanced overlays.

Do not animate every item. Reserve movement, flashing, and stronger color contrast for meaningful events. Use gentle ambient animation to make the base feel alive without obscuring its state.

---

## 4. Main gameplay layout: one consistent HUD

The following is the **single default layout** for ordinary gameplay, preventing competing HUD designs:

| Screen area | Persistent element | Behavior |
| --- | --- | --- |
| **Center, majority of screen** | Interactive base map | Main environment for inspection, construction, exploration, and movement. |
| **Top** | One compact resource/status bar | Shows 4–5 essential indicators, with hover/focus/click detail. |
| **Upper left** | Collapsible mission tracker | Active objective, next step, current research/construction progress. |
| **Upper right** | Compact alerts + main menu access | Groups notifications; shows unread/critical count instead of a permanent feed. |
| **Bottom left or bottom contextual region** | Selection panel, **only when an object is selected** | Name, state, relevant actions, short description, Learn More. |
| **Bottom right** | Highly visible **Next Day / Next Sol** button | Advance simulation after reviewing actionable warnings. |
| **Optional, later** | Minimap or map-layer control | Add only if the play area becomes large enough to need it. |

Keep the center unobstructed, especially for mobile/small windows. Full-screen management views replace or dim the map rather than competing with several floating windows.

**Terminology rule:** A **sol** is a Martian solar day. Use **Next Sol** for Mars; use **Next Day** or **Next Mission Day** for a Moon scenario. Never label a lunar day a sol.

### Responsive behavior

- At desktop width, use edge-aligned compact panels.
- At narrower widths, collapse objective text and resource labels into tappable icons with accessible details; relocate the selection panel as needed.
- Do not scale the entire HUD so small that text becomes unreadable. UI scaling may differ from world-art scaling.
- Essential actions must remain available without requiring hover, because touch controls have no hover state.

---

## 5. Resource bars: actionable information, not decorative meters

Civ VI displays strategic yields continuously. AstroCiv should similarly show the resources that govern survival and exploration.

### Recommended MVP resources

| Indicator | Example meaning | Suggested on-screen information |
| --- | --- | --- |
| **Power** | Stored usable energy and generation/consumption | Stored kWh; net kWh per mission day/sol; warning when forecast insufficient. |
| **Oxygen** | Available breathable oxygen supply | Stored quantity or days of coverage; net daily change. |
| **Water** | Usable stored water | Stored quantity or days of coverage; net daily change. |
| **Food** | Available food reserves | Stored quantity or days of coverage; net daily change. |
| **Research** | Abstract game progression currency, not a physical NASA measurement | Research progress or points per day/sol, clearly labeled as a game mechanic. |

**Critical distinction:** A percentage of storage, an absolute quantity, and a production rate are *different measurements*. Do not show `82%` and `+12 kWh/sol` as if they describe the same thing. A bar may show `stored / capacity`; adjacent text may show the **net rate**. Keep units and period explicit. If detailed physical quantities would overwhelm young players, a simplified “days remaining” estimate is acceptable as long as its definition is consistent.

### Interaction and feedback rules

- **Default bar:** icon, short label or quantity, trend indicator, and subtle condition state.
- **Details on demand:** capacity, production sources, consumption sinks, net change, and forecast.
- **Before committing an action:** preview expected resource impact, such as `Power −3/day; Oxygen +5/day` (example gameplay values, **not** NASA facts).
- **Warnings:** trigger on forecast risk *before* a resource reaches zero; name the affected system and a useful response.
- **Clarity:** Never rely on color alone. Combine color with symbols and text (`Low oxygen`, `−3/day`).
- **Balance:** Make the underlying simulation consistent from one turn to the next; explain any exceptional event in the event log.

**NASA fidelity:** Energy, water, oxygen, and food are realistic mission concerns, but actual consumption and equipment performance vary by system and environment. AstroCiv's simplified resource model must be described as **gameplay abstraction**, not official NASA operational data.

---

## 6. Interaction and window system: three levels of depth

Civ VI uses selection-driven controls and specialized screens rather than putting every command on the map. AstroCiv should define **three distinct information levels**:

### Level A — Persistent HUD

Visible during normal play: core resources, objective, alert indicator, and turn-advance control. It should remain lightweight and predictable.

### Level B — Context panel or short tooltip

Opened by selecting, focusing, or tapping something on the map.

- **Building selected:** name, role, operational state, input/output, one or two actions (e.g., Repair, Upgrade), and **Learn More**.
- **Astronaut selected:** assigned task, status, and available orders.
- **Terrain/feature selected:** description, exploration/research action, and relevant science entry.
- **Tooltip:** a short definition or explanation; it cannot be the only way to access critical information.

Use highlighting to connect a selected item with its panel. Close the panel when selection is cleared or another object is selected.

### Level C — Dedicated management or knowledge window

Used for complex tasks, such as **Research**, **Crew**, **Base Report**, **Construction Catalog**, and **NASA Knowledge Base**. Only **one major window** should be open at a time.

### Unified rules for all windows

- Consistent header, title, close control, pixel-art frame, typography, and button hierarchy.
- **Escape / Back** closes the topmost dismissible layer; returning to the map preserves the player's previous map position and selection where reasonable.
- Small informational overlays can be dismissed without changing the game state.
- Confirmation dialogs are reserved for consequential or irreversible actions—not routine inspection.
- A major window pauses direct map interaction (or clearly blocks it) to prevent accidental actions behind it.
- No deep stacks of multiple modal dialogs. Prefer navigation within one window.

**Window priority:** Ordinary gameplay uses Level A; selection uses Level B; complex management or encyclopedia use Level C. These are alternatives for different needs, not three overlapping permanent HUDs.

---

## 7. NASA Knowledge Base: AstroCiv's Civilopedia

### What Civ VI teaches

The **Civilopedia** is an in-game reference that connects Civ VI's rules and objects to historical/cultural knowledge. It makes learning available when curiosity arises: the player can read a concept, then return to making decisions.

### AstroCiv's version

Create a **NASA Knowledge Base** that explains authentic astronomy, planetary science, space technology, and astronaut operations. It should behave like a searchable, child-friendly encyclopedia—not a wall of mandatory text.

**Core categories** (use these consistently across navigation and content):

1. **Astronomy & Space Weather** — Sun, stars, solar activity, radiation, communications effects.
2. **Planetary Environments** — Moon, Mars, atmosphere, temperature, gravity, terrain.
3. **Spacecraft & Habitats** — power, life support, shielding, rovers, instruments.
4. **Astronaut Life & Missions** — crew health, EVA planning, safety, exploration.
5. **NASA Missions & Discoveries** — relevant real missions, observations, and discoveries.

### Required knowledge-entry structure

Every entry should contain:

| Field | Content |
| --- | --- |
| **Title + category** | A clear term and navigation grouping. |
| **One-sentence summary** | Answer “What is this?” in child-friendly language. |
| **Short explanation** | A few brief paragraphs; optional illustration/diagram. |
| **Why it matters in AstroCiv** | Explicit connection to a building, hazard, decision, or objective. |
| **Reality vs. game** | Explain where game mechanics simplify the science. |
| **Source(s)** | Linked official NASA or other reliable scientific source(s). |
| **Review/update date** | Makes time-dependent facts and data provenance clear. |

### Ways to open it

- From a persistent help/book icon in the main menu.
- From **Learn More** on objects, resource details, research nodes, and events.
- From searchable category navigation inside the Knowledge Base.
- After discoveries, through an optional **Read the Science** link.

**Access rule:** Basic science explanations must always be accessible. Discoveries may highlight new or newly relevant entries, but essential learning content should **not** be locked behind progression. This avoids a contradiction between “discoverable entries” and “available-on-demand help.”

### Example entry: Solar energetic particles

- **Science:** Energetic particles associated with solar activity can create radiation hazards for spacecraft and astronauts, especially beyond Earth's magnetic protection.
- **Gameplay:** A space-weather warning may motivate delaying an excursion or using shelter.
- **Reality-vs-game note:** A short, deterministic countdown is a game simplification. A solar flare, coronal mass ejection, and solar energetic particle event are related but **not interchangeable**; do not claim every flare produces the same radiation threat.
- **Source:** Link to an appropriate NASA space-weather explanation and, if using event data, the NASA DONKI source.

### Scientific sourcing and live-data policy

Potential sources include [NASA Science](https://science.nasa.gov/), [NASA Open APIs](https://api.nasa.gov/) (including DONKI when relevant), and [NASA Open Science Data Repository](https://osdr.nasa.gov/). The specific dataset must match the claim or game mechanic; these are not interchangeable data feeds.

- Include actual source links and identify the retrieval/observation date for data-driven claims.
- Distinguish **live/historical NASA-derived data** from **fictional scripted events** and **gameplay approximations**.
- The game must remain playable if an external API is unavailable: use reviewed, labeled fallback examples or preloaded data.
- Never place private API keys in public source code or front-end bundles; use appropriate server-side configuration when credentials are required.

---

## 8. Player guidance: hints, advisor, objectives, and warnings

Civ VI combines tutorials, an advisor, contextual tooltips, notifications, and turn prompts. In AstroCiv these must form **one coordinated guidance system**, not several competing instruction channels.

### Define a single friendly guide

Use **Mission Control** (an astronaut companion or onboard AI character) as the voice of optional instructional hints. Make messages short, encouraging, and specific.

- **Objectives** answer: **“What am I trying to achieve?”** The tracker shows the current mission and next achievable step.
- **Hints** answer: **“What could I do next?”** Example: “Our battery is running low. Select a solar panel to see why.”
- **Tooltips** answer: **“What does this mean?”** Example: “Net power is production minus consumption.”
- **Warnings** answer: **“What requires attention now?”** Example: “Oxygen may run out in two mission days.”
- **Knowledge Base** answers: **“What is the real science behind this?”**

### Priority and presentation

| Priority | Presentation | Example |
| --- | --- | --- |
| **Critical** | High-visibility alert; specific recommended response | Life support projected to fail before next turn. |
| **Action needed** | Objective/pending-action marker; accessible from Next Day/Next Sol | An astronaut awaits instructions or an essential building is unassigned. |
| **Helpful** | Dismissible Mission Control hint | Consider researching improved solar panels. |
| **Educational** | Optional tooltip or Learn More link | Why solar power differs between environments. |

### Behavioral rules

- Initial onboarding should be **contextual and short**: move/select → inspect resource → build/assign → advance time → observe outcome.
- A hint should offer a **direct path** to the relevant button, building, or window when possible.
- Let players dismiss or reduce noncritical hints. Do not repeat identical advice every turn unless the underlying condition materially changes.
- Do not force players to follow the suggested strategy. Preserve choices and experimentation.
- Critical warnings remain accessible in the alert log even if the immediate popup is dismissed.
- Avoid invented scientific certainty: phrases about scientific risk must match the data/abstraction behind them.

---

## 9. Turn loop, events, and cause-and-effect feedback

Civ VI makes decisions meaningful through a repeatable sequence: **observe → choose → act → end turn → see results**. AstroCiv should use the same mental model.

### Recommended player loop

1. **Observe** the base, resource forecasts, active mission, and alerts.
2. **Inspect** an astronaut, building, terrain feature, or research opportunity.
3. **Choose** a meaningful trade-off—power allocation, crew assignment, repair, construction, research, or exploration.
4. **Preview and confirm** the expected impact where possible.
5. **Advance** using Next Day / Next Sol.
6. **Resolve** production, consumption, repairs, missions, hazards, and discoveries.
7. **Explain outcomes** in a concise summary; update the map, bars, objectives, and notifications.

### Next Day / Next Sol behavior

- The button remains clearly available on the main HUD.
- If a **critical, actionable** issue is about to produce major consequences, display a concise warning and let the player return to address it. Do not silently discard the risk.
- Routine unassigned tasks may show reminders without blocking progress. Establish safe defaults where reasonable.
- Do not overwhelm users with a separate popup for every resource tick. Group related outcomes into one summary or alert list.
- Players should be able to see **why** a value changed (e.g., rover charging used more power; solar generation declined during a dust event).

### Event notification pattern

**Headline → Consequence → Available response → Optional science link.**

Example: “Power production dropped” → “A dust storm reduced solar output” → “Review power allocation” → “Learn about Martian dust storms.” The game must avoid suggesting that a simulated event is a currently occurring NASA-observed event unless it actually is and its source/time are shown.

---

## 10. Construction, research, overlays, and exploration

### Buildings and construction

- Show ghost placement and valid/invalid terrain feedback before constructing.
- A selected building reveals **what it does**, **what it consumes/produces**, and **what the player can do**.
- Use distinct silhouettes and small recognizable details so that the player can distinguish power, habitation, research, water, and life-support buildings at a glance.
- Small animations convey working, idle, damaged, and upgrading states; status icons supplement the visual state.

### Research progression

Start with a **small, readable research path**, not a Civ-scale tree. Research nodes should show prerequisites, cost/progress, an unlock, and a **Learn More** link. Example strands: **Power Systems → Life Support → Environmental Monitoring → Exploration Equipment**. Their relationships can branch once there is meaningful gameplay justification.

Technology celebrations should be brief and skippable. An unlocked building or ability should clearly appear in the relevant construction or action menu.

### Map overlays (“lenses”)

Map layers reveal information that would clutter normal play. Candidates:

- **Power network:** supplied vs. disconnected buildings.
- **Radiation risk:** safer vs. more exposed areas, with a legend and uncertainty where relevant.
- **Resource/environment:** local terrain features, ice targets, or solar exposure where represented in gameplay.

These overlays are **optional P1 features**, not requirements for the smallest viable map. Label each layer and provide a clear way back to normal view.

---

## 11. Sample end-to-end interactions

### Flow A — Low oxygen (onboarding and management)

1. Top bar indicates oxygen declining; the mission tracker recommends investigating.
2. The player selects the life-support building on the map.
3. Context panel shows current oxygen output, consumption, and available action(s).
4. Player previews a repair or upgrade and sees projected resource changes.
5. Player completes the action and advances time.
6. The resource bar and event summary visibly confirm the outcome.
7. A **Learn More** link offers a short NASA-grounded explanation of life-support recycling, with simplification labeled.

### Flow B — Space-weather decision (science and strategy)

1. A sourced or explicitly fictionalized space-weather warning appears.
2. The alert describes **game consequences**, not exaggerated scientific certainty.
3. Player chooses whether to delay an EVA, protect equipment, or accept a modeled risk.
4. The map highlights affected crew or systems; the turn resolves the consequence.
5. The notification links to a NASA Knowledge Base article on solar activity, radiation, and mission safety.

### Flow C — Research discovery (progression and reward)

1. Player selects a research node and sees its effect and cost.
2. Research progress updates in the mission tracker.
3. Completion triggers a short pixel-art reveal and shows the unlocked feature.
4. A “Read the Science” option opens the corresponding Knowledge Base entry.
5. Player returns to the same map position and can immediately use the unlock.

---

## 12. Accessibility and usability quality bar

A cute retro aesthetic must not make the game difficult to understand.

- Keep text readable at target window sizes; support adjustable UI scale where practical.
- Maintain sufficient contrast for text and status symbols over complex environments.
- Pair colors with icons, text, shape, or patterns; never make color the only status signal.
- Provide keyboard focus and clearly visible focus states for menus, buttons, and encyclopedia navigation.
- Do not require hover for critical content; support click/tap/focus alternatives.
- Allow fast text advance/skipping of nonessential cutscenes and optional reductions in motion or flashing.
- Use consistent control labels and predictable back/close behavior.
- Prefer one obvious primary action per panel; avoid dense clusters of equally styled buttons.
- Test with first-time players: can they find a resource shortage, inspect the cause, fix it, advance time, and open the science explanation without coaching?

---

## 13. Implementation priorities for a game-jam scope

### P0 — Must be coherent and playable

1. **Consistent pixel-art foundations**: map, buildings, character sprites, readable typography, icon family, panel frames, scaling rules.
2. **Map-first gameplay**: select building/astronaut, see relevant contextual panel, perform core action.
3. **Compact resource HUD**: explicit amounts, trends, and meaningful resource warnings.
4. **Mission tracker + Mission Control**: one current objective, contextual beginner hints, actionable alerts.
5. **Turn progression**: Next Day/Next Sol, deterministic resource updates, outcomes that players can understand.
6. **Core management**: small construction/assignment/research loops with visible consequences.
7. **NASA Knowledge Base MVP**: a small number of accurate, curated, searchable or browsable entries connected to actual interactions, with reliable sources and reality-vs-game notes.
8. **Window consistency**: predictable navigation, one major window at a time, accessible close/back, no overlapping clutter.

### P1 — Valuable polish after P0 works

- More research branches, nuanced crew roles, and improved forecasts.
- Optional power/radiation/environment map overlays.
- More encyclopedia entries, illustrations, and content search refinements.
- Discovery animations, short story cutscenes, and achievement moments.
- Minimap if map scale warrants it; richer history/event logs.

### P2 — Only if time remains

- Extensive research trees, advanced dashboards, sophisticated map analysis, many tutorial modes, complex story/event branching.

**Scope rule:** A small number of deeply connected systems is better than many disconnected icons, windows, and encyclopedia articles. Every P0 feature should support at least one complete gameplay-and-learning loop.

---

## 14. Concrete design decisions (to prevent ambiguity)

| Question | Final decision for this guide |
| --- | --- |
| Does the HUD show everything at once? | **No.** One compact persistent HUD; contextual details on selection; full windows for complex systems. |
| How many primary resources? | **Five:** power, oxygen, water, food, research. The first four are operational; research is explicitly an abstract game metric. |
| Does every action open a popup? | **No.** Routine actions use a selection panel. Only consequential decisions, major discoveries, and critical events justify dialogs. |
| Who guides the player? | **Mission Control** provides optional hints. Objectives and critical alerts are separate visual states of the same coordinated guidance system. |
| Is the encyclopedia required reading? | **No.** It is accessible at any time; discoveries surface relevant content but do not gate basic explanations. |
| Is all game content “real NASA data”? | **No.** Clearly separate sourced facts, actual datasets, fictional narrative events, and simplified gameplay mechanics. |
| Can hints dictate strategy? | **No.** They recommend and explain; player agency remains central. |
| Does “Next Sol” apply everywhere? | **No.** Use it on Mars; use Next Day / Next Mission Day for Moon missions. |
| Do warnings prevent advancing time? | **Not by default.** Critical issues prompt explicit review; ordinary pending actions do not cause repeated hard stops. |
| Are overlays and minimap mandatory? | **No.** They are P1 unless the actual map size or gameplay needs justify promoting them. |
| Must the game be technically 16-bit? | **No.** It must look coherently 16-bit-inspired through disciplined pixel-art construction. |
| How many major windows can be open? | **One at a time**, plus a genuinely necessary confirmation dialog or tooltip. |

---

## 15. Acceptance checklist

A new player should be able to:

- [ ] Recognize the base, buildings, astronauts, and interactive environment without reading a manual.
- [ ] Identify the current mission objective and the main available action in a few seconds.
- [ ] Read resource amounts **and** recognize positive/negative trends without interpreting ambiguous percentages.
- [ ] Select a building, understand its function, take an action, and see the resulting consequences.
- [ ] Advance time deliberately and understand any major changes that occurred.
- [ ] Follow helpful hints, dismiss them, or solve problems using another valid strategy.
- [ ] Open the NASA Knowledge Base from both a central menu and a relevant object/event.
- [ ] Differentiate NASA-confirmed science from simulation simplifications.
- [ ] Close windows and return to the map without losing navigation context.
- [ ] Experience consistent, crisp, readable pixel art across gameplay, UI, and cutscenes.

---

## 16. Final design statement

**AstroCiv is a map-first, turn-based, 2D pixel-art space-management adventure.** Inspired by the strengths of *Civilization VI*, it combines clear resource bars, contextual building/crew interactions, meaningful research, a friendly Mission Control advisor, transparent alerts, and a searchable **NASA Knowledge Base**. The scientific reference supports curiosity at the exact moment that gameplay raises a question; it does not interrupt or replace the game.

The desired experience is **cozy but strategic, cute but legible, educational but genuinely playful**. Its central reward is seeing a tiny space settlement become more capable—and understanding the real science behind the decisions that made it possible.
