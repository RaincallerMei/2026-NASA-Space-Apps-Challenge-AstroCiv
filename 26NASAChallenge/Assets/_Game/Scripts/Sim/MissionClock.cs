using System;

namespace Mapmakers.Sim
{
    // Turns real seconds into whole simulation steps. The simulation never sees frame time.
    // Pause sources: the player's Pause button, open windows (counted, so nested windows stay paused),
    // and losing app focus (sets the player's pause, so the player must press Resume).
    public sealed class MissionClock
    {
        public static readonly int[] Speeds = { 1, 2, 4 };

        readonly double dtHours;
        readonly double secondsPerHour;
        readonly double maxSecondsPerFrame;
        double pendingHours;
        int holds;

        public bool Started { get; private set; }
        public bool UserPaused { get; private set; }
        public int Speed { get; private set; } = 1;
        public bool Paused => !Started || UserPaused || holds > 0;
        public int Holds => holds;
        public double PendingHours => pendingHours;

        public MissionClock(double dtHours, double secondsPerHour, double maxSecondsPerFrame)
        {
            this.dtHours = dtHours;
            this.secondsPerHour = secondsPerHour;
            this.maxSecondsPerFrame = maxSecondsPerFrame;
        }

        public void Start() => Started = true;
        public void SetPaused(bool paused) => UserPaused = paused;
        public void TogglePause() => UserPaused = !UserPaused;
        public void Hold() => holds++;
        public void Release() => holds = Math.Max(0, holds - 1);
        public void FocusLost() => UserPaused = true;

        // Changing speed keeps the fraction of a step already collected.
        public void SetSpeed(int speed)
        {
            if (Array.IndexOf(Speeds, speed) < 0) return;
            Speed = speed;
        }

        // Number of fixed steps to run for this frame. A long frame (or a suspended browser tab) is clamped,
        // so there is never a big catch-up burst.
        public int Advance(double realSeconds)
        {
            if (Paused || realSeconds <= 0) return 0;
            pendingHours += Math.Min(realSeconds, maxSecondsPerFrame) * Speed / secondsPerHour;
            int steps = (int)Math.Floor(pendingHours / dtHours + 1e-9);
            pendingHours -= steps * dtHours;
            if (pendingHours < 0) pendingHours = 0;
            return steps;
        }
    }
}
