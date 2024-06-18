using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaProp
    {
        public string Prop { get; set; }
        public string FullProp { get => string.Join('.', new List<string>(Path) { Prop }); }
        public List<string> Path { get; set; } = new List<string>();
        public string DataType { get; set; }
        public string ReturnType { get; set; }
        public string DisplayName { get; set; }
        public string? Tab { get; set; }
        public string? Group { get; set; }
        public bool Ignore {  get; set; }

        public override string ToString()
        {
            var prop = string.Join('.', new List<string>(Path) { Prop });
            return $"{prop} {ReturnType} [{Tab},{Group}]";
        }
    }
}
