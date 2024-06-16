using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class MetaObjEntityLibrary : MetaObj
    {
        public MetaObjEntityLibrary(string archetype, string? displayName = null)
            : base(archetype, displayName)
        {
            AddProp("Description", "RichText");
            AddAllowedSelf();
        }
    }
}
