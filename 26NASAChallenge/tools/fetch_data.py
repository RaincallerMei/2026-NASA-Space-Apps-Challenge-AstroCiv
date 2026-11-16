#!/usr/bin/env python3
"""Fetch NASA and CSA data once and bake small offline fixtures for Mapmakers of the Moon.

The game never calls these services. It reads only the files written to
Assets/_Game/Data/Resources/Fixtures/.

Key safety: the NASA key is read from the NASA_API_KEY environment variable only
(falls back to DEMO_KEY). It is never printed and never written to any file.

Usage:  python tools/fetch_data.py
Standard library only. Safe to run again: a real fixture is never replaced by a
synthetic one when a later fetch fails.
"""
import array
import csv
import datetime
import io
import json
import math
import os
import re
import statistics
import sys
import urllib.error
import urllib.parse
import urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "_Game", "Data", "Resources", "Fixtures")
API_KEY = os.environ.get("NASA_API_KEY") or "DEMO_KEY"
NOW = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
UA = {"User-Agent": "MapmakersOfTheMoon-fixture-fetch/1.0 (educational game)"}
LOG = []

NASA_NOTE = ("NASA data is generally not copyrighted in the United States. Credit NASA. "
             "No NASA logo and no implied endorsement.")


def strip_key(url):
    return re.sub(r"([?&])api_key=[^&]*&?", r"\1", url).rstrip("?&")


def http_get(url, headers=None, timeout=90):
    request = urllib.request.Request(url, headers={**UA, **(headers or {})})
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.status, response.read()


def record(name, url, status, fixture, classification, units, note=""):
    LOG.append({"name": name, "query_url": strip_key(url), "status": str(status), "date": NOW,
                "fixture": fixture, "classification": classification, "units": units, "note": note})
    print(f"  [{status}] {name}: {classification} -> {fixture}")


def try_get(name, url, headers=None):
    """Returns (status, bytes or None). Never raises."""
    try:
        return http_get(url, headers)
    except urllib.error.HTTPError as error:
        return error.code, None
    except Exception as error:  # network, DNS, timeout
        return type(error).__name__, None


def existing_is_real(filename):
    path = os.path.join(OUT, filename)
    if not os.path.exists(path):
        return False
    with open(path, encoding="utf-8") as handle:
        head = handle.read(4000)
    return "classification" in head and "synthetic" not in head


