namespace Rpg.Cms.Services.Templates
{
    public class DocTypeFolderTemplate
    {
        public Guid Key { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Alias { get; set; }

        public DocTypeFolderTemplate(string identifier, string name)
        {
            Name = name;
            Alias = identifier == name
                ? identifier
                : $"{identifier}_{name}";
        }
    }
}
