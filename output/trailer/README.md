<!-- trailer-v4-current -->
**Latest: [revised 29-second trailer](AstroCiv_Trailer_29s_v4.mp4)** | [Timings, credits and validation](v4/README.md) | [Contact sheet](v4/contact-sheet.jpg). Earlier cuts and their historical notes follow.

---

<!-- artemis-29s-current -->
**Previous version: [29-second Artemis II trailer](AstroCiv_Trailer_29s.mp4)** | [Credits, shots and validation](README_29S.md). Earlier cut and notes follow.

---

# AstroCiv — recreated 28-second gameplay trailer

[Watch the MP4](AstroCiv_Trailer_28s.mp4) · [Previous cut](AstroCiv_Trailer_28s_previous.mp4) · [Credits](CREDITS.md) · [Shot manifest](TRAILER_MANIFEST.json) · [Contact sheet](trailer-contact-sheet.jpg)

**28.00 seconds**, 1280×720, 30 fps, H.264 video and AAC stereo audio; **3.64 MB**. Recreated 15 November 2026 to sell the adventure, show science being used in play, celebrate an earned achievement, and connect the Ontario story to curiosity and future problem-solving.

The opening cuts from the Moon to Mars. Real placement footage shows an outpost taking shape. A close view of the NASA sunlight card makes the knowledge legible; the greenhouse connects resources to food. The storm warning cites NASA DONKI, followed by actual shelter/recall actions. The result is an earned **S, 98/100, three stars, 100% crew safety**, followed by the commander connecting with a classroom on Earth. The closing card invites viewers to play and gives the browser-game address.

| Time | On-screen pitch | Recorded source |
|---|---|---|
| 0–1.2 s | WANT TO EXPLORE THE MOON & MARS? | v2_moon_hero.webm |
| 1.2–2.4 s | WANT TO EXPLORE THE MOON & MARS? | v2_mars.webm |
| 2.4–6.6 s | BUILD A HOME BEYOND EARTH. | v2_building.webm |
| 6.6–10 s | REAL NASA DATA. SMARTER DECISIONS. | v2_science_sun.webm |
| 10–12.8 s | GROW FOOD. KEEP YOUR CREW GOING. | v2_greenhouse.webm |
| 12.8–16.2 s | SOLAR STORM. YOUR CREW NEEDS YOU. | v2_storm_warning.webm |
| 16.2–18.6 s | PROTECT PEOPLE. THINK AHEAD. | v2_storm_response.webm |
| 18.6–21.4 s | BUILD. TEST. EARN YOUR MOMENT. | v2_storm_report.webm |
| 21.4–24.4 s | BIG DREAMS START IN ONTARIO. | v2_earth_ending.webm |
| 24.4–28 s | YOUR CREW NEEDS YOU. PLAY ASTROCIV. | v2_earth_ending.webm |

Captions and the existing **Starry Twinkles** soundtrack carry the pitch; no voiceover was generated or recorded. Captions sit below the gameplay so building cards, resources and objectives remain visible. The NASA card is cropped closer for readability. Some gameplay is sped up and waiting time is removed; the manifest records source offsets and speeds. All visual source material is captured from the real game. Game balance and storm timing remain simplified educational rules.

Capture used the local `docs/` WebGL build in a fresh, isolated Chrome profile. To reach the requested shots, prerequisite mission-selection flags were set in that capture profile; best scores were never edited. The storm mission used its own native fallback starter base, then was played through: recall the geologist, power the shelter, rest the greenhouse while the battery was low, restore food production and reach inspection. Tutorial 1 also earned S 93/100 in a separate recording. This is a montage of working screens, not a claim to show an uninterrupted campaign playthrough. The user's normal browser/game saves were not used.

Validation decoded all **840 frames**, confirmed **28.00 seconds**, H.264/AAC streams, non-silent audio and no sustained black frames. Audio mean **-20.7 dBFS**, peak **-5.6 dBFS**. The contact sheet samples every edited shot. Source hashes and WebGL build hashes are in the manifest; render/validation logs are under `tmp/trailer/edit-v2/` in this workspace. Human listening/playback approval is not recorded.

The previous MP4 is preserved. Raw recordings and capture plans remain available for further edits. Rebuild with `python output/trailer/scripts/recreate_trailer.py` from the repository root; FFmpeg is supplied by imageio-ffmpeg 0.8.1 under `tmp/trailer-deps-v2/` (or the earlier `tmp/trailer-deps/` fallback); Pillow assembles the review sheet. In this Windows workspace, the encoder's installed folder requires running the script outside the sandbox. The finalizer updates these notes and the Submission desk after rendering. No public upload or challenge submission was performed.
