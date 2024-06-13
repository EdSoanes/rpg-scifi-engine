using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class ActionLibraryTemplate : DocTypeTemplate
    {
        public ActionLibraryTemplate(string identifier)
            : base(identifier, "Action Library", "icon-books", false)
        {
            AddProp<RichTextUIAttribute>("Description");
            AddAllowedAlias(new ActionComponentTemplate(identifier).Alias);
            AddAllowedSelf();
        }
    }
}
