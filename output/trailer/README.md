# AstroCiv — 28-second gameplay trailer

[Watch the MP4](AstroCiv_Trailer_28s.mp4) · [Credits](CREDITS.md) · [Shot and validation manifest](TRAILER_MANIFEST.json) · [Contact sheet](trailer-contact-sheet.jpg)

**28.00 seconds**, 1280×720, 30 fps, H.264 video and AAC stereo audio. The file is about 3.4 MB. It is a local draft; no upload or event submission has been performed.

| Time | Recorded shot / edit |
|---|---|
| 0–3 s | Actual main menu: “Your crew. Your call.” |
| 3–7 s | Actual launch cutscene: “A dream becomes a mission.” |
| 7–13 s | Habitat / Solar Array / Greenhouse placement: “Build a home beyond Earth.” |
| 13–18 s | Crew introduction: “Every building supports someone.” |
| 18–24 s | Running commander mission, resources and inspection clock: “Balance resources. Plan ahead.” |
| 24–28 s | Closing title, play invitation, music/art disclosure |

The soundtrack is the supplied **Starry Twinkles** by **raincalleer / Suno**. The edit uses seconds 18–46 of that file, with loudness processing and opening/closing fades. No new music or synthetic gameplay footage was generated. Original game audio was not recorded; the final soundtrack is the selected music.

The source footage was recorded from the real Unity WebGL game with mouse-driven actions in a fresh Chrome profile. Only its canvas was captured, not the user's desktop. Raw clips remain in `raw/`. Where a browser recording ended slightly early, the editor can hold the final recorded frame to meet the shot duration; it does not invent gameplay.

Validation: decoded all **840 frames**, verified **28.00 seconds**, H.264/AAC streams and non-silent audio. Measured audio mean −20.7 dBFS and peak −5.7 dBFS. Reviewed the contact sheet for framing, captions and coverage. Human playback/listening approval is not recorded; review the MP4 before using it as a formal submission demo.

The game build used for capture is identified in the manifest. The trailer does not claim to show every later unbuilt source change or every mission outcome. It deliberately does not claim a complete campaign playthrough.

Capture/edit source scripts and shot plans are retained in `scripts/` for this workspace. The editor expects an FFmpeg executable under the temporary dependency directory used in this session; adjust that path for another machine. Free manual workflow: record short takes in OBS, import them and the original MP3 into Shotcut, trim the timeline to 28 seconds, apply audio fades, and export H.264/AAC MP4. See the [research/workflow note](../../Work%20Log%20and%20Review/TRAILER_WORKFLOW.md) for official links.

For formal submission, review required team/member names, retain music permissions and AI indicators, then host the final approved video somewhere judges can open without signing in. Creating this file has not done those publication/submission steps.
