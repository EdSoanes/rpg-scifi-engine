namespace Rpg.SciFi.Engine.Archetypes.Armor
{
    public class Helmet : Artifact, IPoweredArtifact
    {
        public string[] AllowedPowerSources => new[]
{
            "energy",
            "battery"
        };

        public ConsumableArtifact? PowerSource { get; set; }

        public Helmet() 
        {
            Name = "Scout Helm";
            Description = "Armored helmet suitable for scouting";

            Resistances.Impact.Value = "10";
            Resistances.Stun.Value = "10";
            Resistances.Pierce.Value = "10";
            Resistances.Blast.Value = "5";

            States = new State[]
            {
                new State
                {
                    Name = "active",
                    Description = "Activated",
                    Conditions = new Condition[]
                    {
                        new Condition
                        {
                            Name = "Enhanced Vision",
                            Description = "Increased ability to detect within visual light frequencies",
                            Value = 10,
                            Unit = "",
                            Radius = 200,
                        },
                        new Condition
                        {
                            Name = "Enhanced Hearing",
                            Description = "Increased ability to detect sounds",
                            Value = 10,
                            Radius = 200,
                        },
                        new Condition
                        {
                            Name = "Emits electromagnetic radiation",
                            Description = "Slight increase to your electromagnetic signature",
                            Value = 1,
                            Radius = 200,
                        }
                    }
                }
            };
        }

        public void Consume(int count) => PowerSource?.Consume(count);

        public void Refill(int count) => PowerSource?.Refill(count);
    }
}
