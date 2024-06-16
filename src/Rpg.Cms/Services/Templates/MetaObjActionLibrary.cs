using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class MetaObjActionLibrary : MetaObj
    {
        public MetaObjActionLibrary(string archetype, string? displayName = null)
            : base(archetype, displayName)
        {
            AddProp("Description", "RichText");
            AddAllowedSelf();
        }
    }
}
