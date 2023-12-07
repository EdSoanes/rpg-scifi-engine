using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaEntity
    {
        [JsonProperty] public Guid? Id { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string Path { get; private set; }
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public string Class { get; private set; }
        [JsonIgnore] public Entity? Entity { get; private set; }
        [JsonProperty] public string[] SetupMethods { get; private set; } = new string[0];
        [JsonProperty] public MetaAction[] AbilityMethods { get; private set; } = new MetaAction[0];

        [JsonProperty] public ModifierStore Mods { get; private set; } = new ModifierStore();

        internal void SetEntity(Entity entity) => Entity = entity;

        [JsonConstructor]
        public MetaEntity(string path, string type, string @class, string name)
        {
            Path = path;
            Type = type;
            Class = @class;
            Name = name;
        }

        public MetaEntity(object obj, string path)
        {
            Id = (obj as Entity)?.Id;
            Entity = obj as Entity;
            Name = (obj as Entity)?.Name ?? obj.GetType().Name;
            Type = obj.GetType().Name;
            Class = obj.GetEntityClass();
            SetupMethods = obj.GetSetupMethods();
            AbilityMethods = obj.GetAbilityMethods();
            Path = path;
        }

        public override string ToString()
        {
            return $"{Path} => [{Id}].{Type}({(Name == Type ? "" : Name)})";
        }
    }
}
