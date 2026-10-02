using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;

namespace Rpg.Cyborgs.Tests
{
    public class MetaGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void BuildSystem_Discovered_EnsureValues()
        {
            var system = RpgSystemFactory.Build();

            Assert.That(system, Is.Not.Null);
            Assert.That(system.Identifier, Is.EqualTo("Cyborgs"));
            Assert.That(system.Name, Is.EqualTo("Cyborgs & Sidearms"));
            Assert.That(system.Namespaces, Does.Contain("Rpg.Cyborgs"));
        }

        [Test]
        public void BuildSystem_EnsureObjects()
        {
            var system = RpgSystemFactory.Build(new CyborgsSystem());

            var archetypes = system.Objects.Select(x => x.Archetype).ToArray();
            Assert.That(archetypes, Does.Contain(nameof(PlayerCharacter)));
            Assert.That(archetypes, Does.Contain(nameof(BodyPart)));
            Assert.That(archetypes, Does.Contain(nameof(MeleeWeapon)));
            Assert.That(archetypes, Does.Contain(nameof(RangedWeapon)));
            Assert.That(archetypes, Does.Contain(nameof(Armour)));

            //Abstract types and actions are not objects
            Assert.That(archetypes, Does.Not.Contain(nameof(Actor)));
            Assert.That(archetypes, Does.Not.Contain(nameof(Item)));
            Assert.That(archetypes, Does.Not.Contain(nameof(MeleeAttack)));

            var pc = system.GetMetaObject(nameof(PlayerCharacter))!;
            Assert.That(pc.Archetypes, Is.EqualTo(new[] { nameof(RpgObject), nameof(Actor), nameof(PlayerCharacter) }));

            var strength = pc.Properties.FirstOrDefault(x => x.Prop == nameof(Actor.Strength));
            Assert.That(strength, Is.Not.Null);
            Assert.That(strength.PropertyType, Is.EqualTo(RpgPropertyType.Int));
            Assert.That(strength.Editor, Is.EqualTo(EditorType.Int32));
            Assert.That(strength.Group, Is.EqualTo("Stats"));

            var hands = pc.Properties.FirstOrDefault(x => x.Prop == nameof(Actor.Hands));
            Assert.That(hands, Is.Not.Null);
            Assert.That(hands.PropertyType, Is.EqualTo(RpgPropertyType.Children));
            Assert.That(hands.Tab, Is.EqualTo("Gear"));

            var head = pc.Properties.FirstOrDefault(x => x.Prop == nameof(Actor.Head));
            Assert.That(head, Is.Not.Null);
            Assert.That(head.PropertyType, Is.EqualTo(RpgPropertyType.Child));

            var injurySeverity = system.GetMetaObject(nameof(BodyPart))!.Properties.FirstOrDefault(x => x.Prop == nameof(BodyPart.InjurySeverity));
            Assert.That(injurySeverity, Is.Not.Null);
            Assert.That(injurySeverity.Editor, Is.EqualTo(EditorType.Select));
        }

        [Test]
        public void BuildSystem_EnsureActionsAndStates()
        {
            var system = RpgSystemFactory.Build(new CyborgsSystem());

            var pc = system.GetMetaObject(nameof(PlayerCharacter))!;
            Assert.That(pc.AllowedActions.Select(x => x.Name).Order().ToArray(),
                Is.EqualTo(new[] { "Aim", "ArmourCheck", "MeleeParry", "Run", "TakeDamage", "TakeInjury" }));
            Assert.That(pc.AllowedStates.Count, Is.EqualTo(10));
            Assert.That(pc.AllowedStates.Select(x => x.Name), Does.Contain(nameof(VeryFast)));

            var sword = system.GetMetaObject(nameof(MeleeWeapon))!;
            Assert.That(sword.AllowedActions.Select(x => x.Name).Order().ToArray(),
                Is.EqualTo(new[] { "MeleeAttack", "Transfer" }));

            var bodyPart = system.GetMetaObject(nameof(BodyPart))!;
            Assert.That(bodyPart.AllowedStates.Count, Is.EqualTo(10));
            Assert.That(bodyPart.AllowedActions.Count, Is.EqualTo(0));
        }
    }
}
