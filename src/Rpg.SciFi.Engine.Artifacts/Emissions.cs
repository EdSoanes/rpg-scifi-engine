using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class BaseEmission
    {
        public BaseEmission() { }
        public BaseEmission(string name, string description, int min, int max, int value, int radius) 
        {
            Name = name;
            Min = min;
            Max = max;
            Value = value;
            Radius = radius;
        }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; private set; }

        [JsonProperty] public int Min { get; private set; } = 0;
        [JsonProperty] public int Max { get; private set; } = 100;
        [JsonProperty] public int Value { get; private set; } = 0;
        [JsonProperty] public int Radius { get; private set; } = 100;
    }

    public class Emission : Modifiable<BaseEmission>
    {
        public Emission() { }
        public Emission(string name, string description, int min, int max, int value, int radius)
        {
            BaseModel = new BaseEmission(name, description, min, max, value, radius);
        }

        public Guid Id => BaseModel.Id;
        public string Name => BaseModel.Name;
        public string? Description => BaseModel.Description;
        public int BaseMin => BaseModel.Min;
        public int Min => BaseMin + ModifierRoll("Min");
        public int BaseMax => BaseModel.Max;
        public int Max => BaseMax + ModifierRoll("Max");
        public int BaseValue => BaseModel.Value;
        public int Value => BaseValue + ModifierRoll("Value");
        public int BaseRadius => BaseModel.Radius;
        public int Radius => BaseRadius + ModifierRoll("Radius");
    }

    public class EmissionSignature
    {
        [JsonProperty] public Emission VisibleLight { get; protected set; } = new Emission();
        [JsonProperty] public Emission Heat { get; protected set; } = new Emission();
        [JsonProperty] public Emission Radiation { get; protected set; } = new Emission();
        [JsonProperty] public Emission Sound { get; protected set; } = new Emission();
        [JsonProperty] public Emission Eletromagnetic { get; protected set; } = new Emission();
    }
}
