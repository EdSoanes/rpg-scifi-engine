using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class BaseEmission
    {
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; protected set; }

        [JsonProperty] public int Min { get; protected set; } = 0;
        [JsonProperty] public int Max { get; protected set; } = 100;
        [JsonProperty] public int Value { get; protected set; } = 0;
        [JsonProperty] public int Radius { get; protected set; } = 100;
    }

    public class VisibleLight : BaseEmission { }
    public class Electromagnetic : BaseEmission { }
    public class Radiation : BaseEmission { }
    public class Sound : BaseEmission { }
    public class Heat : BaseEmission
    {
        public Heat()
        {
            Value = 2;
        }
    }

    public class Emission<T> : Modifiable<T> where T : BaseEmission, new()
    {
        public Emission()
        {
            Name = nameof(Emission<T>);
        }

        public string? Description { get; protected set; }
        public int Min => BaseModel.Min + Modifications.Sum(x => x.DiceEval(this));
        public int Max => BaseModel.Max + Modifications.Sum(x => x.DiceEval(this));
        public int Value => BaseModel.Value + Modifications.Sum(x => x.DiceEval(this));
        public int Radius => BaseModel.Radius + Modifications.Sum(x => x.DiceEval(this));
    }

    public class EmissionSignature
    {
        [JsonProperty] public Emission<VisibleLight> VisibleLight { get; protected set; } = new Emission<VisibleLight>();
        [JsonProperty] public Emission<Heat> Heat { get; protected set; } = new Emission<Heat>();
        [JsonProperty] public Emission<Radiation> Radiation { get; protected set; } = new Emission<Radiation>();
        [JsonProperty] public Emission<Sound> Sound { get; protected set; } = new Emission<Sound>();
        [JsonProperty] public Emission<Electromagnetic> Eletromagnetic { get; protected set; } = new Emission<Electromagnetic>();
    }
}
