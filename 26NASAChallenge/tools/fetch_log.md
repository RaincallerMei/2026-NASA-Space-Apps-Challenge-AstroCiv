# Fetch log (written by tools/fetch_data.py)

| Source | Query URL (key removed) | HTTP status | Date (UTC) | Units | Fixture | Classification | Note |
|---|---|---|---|---|---|---|---|
| DONKI FLR via api.nasa.gov | `https://api.nasa.gov/DONKI/FLR?startDate=2024-05-10&endDate=2024-05-12` | 404 | 2026-11-14T15:51:58Z | event metadata | - | not used |  |
| DONKI FLR via CCMC | `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2024-05-10&endDate=2024-05-12` | 200 | 2026-11-14T15:51:58Z | event metadata | donki_event_pack.json | NASA-derived |  |
| DONKI CME via api.nasa.gov | `https://api.nasa.gov/DONKI/CME?startDate=2024-05-10&endDate=2024-05-12` | 200 | 2026-11-14T15:51:58Z | event metadata | - | NASA-derived |  |
| DONKI SEP via api.nasa.gov | `https://api.nasa.gov/DONKI/SEP?startDate=2024-05-10&endDate=2024-05-12` | 200 | 2026-11-14T15:51:58Z | event metadata | - | NASA-derived |  |
| DONKI GST via api.nasa.gov | `https://api.nasa.gov/DONKI/GST?startDate=2024-05-10&endDate=2024-05-12` | 429 | 2026-11-14T15:51:58Z | event metadata | - | not used |  |
| DONKI GST via CCMC | `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/GST?startDate=2024-05-10&endDate=2024-05-12` | 200 | 2026-11-14T15:51:58Z | event metadata | donki_event_pack.json | NASA-derived |  |
| RadLab ISS dose rate | `https://visualization.osdr.nasa.gov/radlab/api/?spacecraft=ISS&instrument=DosTel&timestamp%3E=2024-05-11T00:00&timestamp%3C=2024-05-11T06:00&absorbed_dose_rate&format=csv` | 200 | 2026-11-14T15:51:58Z | absorbed_dose_rate in microgray per hour (uGy/h) | radlab_iss_sample.csv | NASA-derived | 408 rows |
| LOLA polar illumination | `http://imbrium.mit.edu/EXTRAS/ILLUMINATION/IMG/AVGVISIB_85S_060M_201608.IMG` | 206 | 2026-11-14T15:51:58Z | percent of time the tile is sunlit (average solar disc visibility), 0-100 | illumination_grid.csv | simplified | real map, cropped and averaged per tile |
| NASA POWER Mississauga | `https://power.larc.nasa.gov/api/temporal/climatology/point?parameters=ALLSKY_SFC_SW_DWN&community=RE&latitude=43.59&longitude=-79.64&format=JSON` | 200 | 2026-11-14T15:51:58Z | kW-hr/m^2/day | power_mississauga.json | NASA-derived | monthly climatology |
| OSDR study cards | `https://osdr.nasa.gov/osdr/data/search?type=cgene&size=5&term=<accession>` | 200 | 2026-11-14T15:51:58Z | text | osdr_cards.json | NASA-derived | 8 studies |
| CSA food cards | `https://www.asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/` | 200 | 2026-11-14T15:51:58Z | text | csa_food_cards.json | CSA-derived | 3 pages |

## Run 2026-11-16T02:05:09Z: fetch_donki_storms, fetch_osdr

| Source | Query URL (key removed) | HTTP status | Date (UTC) | Units | Fixture | Classification | Note |
|---|---|---|---|---|---|---|---|
| DONKI FLR scan via CCMC | `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2021-01-01&endDate=2025-12-31 (in 60-day windows)` | 200 | 2026-11-16T02:05:09Z | event metadata | donki_storm_set.json | NASA-derived | 2476 flares |
| DONKI storm set | `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR?startDate=2021-01-01&endDate=2025-12-31 (in 60-day windows)` | 200 | 2026-11-16T02:05:09Z | event metadata | donki_storm_set.json | NASA-derived | 8 storms |
| OSDR study cards | `https://osdr.nasa.gov/osdr/data/search?type=cgene&size=5&term=<accession>` | 200 | 2026-11-16T02:05:09Z | text | osdr_cards.json | NASA-derived | 12 studies |
