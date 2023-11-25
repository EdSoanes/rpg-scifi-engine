using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public class MetaModLocator
    {
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Prop { get; private set; }

        public MetaModLocator(Guid id, string prop)
        {
            Id = id;
            Prop = prop;
        }

        public override string ToString()
        {
            var metaEntity = MetaEngine.Meta<Entity>(Id);
            return metaEntity != null
                ? $"{metaEntity.Type}[{Id}].{Prop}"
                : $"{{unknown}}[{Id}].{Prop}";
        }

        public static bool operator ==(MetaModLocator m1, MetaModLocator m2) => m1.Id == m2.Id && m1.Prop == m2.Prop;

        public static bool operator !=(MetaModLocator m1, MetaModLocator m2) => m1.Id != m2.Id || m1.Prop != m2.Prop;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is MetaModLocator)) 
                return false;

            return this == (MetaModLocator)obj;
        }
    }
}
