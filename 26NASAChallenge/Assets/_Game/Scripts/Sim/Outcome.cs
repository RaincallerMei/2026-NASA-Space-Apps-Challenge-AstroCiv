namespace Mapmakers.Sim
{
    public enum OutcomeKind { Win, CloseCall, Fail }

    public sealed class Outcome
    {
        public OutcomeKind kind;
        public int stars;
        public FailReason failReason;
        public bool recalledBeforeOnset;
        public bool shelterUsed;
        public bool greenhouseShed;
        public bool storedEnergy;
        public int stormBrownouts;
        public double minSafety;

        public static Outcome Evaluate(Balance balance, SimState s)
        {
            var o = new Outcome
            {
                failReason = s.failReason,
                recalledBeforeOnset = s.recallIssued && s.recallRelHour < balance.storm.onsetHour,
                shelterUsed = s.shelterHours > 0,
                greenhouseShed = s.greenhouseOffInStorm,
                storedEnergy = s.batteriesAtOnset >= 2,
                stormBrownouts = s.stormBrownouts,
                minSafety = s.minSafetyStorm
            };
            if (s.failed)
            {
                o.kind = OutcomeKind.Fail;
                o.stars = 0;
            }
            else if (s.stormBrownouts > 0 || s.minSafetyStorm < balance.closeCallSafety)
            {
                o.kind = OutcomeKind.CloseCall;
                o.stars = 1;
            }
            else
            {
                o.kind = OutcomeKind.Win;
                o.stars = s.minSafetyStorm >= balance.perfectSafety ? 3 : 2;
            }
            return o;
        }
    }
}
