using Rpg.Cms.Services.Factories;
using Rpg.ModObjects.Meta;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Rpg.Cms.Services.Synchronizers
{
    public class DocTypeSynchronizer : IDocTypeSynchronizer
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentTypeEditingService _contentTypeEditingService;
        private readonly DocTypeModelFactory _docTypeModelFactory;

        public DocTypeSynchronizer(
            IContentTypeService contentTypeService,
            IContentTypeEditingService contentTypeEditingService,
            DocTypeModelFactory docTypeModelFactory)
        {
            _contentTypeService = contentTypeService;
            _contentTypeEditingService = contentTypeEditingService;
            _docTypeModelFactory = docTypeModelFactory;
        }

        public List<IContentType> GetAllDocTypes(SyncSession session)
        {
            return _contentTypeService.GetAll()
                .Where(x => x.Alias.StartsWith(session.System.Identifier))
                .ToList();
        }

        public async Task<IContentType?> Sync(SyncSession session, MetaObj metaObject, IUmbracoEntity parentFolder)
        {
            var docType = session.GetDocType(session.GetDocTypeAlias(metaObject), faultOnNotFound: false);
            if (docType != null)
            {
                var updateDocType = _docTypeModelFactory.UpdateModel(session, metaObject, docType);
                docType = await UpdateAsync(session, docType, updateDocType);
            }
            else
            {
                var createDocType = _docTypeModelFactory.CreateModel(session, metaObject);
                createDocType.ContainerKey = parentFolder.Key;

                docType = await CreateAsync(session, createDocType);

                if (docType != null && metaObject.AllowedChildArchetypes.Any())
                {

                }
            }


            return docType;
        }

        private async Task<IContentType?> CreateAsync(SyncSession session, ContentTypeCreateModel createDocType)
        {
            var attempt = await _contentTypeEditingService.CreateAsync(createDocType, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            var docType = attempt.Result!;

            session.DocTypes.Add(docType);
            return docType;
        }

        private async Task<IContentType?> UpdateAsync(SyncSession session, IContentType? docType, ContentTypeUpdateModel updateDocType)
        {
            var attempt = await _contentTypeEditingService.UpdateAsync(docType, updateDocType, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException(attempt.Status.ToString());

            session.DocTypes.Remove(docType);

            docType = attempt.Result!;
            session.DocTypes.Add(docType);
            return docType;
        }
    }
}
