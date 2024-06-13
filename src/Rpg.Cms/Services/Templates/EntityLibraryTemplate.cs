using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class EntityLibraryTemplate : DocTypeTemplate
    {
        public EntityLibraryTemplate(string identifier)
            : base(identifier, "Entity Library", "GitBook", false)
        {
            AddProp<RichTextUIAttribute>("Description");
            AddAllowedSelf();
        }
    }
}