def write_text(filename, text):
    os.makedirs(OUT, exist_ok=True)
    with open(os.path.join(OUT, filename), "w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def write_json(filename, data):
    write_text(filename, json.dumps(data, indent=2, ensure_ascii=False) + "\n")


def keep_or_synthetic(filename, name, url, status, units, make_synthetic):
    """A fetch failed: keep an earlier real fixture, otherwise write a labelled synthetic one."""
    if existing_is_real(filename):
        record(name, url, status, filename, "kept earlier real fixture", units, "fetch failed this run")
        return
    make_synthetic()
    record(name, url, status, filename, "synthetic", units, "fetch failed; hand-made fixture")


def csv_header(fields):
    return "".join(f"# {key}: {value}\n" for key, value in fields.items())


# ---------------------------------------------------------------- DONKI

DONKI_START, DONKI_END = "2024-05-10", "2024-05-12"
FLARE_ID = "2024-05-11T01:10:00-FLR-001"
CCMC = "https://ccmc.gsfc.nasa.gov/DONKI/WS/get/{kind}?startDate={start}&endDate={end}"
GATEWAY = "https://api.nasa.gov/DONKI/{kind}?startDate={start}&endDate={end}&api_key={key}"


def fetch_donki_kind(kind):
    gateway = GATEWAY.format(kind=kind, start=DONKI_START, end=DONKI_END, key=API_KEY)
    status, body = try_get("DONKI " + kind, gateway)
    record(f"DONKI {kind} via api.nasa.gov", gateway, status, "-", "not used" if body is None else "NASA-derived", "event metadata")
    if body is not None:
        return strip_key(gateway), json.loads(body)
    direct = CCMC.format(kind=kind, start=DONKI_START, end=DONKI_END)
    status, body = try_get("DONKI " + kind, direct)
    if body is None:
        raise RuntimeError(f"DONKI {kind}: {status}")
    record(f"DONKI {kind} via CCMC", direct, status, "donki_event_pack.json", "NASA-derived", "event metadata")
    return direct, json.loads(body)


def fetch_donki():
    filename = "donki_event_pack.json"
    try:
        urls = []
        url, flares = fetch_donki_kind("FLR"); urls.append(url)
        url, cmes = fetch_donki_kind("CME"); urls.append(url)
        url, seps = fetch_donki_kind("SEP"); urls.append(url)
        url, gsts = fetch_donki_kind("GST"); urls.append(url)

        flare = next(f for f in flares if f["flrID"] == FLARE_ID)
        linked = [e["activityID"] for e in flare.get("linkedEvents") or []]
        cme = next(c for c in cmes if c["activityID"] in linked)
        analyses = cme.get("cmeAnalyses") or []
        best = next((a for a in analyses if a.get("isMostAccurate")), analyses[0] if analyses else {})
        sep_events = [s for s in seps if s["sepID"] in linked]
        gst = max(gsts, key=lambda g: max(k["kpIndex"] for k in g["allKpIndex"]))
        top_kp = max(gst["allKpIndex"], key=lambda k: k["kpIndex"])

        write_json(filename, {
            "source": "NASA CCMC DONKI (Space Weather Database Of Notifications, Knowledge, Information)",
            "query_url": " ; ".join(urls),
            "retrieved_utc": NOW,
            "units": "times in UTC; flare class = GOES X-ray class; CME speed in km/s; Kp index (0-9)",
            "license_note": NASA_NOTE,
            "classification": "NASA-derived",
            "selection_note": ("One historic window, 2024-05-10 to 2024-05-12. Chosen because DONKI links this "
                               "X-class flare to a CME and to solar energetic particle (SEP) detections. "
                               "The game's hour-by-hour timeline and crew effects are fictional."),
            "flare": {
                "id": flare["flrID"], "classType": flare["classType"],
                "beginTime": flare["beginTime"], "peakTime": flare["peakTime"], "endTime": flare["endTime"],
                "sourceLocation": flare.get("sourceLocation") or "",
                "activeRegionNum": int(flare.get("activeRegionNum") or 0),
                "link": flare.get("link") or ""
            },
            "cme": {
                "id": cme["activityID"], "startTime": cme["startTime"],
                "speedKmS": float(best.get("speed") or 0), "link": cme.get("link") or ""
            },
            "sep": [{"id": s["sepID"], "eventTime": s["eventTime"],
                     "instrument": ", ".join(i["displayName"] for i in s.get("instruments") or []),
                     "link": s.get("link") or ""} for s in sep_events],
            "gst": {
                "id": gst["gstID"], "startTime": gst["startTime"],
                "maxKp": float(top_kp["kpIndex"]), "maxKpTime": top_kp["observedTime"],
                "link": gst.get("link") or "",
                "note": "Earth geomagnetic storm index. Context only. It is not a Moon radiation dose."
            }
        })
    except Exception as error:
        keep_or_synthetic(filename, "DONKI event pack", CCMC.format(kind="FLR", start=DONKI_START, end=DONKI_END),
                          type(error).__name__, "event metadata", lambda: write_json(filename, {
                              "source": "Hand-made training event (no NASA data)",
                              "query_url": "", "retrieved_utc": NOW, "units": "times in UTC",
                              "license_note": "Invented for play.", "classification": "synthetic",
                              "selection_note": "DONKI could not be reached. This event is invented.",
                              "flare": {"id": "SYNTHETIC-FLR", "classType": "X1.0", "beginTime": "2030-01-01T01:00Z",
                                        "peakTime": "2030-01-01T01:15Z", "endTime": "2030-01-01T01:30Z",
                                        "sourceLocation": "", "activeRegionNum": 0, "link": ""},
                              "cme": {"id": "SYNTHETIC-CME", "startTime": "2030-01-01T01:30Z", "speedKmS": 0, "link": ""},
                              "sep": [{"id": "SYNTHETIC-SEP", "eventTime": "2030-01-01T02:00Z", "instrument": "invented", "link": ""}],
                              "gst": {"id": "SYNTHETIC-GST", "startTime": "2030-01-01T12:00Z", "maxKp": 0,
                                      "maxKpTime": "2030-01-01T15:00Z", "link": "", "note": "Invented."}
                          }))


# ---------------------------------------------------------------- DONKI storm set (Free Build)

STORM_SCAN = ("2021-01-01", "2025-12-31")
STORM_COUNT = 8


def donki_ccmc(kind, start, end):
    """CCMC DONKI web service (no key). It allows at most 60 days per request."""
    url = CCMC.format(kind=kind, start=start, end=end)
    status, body = try_get("DONKI " + kind, url)
    if body is None:
        raise RuntimeError(f"DONKI {kind} {start}: {status}")
    return url, json.loads(body) if body.strip() else []


def flare_size(class_type):
    scale = {"A": 0.001, "B": 0.01, "C": 0.1, "M": 1, "X": 10}
    try:
        return scale[class_type[0]] * float(class_type[1:])
    except (KeyError, ValueError, IndexError):
        return 0


def linked_ids(event, tag):
    return [e["activityID"] for e in event.get("linkedEvents") or [] if tag in e["activityID"]]


def fetch_donki_storms():
    """X-class flares that DONKI links to a CME and to particle (SEP) detections.
    Keeps the strongest flare from each active region, then the STORM_COUNT strongest of those."""
    filename = "donki_storm_set.json"
    scan_url = CCMC.format(kind="FLR", start=STORM_SCAN[0], end=STORM_SCAN[1]) + " (in 60-day windows)"
    try:
        flares, day, last = [], datetime.date.fromisoformat(STORM_SCAN[0]), datetime.date.fromisoformat(STORM_SCAN[1])
        while day <= last:
            end = min(day + datetime.timedelta(days=59), last)
            flares += donki_ccmc("FLR", day, end)[1]
            day = end + datetime.timedelta(days=1)
        record("DONKI FLR scan via CCMC", scan_url, 200, filename, "NASA-derived", "event metadata", f"{len(flares)} flares")

        best = {}
        for f in flares:
            if not (f.get("classType") or "").startswith("X") or not linked_ids(f, "CME") or not linked_ids(f, "SEP"):
                continue
            region = f.get("activeRegionNum") or f["flrID"]
            if region not in best or flare_size(f["classType"]) > flare_size(best[region]["classType"]):
                best[region] = f
        picks = sorted(best.values(), key=lambda f: -flare_size(f["classType"]))[:STORM_COUNT]

        storms, urls = [], []
        for f in picks:
            peak = datetime.datetime.fromisoformat(f["peakTime"].replace("Z", "+00:00"))
            start, end = (peak - datetime.timedelta(days=1)).date(), (peak + datetime.timedelta(days=4)).date()
            url, cmes = donki_ccmc("CME", start, end); urls.append(url)
            _, seps = donki_ccmc("SEP", start, end)
            _, gsts = donki_ccmc("GST", start, end)
            cme = next(c for c in cmes if c["activityID"] in linked_ids(f, "CME"))
            analyses = cme.get("cmeAnalyses") or []
            analysis = next((a for a in analyses if a.get("isMostAccurate")), analyses[0] if analyses else {})
            sep_list = sorted((s for s in seps if s["sepID"] in linked_ids(f, "SEP")), key=lambda s: s["eventTime"])
            gst_ids = linked_ids(cme, "GST")
            gst = next((g for g in gsts if g["gstID"] in gst_ids), None)
            top = max(gst["allKpIndex"], key=lambda k: k["kpIndex"]) if gst and gst.get("allKpIndex") else None
            storms.append({
                "flare": {"id": f["flrID"], "classType": f["classType"], "beginTime": f["beginTime"],
                          "peakTime": f["peakTime"], "endTime": f.get("endTime") or "",
                          "sourceLocation": f.get("sourceLocation") or "",
                          "activeRegionNum": int(f.get("activeRegionNum") or 0), "link": f.get("link") or ""},
                "cme": {"id": cme["activityID"], "startTime": cme["startTime"],
                        "speedKmS": float(analysis.get("speed") or 0), "link": cme.get("link") or ""},
                "sep": [{"id": s["sepID"], "eventTime": s["eventTime"],
                         "instrument": ", ".join(i["displayName"] for i in s.get("instruments") or []),
                         "link": s.get("link") or ""} for s in sep_list],
                "gst": {"id": gst["gstID"] if gst else "", "startTime": gst["startTime"] if gst else "",
                        "maxKp": float(top["kpIndex"]) if top else 0, "maxKpTime": top["observedTime"] if top else "",
                        "link": (gst.get("link") or "") if gst else "",
                        "note": "Earth geomagnetic storm index linked to this CME, if DONKI lists one. Never a Moon dose."}
            })
        write_json(filename, {
            "source": "NASA CCMC DONKI (Space Weather Database Of Notifications, Knowledge, Information)",
            "query_url": scan_url + " ; then CME, SEP and GST around each flare: " + " ; ".join(urls),
            "retrieved_utc": NOW,
            "units": "times in UTC; flare class = GOES X-ray class; CME speed in km/s; Kp index (0-9)",
            "license_note": NASA_NOTE,
            "classification": "NASA-derived",
            "selection_note": (f"X-class flares from {STORM_SCAN[0]} to {STORM_SCAN[1]} that DONKI links to a CME and to "
                               "solar energetic particle (SEP) detections. Strongest flare per active region, then the "
                               f"{STORM_COUNT} strongest. Free Build picks one per storm. Game timing and crew effects are fictional."),
            "storms": storms})
        record("DONKI storm set", scan_url, 200, filename, "NASA-derived", "event metadata", f"{len(storms)} storms")
    except Exception as error:
        if existing_is_real(filename):
            record("DONKI storm set", scan_url, type(error).__name__, filename, "kept earlier real fixture", "event metadata", str(error))
        else:
            raise


# ---------------------------------------------------------------- RadLab (ISS dosimetry)

RADLAB = ("https://visualization.osdr.nasa.gov/radlab/api/?spacecraft=ISS&instrument=DosTel"
          "&timestamp%3E=2024-05-11T00:00&timestamp%3C=2024-05-11T06:00&absorbed_dose_rate&format=csv")


def fetch_radlab():
    filename = "radlab_iss_sample.csv"
    units = "absorbed_dose_rate in microgray per hour (uGy/h)"
    status, body = try_get("RadLab", RADLAB)
    rows = []
    if body is not None:
        for row in csv.DictReader(io.StringIO(body.decode("utf-8"))):
            if row.get("absorbed_dose_rate"):
                rows.append((row["timestamp"], row["instrument_id"], float(row["absorbed_dose_rate"])))
    if len(rows) < 50:
        def synthetic():
            fake = [(f"2030-01-01T00:{m:02d}:00", "INVENTED", 5.0 + (m % 7) * 0.3) for m in range(0, 60, 2)]
            write_radlab(filename, fake, "Hand-made sample (no NASA data)", "", "synthetic", units)
        keep_or_synthetic(filename, "RadLab ISS dose rate", RADLAB, status, units, synthetic)
        return
    rows = rows[:500]
    write_radlab(filename, rows, "NASA OSDR RadLab, ISS DosTel dosimeters", RADLAB, "NASA-derived", units)
    record("RadLab ISS dose rate", RADLAB, status, filename, "NASA-derived", units, f"{len(rows)} rows")


def write_radlab(filename, rows, source, url, classification, units):
    values = [r[2] for r in rows]
    header = csv_header({
        "source": source, "query_url": url, "retrieved_utc": NOW, "units": units,
        "license_note": NASA_NOTE + " See OSDR terms of use.", "classification": classification,
        "note": "ISS is in low Earth orbit, inside Earth's magnetic field. This is not a Moon dose.",
        "rows": len(rows), "median_uGy_per_h": round(statistics.median(values), 2),
        "min_uGy_per_h": round(min(values), 2), "max_uGy_per_h": round(max(values), 2)
    })
    body = "timestamp_utc,instrument,absorbed_dose_rate_uGy_per_h\n" + "".join(f"{t},{i},{v}\n" for t, i, v in rows)
    write_text(filename, header + body)


# ---------------------------------------------------------------- LOLA polar illumination

LOLA_IMG = "http://imbrium.mit.edu/EXTRAS/ILLUMINATION/IMG/AVGVISIB_85S_060M_201608.IMG"
LOLA_PAGE = "https://pgda.gsfc.nasa.gov/products/69"
LINES = SAMPLES = 5058          # from the PDS label AVGVISIB_85S_060M_201608.LBL
RECORD_BYTES = 10116
SCALE = 0.00004                 # DN * SCALE = fraction of time the pixel is sunlit
METRES_PER_PIXEL = 60.0
PROJECTION_OFFSET = 2528.5      # LINE_ and SAMPLE_PROJECTION_OFFSET
MOON_RADIUS_M = 1737400.0
CROP_LINE, CROP_SAMPLE = 2604, 2480   # top-left pixel of the crop (0-based)
PIXELS_PER_TILE = 2                   # 2 x 2 pixels = one 120 m game tile
GRID_W, GRID_H = 20, 15


def crop_centre_lat_lon():
    line = CROP_LINE + GRID_H * PIXELS_PER_TILE / 2.0
    sample = CROP_SAMPLE + GRID_W * PIXELS_PER_TILE / 2.0
    x = (sample - PROJECTION_OFFSET) * METRES_PER_PIXEL
    y = (PROJECTION_OFFSET - line) * METRES_PER_PIXEL
    r = math.hypot(x, y)
    colatitude = 2 * math.degrees(math.atan(r / (2 * MOON_RADIUS_M)))
    return -90 + colatitude, math.degrees(math.atan2(x, y)) % 360, r / 1000.0


def fetch_lola():
    filename = "illumination_grid.csv"
    units = "percent of time the tile is sunlit (average solar disc visibility), 0-100"
    rows_needed = GRID_H * PIXELS_PER_TILE
    first = CROP_LINE * RECORD_BYTES
    last = (CROP_LINE + rows_needed) * RECORD_BYTES - 1
    status, body = try_get("LOLA illumination", LOLA_IMG, {"Range": f"bytes={first}-{last}"})
    if body is None or len(body) != rows_needed * RECORD_BYTES:
        def synthetic():
            grid = [[round(max(4, 62 - 3.2 * abs(x - 3 + y * 0.15) - 0.6 * x), 1) for x in range(GRID_W)] for y in range(GRID_H)]
            write_illumination(filename, grid, "Hand-made grid (no NASA data)", "", "synthetic", units, "Invented ridge shape.")
        keep_or_synthetic(filename, "LOLA polar illumination", LOLA_IMG, status, units, synthetic)
        return
    pixels = array.array("h")
    pixels.frombytes(body)
    if sys.byteorder != "little":
        pixels.byteswap()
    grid = []
    for ty in range(GRID_H):
        row = []
        for tx in range(GRID_W):
            total = 0.0
            for py in range(PIXELS_PER_TILE):
                for px in range(PIXELS_PER_TILE):
                    line = ty * PIXELS_PER_TILE + py
                    sample = CROP_SAMPLE + tx * PIXELS_PER_TILE + px
                    total += pixels[line * SAMPLES + sample] * SCALE
            row.append(round(100 * total / PIXELS_PER_TILE ** 2, 1))
        grid.append(row)
    lat, lon, km = crop_centre_lat_lon()
    note = (f"Crop of {GRID_W}x{GRID_H} tiles, each the mean of {PIXELS_PER_TILE}x{PIXELS_PER_TILE} pixels (120 m). "
            f"Top-left pixel line {CROP_LINE}, sample {CROP_SAMPLE}. Centre about {lat:.2f} deg latitude, "
            f"{lon:.1f} deg E, {km:.1f} km from the south pole (computed from the label's polar stereographic "
            "projection). Long-term averages, not live shadows. First data row is the top of the map.")
    write_illumination(filename, grid, "LRO LOLA average illumination map AVGVISIB_85S_060M_201608 (PDS LRO-L-LOLA-4-GDR-V1.0); "
                       "Mazarico et al. 2011, Icarus 211, 1066-1081", LOLA_IMG + " (HTTP Range request for the crop rows); " + LOLA_PAGE,
                       "simplified", units, note)
    record("LOLA polar illumination", LOLA_IMG, status, filename, "simplified", units, "real map, cropped and averaged per tile")


def write_illumination(filename, grid, source, url, classification, units, note):
    header = csv_header({"source": source, "query_url": url, "retrieved_utc": NOW, "units": units,
                         "license_note": NASA_NOTE + " Cite Mazarico et al. 2011.", "classification": classification,
                         "note": note, "width": GRID_W, "height": GRID_H})
    write_text(filename, header + "".join(",".join(str(v) for v in row) + "\n" for row in grid))


# ---------------------------------------------------------------- MOLA Mars topography (Jezero crater)

MOLA_IMG = "https://pds-geosciences.wustl.edu/mgs/mgs-m-mola-5-megdr-l3-v1/mgsl_300x/meg016/megt90n000eb.img"
MOLA_RECORD_BYTES = 11520        # from megt90n000eb.lbl: 5760 samples x 16-bit MSB
MOLA_SAMPLES = 5760
MOLA_PPD = 16.0                  # MAP_RESOLUTION, 3.705 km per pixel
JEZERO_LAT, JEZERO_LON = 18.38, 77.58   # crater centre (USGS/IAU)


def fetch_mola_jezero():
    filename = "mars_jezero_mola.csv"
    units = "metres above the Mars areoid (MOLA median topography)"
    top = int(round(1440.5 - JEZERO_LAT * MOLA_PPD - 1 - GRID_H / 2.0))
    left = int(round(JEZERO_LON * MOLA_PPD + 0.5 - 1 - GRID_W / 2.0))
    first, last = top * MOLA_RECORD_BYTES, (top + GRID_H) * MOLA_RECORD_BYTES - 1
    status, body = try_get("MOLA Jezero topography", MOLA_IMG, {"Range": f"bytes={first}-{last}"})
    if body is None or len(body) != GRID_H * MOLA_RECORD_BYTES:
        record("MOLA Jezero topography", MOLA_IMG, status, filename, "not written", units, "fetch failed")
        return
    pixels = array.array("h")
    pixels.frombytes(body)
    if sys.byteorder == "little":
        pixels.byteswap()
    grid = [[pixels[row * MOLA_SAMPLES + left + col] for col in range(GRID_W)] for row in range(GRID_H)]
    lat_top = (1440.5 - (top + 1)) / MOLA_PPD
    lon_left = (left + 1 - 0.5) / MOLA_PPD
    note = (f"Crop of {GRID_W}x{GRID_H} pixels at 16 pixels per degree (3.7 km each). Top-left pixel line {top}, sample {left} "
            f"(0-based), about {lat_top:.2f} N, {lon_left:.2f} E. Jezero crater (Mars 2020 Perseverance site, centre 18.38 N 77.58 E). "
            "First data row is the top (north) of the map.")
    header = csv_header({"source": "MGS MOLA Mission Experiment Gridded Data Record MEGT90N000EB (PDS MGS-M-MOLA-5-MEGDR-L3-V1.0)",
                         "query_url": MOLA_IMG + " (HTTP Range request for the crop rows)", "retrieved_utc": NOW, "units": units,
                         "license_note": NASA_NOTE, "classification": "simplified", "note": note,
                         "width": GRID_W, "height": GRID_H})
    write_text(filename, header + "".join(",".join(str(v) for v in row) + "\n" for row in grid))
    record("MOLA Jezero topography", MOLA_IMG, status, filename, "simplified", units, "real elevations, one pixel per tile")


# ---------------------------------------------------------------- NASA POWER (Mississauga)

POWER = ("https://power.larc.nasa.gov/api/temporal/climatology/point?parameters=ALLSKY_SFC_SW_DWN"
         "&community=RE&latitude=43.59&longitude=-79.64&format=JSON")
MONTHS = ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"]


def fetch_power():
    filename = "power_mississauga.json"
    status, body = try_get("NASA POWER", POWER)
    try:
        data = json.loads(body)
        values = data["properties"]["parameter"]["ALLSKY_SFC_SW_DWN"]
        units = data["parameters"]["ALLSKY_SFC_SW_DWN"]["units"]
        write_power(filename, "NASA POWER (Prediction Of Worldwide Energy Resources), climatology API", POWER, "NASA-derived",
                    units, data["header"].get("range", ""), [float(values[m]) for m in MONTHS], float(values["ANN"]))
        record("NASA POWER Mississauga", POWER, status, filename, "NASA-derived", units, "monthly climatology")
    except Exception:
        keep_or_synthetic(filename, "NASA POWER Mississauga", POWER, status, "kW-hr/m^2/day", lambda: write_power(
            filename, "Hand-made values (no NASA data)", "", "synthetic", "kW-hr/m^2/day", "invented",
            [1.4, 2.1, 3.4, 4.5, 5.5, 6.1, 6.0, 5.3, 4.2, 2.5, 1.6, 1.2], 3.7))


def write_power(filename, source, url, classification, units, period, monthly, annual):
    write_json(filename, {
        "source": source, "query_url": url, "retrieved_utc": NOW, "units": units,
        "license_note": NASA_NOTE, "classification": classification,
        "parameter": "ALLSKY_SFC_SW_DWN (all-sky surface shortwave downward irradiance)",
        "place": "Mississauga, Ontario (latitude 43.59, longitude -79.64, approximate)",
        "period": period, "months": MONTHS, "monthly": monthly, "annual": annual,
        "note": "Earth-surface sunlight. Never used as lunar power."
    })


# ---------------------------------------------------------------- OSDR study cards

OSDR_SEARCH = "https://osdr.nasa.gov/osdr/data/search?type=cgene&size=5&term="
OSDR_STUDY = "https://osdr.nasa.gov/bio/repo/data/studies/"
# Curated list. Kid lines are written from the study title only.
OSDR_PICKS = [
    ("OSD-268", "plants", "Scientists tested lettuce grown on the Space Station."),
    ("OSD-767", "plants", "A tomato crop grew on the Space Station under different lights."),
    ("OSD-622", "plants", "Dwarf wheat was grown in microgravity to study its leaves."),
    ("OSD-678", "plants", "Light changes how a small plant reacts to spaceflight."),
    ("OSD-412", "plants", "Leafy greens grew together in the Veggie garden on the station."),
    ("OSD-7", "plants", "Roots and shoots of a plant react differently to spaceflight."),
    ("OSD-342", "health", "Mice on the station helped study muscle and bone loss."),
    ("OSD-401", "health", "Mouse leg muscles change as they adapt to space."),
    ("OSD-366", "radiation", "Cosmic-ray-like radiation broke mouse DNA. Mice from 15 families reacted differently."),
    ("OSD-658", "radiation", "Plant seedlings hit by space-like radiation switched on their DNA repair."),
    ("OSD-479", "radiation", "Scientists studied how deep space radiation affects thinking, using mice."),
    ("OSD-984", "radiation", "Space-like radiation and weightless-like rest had long-term effects on mouse muscles."),
]


def fetch_osdr():
    filename = "osdr_cards.json"
    cards, last_status, last_url = [], "", OSDR_SEARCH
    for accession, topic, kid_line in OSDR_PICKS:
        last_url = OSDR_SEARCH + urllib.parse.quote(accession)
        last_status, body = try_get("OSDR", last_url)
        if body is None:
            continue
        hits = json.loads(body).get("hits", {}).get("hits", [])
        match = next((h["_source"] for h in hits if h["_source"].get("Accession") == accession), None)
        if match is None:
            continue
        organism = match.get("organism") or ""
        where = match.get("Flight Program") or match.get("Project Type") or ""
        cards.append({"accession": accession, "topic": topic, "title": match.get("Study Title", "").strip(),
                      "kidLine": kid_line, "organism": organism if isinstance(organism, str) else ", ".join(organism),
                      "where": where if isinstance(where, str) else ", ".join(where),
                      "url": OSDR_STUDY + accession})
    if len(cards) < 6:
        keep_or_synthetic(filename, "OSDR study cards", last_url, last_status, "text", lambda: write_json(filename, {
            "source": "Hand-made cards (no NASA data)", "query_url": "", "retrieved_utc": NOW, "units": "text",
            "license_note": "Invented for play.", "classification": "synthetic",
            "cards": [{"accession": "SYNTHETIC-1", "topic": "plants", "title": "Invented plant study",
                       "kidLine": "Plants can grow in space gardens.", "organism": "", "url": ""}]}))
        return
    write_json(filename, {
        "source": "NASA Open Science Data Repository (OSDR)",
        "query_url": OSDR_SEARCH + "<accession>", "retrieved_utc": NOW, "units": "text (study titles)",
        "license_note": NASA_NOTE + " Study titles belong to their authors; cards link to the study page. See OSDR terms of use.",
        "classification": "NASA-derived",
        "note": ("Titles are copied from OSDR. Kid lines are simplified summaries of the title only. "
                 "'where' is the OSDR flight program or project type; Ground Study radiation is made on Earth."),
        "cards": cards})
    record("OSDR study cards", OSDR_SEARCH + "<accession>", last_status, filename, "NASA-derived", "text", f"{len(cards)} studies")


# ---------------------------------------------------------------- CSA food cards

CSA = "https://www.asc-csa.gc.ca/eng/"
# Each card: page, a phrase that must still be on the page, and a kid line written from that page.
CSA_PICKS = [
    ("Tomatosphere", "youth-educators/tomatosphere.asp", "three",
     "Over three million students have taken part in Tomatosphere.",
     "Plants can provide food, produce oxygen, and recycle carbon dioxide."),
    ("Deep Space Food Challenge", "sciences/food-production/deep-space-food-challenge.asp", "Deep Space Food Challenge",
     "Canada and NASA ran a contest for new ways to grow space food.",
     "The challenge looked for food systems that need little water and energy."),
    ("Growing food far from Earth", "sciences/food-production/growing-healthy-food-in-space-and-remote-areas.asp", "self-sufficient",
     "Far from Earth, supply trips are rare. Crews must grow food.",
     "On the Moon or Mars, astronauts will need to be more self-sufficient."),
]


def fetch_csa():
    filename = "csa_food_cards.json"
    cards, last_status, last_url = [], "", CSA
    for title, path, phrase, kid_line, fact in CSA_PICKS:
        last_url = CSA + path
        last_status, body = try_get("CSA", last_url)
        if body is None:
            continue
        text = re.sub(r"\s+", " ", re.sub(r"<[^>]+>", " ", body.decode("utf-8", "ignore")))
        if phrase not in text:
            continue
        modified = re.search(r"Date modified:?[^0-9]{0,40}([0-9]{4}-[0-9]{2}-[0-9]{2})", text)
        cards.append({"title": title, "kidLine": kid_line, "fact": fact, "url": last_url,
                      "pageModified": modified.group(1) if modified else "", "credit": "© Canadian Space Agency"})
    if len(cards) < 2:
        keep_or_synthetic(filename, "CSA food cards", last_url, last_status, "text", lambda: write_json(filename, {
            "source": "Hand-made cards (no CSA data)", "query_url": "", "retrieved_utc": NOW, "units": "text",
            "license_note": "Invented for play.", "classification": "synthetic",
            "cards": [{"title": "Space gardens", "kidLine": "Crews far from Earth may grow their own food.",
                       "fact": "", "url": "", "pageModified": "", "credit": ""}]}))
        return
    write_json(filename, {
        "source": "Canadian Space Agency, food production toolkit and linked pages",
        "query_url": CSA + "youth-educators/toolkits/food-production/", "retrieved_utc": NOW, "units": "text",
        "license_note": ("© Canadian Space Agency. Non-commercial educational use with credit. "
                         "No endorsement implied. Commercial use needs written permission."),
        "classification": "CSA-derived",
        "note": "Kid lines and facts are short paraphrases of the linked CSA page.",
        "cards": cards})
    record("CSA food cards", CSA + "youth-educators/toolkits/food-production/", last_status, filename, "CSA-derived", "text", f"{len(cards)} pages")


# ---------------------------------------------------------------- report

def write_report():
    lines = ["| Source | Query URL (key removed) | HTTP status | Date (UTC) | Units | Fixture | Classification | Note |",
             "|---|---|---|---|---|---|---|---|"]
    for e in LOG:
        lines.append(f"| {e['name']} | `{e['query_url']}` | {e['status']} | {e['date']} | {e['units']} | {e['fixture']} | {e['classification']} | {e['note']} |")
    path = os.path.join(ROOT, "tools", "fetch_log.md")
    # Each run adds its own section, so a partial run never erases the record of an earlier one.
    fresh = not os.path.exists(path)
    with open(path, "a", encoding="utf-8", newline="\n") as handle:
        if fresh:
            handle.write("# Fetch log (written by tools/fetch_data.py)\n")
        handle.write(f"\n## Run {NOW}: {', '.join(sys.argv[1:]) or 'all steps'}\n\n" + "\n".join(lines) + "\n")
    print("Wrote", os.path.relpath(path, ROOT))


def main():
    print("Fetching fixtures. Key source:", "NASA_API_KEY (environment)" if os.environ.get("NASA_API_KEY") else "DEMO_KEY")
    steps = (fetch_donki, fetch_donki_storms, fetch_radlab, fetch_lola, fetch_mola_jezero, fetch_power, fetch_osdr, fetch_csa)
    only = sys.argv[1:]
    for step in steps:
        if not only or step.__name__ in only:
            step()
    write_report()
    assert API_KEY == "DEMO_KEY" or not any(API_KEY in json.dumps(e) for e in LOG), "key leaked into the log"


if __name__ == "__main__":
    main()
