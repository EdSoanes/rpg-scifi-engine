using Newtonsoft.Json;

namespace Rpg.ModObjects.Meta
{
    public class MetaState
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string Archetype { get; private set; }

        [JsonConstructor] private MetaState() { }

        public MetaState(Type stateType)
        {
            Name = stateType.Name;
            Archetype = stateType.BaseType!.GenericTypeArguments[0].Name;
        }

        public override string ToString()
        {
            return $"{Name} ({Archetype})";
        }
    }
}
