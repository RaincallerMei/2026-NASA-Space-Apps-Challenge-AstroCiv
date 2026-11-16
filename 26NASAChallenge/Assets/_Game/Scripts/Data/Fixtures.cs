using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Mapmakers.Data
{
    [Serializable]
    public class FixtureHeader
    {
        public string source;
        public string query_url;
        public string retrieved_utc;
        public string units;
        public string license_note;
        public string classification;

        public string RetrievedDate => string.IsNullOrEmpty(retrieved_utc) ? "" : retrieved_utc.Substring(0, Math.Min(10, retrieved_utc.Length));
        public bool IsSynthetic => classification == "synthetic";
        public string Badge => IsSynthetic ? "Practice data (not real)" : "Real data";
    }

    [Serializable] public class DonkiFlare { public string id, classType, beginTime, peakTime, endTime, sourceLocation, link; public int activeRegionNum; }
    [Serializable] public class DonkiCme { public string id, startTime, link; public float speedKmS; }
    [Serializable] public class DonkiSep { public string id, eventTime, instrument, link; }
    [Serializable] public class DonkiGst { public string id, startTime, maxKpTime, link, note; public float maxKp; }

    [Serializable]
    public class DonkiPack : FixtureHeader
    {
        public string selection_note;
        public DonkiFlare flare;
        public DonkiCme cme;
        public DonkiSep[] sep;
        public DonkiGst gst;

        public DonkiStorm Storm => new DonkiStorm { flare = flare, cme = cme, sep = sep, gst = gst };
    }

    // One real storm: the flare, its linked CME and particle (SEP) detections, and the Earth storm index if any.
    [Serializable]
    public class DonkiStorm
    {
        public DonkiFlare flare;
        public DonkiCme cme;
        public DonkiSep[] sep;
        public DonkiGst gst;

        public const double MoonDistanceKm = 384400; // NASA NSSDC Moon Fact Sheet, semimajor axis

        public string Date => flare.beginTime.Length >= 10 ? flare.beginTime.Substring(0, 10) : flare.beginTime;

        // Time from the flare peak to the first linked particle detection after it, in words.
        public string ParticleDelay()
        {
            if (sep == null || !Utc(flare.peakTime, out var peak)) return null;
            double best = double.MaxValue;
            foreach (var s in sep)
                if (Utc(s.eventTime, out var t) && t >= peak) best = Math.Min(best, (t - peak).TotalMinutes);
            if (best == double.MaxValue) return null;
            if (best < 90) return Math.Round(best) + " minutes";
            if (best < 48 * 60) return "about " + Math.Round(best / 60) + " hours";
            return "about " + Math.Round(best / 1440) + " days";
        }

        // Words that fill {placeholders} in science facts. A value that DONKI does not give is left out.
        public void AddValues(IDictionary<string, string> values)
        {
            values["flare"] = flare.classType;
            values["date"] = Date;
            if (cme != null && cme.speedKmS > 0)
            {
                values["speed"] = cme.speedKmS.ToString("0", CultureInfo.InvariantCulture);
                values["moonMinutes"] = Math.Max(1, Math.Round(MoonDistanceKm / cme.speedKmS / 60)).ToString(CultureInfo.InvariantCulture);
            }
            string delay = ParticleDelay();
            if (delay != null) values["sepDelay"] = delay;
            if (gst != null && gst.maxKp > 0) values["kp"] = gst.maxKp.ToString("0.#", CultureInfo.InvariantCulture);
        }

        static bool Utc(string text, out DateTime time)
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time);
        }
    }

    [Serializable]
    public class DonkiStormSet : FixtureHeader
    {
        public string selection_note;
        public DonkiStorm[] storms;

        // The same seed and storm number always give the same real storm.
        public DonkiStorm Pick(int seed, int stormNumber)
        {
            if (storms == null || storms.Length == 0) return null;
            int i = (int)((((long)seed * 31 + stormNumber) % storms.Length + storms.Length) % storms.Length);
            return storms[i];
        }
    }

    [Serializable]
    public class PowerClimate : FixtureHeader
    {
        public string parameter, place, period, note;
        public string[] months;
        public float[] monthly;
        public float annual;
    }

    [Serializable] public class OsdrCard { public string accession, topic, title, kidLine, organism, where, url; }
    [Serializable] public class OsdrCards : FixtureHeader { public string note; public OsdrCard[] cards; }
    [Serializable] public class CsaCard { public string title, kidLine, fact, url, pageModified, credit; }
    [Serializable] public class CsaCards : FixtureHeader { public string note; public CsaCard[] cards; }

    public class CsvFixture
    {
        public FixtureHeader header = new FixtureHeader();
        public Dictionary<string, string> meta = new Dictionary<string, string>();
        public List<string[]> rows = new List<string[]>();
    }

    public class RadlabSample
    {
        public FixtureHeader header;
        public int rows;
        public float medianUGyPerHour, minUGyPerHour, maxUGyPerHour;

        public void AddValues(IDictionary<string, string> values)
        {
            values["issMedian"] = medianUGyPerHour.ToString("0.#", CultureInfo.InvariantCulture);
            values["issMax"] = maxUGyPerHour.ToString("0", CultureInfo.InvariantCulture);
        }
    }

    public class IlluminationGrid
    {
        public FixtureHeader header;
        public int width, height;
        public double[,] percent; // [row from top, column]
    }

    public class ElevationGrid
    {
        public FixtureHeader header;
        public int width, height;
        public double[,] metres; // [row from top (north), column]
    }

    // Parses the offline fixtures. Parsing takes plain strings so it can be unit-tested.
    public static class FixtureLoader
    {
        public const string Folder = "Fixtures/";
        public static readonly string[] JsonFixtures = { "donki_event_pack", "donki_storm_set", "power_mississauga", "osdr_cards", "csa_food_cards" };
        public static readonly string[] CsvFixtures = { "radlab_iss_sample", "illumination_grid", "mars_jezero_mola" };
        public static readonly string[] Classifications = { "NASA-derived", "CSA-derived", "simplified", "synthetic" };

        public static string LoadText(string name)
        {
            var asset = Resources.Load<TextAsset>(Folder + name);
            return asset != null ? asset.text : null;
        }

        public static FixtureHeader ParseJsonHeader(string json) => JsonUtility.FromJson<FixtureHeader>(json);
        public static DonkiPack ParseDonki(string json) => JsonUtility.FromJson<DonkiPack>(json);
        public static DonkiStormSet ParseStormSet(string json) => JsonUtility.FromJson<DonkiStormSet>(json);
        public static PowerClimate ParsePower(string json) => JsonUtility.FromJson<PowerClimate>(json);
        public static OsdrCards ParseOsdr(string json) => JsonUtility.FromJson<OsdrCards>(json);
        public static CsaCards ParseCsa(string json) => JsonUtility.FromJson<CsaCards>(json);

        public static CsvFixture ParseCsv(string text)
        {
            var fixture = new CsvFixture();
            foreach (var raw in text.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line[0] != '#')
                {
                    fixture.rows.Add(line.Split(','));
                    continue;
                }
                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                fixture.meta[line.Substring(1, colon - 1).Trim()] = line.Substring(colon + 1).Trim();
            }
            var h = fixture.header;
            fixture.meta.TryGetValue("source", out h.source);
            fixture.meta.TryGetValue("query_url", out h.query_url);
            fixture.meta.TryGetValue("retrieved_utc", out h.retrieved_utc);
            fixture.meta.TryGetValue("units", out h.units);
            fixture.meta.TryGetValue("license_note", out h.license_note);
            fixture.meta.TryGetValue("classification", out h.classification);
            return fixture;
        }

        public static RadlabSample ParseRadlab(string text)
        {
            var csv = ParseCsv(text);
            return new RadlabSample
            {
                header = csv.header,
                rows = csv.rows.Count - 1,
                medianUGyPerHour = Number(csv.meta, "median_uGy_per_h"),
                minUGyPerHour = Number(csv.meta, "min_uGy_per_h"),
                maxUGyPerHour = Number(csv.meta, "max_uGy_per_h")
            };
        }

        public static IlluminationGrid ParseIllumination(string text)
        {
            var csv = ParseCsv(text);
            var grid = new IlluminationGrid { header = csv.header, height = csv.rows.Count, width = csv.rows[0].Length };
            grid.percent = new double[grid.height, grid.width];
            for (int row = 0; row < grid.height; row++)
                for (int col = 0; col < grid.width; col++)
                    grid.percent[row, col] = double.Parse(csv.rows[row][col], CultureInfo.InvariantCulture);
            return grid;
        }

        public static ElevationGrid ParseElevation(string text)
        {
            var csv = ParseCsv(text);
            var grid = new ElevationGrid { header = csv.header, height = csv.rows.Count, width = csv.rows[0].Length };
            grid.metres = new double[grid.height, grid.width];
            for (int row = 0; row < grid.height; row++)
                for (int col = 0; col < grid.width; col++)
                    grid.metres[row, col] = double.Parse(csv.rows[row][col], CultureInfo.InvariantCulture);
            return grid;
        }

        static float Number(Dictionary<string, string> meta, string key)
        {
            return meta.TryGetValue(key, out string value) ? float.Parse(value, CultureInfo.InvariantCulture) : 0f;
        }

        public static DonkiPack Donki() => ParseDonki(LoadText("donki_event_pack"));
        public static DonkiStormSet StormSet() => ParseStormSet(LoadText("donki_storm_set"));
        public static PowerClimate Power() => ParsePower(LoadText("power_mississauga"));
        public static OsdrCards Osdr() => ParseOsdr(LoadText("osdr_cards"));
        public static CsaCards Csa() => ParseCsa(LoadText("csa_food_cards"));
        public static RadlabSample Radlab() => ParseRadlab(LoadText("radlab_iss_sample"));
        public static IlluminationGrid Illumination() => ParseIllumination(LoadText("illumination_grid"));
        public static ElevationGrid Jezero() => ParseElevation(LoadText("mars_jezero_mola"));
    }
}
