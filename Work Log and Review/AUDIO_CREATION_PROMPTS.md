# AstroCiv original audio: sources, methods and prompts

Researched 15 November 2026. This file supplies prompts and production methods; no paid generation, subscriptions, audio downloads or game integration were performed.

## Where to create audio

| Tool / method | Best use | Access and usage boundaries | Primary sources |
|---|---|---|---|
| Suno | Full instrumental lo-fi music drafts | Free/Basic output is restricted to personal non-commercial use. Current terms additionally require an authorized download for permitted commercial exploitation; do not stream-rip. Paid status alone is not the full rights check. Confirm permission for embedding/distributing in a public game and repository before release. | [Free-plan explanation](https://help.suno.com/en/articles/9601601), [current terms, effective September 3, 2026](https://suno.com/terms) |
| ElevenLabs Sound Effects | Text-generated clicks, sci-fi notifications, short construction sounds and ambient loops | Free plan lacks commercial rights and requires attribution on non-commercial sharing. Paid plans have additional product-specific terms; beta output has restrictions. Its creative terms restrict distributing isolated output/sample libraries: assess public raw-audio commits separately from embedding audio in the game. | [Generator](https://elevenlabs.io/sound-effects), [SFX guide](https://elevenlabs.io/docs/overview/capabilities/sound-effects), [publishing rights](https://elevenlabs.io/docs/help-center/legal/can-i-publish-the-content-i-generate-on-the-platform), [creative terms](https://elevenlabs.io/creative-terms) |
| Gemini / Google Lyria | Instrumental music creation through Gemini, AI Studio or an audio-generation API | Google currently documents Lyria music generation. Availability, limits, billing and output conditions depend on the surface/account; do not assume a free commercial license. Use the model actually offered to the account and preserve provenance/watermarks. | [Lyria](https://deepmind.google/models/lyria/), [music API guide](https://ai.google.dev/gemini-api/docs/music-generation), [real-time guide](https://ai.google.dev/gemini-api/docs/realtime-music-generation) |
| Stable Audio Open | Locally generated effects, samples and short textures | Requires suitable local hardware/dependencies and acceptance of the model's actual license. The 1.0 model supports up to 47 seconds; it is designed for samples/effects rather than finished long songs. Check the current model-specific license and commercial eligibility; hosted Stable Audio plans are separate. | [Official introduction](https://stability.ai/news-updates/introducing-stable-audio-open), [model and license](https://huggingface.co/stabilityai/stable-audio-open-1.0), [usage licensing](https://kb.stability.ai/knowledge-base/stable-audio-commerical-and-usage-licensing) |
| jsfxr / sfxr.me | Free procedural UI blips, pickups and retro effects | Browser synthesis and WAV export; no generative model subscription is needed. The implementation is open source; inspect its license if incorporating the engine/code. Keep exported-sound provenance separate from the engine license. | [Sound editor](https://sfxr.me/), [source and license](https://github.com/chr15m/jsfxr) |
| Claude/GPT/Gemini writing local synthesis code | Precise original one-shots, simple musical sketches, batch variants | A coding agent can write a Python standard-library WAV generator or Web Audio program. This does not require native text-to-SFX capability. Run locally, inspect the outputs and disclose AI-assisted code/audio production. | [Python wave](https://docs.python.org/3/library/wave.html), [Web Audio specification](https://www.w3.org/TR/webaudio/), [Audacity editing manual](https://manual.audacityteam.org/) |

Recommended zero-cost path: synthesize original short effects locally, or use the CC0 packs in [AUDIO_CANDIDATES.md](AUDIO_CANDIDATES.md). Use Suno/Lyria/ElevenLabs as options only after checking the account's generation, download, game-use and repository-distribution permissions. Meta's [AudioGen model](https://huggingface.co/facebook/audiogen-medium) is CC BY-NC 4.0; do not treat it as an unrestricted commercial alternative.

## Shared sound direction

Warm, friendly astronaut-training game for ages 7–12. Cozy retro-futuristic electronics; soft rounded transients; restrained sparkle; legible cues on laptop speakers. Original sounds only. No voices, recognizable songs, artist imitation or NASA mission recordings. No clipping, harsh sirens, startling sub-bass or long silence. Keep warnings distinct from rewards. Pair important audio with visible feedback.

For each generation: make three variants, pick one by listening in context, and log the tool/model, prompt, account tier, source/job ID, license, human edits and final file. Durations and BPM below are targets requiring verification, not promises the generator will obey.

## Music prompts

Use instrumental mode where available; leave lyric fields empty. Request stems where supported. A prompt cannot guarantee a seamless loop, so edit and listen across the seam afterward.

### Main menu — “A small window onto the stars”

> Original instrumental space lo-fi for a cozy children's lunar-base game menu. 72 BPM, gentle swung drums, warm electric piano, soft rounded bass, slow airy synth pads and occasional delicate star-like bell notes. Safe, welcoming, curious, like planning a space adventure at a comfortable desk. Sparse arrangement with room for interface sounds. No vocals or spoken words, no dramatic rise or large drop. Target two minutes with a stable middle suitable for a seamless 8- or 16-bar loop. Keep the opening gentle and the ending harmonically compatible with the beginning. Provide a clean version without vinyl crackle if possible.

### Cutscene — “From a drawing to the Moon”

> Original instrumental cosmic lo-fi at 86 BPM. Begin with the same warm electric-piano and soft-synth family as a cozy menu, then add light percussion, a hopeful arpeggio and a restrained melodic lift. Convey curiosity becoming confidence as a childhood base drawing becomes an astronaut's mission. More vibrant than menu music while leaving space for reading and narration. Warm wonder, no bombastic orchestra, no vocals, no sudden loud transitions. Target 90 seconds with clear edit points every 8 bars, a modest emotional rise and a gentle resolution. Also provide a sustained, low-melody bed for dialogue if available.

### Gameplay — “Mission clock”

> Original instrumental ambient lo-fi for thoughtful lunar-base management. 96 BPM, stable understated beat, rounded bass, warm pads, tiny sequencer pulses and sparse high-register mallet notes. Slight forward motion suggests time passing without making the player anxious. Keep melody minimal so resource decisions and warnings remain clear. No vocals, no horror, no loud alarm or literal clock embedded in the main mix. Target two minutes with an edit-friendly 16-bar loop. If stems are supported, deliver atmosphere, beat and pulse separately, plus a compatible slightly fuller variation for the final portion of a mission.

The mission's actual countdown should use a separately generated cue below, scheduled by game time. This keeps the warning meaningful when the player pauses or changes simulation speed.

## Individual sound-effect prompts

Append the shared sound direction to each prompt. Use one-shot generation unless explicitly marked as a loop. Duration is the desired edited length; if a tool's minimum duration is longer, trim the useful event in an editor.

| Event / output name | Target | Prompt |
|---|---|---|
| Hover / `ui_hover.wav` | 0.06–0.10 s | One extremely soft rounded synth pip, light and unobtrusive, no metallic click or tail. Quieter than selection. |
| Select / `ui_select.wav` | 0.12–0.18 s | A single warm electronic pluck with a tiny upward pitch movement, tactile and clear, almost dry. |
| Confirm / `ui_confirm.wav` | 0.20–0.30 s | Two soft rising notes, inviting and confident, rounded bell-synth timbre, short clean decay. |
| Back or cancel / `ui_cancel.wav` | 0.15–0.25 s | Two subdued descending electronic notes, neutral navigation feedback, no negative buzzer. |
| Building selection / `build_select.wav` | 0.15–0.25 s | Soft tool-ready electronic chirp, slightly lower than a menu click, no construction impact yet. |
| Valid building placement / `build_place.wav` | 0.35–0.55 s | A small cushioned mechanical clunk followed by a warm activation chirp, miniature lunar module settling into its socket. |
| Invalid placement / `build_invalid.wav` | 0.20–0.35 s | Two gentle low hollow blips that mean try another tile, distinct from a positive chime, friendly and not scolding. |
| Remove building / `build_remove.wav` | 0.30–0.45 s | A soft mechanical release followed by a short downward synth puff, restrained and non-destructive. |
| Repair complete / `repair_complete.wav` | 0.45–0.65 s | Tiny two-step ratchet followed by a clean positive ping, precise miniature machinery, no sparks or harsh drilling. |
| Toggle building on / `power_on.wav` | 0.20–0.35 s | Low rounded electronic hum rising into a short stable note, small system waking up. |
| Toggle building off / `power_off.wav` | 0.20–0.35 s | Matching short hum descending and stopping smoothly, calm system shutdown. |
| Resource collected / `resource_collect.wav` | 0.18–0.30 s | A tiny bubbly digital pickup, clean two-note sparkle, restrained enough for frequent collection. |
| Rover dispatch / `rover_dispatch.wav` | 0.55–0.80 s | Small enclosed electric motor whirr and one confirmation pip, toy-scale sci-fi machinery, no outdoor wind. |
| Crew message / `crew_message.wav` | 0.35–0.50 s | Two warm radio-console chimes, faint filtered electronic texture, welcoming notification without spoken words or real radio recording. |
| Objective complete / `objective_complete.wav` | 0.70–1.00 s | Three ascending soft mallet-synth notes, modest accomplishment, shorter and quieter than level victory. |
| Level unlocked / `level_unlock.wav` | 0.80–1.20 s | A gentle upward electronic shimmer resolving to two bright notes, discovery and possibility. |
| Level won / `level_win.wav` | 2.0–3.0 s | Short original warm space-game victory phrase, four hopeful bell-synth notes over a soft major chord, joyful but not explosive, with a complete gentle ending. |
| Mission stopped / `mission_stop.wav` | 1.2–1.8 s | Gentle descending two-note motif ending on a soft suspended chord, supportive invitation to retry, no death sound, tragedy or harsh failure buzzer. |
| Low resource warning / `warning_resource.wav` | 0.40–0.65 s | A clear but soft two-tone amber-alert signal, midrange rounded synth, separated notes, informative rather than alarming. |
| Solar-storm warning / `warning_storm.wav` | 0.80–1.20 s | Distinctive three-pulse radio-console warning with a subtle filtered shimmer, measured urgency, no siren, explosion or frightening stinger. |
| Approaching evaluation / `timer_tick.wav` | 0.06–0.10 s | A single soft wooden-electronic tick, no reverb, no embedded repeating rhythm. Designed to be scheduled individually by a game clock. |
| Evaluation reached / `timer_end.wav` | 0.65–0.90 s | A clear two-note bell-synth time marker with a gentle final note, neutral completion rather than win or failure. |
| Pause / `pause.wav` | 0.18–0.25 s | Short soft downward two-note interface gesture, calm and immediately recognizable. |
| Resume / `resume.wav` | 0.18–0.25 s | Matching upward two-note gesture using the pause timbre, quiet re-entry. |
| Speed change / `speed_change.wav` | 0.12–0.20 s | Tiny quick paired synth blip; prepare three subtle pitch variants for 1x, 2x and 4x. |
| Wiki open / `wiki_open.wav` | 0.25–0.40 s | A soft digital page-turn swoosh with one warm chime, curiosity and learning, no voice. |
| Cutscene transition / `scene_transition.wav` | 0.60–1.00 s | Short airy synth sweep and restrained star-like sparkle, gentle forward movement, no cinematic impact. |
| Habitat room tone / `habitat_loop.wav` | 8–12 s loop | Seamless quiet enclosed-space life-support hum, smooth filtered airflow and subtle low machinery, no speech, rhythm, alarms or outdoor wind; same energy at both boundaries. |

## Prompt for a coding agent: create the SFX locally

> Create an original procedural sound-effect pack for AstroCiv using Python 3 standard-library `wave`, `math`, `random`, `struct` and `json`, with no samples, downloads, API calls or paid dependencies. Implement the event filenames, target durations and sound descriptions in the SFX table of this file. Combine sine/triangle oscillators, pitch envelopes, filtered seeded noise and short amplitude envelopes. Use additive synthesis for bell notes and a low-passed noise burst for cushioned mechanical sounds. Use a deterministic seed and expose parameters in a manifest. Export 48 kHz 16-bit PCM WAV, mono for one-shots and stereo only where a loop benefits from it. Apply a few milliseconds of fade at each one-shot edge, remove DC offset, leave at least 3 dB of peak headroom, and prevent clipping. Build periodic room tone with matching phase/envelope at its loop boundary; measure the boundary and audition repeated playback. Write an HTML audition page with named play buttons and volume controls. Produce three variants per event and a manifest containing settings, seed, duration, sample rate, peak/RMS and origin. Add checks for valid headers, actual duration, non-silence, finite samples, clipping and loop discontinuity. Do not modify the game until a human has selected the variants. Report actual checks and avoid calling the files reviewed before listening.

## Prompt for a coding agent: sketch original lo-fi music locally

> Write a deterministic Python or Web Audio synthesizer for three original instrumental AstroCiv sketches using the music briefs above: menu 72 BPM, cutscene 86 BPM, gameplay 96 BPM. Compose fresh chord voicings and melodies; do not copy an artist, song or supplied recording. Use warm additive electric piano, soft drums, rounded bass and airy pads. Create 8- or 16-bar sections, a fixed random seed and a restrained mix. Export separate atmosphere, rhythm and melodic stems plus a stereo mix, preserving loop tails by wrapping them into the start rather than cutting them. Deliver lossless WAV files, an audition page, a manifest, and measured clipping/loop checks. Treat the result as a synthetic prototype requiring a musician/listening review; do not claim it matches studio-generated music. No game integration or external services.

## Editing, release and disclosure

- Listen on headphones and laptop speakers; compare cues at the intended in-game volume. Check repeated placement and warnings for fatigue.
- Use 2–5 ms one-shot edge fades and musical crossfades where appropriate. Keep music loop edits on bar boundaries and inspect tails.
- Store generation metadata beside each selected master. Record prompts/settings, tool/model, plan, permissions, seed/job ID, edits and human reviewer; retain vendor provenance metadata.
- The supplied submission guide requires AI-generated audio to be acknowledged in descriptive text **and metadata**. Also document effects produced through AI-authored synthesis code accurately. Do not describe proposed assets as used.
- If importing third-party tracks alongside original effects, keep their credits separate. Recheck the final release's licenses, disclosure and public-repository distribution conditions.
