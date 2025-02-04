namespace Rpg.Experimental.System
{
    public class MetaProperty
    {
        public string Prop { get; internal set; }
        public string FullProp { get => string.Join('.', new List<string>(Path) { Prop }); }
        public List<string> Path { get; internal set; } = new List<string>();
        public RpgPropertyType PropertyType { get; internal set; }
        public EditorType Editor { get; internal set; }
        public bool IsNullable { get; internal set; } = true;

        public string DisplayName { get; internal set; }
        public string Tab { get; internal set; } = string.Empty;
        public string Group { get; internal set; } = string.Empty;
        public Dictionary<string, object?> Attributes { get; internal set; } = new();

        public MetaProperty Clone()
        {
            var res = new MetaProperty
            {
                Prop = this.Prop,
                Path = this.Path,
                PropertyType = this.PropertyType,
                Editor = this.Editor,
                Tab = this.Tab,
                Group = this.Group,
                DisplayName = this.DisplayName,
            };

            foreach (var key in Attributes.Keys)
                res.Attributes.Add(key, Attributes[key]);

            return res;
        }

        internal RpgProperty CloneAsProperty()
        {
            var res = new RpgProperty
            {
                Prop = this.Prop,
                Path = this.Path,
                PropertyType = this.PropertyType,
                Editor = this.Editor,
                Tab = this.Tab,
                Group = this.Group,
                DisplayName = this.DisplayName,
            };

            foreach (var key in Attributes.Keys)
                res.Attributes.Add(key, Attributes[key]);

            return res;
        }

        public override string ToString()
        {
            var prop = string.Join('.', new List<string>(Path) { Prop });
            return $"{prop} {Editor} [{Tab},{Group}]";
        }
    }
}
