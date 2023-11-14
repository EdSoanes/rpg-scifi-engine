using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Expressions
{
    public struct Property
    {
        private string _path = string.Empty;
        private string[] _nodes;

        [JsonProperty]
        private string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                _nodes = _path.Split('.').ToArray();
            }
        }

        public Guid EntityId { get; private set; }
        public string Prop => _nodes?.LastOrDefault() ?? string.Empty;

        public Property(string path) => Path = path;
        public Property(Guid entityId) => EntityId = entityId;

        public static implicit operator string(Property d) => d.Path;
        public static implicit operator Property(string path) => new Property(path);

        public static implicit operator Guid(Property d) => d.EntityId;
        public static implicit operator Property(Guid entityId) => new Property(entityId);

        public static bool operator == (Property d1, Property d2) => d1.Path == d2.Path;
        public static bool operator != (Property d1, Property d2) => d1.Path != d2.Path;

    }
}
