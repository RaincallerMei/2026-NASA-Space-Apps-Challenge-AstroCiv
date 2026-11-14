namespace Mapmakers.Sim
{
    // Extension point for the later rover expedition chapter. Intentionally not implemented.
    public interface IExpeditionSystem
    {
        bool IsUnlocked(SimState state);
        void Step(SimState state, double dtHours);
    }
}
