using System;
using System.Collections.Generic;

namespace Mapmakers.Sim
{
    public enum EventKind { Shadow, SolarStorm, Fault, SupplyDrop, Sunrise }

    // One scheduled event. Shadow and SolarStorm warn at the start of the day before `day`.
    [Serializable]
    public sealed class ScheduledEvent
    {
        public int day;
        public string kind;
        public double strength = 1;
        public int amount;

        public EventKind Kind => (EventKind)Enum.Parse(typeof(EventKind), kind);

        public static ScheduledEvent Make(int day, EventKind kind, double strength = 1, int amount = 0)
        {
            return new ScheduledEvent { day = day, kind = kind.ToString(), strength = strength, amount = amount };
        }
    }

    // What happens on which day. days = 0 means no clock (the drawing tutorial).
    [Serializable]
    public sealed class Scenario
    {
        public int days;
        public bool shelterUnlocked;
        public int maxBatteries = 1;
        public List<ScheduledEvent> events = new List<ScheduledEvent>();

        public List<ScheduledEvent> On(int day, EventKind kind)
        {
            var found = new List<ScheduledEvent>();
            foreach (var e in events)
                if (e.day == day && e.Kind == kind) found.Add(e);
            return found;
        }

        public int Count(EventKind kind)
        {
            int n = 0;
            foreach (var e in events)
                if (e.Kind == kind) n++;
            return n;
        }
    }

    // Words that change between worlds. Mars uses "Sol" for its day and dust storms instead of long shadows.
    // Both planets share the same fictional balance numbers on purpose (game simplification).
    public sealed class PlanetProfile
    {
        public string id;
        public string name;
        public string dayWord;
        public string dimmingName;
        public string dimmingWarning;
        public string dimmingStart;
        public string dimmingEnd;

        public static readonly PlanetProfile Moon = new PlanetProfile
        {
            id = "Moon", name = "Moon", dayWord = "Day", dimmingName = "Long shadow",
            dimmingWarning = "Tomorrow a long shadow covers the base.",
            dimmingStart = "A long shadow covers the base. Less sun today.",
            dimmingEnd = "The shadow has passed. Full sun is back."
        };

        public static readonly PlanetProfile Mars = new PlanetProfile
        {
            id = "Mars", name = "Mars", dayWord = "Sol", dimmingName = "Dust storm",
            dimmingWarning = "Tomorrow a dust storm dims the Sun.",
            dimmingStart = "Dust fills the sky. Less sun today.",
            dimmingEnd = "The dust has settled. Full sun is back."
        };

        public static PlanetProfile Get(string id) => id == "Mars" ? Mars : Moon;
    }
}
