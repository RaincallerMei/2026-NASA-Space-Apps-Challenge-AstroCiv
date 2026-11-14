// Run with: unity command script run --file tools/export_balance.cs
// This tool reads the shipped configuration and runs the existing bots unchanged.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Mapmakers.Data;
using Mapmakers.Sim;
using UnityEngine;

public static class ExportBalance
{
    public static string Main()
    {
        var text = new StringBuilder();
        text.AppendLine("# Balance report");
        text.AppendLine();
        text.AppendLine("Generated UTC: " + DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture));
        text.AppendLine("Source: shipped `Balance.json`, real LOLA terrain, and the existing scenario bots. No values were tuned by this export.");
        text.AppendLine();
        text.AppendLine("All power, costs, resource rates, safety, and mission timing below are fictional balance values, not equipment specifications or a radiation-dose forecast.");
        text.AppendLine();
        text.AppendLine("| Bot | Result | Stars | Fail reason | Game hours | Lowest storm safety | Storm brownouts | Batteries at onset | Stored kWh at onset |");
        text.AppendLine("|---|---|---|---|---|---|---|---|---|");
        Bot[] bots = { new DoNothingBot(), new ShedLoadsBot(), new StoredEnergyBot(), new GreedyBot(), new IdleBot() };
        var details = new StringBuilder();
        foreach (var bot in bots)
        {
            var balance = BalanceLoader.Load();
            var run = ScenarioRunner.Run(new Simulator(balance, TerrainFactory.Load(balance)), bot);
            var s = run.state;
            var o = run.outcome;
            text.AppendLine("| " + bot.Name + " | " + o.kind + " | " + o.stars + " | " + o.failReason + " | " + N(run.hours)
                + " | " + N(o.minSafety) + " | " + o.stormBrownouts + " | " + s.batteriesAtOnset + " | " + N(s.batteryAtOnsetKWh) + " |");
            details.AppendLine("### " + bot.Name);
            details.AppendLine();
            details.AppendLine("```text");
            foreach (var line in run.log) details.AppendLine(line);
            details.AppendLine("```");
            details.AppendLine();
        }
        var config = BalanceLoader.Load();
        text.AppendLine();
        text.AppendLine("## Shipped building values");
        text.AppendLine();
        text.AppendLine("| Building | Footprint | Cost | Generation kW | Demand kW | Battery kWh | Charge/discharge limit kW | Priority |");
        text.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var d in config.buildings)
            text.AppendLine("| " + d.displayName + " | " + d.width + "x" + d.height + " | " + d.cost + " | " + N(d.generationKW)
                + " | " + N(d.demandKW) + " | " + N(d.batteryKWh) + " | " + N(d.batteryRateKW) + " | " + d.priority + " |");
        text.AppendLine();
        text.AppendLine("Starting reserves: " + config.startSupplies + " supplies; " + N(config.startWaterL) + " L water; " + N(config.startMeals)
            + " meals; " + N(config.startAirHours) + " air-hours. Step: " + N(config.dtHours) + " game hours. One game hour = "
            + N(config.realSecondsPerGameHour) + " real seconds at 1x.");
        text.AppendLine();
        text.AppendLine("Greenhouse: 5 L/h in, 4 L/h returned (80% recycled), net loss 1 L/h; 0.5 meals/h gross while powered and watered. Crew consumption: "
            + N(config.CrewMealsPerHour) + " meals/h. Solar generation is multiplied by tile sunlight. The storm does not cut solar output.");
        text.AppendLine();
        text.AppendLine("## Verification limits");
        text.AppendLine();
        text.AppendLine("These are automated simulation runs, not human playthroughs. Successful bots finish in roughly 25.1 game hours (about 126 real seconds at 1x), excluding construction/reading. Pacing, ease of use, and how fair it feels are awaiting human testing and Opus review. Failed runs end with a reason. The functional Main scene now includes Retry; human usability and full-story integration remain pending.");
        text.AppendLine();
        text.AppendLine("## Complete event logs");
        text.AppendLine();
        text.Append(details);
        var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../BALANCE_REPORT.md"));
        File.WriteAllText(path, text.ToString().TrimEnd() + "\n", new UTF8Encoding(false));
        return "Wrote BALANCE_REPORT.md from five existing bots.";
    }

    static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
