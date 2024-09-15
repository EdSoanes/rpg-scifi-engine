using System.Text.Json.Serialization;

namespace Rpg.ModObjects
{
    public abstract class RpgComponent : RpgObject
    {
        [JsonConstructor] public RpgComponent() { }

        public RpgComponent(string name)
        {
            Name = name;
        }
    }
}
