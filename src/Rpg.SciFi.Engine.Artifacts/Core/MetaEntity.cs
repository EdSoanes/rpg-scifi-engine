using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public class MetaEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string EntityType { get; set; }
        public object? Entity { get; set; }
        public List<MetaAction> Actions { get; set; } = new List<MetaAction>();
        public List<string> ModifiableProperties { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{Path}.{EntityType}={Name}";
        }
    }
}
