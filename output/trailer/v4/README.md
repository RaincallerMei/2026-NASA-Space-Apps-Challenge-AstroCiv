# AstroCivilization - revised 29-second trailer

[Watch the MP4](../AstroCiv_Trailer_29s_v4.mp4) | [Contact sheet](contact-sheet.jpg) | [Credits and source records](CREDITS.md) | [Manifest](MANIFEST.json) | [Previous 29-second cut](../AstroCiv_Trailer_29s.mp4)

**29.00 seconds; 1920 x 1080; 30 fps; H.264/AAC; 10.47 MB.** Uses the supplied **Pastel Twinkles** soundtrack. Built from the user's revised scene instructions: the animated NASA poster over the home screen, 4x Moon/Mars construction, both Greenhouse resource cases, strategic placement, scrolling scientific achievements, Free Build and five moving construction maps. The full-composition ending says **You are the next explorer!** and begins at **27.50 seconds**, with a half-transparent caption panel.

| Time | Title |
|---|---|
| 0.00-3.00 s | Want to explore humanity's future on Moon and Mars? |
| 3.00-6.43 s | Build the first Moon base for human civilization! |
| 6.43-8.87 s | Balance power, water, air and food |
| 8.87-13.30 s | Switch buildings on and off to actively learn resource tradeoffs |
| 13.30-16.73 s | Place your building strategically |
| 16.73-22.67 s | Unlock all Science Achievements! |
| 22.67-24.60 s | Build your base on different Moon & Mars maps |
| 24.60-27.50 s | Build your base on different Moon & Mars maps |
| 27.50-29.00 s | You are the next explorer! |

The Moon and Mars invitation clips each last **1.50 seconds at 4x** and use gentle camera movement up to **5%**. Five different Free Build construction clips each have a **0.50-second** moving gameplay core at 4x; transitions are **two frames (0.0667 seconds)**, matching the requested rapid 0.07-second transition at 30 fps. Other scene transitions use short four-frame blends. The three seconds removed from the hook/ending were distributed across the seven middle sections in whole-frame increments (13 frames in six sections and 12 frames in the map montage).

Greenhouse OFF restores power while food falls; Greenhouse ON restores a positive food rate. The placement demonstration shows the selected Solar Array improving from 2.1 to 2.9 kW after a nearby Battery is built. The achievements scroll includes both earned facts and locked hints. Actual science citations remain visible. Gameplay was recorded continuously from an isolated current build/profile. No resource values, achievements or scores were fabricated.

The authentic NASA poster remains unaltered apart from normal framing/fade/scale. Game art and the supplied Suno track are disclosed. The full ending uses a derivative of the existing cutscene with classroom drawings in the monitor; see its saved asset and exact image-edit prompt in the credits.

Verified **870 decoded frames**, exactly **29.00 seconds**, audible unclipped soundtrack, and no sustained black frames. Audio mean -19.9 dBFS; peak -4.0 dBFS. Full-frame samples and the key action/result sequences were reviewed. Local file only; previous cuts remain available.

Rebuild with `python output/trailer/scripts/produce_trailer_v4.py`, then `python output/trailer/scripts/finalize_trailer_v4.py`. It uses the existing FFmpeg binary in `tmp/trailer-deps-v2/` and Pillow. This Windows workspace requires outside-sandbox access to the local encoder folder. Capture plans and source clips are preserved for further edits.
