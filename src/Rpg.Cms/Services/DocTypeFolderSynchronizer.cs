using Rpg.Cms.Services.Templates;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services
{
    public class DocTypeFolderSynchronizer : IDocTypeFolderSynchronizer
    {
        private readonly IContentTypeContainerService _contentTypeContainerService;

        public DocTypeFolderSynchronizer(IContentTypeContainerService contentTypeContainerService)
        {
            _contentTypeContainerService = contentTypeContainerService;
        }

        public async Task<IUmbracoEntity> Synchronize(RpgSyncSession session, DocTypeFolderTemplate template, int parentId)
        {
            var parentFolder = session.DocTypeFolders.FirstOrDefault(x => x.Id == parentId);
            var folder = session.DocTypeFolders.FirstOrDefault(x => x.ParentId == (parentFolder?.Id ?? -1) && x.Name == template.Name);
            if (folder != null)
                return folder;

            var attempt = await _contentTypeContainerService.CreateAsync(template.Key, template.Name, parentFolder?.Key, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            var entityContainer = attempt.Result!;
            session.DocTypeFolders.Add(entityContainer);

            return entityContainer;
        }
    }
}
