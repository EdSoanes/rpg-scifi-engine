using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions.Parsers;

namespace Rpg.SciFi.Engine.Artifacts.Expressions
{
    public struct Property
    {
        [JsonProperty] private string _path { get; set; } = string.Empty;
        [JsonProperty] public Guid ContextId { get; private set; } = Guid.Empty;
        [JsonProperty] public string Prop { get; private set; } = string.Empty;

        public Property() { }

        public Property(string path)
        {
            _path = path;
            var propInfo = PropertyParser.Parse(path);
            ContextId = propInfo.ContextId ?? Guid.Empty;
            Prop = propInfo.Prop;
        }

        public static implicit operator string(Property d) => d._path;
        public static implicit operator Property(string path) => new Property(path);

        public static bool operator == (Property d1, Property d2) => d1.ContextId == d2.ContextId && d1.Prop == d2.Prop;
        public static bool operator != (Property d1, Property d2) => d1.ContextId != d2.ContextId || d1.Prop != d2.Prop;

        public override bool Equals(object? obj)
        {
            return obj != null && obj is Property && ((Property)obj) == this;
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();           
        }
    }
}
