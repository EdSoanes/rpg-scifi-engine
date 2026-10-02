using Rpg.Cyborgs;
using Rpg.Experimental.System;

namespace Rpg.Experimental.Server.Tests
{
    /// <summary>
    /// A content library held in memory: one character, Benny, carrying a sword and wearing a vest. The
    /// values are given the way a content management system gives them, as loosely typed values by name.
    /// </summary>
    internal class TestContentFactory : IContentFactory
    {
        public static readonly Guid BennyKey = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly RpgSystems _systems;

        public TestContentFactory(RpgSystems systems)
            => _systems = systems;

        public RpgContent[] ListEntities(string systemIdentifier)
            => [new RpgContent { Key = BennyKey, System = systemIdentifier, Archetype = nameof(PlayerCharacter), Name = "Benny" }];

        public RpgObject CreateEntity(string systemIdentifier, string archetype, string contentId)
        {
            if (contentId != BennyKey.ToString())
                throw new RpgServerException($"Could not find {archetype} {contentId}");

            var system = _systems.Get(systemIdentifier);

            var benny = RpgObjectFactory.Create(system, archetype, new Dictionary<string, object?>
            {
                { "name", "Benny" },
                { "strength", -1L },
                { "agility", "0" },
                { "health", 1 },
                { "brains", 1m },
                { "insight", null },
                { "charisma", 1 },
                { "notAProperty", "ignored" }
            });

            var sword = RpgObjectFactory.Create(system, nameof(MeleeWeapon), new Dictionary<string, object?>
            {
                { "Name", "Excalibur" },
                { "Damage", "d6" },
                { "HitBonus", 1 }
            });

            var vest = RpgObjectFactory.Create(system, nameof(Armour), new Dictionary<string, object?>
            {
                { "Name", "Vest" },
                { "ArmourRating", 3 }
            });

            RpgObjectFactory.AddChild(benny, nameof(Actor.Hands), sword);
            RpgObjectFactory.AddChild(benny, nameof(Actor.Wearing), vest);

            return benny;
        }
    }

    internal static class TestServer
    {
        public const string System = "Cyborgs";

        public static (RpgSessionlessServer, RpgSystems) Create()
        {
            var systems = new RpgSystems();
            systems.Register(new CyborgsSystem());

            return (new RpgSessionlessServer(systems, new TestContentFactory(systems)), systems);
        }

        /// <summary>
        /// Send a request the way a web client does: as json, with the response read back from json
        /// </summary>
        public static RpgResponse<TData> Send<TOp, TData>(string sheet, TOp op, Func<RpgRequest<TOp>, RpgResponse<TData>> call)
        {
            var requestJson = RpgServerJson.Serialize(new RpgRequest<TOp> { Sheet = sheet, Op = op });
            var request = RpgServerJson.Deserialize<RpgRequest<TOp>>(requestJson);

            var responseJson = RpgServerJson.Serialize(call(request));
            return RpgServerJson.Deserialize<RpgResponse<TData>>(responseJson);
        }
    }
}
