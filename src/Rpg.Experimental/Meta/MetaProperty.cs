namespace Rpg.Experimental.Meta
{
    public class MetaProperty
    {
        public string Prop { get; set; }
        public string FullProp { get => string.Join('.', new List<string>(Path) { Prop }); }
        public List<string> Path { get; set; } = new List<string>();

        public string DisplayName { get; set; }
        public EditorType Editor { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public Dictionary<string, object?> Properties { get; set; } = new();

        public MetaProperty Clone()
            => new MetaProperty
            {
                Prop = this.Prop,
                Path = this.Path,
                Editor = this.Editor,
                Tab = this.Tab,
                Group = this.Group,
                DisplayName = this.DisplayName,
            };

        public override string ToString()
        {
            var prop = string.Join('.', new List<string>(Path) { Prop });
            return $"{prop} {Editor} [{Tab},{Group}]";
        }
    }
}
