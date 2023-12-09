using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaEntity
    {
        [JsonIgnore] public Entity Entity { get; set; }

        public Guid? Id { get => Entity.Id; }
        public string Name { get => Entity.Name; }
        public string Type { get => Entity.GetType().Name; }

        [JsonProperty] public string Path { get; set; }
        [JsonProperty] public string Class { get; set; }
        [JsonProperty] public string[] SetupMethods { get; set; } = new string[0];
        [JsonProperty] public MetaAction[] AbilityMethods { get; set; } = new MetaAction[0];

        [JsonProperty] public ModifierStore Mods { get; private set; } = new ModifierStore();

        public override string ToString()
        {
            return $"{Path} => [{Id}].{Type}({(Name == Type ? "" : Name)})";
        }
    }
}
