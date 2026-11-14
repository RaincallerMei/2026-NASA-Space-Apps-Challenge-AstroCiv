using System.Globalization;

namespace Mapmakers.Sim
{
    public static class SimText
    {
        public static string Num(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        public static string Clock(double timeHours, int startClockHour)
        {
            int total = startClockHour + (int)System.Math.Floor(timeHours + 1e-9);
            return (total % 24).ToString("00", CultureInfo.InvariantCulture) + ":00";
        }

        public static int Day(double timeHours, int startClockHour)
        {
            return 1 + (startClockHour + (int)System.Math.Floor(timeHours + 1e-9)) / 24;
        }
    }
}
