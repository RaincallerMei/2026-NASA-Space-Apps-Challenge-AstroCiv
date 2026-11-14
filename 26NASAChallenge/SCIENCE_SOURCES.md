# Science sources

Primary pages read on **2026-11-14**. This pack verifies the existing plan; it does not select new mechanics or rewrite the story. Saved-query results and dates remain in DATA_SOURCES.md and tools/fetch_log.md.

| Source | Verified fact / existing gameplay use | Constraint |
|---|---|---|
| [CSA: Chris Hadfield biography](https://www.asc-csa.gc.ca/eng/astronauts/canadian/retired/bio-chris-hadfield.asp) | Ontario connection and ISS command support the existing inspiration line | Names only; no portrait, quote or endorsement |
| [Sara Seager: personal biography](https://www.saraseager.com/personalbiography) | Born and grew up in Toronto; supports the existing local story | Her career is not the fictional protagonist's biography |
| [Sara Seager: professional biography](https://www.saraseager.com/professionalbiography) | Confirms exoplanet research and former TESS Deputy Science Director role | Use former Deputy, not overall TESS director |
| [CSA: astronaut requirements](https://www.asc-csa.gc.ca/eng/astronauts/how-to-become-an-astronaut/requirements-and-conditions.asp) | Education and relevant experience support the dream-path disclaimer | A degree does not guarantee selection |
| [NASA: ECLSS](https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/) | Air and water systems support the life-support learning objective | Game air-hours and recycling percentages are fictional balance |
| [NASA CCMC: DONKI](https://ccmc.gsfc.nasa.gov/tools/DONKI/) | Event database and related-event metadata support the saved storm alert | Research/prototyping data; not a lunar-dose prediction |
| [NASA RadLab: API fields](https://visualization.osdr.nasa.gov/radlab/gui/data-api/) | `absorbed_dose_rate` is in microgray/hour; supports the existing ISS comparison card | Absorbed dose differs from equivalent dose; ISS values are not Moon values |
| [NASA LOLA: polar illumination](https://pgda.gsfc.nasa.gov/products/69) | Long-term illumination products support the saved tile map | Averaged illumination is not a live shadow timeline |
| [NASA SVS: south-pole shadows](https://svs.gsfc.nasa.gov/4893/) | Sun stays near the horizon at the poles; supports the existing shadow card | A time-compressed sunrise/shadow sequence is a game simplification |
| [NASA POWER: climatology API](https://power.larc.nasa.gov/docs/services/api/temporal/climatology/) | Confirms the climatology service used by the Mississauga fixture | Earth-surface data; units and values come from the saved response |
| [NASA: OSDR overview](https://science.nasa.gov/biological-physical/data/osdr/) | Repository for space-life-science data supports linked study cards | Individual study titles do not establish game crop rates |
| [CSA: Tomatosphere](https://www.asc-csa.gc.ca/eng/youth-educators/tomatosphere.asp) | Supports student participation and plants' food/oxygen roles in the existing card | The reported three-million count describes 2001–2016 participation, not a current annual count |
| [NASA: media guidelines](https://www.nasa.gov/nasa-brand-center/images-and-media/) | Attribution and no-endorsement rules inform credits | Third-party material has separate rights; logos are not general-purpose game branding |
| [CSA: terms](https://www.asc-csa.gc.ca/eng/terms.asp) | Educational non-commercial reuse requires accuracy, credit and no endorsement | Commercial redistribution and third-party content need separate permission checks |

## Resolved checks

- **S1 / Hadfield:** verified from CSA: Ontario connection, Canadian astronaut, ISS commander.
- **S2 / Seager:** verified from her own pages: Toronto origin, exoplanet work, former TESS Deputy Science Director.
- **S3 / Ontario inspiration:** supported by the two separate biographies. No endorsement follows from this.
- **S4 / career:** requirements verified; the fictional degree-to-astronaut sequence stays explicitly aspirational.
- **S6 / polar lighting:** verified general polar geometry; exact outpost sunrise timing is not established by the average map.
- **S8 / greenhouse:** general plant benefits verified. The game's 80% recycling and crop production remain fictional.
- **RadLab units:** official API table confirms microgray/hour for absorbed dose rate. The earlier fetch attempt's HTTP 500 is historical, not an unresolved unit check now.
- **CSA reuse wording:** official terms were read and match educational non-commercial attribution/no-endorsement conditions. This is a record of source terms, not permission to use logos or third-party media.

## Still open / unchanged

- **S5 / Earth low at the exact site:** no site-specific viewing calculation was performed. Keep the existing approved wording; Opus/team must check its scene geometry before final art.
- **S7 / radio delay:** later scope; no new mechanic implemented.
- **S9 / teacher, event, crew:** fictional, as already stated in Story.json.
- The supplied [2026 challenge URL](https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/) returned a fetch error (502) in this check. The subsequently supplied Project Submission Guide PDF establishes submission limits recorded in SUBMISSION_REQUIREMENTS.md. The separate judging guide is still unread, so the board's combined guide-reading task stays open.
- The legacy MIT TESS team URL also returned 502; Seager's own professional biography supplies the verified former-role statement instead.
