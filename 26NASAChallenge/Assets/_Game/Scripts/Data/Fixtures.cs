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
    }

    [Serializable]
    public class PowerClimate : FixtureHeader
    {
        public string parameter, place, period, note;
        public string[] months;
        public float[] monthly;
        public float annual;
    }

    [Serializable] public class OsdrCard { public string accession, topic, title, kidLine, organism, url; }
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
    }

    public class IlluminationGrid
    {
        public FixtureHeader header;
        public int width, height;
        public double[,] percent; // [row from top, column]
    }

    // Parses the offline fixtures. Parsing takes plain strings so it can be unit-tested.
    public static class FixtureLoader
    {
        public const string Folder = "Fixtures/";
        public static readonly string[] JsonFixtures = { "donki_event_pack", "power_mississauga", "osdr_cards", "csa_food_cards" };
        public static readonly string[] CsvFixtures = { "radlab_iss_sample", "illumination_grid" };
        public static readonly string[] Classifications = { "NASA-derived", "CSA-derived", "simplified", "synthetic" };

        public static string LoadText(string name)
        {
            var asset = Resources.Load<TextAsset>(Folder + name);
            return asset != null ? asset.text : null;
        }

        public static FixtureHeader ParseJsonHeader(string json) => JsonUtility.FromJson<FixtureHeader>(json);
        public static DonkiPack ParseDonki(string json) => JsonUtility.FromJson<DonkiPack>(json);
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

        static float Number(Dictionary<string, string> meta, string key)
        {
            return meta.TryGetValue(key, out string value) ? float.Parse(value, CultureInfo.InvariantCulture) : 0f;
        }

        public static DonkiPack Donki() => ParseDonki(LoadText("donki_event_pack"));
        public static PowerClimate Power() => ParsePower(LoadText("power_mississauga"));
        public static OsdrCards Osdr() => ParseOsdr(LoadText("osdr_cards"));
        public static CsaCards Csa() => ParseCsa(LoadText("csa_food_cards"));
        public static RadlabSample Radlab() => ParseRadlab(LoadText("radlab_iss_sample"));
        public static IlluminationGrid Illumination() => ParseIllumination(LoadText("illumination_grid"));
    }
}
