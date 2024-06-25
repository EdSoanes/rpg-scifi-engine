using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services
{
    public class SyncContentService : ISyncContentService
    {
        private readonly IContentService _contentService;
        private readonly IContentEditingService _contentEditingService;
        private readonly IContentPublishingService _contentPublishingService;

        public SyncContentService(
            IContentService contentService,
            IContentEditingService contentEditingService,
            IContentPublishingService contentPublishingService)
        {
            _contentService = contentService;
            _contentEditingService = contentEditingService;
            _contentPublishingService = contentPublishingService;
        }

        public async Task Sync(SyncSession session)
        {
            var systemRoot = await EnsureSystemRootAsync(session);
            var sysChildren = _contentService.GetPagedChildren(systemRoot!.Id, 0, 10000, out _);
            var entityLibrary = await EnsureContentAsync(session, "Entity Library", session.EntityLibraryDocType!.Key, systemRoot, sysChildren);
            var actionLibrary = await EnsureContentAsync(session, "Action Library", session.ActionLibraryDocType!.Key, systemRoot, sysChildren);
            var stateLibrary = await EnsureContentAsync(session, "State Library", session.StateLibraryDocType!.Key, systemRoot, sysChildren);

            var actionSiblings = _contentService.GetPagedChildren(actionLibrary.Id, 0, 10000, out _);
            foreach (var action in session.System.Actions)
            {
                var name = $"{action.OwnerArchetype}.{action.Name}";
                await EnsureContentAsync(session, name, session.ActionDocType!.Key, actionLibrary, actionSiblings);
            }

            var stateSiblings = _contentService.GetPagedChildren(stateLibrary.Id, 0, 10000, out _);
            foreach (var state in session.System.States)
            {
                var name = $"{state.Archetype}.{state.Name}";
                await EnsureContentAsync(session, name, session.StateDocType!.Key, stateLibrary, stateSiblings);
            }
        }

        private async Task<IContent> EnsureSystemRootAsync(SyncSession session)
        {
            var systemRoot = _contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == session.SystemDocType!.Alias);
            if (systemRoot == null)
            {
                var props = new Dictionary<string, object>
                {
                    { "Identifier", session.System.Identifier },
                    { "Version", session.System.Version },
                    { "Description", session.System.Description }
                };

                systemRoot = await CreateContentAsync(session, session.System.Identifier, session.SystemDocType!.Key, Constants.System.RootKey, props);
            }

            if (!systemRoot.Published)
                systemRoot = await PublishAsync(session, systemRoot.Key, session.System.Identifier);

            return systemRoot;
        }

        private async Task<IContent> EnsureContentAsync(SyncSession session, string name, Guid docTypeKey, IContent parent, IEnumerable<IContent>? siblings, Dictionary<string, object>? props = null)
        {
            siblings ??= _contentService.GetPagedChildren(parent!.Id, 0, 10000, out var totalRecords);
            var content = siblings.FirstOrDefault(x => x.ContentType.Key == docTypeKey && x.Name == name);

            return content == null
                ? await CreateContentAsync(session, name, docTypeKey, parent.Key)
                : await UpdateContentAsync(session, content.Key, name, props);
        }

        private async Task<IContent> CreateContentAsync(SyncSession session, string name, Guid docTypeKey, Guid? parentKey, Dictionary<string, object>? props = null)
        {
            var model = new ContentCreateModel
            {
                ContentTypeKey = docTypeKey,
                InvariantName = name,
                ParentKey = parentKey
            };

            if (props != null)
            {
                var invariantProperties = new List<PropertyValueModel>();

                foreach (var prop in props)
                    invariantProperties.Add(new PropertyValueModel { Alias = prop.Key, Value = prop.Value });

                model.InvariantProperties = invariantProperties;
            }

            var attempt = await _contentEditingService.CreateAsync(model, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Failed to create {name} {attempt.Status}", attempt.Exception);

            var content = await PublishAsync(session, attempt.Result.Content!.Key, name);
            return content;
        }

        private async Task<IContent> UpdateContentAsync(SyncSession session, Guid contentKey, string name, Dictionary<string, object>? props = null)
        {
            var model = new ContentUpdateModel
            {
                InvariantName = name,
            };

            if (props != null)
            {
                var invariantProperties = new List<PropertyValueModel>();

                foreach (var prop in props)
                    invariantProperties.Add(new PropertyValueModel { Alias = prop.Key, Value = prop.Value });

                model.InvariantProperties = invariantProperties;
            }

            var attempt = await _contentEditingService.UpdateAsync(contentKey, model, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Failed to update {name}");

            var content = await PublishAsync(session, contentKey, name);
            return content;
        }

        private async Task<IContent> PublishAsync(SyncSession session, Guid contentKey, string name)
        {
            var schedule = new ContentScheduleCollection();
            schedule.Add(new ContentSchedule("*", DateTime.UtcNow, ContentScheduleAction.Release));

            var publishModel = new CultureAndScheduleModel
            {
                CulturesToPublishImmediately = new HashSet<string> { "*" },
                Schedules = schedule
            };

            var attempt = await _contentPublishingService.PublishAsync(contentKey, publishModel, session.UserKey);
            if (!attempt.Success)
                throw new InvalidOperationException($"Failed to publish {name} {attempt.Status}", attempt.Exception);

            return attempt.Result.Content!;
        }
    }
}
