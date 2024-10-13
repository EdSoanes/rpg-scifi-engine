using Newtonsoft.Json;

namespace Rpg.ModObjects.Meta
{
    public class MetaContainer : MetaObj
    {
        [JsonConstructor] protected MetaContainer() { }

        public string Name { get; set; }

        public MetaContainer(Type type, string name)
            : base(type)
        {
            Name = name;
        }
    }
}
