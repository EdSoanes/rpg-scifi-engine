using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services.Synchronizers
{
    public class DataTypeFolderSynchronizer : IDataTypeFolderSynchronizer
    {
        private readonly IEntityService _entityService;
        private readonly IDataTypeContainerService _dataTypeContainerService;

        public DataTypeFolderSynchronizer(
            IDataTypeContainerService dataTypeContainerService,
            IEntityService entityService)
        {
            _dataTypeContainerService = dataTypeContainerService;
            _entityService = entityService;
        }

        public async Task<IUmbracoEntity> Sync(SyncSession session)
        {
            if (session.RootDataTypeFolder != null)
                return session.RootDataTypeFolder;

            var folder = GetRootDataTypeFolder(session);
            if (folder != null)
                return folder;

            var attempt = await _dataTypeContainerService.CreateAsync(null, session.System.Identifier, null, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Create datatype folder {session.System.Identifier} failed with {attempt.Status}");

            return attempt.Result!;
        }

        private IUmbracoEntity? GetRootDataTypeFolder(SyncSession session)
        {
            var folders = new List<IUmbracoEntity>();

            var rootFolder = _entityService.GetPagedChildren(
                Constants.System.RootKey,
                UmbracoObjectTypes.DataTypeContainer,
                0,
                int.MaxValue,
                out var totalItems)
                .FirstOrDefault(x => x.Name == session.System.Identifier);

            return rootFolder;
        }
    }
}
