namespace Rpg.ModObjects.Meta
{
    public interface IMetaSystem
    {
        string Identifier { get; }
        string[]? Namespaces { get; set; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
        MetaObj[] Objects { get; set; }
        MetaAction[] ActionTemplates { get; set; }
        MetaState[] States { get; set; }
        MetaPropAttr[] PropUIs { get; set; }
        MetaObj AsContentTemplate(MetaObj obj);
    }
}
