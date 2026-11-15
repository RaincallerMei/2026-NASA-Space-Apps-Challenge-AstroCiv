# Public game access and publication record

Checked 15 November 2026. This record is being finalized as part of publication.

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

Pending final push, deployment and anonymous browser check. Do not treat configuration alone as proof the game runs for an outside visitor.

**Build freshness limitation:** the available WebGL snapshot was written November 14, 2026 at about 10:53 p.m. Toronto time. Source files changed afterward, including onboarding work arriving during this session. The existing snapshot loaded its Unity main menu in a fresh local Chrome profile without JavaScript exceptions or browser error entries. It must not be described as a rebuild of the latest source unless a later successful build is recorded here. Current Unity CLI attempts have timed out or lost their editor connection; the separate batch attempt exited during license startup. Existing saved test status reported six passing PlayMode smoke tests, but that status alone does not establish when/which caller ran them or validate the published snapshot.

## How to reproduce the outside-visitor check

1. Open the repository while signed out and follow **Play the game in your browser** in the root README.
2. Use a fresh browser profile/private window with no GitHub credentials or existing game storage. Allow the Unity download to finish.
3. Confirm the menu renders, start Story Mode or Free Build, and verify a mission responds to input. No Unity installation or user API key should be needed.
4. Inspect console/network errors if loading fails. The hashed loader, `.data`, `.framework.js` and `.wasm` must all return HTTP 200. `.wasm` should be served as `application/wasm`.
5. Repeat on a second device/network for human confirmation. A successful automated smoke check is not a complete human campaign playtest or a guarantee for every browser, mobile device, GPU or managed network.

## Board verification

Headless Chrome at desktop 1280×960 and mobile 390×844: all six tabs exist; the submission tab displays; all eight draft fields exist; the run log starts collapsed and opens on activation; browser-local draft edits survive reload; no horizontal overflow at the checked widths; no JavaScript exceptions or browser error entries after the final layout check. The generated CDP Enter-key simulation did not toggle the native disclosure; click activation was verified. No keyboard result is claimed from that simulation. The disclosure uses native HTML `details`/`summary` controls.

The board's submission form is a local drafting aid. It does not submit to Space Apps, accept terms, publish a demo or prove team registration.
