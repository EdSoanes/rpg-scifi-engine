using Newtonsoft.Json;
using Rpg.ModObjects.Props;

namespace Rpg.ModObjects
{
    public abstract class RpgComponent : RpgObject
    {
        [JsonProperty] public PropRef? EntityPropRef { get; private set; }

        [JsonConstructor] protected RpgComponent() { }

        public RpgComponent(string name)
        {
            Name = name;
        }

        internal void SetEntityPropRef(string entityId, string[] path)
        {
            EntityPropRef = new PropRef(entityId, string.Join('.', path));
            var altName = path.LastOrDefault();
            if (!string.IsNullOrEmpty(altName) && (Name == GetType().Name || string.IsNullOrEmpty(Name)))
                Name = altName;
        }
    }
}
