namespace Rpg.ModObjects.Server.Services
{
    public interface IContentFactory
    {
        RpgContent[] ListEntities(string systemIdentifier);
        RpgEntity CreateEntity(string systemIdentifier, string archetype, string contentId);
    }
}