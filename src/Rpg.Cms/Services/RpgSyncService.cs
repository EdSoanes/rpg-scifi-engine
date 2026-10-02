using Rpg.Experimental.Server;
using Rpg.Experimental.System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Rpg.Cms.Services
{
    /// <summary>
    /// Brings the content management system into line with a game system: its data types, its document
    /// types, and the library content that every system has.
    /// </summary>
    public class RpgSyncService
    {
        private readonly SyncSessionFactory _syncSessionFactory;
        private readonly ISyncTypesService _syncTypesService;
        private readonly ISyncContentService _syncContentService;

        public RpgSyncService(SyncSessionFactory syncSessionFactory, ISyncTypesService syncTypesService, ISyncContentService syncContentService)
        {
            _syncSessionFactory = syncSessionFactory;
            _syncTypesService = syncTypesService;
            _syncContentService = syncContentService;
        }

        public async Task Sync(RpgSystem system, Guid userKey)
        {
            var session = _syncSessionFactory.CreateSession(userKey, system);

            await _syncTypesService.Sync(session);
            await _syncContentService.Sync(session);
        }
    }

    public class RpgCmsOptions
    {
        public const string Section = "Rpg";

        /// <summary>
        /// Synchronise every game system when the site starts. Useful for a new or empty database, since
        /// everything the sync creates can be created again.
        /// </summary>
        public bool SyncOnStartup { get; set; }
    }

    /// <summary>
    /// Runs the sync when the site has started, if the settings ask for it
    /// </summary>
    public class RpgSyncOnStartup : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
    {
        private readonly IConfiguration _configuration;
        private readonly IRuntimeState _runtimeState;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RpgSystems _systems;
        private readonly ILogger<RpgSyncOnStartup> _logger;

        public RpgSyncOnStartup(
            IConfiguration configuration,
            IRuntimeState runtimeState,
            IServiceScopeFactory serviceScopeFactory,
            RpgSystems systems,
            ILogger<RpgSyncOnStartup> logger)
        {
            _configuration = configuration;
            _runtimeState = runtimeState;
            _serviceScopeFactory = serviceScopeFactory;
            _systems = systems;
            _logger = logger;
        }

        public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
        {
            var options = _configuration.GetSection(RpgCmsOptions.Section).Get<RpgCmsOptions>() ?? new RpgCmsOptions();
            if (!options.SyncOnStartup || _runtimeState.Level != RuntimeLevel.Run)
                return;

            using var scope = _serviceScopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<RpgSyncService>();

            foreach (var system in _systems.All())
            {
                try
                {
                    await syncService.Sync(system, Constants.Security.SuperUserKey);
                    _logger.LogInformation("Rpg system {System} synchronised", system.Identifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Rpg system {System} could not be synchronised", system.Identifier);
                }
            }
        }
    }
}
