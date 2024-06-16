using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class MetaObjSystem : MetaObj
    {
        public MetaObjSystem(string name, string? displayName = null)
        : base(name, displayName)
        {
            AddProp("Identifier", "Text");
            AddProp("Version", "Text");
            AddProp("Description", "RichText");
            AddAllowedSelf();
        }
    }
}
