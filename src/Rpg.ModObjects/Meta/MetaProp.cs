using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Props;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaProp
    {
        public string Prop { get; set; }
        public string FullProp { get => string.Join('.', new List<string>(Path) { Prop }); }
        public List<string> Path { get; set; } = new List<string>();

        public string DisplayName { get; set; }
        public string DataTypeName { get; set; }
        public EditorType Editor { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public bool Ignore {  get; set; }
        [JsonConstructor] private MetaProp() { }

        public MetaProp(string prop, EditorType editor, string? dataTypeAlias = null, string? tab = null, string? group = null)
        {
            Prop = prop;
            Editor = editor;
            DataTypeName = dataTypeAlias ?? editor.ToString();
            Tab = tab ?? string.Empty;
            Group = group ?? string.Empty;
            DisplayName = prop;
        }

        public MetaProp(PropertyInfo propInfo, MetaPropAttribute attr, Stack<string> propStack, string? tab, string? group)
            : this(propInfo.Name, attr.Editor, attr.DataTypeName, tab ?? attr.Tab, group ?? attr.Group)
        {
            Path = propStack.ToList();
            Path.Reverse();
            DisplayName = !string.IsNullOrEmpty(attr?.DisplayName) ? attr.DisplayName : string.Join(' ', new List<string>(Path) { Prop });
            Ignore = attr.Ignore;
        }

        public MetaProp Clone()
            => new MetaProp
            {
                Prop = this.Prop,
                Path = this.Path,
                Editor = this.Editor,
                DataTypeName = this.DataTypeName,
                Tab = this.Tab,
                Group = this.Group,
                DisplayName = this.DisplayName,
                Ignore = this.Ignore
            };

        public override string ToString()
        {
            var prop = string.Join('.', new List<string>(Path) { Prop });
            return $"{prop} {Editor} [{Tab},{Group}]";
        }
    }
}
