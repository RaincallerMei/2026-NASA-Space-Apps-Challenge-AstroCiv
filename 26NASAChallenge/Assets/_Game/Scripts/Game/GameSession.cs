using System;
using Mapmakers.Sim;

namespace Mapmakers.Game
{
    // Non-visual execution of the fixed-step and Pause/1x/2x/4x requirements.
    // Views supply elapsed real seconds and read State; simulation never reads frame time.
    public sealed class GameSession
    {
        public Simulator Simulator { get; }
        public SimState State { get; private set; }
        public int Speed { get; private set; } = 1;
        public bool Finished => State.failed || State.stage == MissionStage.Done;
        double pendingGameHours;

        public GameSession(Simulator simulator)
        {
            Simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
            if (!PositiveFinite(simulator.balance.dtHours) || !PositiveFinite(simulator.balance.realSecondsPerGameHour))
                throw new ArgumentException("Simulation time settings must be positive and finite.", nameof(simulator));
            State = simulator.NewGame();
        }

        public void SetSpeed(int speed)
        {
            if (speed != 0 && speed != 1 && speed != 2 && speed != 4)
                throw new ArgumentOutOfRangeException(nameof(speed), "Use Pause (0), 1, 2, or 4.");
            Speed = speed;
        }

        public void Tick(double realSeconds)
        {
            if (double.IsNaN(realSeconds) || double.IsInfinity(realSeconds) || realSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(realSeconds));
            if (Speed == 0 || Finished) return;

            double dt = Simulator.balance.dtHours;
            pendingGameHours += realSeconds * Speed / Simulator.balance.realSecondsPerGameHour;
            while (pendingGameHours + 1e-10 >= dt && !Finished)
            {
                State = Simulator.Step(State);
                pendingGameHours = Math.Max(0, pendingGameHours - dt);
            }
            if (Finished) pendingGameHours = 0;
        }

        // Reset/Retry both restore the existing NewGame state. Keep the selected speed;
        // a view can explicitly SetSpeed(1) when starting a new run if Opus chooses that UI behavior.
        public void Reset()
        {
            pendingGameHours = 0;
            State = Simulator.NewGame();
        }

        static bool PositiveFinite(double value) => value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
    }
}
