using Rpg.Cyborgs;
using Rpg.Experimental.System;

namespace Rpg.Experimental.Server.Tests
{
    /// <summary>
    /// Creating game system objects from authored values
    /// </summary>
    public class ObjectFactoryTests
    {
        private RpgSystem _system;

        [SetUp]
        public void Setup()
        {
            var systems = new RpgSystems();
            _system = systems.Register(new CyborgsSystem());
        }

        [Test]
        public void Meta_ObjectsWithATemplateConstructor_SayWhatToAuthor()
        {
            var pc = _system.GetMetaObject(nameof(PlayerCharacter))!;

            Assert.That(pc.Template, Is.Not.Null);
            Assert.That(pc.Template!.TypeName, Is.EqualTo(nameof(PlayerCharacterTemplate)));
            Assert.That(pc.Template.Properties.Select(x => x.Prop),
                Is.EqualTo(new[] { "Name", "Strength", "Agility", "Health", "Brains", "Insight", "Charisma" }));

            //How a value is presented comes from the object's own property
            var strength = pc.Template.Properties.Single(x => x.Prop == "Strength");
            Assert.That(strength.Group, Is.EqualTo("Stats"));
            Assert.That(strength.Editor, Is.EqualTo(EditorType.Int32));

            var sword = _system.GetMetaObject(nameof(MeleeWeapon))!;
            Assert.That(sword.Template!.Properties.Select(x => x.Prop), Is.EqualTo(new[] { "Name", "Damage", "HitBonus" }));
            Assert.That(sword.Template.Properties.Single(x => x.Prop == "Damage").Editor, Is.EqualTo(EditorType.Dice));
        }

        [Test]
        public void Meta_ObjectsWithoutATemplateConstructor_CannotBeAuthored()
        {
            Assert.That(_system.Objects.Single(x => x.Archetype == nameof(BodyPart)).Template, Is.Null);
            Assert.That(RpgObjectFactory.CanCreate(_system, nameof(BodyPart)), Is.False);
            Assert.That(RpgObjectFactory.CanCreate(_system, nameof(PlayerCharacter)), Is.True);
            Assert.That(RpgObjectFactory.CanCreate(_system, "NoSuchThing"), Is.False);

            Assert.Throws<ArgumentException>(() => RpgObjectFactory.Create(_system, nameof(BodyPart), new Dictionary<string, object?>()));
            Assert.Throws<ArgumentException>(() => RpgObjectFactory.Create(_system, "NoSuchThing", new Dictionary<string, object?>()));
        }

        [Test]
        public void Create_FromLooselyTypedValues()
        {
            var obj = RpgObjectFactory.Create(_system, nameof(PlayerCharacter), new Dictionary<string, object?>
            {
                { "name", "Benny" },
                { "STRENGTH", -1L },
                { "Agility", "2" },
                { "Health", 1.0 },
                { "Brains", true },
                { "Insight", "" },
                { "Charisma", "not a number" },
                { "Unknown", 99 }
            });

            var pc = (PlayerCharacter)obj;

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength, Is.EqualTo(-1));
            Assert.That(pc.Agility, Is.EqualTo(2));
            Assert.That(pc.Health, Is.EqualTo(1));
            Assert.That(pc.Brains, Is.EqualTo(1));

            //Nothing usable was authored for these, so they keep what the template starts with
            Assert.That(pc.Insight, Is.EqualTo(0));
            Assert.That(pc.Charisma, Is.EqualTo(0));
        }

        [Test]
        public void Create_DiceValues()
        {
            var sword = (MeleeWeapon)RpgObjectFactory.Create(_system, nameof(MeleeWeapon), new Dictionary<string, object?>
            {
                { "Name", "Excalibur" },
                { "Damage", "2d6 + 1" },
                { "HitBonus", 2 }
            });

            Assert.That(sword.Damage, Is.EqualTo(new Dice("2d6 + 1")));
            Assert.That(sword.HitBonus, Is.EqualTo(2));
        }

        [Test]
        public void AddChild_PutsGearOnACharacter_AndTheSheetWorks()
        {
            var pc = (PlayerCharacter)RpgObjectFactory.Create(_system, nameof(PlayerCharacter), new Dictionary<string, object?> { { "Name", "Benny" }, { "Health", 1 } });
            var sword = RpgObjectFactory.Create(_system, nameof(MeleeWeapon), new Dictionary<string, object?> { { "Name", "Excalibur" }, { "Damage", "d6" } });

            Assert.That(RpgObjectFactory.AddChild(pc, "hands", sword), Is.True);
            Assert.That(RpgObjectFactory.AddChild(pc, "hands", sword), Is.True);
            Assert.That(RpgObjectFactory.AddChild(pc, "NoSuchProperty", sword), Is.False);
            Assert.That(pc.Hands.Count, Is.EqualTo(1));

            var sheet = new RpgCharacterSheet(pc, _system);

            Assert.That(pc.StaminaPoints, Is.EqualTo(14));
            Assert.That(sheet.GetObjectAction(sword.Id, "MeleeAttack")!.IsPerformable, Is.True);
        }
    }
}
