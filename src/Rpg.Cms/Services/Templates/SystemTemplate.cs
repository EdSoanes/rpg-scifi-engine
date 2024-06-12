using Rpg.ModObjects.Meta;
using Rpg.Sys.Archetypes;
using static Umbraco.Cms.Core.Constants;

namespace Rpg.Cms.Services.Templates
{
    public class SystemTemplate : DocTypeTemplate
    {
        public SystemTemplate(string identifier)
        : base(identifier, identifier, "icon-settings", true)
        {
            AddProp<TextUIAttribute>("Identifier");
            AddProp<TextUIAttribute>("Version");
            AddProp<RichTextUIAttribute>("Description");
            AddAllowedAlias(new ActionLibraryTemplate(identifier).Alias);
            AddAllowedAlias(new EntityLibraryTemplate(identifier).Alias);
        }
    }
}
