using Rpg.Cms.Services.Templates;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Synchronizers
{
    public class DocTypeFolderSynchronizer : IDocTypeFolderSynchronizer
    {
        private readonly IEntityService _entityService;
        private readonly IContentTypeContainerService _contentTypeContainerService;

        public DocTypeFolderSynchronizer(
            IContentTypeContainerService contentTypeContainerService, 
            IEntityService entityService)
        {
            _contentTypeContainerService = contentTypeContainerService;
            _entityService = entityService;
        }

        public List<IUmbracoEntity> GetAllDocTypeFolders(SyncSession session)
        {
            var folders = new List<IUmbracoEntity>();

            var rootFolder = _entityService.GetPagedChildren(
                Constants.System.RootKey,
                UmbracoObjectTypes.DocumentTypeContainer,
                0,
                int.MaxValue,
                out var totalItems)
                .FirstOrDefault(x => x.Name == session.System.Identifier);

            if (rootFolder != null)
            {
                folders.Add(rootFolder);
                var descendants = _entityService.GetDescendants(rootFolder.Id)
                    .Where(x => x.NodeObjectType == UmbracoObjectTypes.DocumentTypeContainer.GetGuid());

                folders.AddRange(descendants);
            }

            return folders;
        }

        public async Task<IUmbracoEntity> Sync(SyncSession session, string folderName, int parentId)
        {
            var parentFolder = session.DocTypeFolders.FirstOrDefault(x => x.Id == parentId);
            var folder = session.DocTypeFolders.FirstOrDefault(x => x.ParentId == (parentFolder?.Id ?? -1) && x.Name == folderName);
            if (folder != null)
                return folder;

            var attempt = await _contentTypeContainerService.CreateAsync(Guid.NewGuid(), GetFolderName(session, folderName), parentFolder?.Key, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            var entityContainer = attempt.Result!;
            session.DocTypeFolders.Add(entityContainer);

            return entityContainer;
        }

        private string GetFolderName(SyncSession session, string? folderName = null)
            => !string.IsNullOrEmpty(folderName) && session.System.Identifier != folderName
                ? $"{session.System.Identifier}.{folderName}"
                : session.System.Identifier;
    }
}
