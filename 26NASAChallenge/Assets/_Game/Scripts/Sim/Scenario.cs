using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum EventKind { Shadow, SolarStorm, Fault, SupplyDrop, Sunrise }

    // One scheduled event at a game hour. Shadow: dims at `hour`, warned `shadow.warningHours` before.
    // SolarStorm: the warning comes at `hour`; particles follow `storm.onsetHour` later.
    [Serializable]
    public sealed class ScheduledEvent
    {
        public double hour;
        public string kind;
        public double strength = 1;
        public double durationHours;
        public int amount;

        public EventKind Kind => (EventKind)Enum.Parse(typeof(EventKind), kind);

        public static ScheduledEvent Make(double hour, EventKind kind, double strength = 1, int amount = 0)
        {
            return new ScheduledEvent { hour = hour, kind = kind.ToString(), strength = strength, amount = amount };
        }
    }

    // What happens when. durationHours = 0 means no clock (the drawing tutorial).
    [Serializable]
    public sealed class Scenario
    {
        public double durationHours;
        public bool shelterUnlocked;
        public int maxBatteries = 1;
        public List<ScheduledEvent> events = new List<ScheduledEvent>();

        public int Count(EventKind kind)
        {
            int n = 0;
            foreach (var e in events)
                if (e.Kind == kind) n++;
            return n;
        }

        public bool Timed => durationHours > 0;
    }

    // Words and numbers that change between worlds.
    // Mars: a sol is 24.66 h and sunlight is 43% of Earth's (NASA Mars Fact Sheet). solarScale is the game's softened version.
    public sealed class PlanetProfile
    {
        public string id;
        public string name;
        public string dayWord;
        public double dayHours = 24;
        public double solarScale = 1;
        public int extraSolar;
        public int extraSupplies;
        public string waterSource;
        public string soilName;
        public string dimmingName;
        public string dimmingWarning;
        public string dimmingStart;
        public string dimmingEnd;

        public static readonly PlanetProfile Moon = new PlanetProfile
        {
            id = "Moon", name = "Moon", dayWord = "Day", waterSource = "ice", soilName = "soft soil",
            dimmingName = "Long shadow",
            dimmingWarning = "A long shadow is coming. Less sun soon.",
            dimmingStart = "A long shadow covers the base. Less sun now.",
            dimmingEnd = "The shadow has passed. Full sun is back."
        };

        public static readonly PlanetProfile Mars = new PlanetProfile
        {
            id = "Mars", name = "Mars", dayWord = "Sol", dayHours = 24.6597, solarScale = 0.6, extraSolar = 3, extraSupplies = 60,
            waterSource = "wet clay", soilName = "lake mud",
            dimmingName = "Dust storm",
            dimmingWarning = "A dust storm is coming. Less sun soon.",
            dimmingStart = "Dust fills the sky. Less sun now.",
            dimmingEnd = "The dust has settled. Full sun is back."
        };

        public static PlanetProfile Get(string id) => id == "Mars" ? Mars : Moon;
    }
}
