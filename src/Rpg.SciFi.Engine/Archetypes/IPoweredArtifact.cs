namespace Rpg.SciFi.Engine.Archetypes
{
    public interface IPoweredArtifact
    {
        ConsumableArtifact? PowerSource { get; }
        string[] AllowedPowerSources { get; }
        void Consume(int count);
        void Refill(int count);
    }
}
