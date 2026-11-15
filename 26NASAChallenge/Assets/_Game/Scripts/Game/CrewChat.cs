using Mapmakers.Data;
using Mapmakers.Sim;

namespace Mapmakers.Game
{
    // Picks which Story.json chat each crew member has right now. Lines react to the base and remember choices.
    public static class CrewChat
    {
        public static string When(string crewId, MissionRun run)
        {
            var s = run.State;
            switch (crewId)
            {
                case "geologist":
                    if (s.StormActive && s.geologist == CrewLocation.Outside && !s.recallIssued) return "storm_outside";
                    if (s.StormActive) return "storm_inside";
                    if (s.stormsStarted > 0) return s.earlyRecalls >= s.stormsStarted ? "after_called_first" : "after_not_called";
                    return s.Count(BuildingType.RoverGarage) > 0 ? "rover" : "default";
                case "engineer":
                    if (s.StormActive) return "storm";
                    double cap = run.sim.BatteryCapacity(s);
                    // Only talk about a draining battery when there is one.
                    if (cap > 0 && (s.report.Has("loads_off") || (s.batteryKWh < 0.4 * cap && s.report.batteryFlowKW < 0))) return "battery_low";
                    if (s.Count(BuildingType.Battery) >= 2) return "two_batteries";
                    return "default";
                case "botanist":
                    if (s.StormActive) return "storm";
                    if (s.harvestDone) return "harvest";
                    if (s.Count(BuildingType.Greenhouse) > 0) return "sprouts";
                    return "default";
                default:
                    return "default";
            }
        }

        public static ChatEntry Entry(StoryData story, string crewId, MissionRun run)
        {
            return story.Chat(crewId, When(crewId, run)) ?? story.Chat(crewId, "default");
        }

        // Runs a reply's action. Returns false when the action could not happen (for example, the radio is off).
        public static bool Apply(ChatReply reply, MissionRun run)
        {
            if (reply.action == "recall") return run.Recall();
            return true;
        }
    }
}
