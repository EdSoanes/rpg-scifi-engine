using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModdableProperty
    {
        [JsonProperty] public Guid RootId { get; private set; }
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public string? Prop { get; private set; }
        [JsonProperty] public string? Method { get; private set; }
        [JsonProperty] public string Source { get; private set; }

        public ModdableProperty(Guid rootId, Guid id, string type, string? prop, string? method)
        {
            RootId = rootId;
            Id = id;
            Type = type;
            Prop = prop;
            Method = method?.EndsWith("()") ?? false ? method : $"{method}()";
            Source = $"{Type}.{Prop ?? Method ?? throw new ArgumentException("Either Prop or Method must be set")}";
        }

        public override string ToString()
        {
            var metaEntity = Meta.Get(Id)?.Meta;
            return metaEntity != null
                ? $"{Id}({metaEntity.Name}).{Prop}"
                : $"{Id}(unknown).{Prop}";
        }

        public static bool operator ==(ModdableProperty? m1, ModdableProperty? m2) => (m1 == null && m2 == null) || (m1?.Id == m2?.Id && m1?.Source == m2?.Source);

        public static bool operator !=(ModdableProperty? m1, ModdableProperty? m2) => m1?.Id != m2?.Id || m1?.Source != m2?.Source;

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
