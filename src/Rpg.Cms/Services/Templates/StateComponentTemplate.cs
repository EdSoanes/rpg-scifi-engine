using Rpg.ModObjects.Meta;

namespace Rpg.Cms.Services.Templates
{
    public class StateComponentTemplate : DocTypeTemplate
    {
        public StateComponentTemplate(string identifier)
            : base(identifier, "State", "icon-checkbox", false)
        {
            AddProp<RichTextUIAttribute>("Description");
        }
    }
}
