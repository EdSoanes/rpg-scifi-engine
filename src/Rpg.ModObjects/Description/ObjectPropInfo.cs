using Newtonsoft.Json;

namespace Rpg.ModObjects.Description
{
    public class ObjectPropInfo
    {
        [JsonProperty] public string EntityId { get; internal init; }
        [JsonProperty] public string Name { get; internal init; }
        [JsonProperty] public string Archetype { get; internal init; }
        [JsonProperty] public string PropPath { get; internal init; }
        [JsonProperty] public PropInfo PropInfo { get; internal set; }

        [JsonConstructor] protected ObjectPropInfo() { }

        internal ObjectPropInfo(RpgObject rpgObj, string propPath)
        {
            EntityId = rpgObj.Id;
            Name = rpgObj.Name;
            Archetype = rpgObj.Archetype;
            PropPath = propPath;
        }
    }
}
