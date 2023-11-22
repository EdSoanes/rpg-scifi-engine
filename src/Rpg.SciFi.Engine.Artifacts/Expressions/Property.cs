using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions.Parsers;

namespace Rpg.SciFi.Engine.Artifacts.Expressions
{
    public struct Property
    {
        [JsonProperty] private Guid? _contextId { get; set; }
        [JsonProperty] private string? _path { get; set; }

        public Guid ContextId { get => PropertyParser.Parse(_contextId, _path).ContextId; }
        public string Prop { get => PropertyParser.Parse(_contextId, _path).Prop; }

        public Property() { }

        public Property(Guid contextId)
        {
            _contextId = contextId;
        }

        public Property(string path)
        {
            _path = path;
        }

        public Property(Guid contextId, string path)
        {
            _contextId = contextId;
            _path = path;
        }

        public static implicit operator Guid(Property property) => property.ContextId;
        public static implicit operator Property(Guid contextId) => new Property(contextId);

        public static implicit operator string(Property property) => property._path;
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
