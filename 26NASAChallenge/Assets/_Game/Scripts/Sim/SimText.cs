using System;
using System.Globalization;

namespace Mapmakers.Sim
{
    public static class SimText
    {
        public static string Num(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        public static int Day(double timeHours, double dayHours = 24) => 1 + (int)Math.Floor(timeHours / dayHours + 1e-9);

        public static string Clock(double timeHours, int startClockHour, double dayHours = 24)
        {
            double inDay = (startClockHour + timeHours) % dayHours;
            int hour = (int)Math.Floor(inDay + 1e-9);
            return Math.Min(hour, 24).ToString("00", CultureInfo.InvariantCulture) + ":00";
        }

        // "2 h", "1 day 3 h" style, for countdowns and depletion estimates.
        public static string Duration(double hours, string dayWord = "day", double dayHours = 24)
        {
            if (double.IsInfinity(hours) || double.IsNaN(hours)) return "-";
            if (hours < 1) return "under 1 h";
            int whole = (int)Math.Floor(hours + 1e-9);
            int days = (int)Math.Floor(whole / dayHours);
            int rest = (int)Math.Floor(whole - days * dayHours);
            if (days == 0) return rest + " h";
            string word = dayWord.ToLowerInvariant() + (days > 1 ? "s" : "");
            return days + " " + word + (rest > 0 ? " " + rest + " h" : "");
        }
    }
}
