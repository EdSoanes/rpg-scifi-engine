using Rpg.Cms.Models;
using Rpg.ModObjects;

namespace Rpg.Cms.Controllers.Services
{
    public interface IContentFactory
    {
        RpgContent[] ListEntities(string systemIdentifier);
        RpgEntity GetEntity(string systemIdentifier, string archetype, string id);
    }
}