using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Expressions
{
    public struct Action
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

        public Action(string path) => Path = path;
        public Action(Guid entityId) => EntityId = entityId;

        public static implicit operator string(Action d) => d.Path;
        public static implicit operator Action(string path) => new Action(path);

        public static implicit operator Guid(Action d) => d.EntityId;
        public static implicit operator Action(Guid entityId) => new Action(entityId);

        public static bool operator ==(Action d1, Action d2) => d1.Path == d2.Path;
        public static bool operator !=(Action d1, Action d2) => d1.Path != d2.Path;
    }
}
