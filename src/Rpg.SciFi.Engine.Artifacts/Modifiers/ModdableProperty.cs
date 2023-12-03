using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModdableProperty
    {
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Prop { get; private set; }

        public ModdableProperty(Guid id, string prop)
        {
            Id = id;
            Prop = prop;
        }

        public override string ToString()
        {
            var metaEntity = Id.MetaData();
            return metaEntity != null
                ? $"{metaEntity.Type}[{Id}].{Prop}"
                : $"{{unknown}}[{Id}].{Prop}";
        }

        public static bool operator ==(ModdableProperty m1, ModdableProperty m2) => m1.Id == m2.Id && m1.Prop == m2.Prop;

        public static bool operator !=(ModdableProperty m1, ModdableProperty m2) => m1.Id != m2.Id || m1.Prop != m2.Prop;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is ModdableProperty))
                return false;

            return this == (ModdableProperty)obj;
        }
    }
}
