using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class ActionComponentTemplate : DocTypeTemplate
    {
        public ActionComponentTemplate(string identifier)
            : base(identifier, "Action", "icon-command", false)
        {
            AddProp<RichTextUIAttribute>("Description");
        }
    }
}
