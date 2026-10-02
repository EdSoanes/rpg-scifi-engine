using Newtonsoft.Json;
using Rpg.Experimental;

namespace Rpg.Cyborgs
{
    /// <summary>
    /// Something that can be carried, worn and transferred between objects
    /// </summary>
    public abstract class Item : RpgObject
    {
        [JsonConstructor] protected Item() { }

        public Item(string name)
            : base(name) { }
    }
}
