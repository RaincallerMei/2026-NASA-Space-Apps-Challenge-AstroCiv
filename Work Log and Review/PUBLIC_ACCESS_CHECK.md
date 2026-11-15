# Public game access and publication record

Checked 15 November 2026. Initial anonymous check: approximately 1:25–1:27 a.m. Toronto time (EST); fresh-build verification: approximately 1:32–1:33 a.m. EST.

## Addresses

- [Public source repository](https://github.com/RaincallerMei/2026-NASA-Space-Apps-Challenge-AstroCiv).
- [Browser game](https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/).

## Initial findings and preparation

- GitHub's unauthenticated repository API confirmed `private: false` and default branch `main`.
- The public remote initially had only the initial checkpoint and an outdated README saying no playable game existed. The current working branch contains later game development.
- The game URL initially returned HTTP 404, and repository metadata reported Pages disabled.
- Enabled GitHub Pages with the existing authorized repository credential: `main`, `/docs`, legacy branch publishing. Credentials were kept in process memory and were not written to project files.
- Added a root README and a play link in the Unity-project README. Exempted `docs/Build/` from the general Unity build-output ignore rule so the WebGL payload is actually versioned.
- The existing WebGL snapshot contains about 69.8 MiB of files; its largest file is below GitHub's 100 MiB single-file limit. Its four hashed loader/data/framework/WASM references resolve locally, and its WASM header is valid.

## Final verification

**Public browser play works without sign-in.** The source was introduced in `ee7bb91`, and the successful fresh WebGL rebuild was pushed in `deae5017f4af38ce3a779e1bef59fa0e4d2993e1` to both `main` and `realtime-astrociv-mars`. The first hosted snapshot passed anonymous HTTP/browser checks; the fresh build includes the newer onboarding, wiki and interface changes. See the final recheck below for its public-access result.

| Anonymous check | Result |
|---|---|
| Public repository | Public; default branch `main` |
| Game HTML | HTTP 200, `text/html` |
| Hashed loader and framework JS | Both HTTP 200, `application/javascript` |
| Unity `.data` | HTTP 200, `application/octet-stream` |
| Unity `.wasm` | HTTP 200, `application/wasm` |
| Menu and Free Build setup | Rendered and accepted input |
| Mission | Start button entered the map; Start mission began the clock; time and resource values changed |
| Placement input | Selecting Habitat and attempting placement displayed boundary/placement validation; this check does not claim a completed valid placement or level |
| Runtime exceptions / browser error entries | 0 / 0 during the public smoke check |
| Login, Unity installation or player API key | None required for the public smoke check |

Evidence: [anonymous menu](Public%20Access%20Evidence/anonymous-menu.png), [running mission](Public%20Access%20Evidence/running-mission.png), [placement feedback](Public%20Access%20Evidence/placement-feedback.png), [build summary](Public%20Access%20Evidence/build-summary.json). These are screenshots of the deployed build, not newly generated game artwork. The README play link points to this tested address. Documentation-only follow-up commits record results without changing the tested Unity payload.

**Fresh-build recheck passed:** GitHub Pages reported `built` for `deae501`; the anonymous public HTML matched `docs/index.html` byte for byte. All four newly hashed payload URLs returned HTTP 200, with `.wasm` served as `application/wasm`, and public `build-info.json` matched the successful Unity build. A new isolated Chrome profile loaded the menu, opened Free Build, started the mission and displayed advancing time/resources, the new wiki button and the Information/Objectives panels. No runtime exceptions or browser error entries occurred. The retained screenshots now show this fresh deployment. [Machine-readable anonymous HTTP evidence](Public%20Access%20Evidence/anonymous-http-check.json).

The verified player-source checkpoint is `ee7bb91`, packaged in the fresh browser release `deae501`. Another local session continued changing source/UI and gameplay documentation after that build. Those later uncommitted edits are preserved separately and are not claimed as part of the tested deployment.

## Resolved build delay and current build evidence

The initial snapshot was from November 14 at about 10:53 p.m. Toronto time. CLI recompilation/test/build calls timed out or lost their connection, and a separate batch launch exited at licensing startup. A 600-second client timeout did **not** mean the already-started editor build had stopped: it eventually finished.

Unity's latest `Library/BuildHistory` summary records **Succeeded**, platform **WebGL**, started `2026-11-15T06:15:30Z`, duration **753,886 ms**, **0 errors**, **354 warnings**, session `0137f4adc28fa1b429bc1a2b43fddb98`. The output was written at approximately **1:28 a.m. EST on November 15**. A sanitized summary is checked in and served as [build-info.json](https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/build-info.json). The player-source changes were present before this build began; subsequent temporary Editor test-runner work is not player code and was excluded from publication.

Warnings include existing package shader-performance diagnostics and Unity dashboard service-link configuration. They did not fail the build; public browser checks remain necessary. The build is about **69.9 MiB** on disk and needs an initial download. No claim of zero warnings, complete campaign testing, mobile compatibility or human learning validation is made.

The saved editor recompile status reported completion without compile errors. An existing saved test status also reported six passing PlayMode smoke tests; that record alone does not establish which caller ran them. This publication's direct validation is the successful build, static payload/JSON checks, board browser checks and anonymous public game smoke test.

## How to reproduce the outside-visitor check

1. Open the repository while signed out and follow **Play the game in your browser** in the root README.
2. Use a fresh browser profile/private window with no GitHub credentials or existing game storage. Allow the Unity download to finish.
3. Confirm the menu renders, start Story Mode or Free Build, and verify a mission responds to input. No Unity installation or user API key should be needed.
4. Inspect console/network errors if loading fails. The hashed loader, `.data`, `.framework.js` and `.wasm` must all return HTTP 200. `.wasm` should be served as `application/wasm`.
5. Repeat on a second device/network for human confirmation. A successful automated smoke check is not a complete human campaign playtest or a guarantee for every browser, mobile device, GPU or managed network.

## Board verification

Headless Chrome at desktop 1280×960 and mobile 390×844: all six tabs exist; the submission tab displays; all eight draft fields exist; the run log starts collapsed and opens on activation; browser-local draft edits survive reload; no horizontal overflow at the checked widths; no JavaScript exceptions or browser error entries after the final layout check. The generated CDP Enter-key simulation did not toggle the native disclosure; click activation was verified. No keyboard result is claimed from that simulation. The disclosure uses native HTML `details`/`summary` controls.

The board's submission form is a local drafting aid. It does not submit to Space Apps, accept terms, publish a demo or prove team registration.
