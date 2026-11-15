# Automatic gameplay trailer capture

Research and production record, 15 November 2026.

## Can GPT or Claude do this?

Yes, when connected to a computer/browser controller, a recorder and an editor. The language model plans shots and directs actions; the recording software captures real frames, and the editor trims and mixes them. A normal text-only chat has no automatic access to your game or screen. A computer-use API still needs an application to execute the requested actions.

- [OpenAI computer use](https://developers.openai.com/api/docs/guides/tools-computer-use) documents browser/desktop control and the environment/driver that executes actions.
- [Claude computer use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/computer-use-tool) documents screenshots and mouse/keyboard interaction in a controlled environment.
- [Playwright video recording](https://playwright.dev/docs/videos) provides automatic browser recording, saved when the browser context closes. It is useful for a repeatable browser-game workflow.
- [Canvas captureStream](https://developer.mozilla.org/en-US/docs/Web/API/HTMLCanvasElement/captureStream) captures a canvas as an actual video stream; MediaRecorder can save it. This project's production attempt uses the existing Unity WebGL build in an isolated Chrome profile and records only the game canvas.
- [FFmpeg](https://ffmpeg.org/ffmpeg.html) performs trimming, video encoding and audio mixing. No generative video model is necessary to make a trailer from real footage.

## This trailer's brief

- Target: **28.0 seconds**, under both the user's 29-second ceiling and the supplied submission guide's 30-second ceiling.
- Show the real main menu, building/gameplay interactions, selected cutscenes and a short closing hook.
- Music: the exact supplied `Art/BGM/Starry Twinkles.mp3`, edited as an excerpt with fades. No replacement soundtrack or new audio generation.
- Preserve the game's generated-art indicators. The final file's description/metadata also acknowledges the Suno-generated music and AI-assisted recording/editing.
- Capture only the game in a fresh browser profile. Do not record the user's desktop, account pages or other applications. Do not change the real player's saved progress.
- Use actual current WebGL visuals. Trailer captions can summarize mechanics; they must not imply features or results absent from the captured game.

## Music provenance

The supplied MP3's embedded tags identify title **Starry Twinkles**, artist **raincalleer**, Suno song ID `f5182621-6979-42be-864b-69517b18be60`, and [this song page](https://suno.com/song/f5182621-6979-42be-864b-69517b18be60). Embedded comments explicitly say it was made with Suno. The track is approximately 203.16 seconds long. Its generation model, prompt and account tier are not recorded here; do not infer them from the filename or artist tag. This session records/edits gameplay and uses the provided music; it does not generate new music.

For a public release, the team should retain the download/usage rights applicable to its Suno account. The local trailer draft does not establish a broader license. Source tags are retained in the original file; the exported video adds audio-generation acknowledgement in its metadata and companion credits.

## Free software if you want to revise or recapture manually

| Tool | Use | Official source |
|---|---|---|
| OBS Studio (already installed) | Record the game/browser window; Game Capture for a compatible native game | [Quick start](https://obsproject.com/kb/quick-start-guide), [download](https://obsproject.com/download) |
| Shotcut | Free/open-source timeline editor on Windows; trim, captions, fades and soundtrack | [Official downloads](https://shotcut.org/download/) |
| FFmpeg | Free command-line editing and export; good for repeatable agent-driven cuts | [Official downloads](https://ffmpeg.org/download.html), [documentation](https://ffmpeg.org/ffmpeg.html) |
| Playwright | Free browser automation and video recording; can be driven by agent-written scripts | [Video guide](https://playwright.dev/docs/videos) |

For a browser build in OBS, start with **Window Capture** for the browser and crop to the game; use Game Capture when recording a compatible native game. Choose 1280×720 or 1920×1080 at 30 fps, disable microphone recording unless wanted, and record a few short takes. Add Starry Twinkles on the editor's audio track, trim the timeline to 28 seconds, fade the soundtrack at both ends, and export H.264 MP4 with AAC audio. Verify the exported duration rather than relying only on timeline labels.

## Review before using this as a formal submission demo

Review the final video and music level. Add verified team/member names if required by the actual submission form/guide, confirm the final music rights, retain AI indicators and source credits, and test any eventual public video link without signing in. Producing a local MP4 does not upload it or submit the project.

The final output, shot timings and measured validation are recorded in `output/trailer/` when production completes. Intermediate recordings are kept as real video clips for re-editing.
