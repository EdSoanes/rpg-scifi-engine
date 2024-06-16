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
        public string Type { get; set; }

        public string DisplayName { get; set; }
        public string? Tab { get; set; }
        public string? Group { get; set; }
        public Dictionary<string, object?> Values { get; private set; }

        public T GetValue<T>(string prop, T def)
        {
            if (Values.TryGetValue(prop, out var val) && val != null)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)(val.ToString() ?? string.Empty);

                if (val is T)
                    return (T)val;
            }

            return def;
        }

        public override string ToString()
        {
            var prop = string.Join('.', new List<string>(Path) { Prop });
            return $"{prop} {Type} [{Tab},{Group}]";
        }
    }
}
