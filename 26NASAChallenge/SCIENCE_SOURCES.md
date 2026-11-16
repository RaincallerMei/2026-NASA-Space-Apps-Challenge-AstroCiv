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

## Level 4 (Mars) and layout sources — added 14 November 2026

| Source | What it supports | Game simplification |
|---|---|---|
| [NSSDC Mars Fact Sheet](https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html) | Solar irradiance 586.2 W/m² (0.431 of Earth); day 24.6597 h; atmosphere 95.1% CO2 | Game uses 0.6 of Moon solar output (softened for play); no Mars night cycle |
| [JPL Mars 2020 press kit: landing site](https://www.jpl.nasa.gov/news/press_kits/mars_2020/landing/mission/landing_site/) | Jezero: about 18° N, 77° E, 45 km wide; held a lake about 3.5 billion years ago; ancient river delta | Lake mud (soil) and wet clay (water) tiles are hand-picked game features, not mapped mineralogy |
| [MGS MOLA MEGDR 16 px/deg (PDS Geosciences)](https://pds-geosciences.wustl.edu/mgs/mgs-m-mola-5-megdr-l3-v1/mgsl_300x/meg016/) | Real heights for the 20×15 Jezero crop (`mars_jezero_mola.csv`) | Steep tiles (>300 m step) are blocked; height → sunlight is a gameplay assumption |
| [JPL: MOXIE completes its mission (6 Sept 2023)](https://www.jpl.nasa.gov/news/nasas-oxygen-generating-experiment-moxie-completes-mars-mission/) | MOXIE made 122 g of oxygen in total, up to 12 g per hour | Card text only; no MOXIE building or rate in the sim |
| [NSSDC: Ice on the Moon](https://nssdc.gsfc.nasa.gov/planetary/ice/ice_moon.html) | LCROSS (2009) found water ice in a shadowed crater (Cabeus) | Game puts ice in every dark tile; real shadowed craters do not all hold ice |

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

## Science facts in the game: pages read 15 November 2026

Each fact pops up once, the first time its event happens (never in the Level 1 drawing). It stays in the Information panel (click it to open the Wiki) and in **Wiki → Science**, with its source. Text in bold is filled from real data at play time; the values shown are Story Mode's May 2024 storm. Free Build fills them from the real storm the player faces. Every trigger is tested to be an event the simulation really logs (`DataTests.ScienceFactTriggers_AreEventsTheSimulatorLogs`).

| Fact | When it pops up | Planet | Speaker | Text (Story Mode values) | Source | Label |
|---|---|---|---|---|---|---|
| `c_lowsun` A low Sun | Solar Array placed | Moon | Priya | At the Moon's poles, the Sun never climbs more than 1.5 degrees high. So panels go on high, bright ground. Brighter tiles here make more power. | [NASA Scientific Visualization Studio, south-pole shadows](https://svs.gsfc.nasa.gov/4893/) | NASA-derived |
| `c_ridge` No spot is always sunny | Battery placed | Moon | Priya | On a real south-pole map, our sunniest spot is lit 71% of the time. Even the best ridge has dark hours. Batteries carry the base through them. | [LRO LOLA average illumination map (Mazarico et al. 2011, Icarus 211), our 20 x 15 crop](https://pgda.gsfc.nasa.gov/products/69) | simplified from real map |
| `c_craters` Long shadows | Long shadow / dust storm starts | Moon | Priya | On NASA's polar map, some crater floors see sunlight under 4% of the time. The map was worked out from heights measured by LRO's LOLA laser. | [LRO LOLA average illumination map (Mazarico et al. 2011, Icarus 211), our 20 x 15 crop](https://pgda.gsfc.nasa.gov/products/69) | simplified from real map |
| `c_ice` Ice in the dark | Rover Garage placed | Moon | Yuki | Some craters near the Moon's poles never see the Sun. They can trap ice. In 2009, NASA's LCROSS found water ice in one, called Cabeus. | [NASA NSSDC, Ice on the Moon](https://nssdc.gsfc.nasa.gov/planetary/ice/ice_moon.html) | NASA-derived |
| `c_sun_home` Sun at home | Sunrise (Level 3 only) | Moon | Priya | Back home in Mississauga, June brings about five times more sun energy than December. NASA POWER: June gets about 6.1 kWh per square metre daily; December 1.2. | [NASA POWER climatology, Mississauga, 2001 to 2025](https://power.larc.nasa.gov/api/temporal/climatology/point?parameters=ALLSKY_SFC_SW_DWN&community=RE&latitude=43.59&longitude=-79.64&format=JSON) | NASA-derived |
| `c_mars_sun` Less sunlight on Mars | Solar Array placed | Mars | Priya | Sunlight on Mars is about 43% as strong as on Earth. So a Mars base needs more panels for the same power. | [NASA NSSDC Mars Fact Sheet](https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html) | NASA-derived; game power is softened |
| `c_sol` A Mars day | Battery placed | Mars | Priya | A Mars day, called a sol, lasts about 24 hours and 40 minutes. Panels rest at night, so batteries matter. This game skips Mars nights. | [NASA NSSDC Mars Fact Sheet](https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html) | NASA-derived |
| `c_moxie` Making oxygen from Mars air | Air Support placed | Mars | Priya | Mars air is 95% carbon dioxide. NASA's MOXIE made oxygen from it. At its best it made 12 grams an hour, and 122 grams in all. | [NASA JPL, MOXIE completes Mars mission (6 Sept 2023); NASA NSSDC Mars Fact Sheet](https://www.jpl.nasa.gov/news/nasas-oxygen-generating-experiment-moxie-completes-mars-mission/) | NASA-derived |
| `c_jezero` Jezero crater | Rover Garage placed | Mars | Yuki | NASA's Perseverance landed in Jezero crater in 2021. It once held a lake. This map uses real heights from the MOLA laser on Mars Global Surveyor. | [NASA JPL Mars 2020 press kit; MGS MOLA MEGDR (NASA PDS)](https://www.jpl.nasa.gov/news/press_kits/mars_2020/landing/mission/landing_site/) | simplified from real map |
| `c_dust` Dust storms | Long shadow / dust storm starts | Mars | Priya | In 2018, a Mars-wide dust storm covered NASA's solar-powered Opportunity rover. Built for 90 Mars days, it worked for over 15 years before going silent. | [NASA news release, Opportunity mission comes to end (13 Feb 2019)](https://www.nasa.gov/news-release/nasas-record-setting-opportunity-rover-mission-on-mars-comes-to-end/) | NASA-derived |
| `c_air` Making air | Air Support placed | Moon | Priya | The Space Station makes oxygen by splitting water into oxygen and hydrogen. It also cleans and reuses about 90% of its water. | [NASA, Environmental Control and Life Support Systems (updated 4 April 2025)](https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/) | NASA-derived |
| `c_radio` Talking across space | Radio Dish placed | both | Yuki | A radio call reaches the Moon in about 1.3 seconds. From Mars, one way can take more than 20 minutes. | [NASA NSSDC Moon Fact Sheet (384,400 km at the speed of light); NASA Moon to Mars white paper (2023)](https://www.nasa.gov/wp-content/uploads/2024/01/mars-communications-disruption-and-delay.pdf) | NASA-derived; Moon time worked out |
| `c_rock` Tiny space rocks | Space rock breaks a panel (Free Build, Normal/Hard only) | both | Priya | Photos of the Space Station show thousands of hits from tiny space particles. Engineers keep testing better shields to protect spacecraft. | [NASA Johnson, Hypervelocity Impact Technology team](https://hvit.jsc.nasa.gov/) | NASA-derived |
| `c_veggie` Space salad | Greenhouse placed | both | Mateo | Space Station crews have grown lettuce, mizuna mustard and dwarf tomatoes. Scientists checked the space lettuce for germs and nutrients. | [NASA Veg-04 and Veg-05 investigation page; NASA OSDR studies OSD-268 and OSD-767](https://science.nasa.gov/biological-physical/investigations/veg-05) | NASA-derived |
| `c_saint_jacques` A Canadian space gardener | First harvest | both | Mateo | Canadian astronaut David Saint-Jacques filled the water tank of a space plant system. It was the Veggie PONDS system, on the International Space Station. | [Canadian Space Agency, Growing healthy food in space (photo caption). � Canadian Space Agency](https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp) | CSA-derived |
| `c_plants` Why grow plants in space? | Button only (briefing or harvest window) | both | Mateo | Plants can provide food, produce oxygen, and recycle carbon dioxide. Over three million students took part in Tomatosphere from 2001 to 2016. | [Canadian Space Agency, Tomatosphere. � Canadian Space Agency](https://www.asc-csa.gc.ca/eng/youth-educators/tomatosphere.asp) | CSA-derived |
| `c_far` Far from the grocery store | Supply lander | both | Mateo | On the Moon or Mars, astronauts will need to be more self-sufficient. Supply trips will be rare, so crews must grow food. | [Canadian Space Agency, Growing healthy food in space. � Canadian Space Agency](https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp) | CSA-derived |
| `c_body` Staying strong | Habitat placed | both | Mission control | Bodies lose muscle and bone in space. Mice on the Station helped study it. One study tested a way to protect them: OSDR study OSD-342. | [NASA Open Science Data Repository, OSD-342](https://osdr.nasa.gov/bio/repo/data/studies/OSD-342) | NASA-derived |
| `c_shelter` Why shelter? | Shelter placed | both | Yuki | Space radiation comes from the Sun's particles and from cosmic rays. It can damage DNA inside our cells. Shielding helps protect the crew. | [NASA Human Research Program, About the Space Radiation Element (31 July 2024)](https://www.nasa.gov/reference/about-the-space-radiation-element/) | NASA-derived |
| `c_hera` A radiation alarm | Solar storm warning | both | Mission control | A NASA sensor called HERA can warn crews to take shelter from solar flares. It flew inside Orion on Artemis I, alongside 5,600 small passive sensors. | [NASA, Artemis I radiation measurements validate Orion safety (9 Oct 2024); George et al. 2024, Nature 634, 48-52](https://www.nasa.gov/humans-in-space/artemis-i-radiation-measurements-validate-orion-safety-for-astronauts/) | NASA-derived |
| `c_iss` A real reading from the Space Station | Storm particles arrive | both | Priya | On 11 May 2024, a Space Station sensor read about **4.1** microgray an hour. The highest was **28**. Station readings are not Moon doses. | [NASA OSDR RadLab, ISS DosTel, 11 May 2024, 00:00 to 06:00 UTC](https://visualization.osdr.nasa.gov/radlab/gui/data-api/) | NASA-derived |
| `c_cme` A fast cloud | Solar storm warning | both | Mission control | This storm's real cloud of solar gas flew at **1263** km per second. That fast, it could cross from Earth to the Moon in **5** minutes. | [NASA CCMC DONKI (CME analysis); NASA NSSDC Moon Fact Sheet](https://ccmc.gsfc.nasa.gov/tools/DONKI/) | NASA-derived; time worked out |
| `c_sep` Particles arrive | Storm particles arrive | both | Yuki | Real data: fast particles were logged **47 minutes** after the flare's peak. Our 8 real storms ranged from 16 minutes to almost 3 days. | [NASA CCMC DONKI (linked SEP events)](https://ccmc.gsfc.nasa.gov/tools/DONKI/) | NASA-derived |
| `c_kp` Storm on Earth | Storm over | both | Priya | Around this storm, Earth's Kp storm scale reached **9**. It tops out at 9. Strong storms on Earth can paint the sky with auroras. | [NASA CCMC DONKI (geomagnetic storm); NASA Science, the most intense solar storm in decades (16 May 2024)](https://science.nasa.gov/science-research/heliophysics/how-nasa-tracked-the-most-intense-solar-storm-in-decades/) | NASA-derived |

### Checks and corrections made while verifying

- **Corrected an existing card:** `c_ridge` said nearby crater floors are "never lit". The real map crop's darkest tile is lit 3.6% of the time, so the card now says under 4%. (Permanently shadowed craters do exist elsewhere; `c_ice` keeps that NSSDC-sourced wording.)
- **Greenhouse vegetables:** the CSA food-production page names no specific vegetables. The crops come from NASA's Veg-04/Veg-05 page (mizuna mustard, dwarf tomatoes) and OSDR OSD-268 (lettuce). The CSA link is the photo caption of David Saint-Jacques filling the Veggie PONDS reservoir (page modified 2026-07-24).
- **Only one Greenhouse is allowed** (`maxCount` 1), so the Saint-Jacques fact uses the first harvest instead of a second Greenhouse.
- **Space rock (`fault`)** exists only in Free Build on Normal and Hard. **Sunrise** exists only in Level 3. **Long shadow** is called **Dust storm** on Mars, so it has a Moon fact and a Mars fact.
- **Moon maps:** Tutorial 2 and Free Build use made-up maps, so Moon facts describe "a real south-pole map", never "this map".
- **Radio delay:** 1.3 s is worked out (384,400 km at the speed of light). Mars uses NASA's "upward of 21-23 minutes" one way, worded "more than 20 minutes".
- **HERA:** NASA says HERA flew inside Orion on Artemis I and "can warn crew members need to take shelter". The 5,600 sensors are passive sensors; the fact says so.
- **ISS reading:** 11 May 2024, 00:00-06:00 UTC median 4.14 and maximum 28.2 microgray per hour. The fact says these are not Moon doses.
- **Particle gap:** across the 8 storms the first linked particles came 16 minutes to 2.6 days after the peak. The fact says "16 minutes to almost 3 days", and a test ties that sentence to the fixture.
- **Not used:** a Tomatosphere "per year" count, Artemis mannequin dose numbers (found only in press coverage, not on the NASA page), and any lunar dose prediction.

| Page read | What it supports |
|---|---|
| [NASA Veg-05 investigation](https://science.nasa.gov/biological-physical/investigations/veg-05) (updated 22 May 2023) | Veg-04 mizuna mustard; Veg-05 dwarf tomatoes |
| [CSA: Growing healthy food in space](https://www.asc-csa.gc.ca/eng/sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp) (modified 2026-07-24) | Saint-Jacques and Veggie PONDS caption; self-sufficiency |
| [NASA ECLSS](https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/) (updated 4 Apr 2025) | Oxygen by electrolysis; about 90% water recovered; CO2 sieves |
| [NASA NSSDC Moon Fact Sheet](https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html) | 384,400 km semimajor axis (radio time, CME comparison) |
| [NASA Moon to Mars: communications disruption and delay (2023)](https://www.nasa.gov/wp-content/uploads/2024/01/mars-communications-disruption-and-delay.pdf) | Mars one-way delay upward of 21-23 minutes |
| [NASA HRP: About the Space Radiation Element](https://www.nasa.gov/reference/about-the-space-radiation-element/) (31 Jul 2024) | Sun particles and cosmic rays; DNA damage; shielding research |
| [NASA: Artemis I radiation measurements](https://www.nasa.gov/humans-in-space/artemis-i-radiation-measurements-validate-orion-safety-for-astronauts/) (9 Oct 2024) | HERA warning; 5,600 passive and 34 active sensors; varies by location; orientation halved dose; Helga and Zohar |
| [George et al. 2024, Nature 634, 48-52](https://doi.org/10.1038/s41586-024-07927-7) | The peer-reviewed Artemis I paper the NASA article cites (DOI resolves; full text not read) |
| [NASA: Opportunity mission ends](https://www.nasa.gov/news-release/nasas-record-setting-opportunity-rover-mission-on-mars-comes-to-end/) (13 Feb 2019) | 2018 Mars-wide dust storm; designed for 90 Martian days; over 15 years |
| [NASA JSC Hypervelocity Impact Technology](https://hvit.jsc.nasa.gov/) | Thousands of impacts seen on ISS hardware; shielding work |
| [NASA Science: the most intense solar storm in decades](https://science.nasa.gov/science-research/heliophysics/how-nasa-tracked-the-most-intense-solar-storm-in-decades/) (16 May 2024) | Strongest storm in two decades; auroras in the southern U.S. |
| [NASA RadLab data API](https://visualization.osdr.nasa.gov/radlab/gui/data-api/) | Instrument list has no Orion or Artemis series |
