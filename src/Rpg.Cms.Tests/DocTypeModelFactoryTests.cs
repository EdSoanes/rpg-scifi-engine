using Rpg.Cms.Extensions;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Tests.Models;
using Rpg.Cyborgs;
using Rpg.Experimental.Server;
using Rpg.Experimental.System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Strings;

namespace Rpg.Cms.Tests
{
    /// <summary>
    /// How a game system's meta data becomes data types and document types
    /// </summary>
    public class DocTypeModelFactoryTests
    {
        private RpgSystem _system;
        private SyncSession _session;

        [SetUp]
        public void Setup()
        {
            _system = new RpgSystems().Register(new CyborgsSystem());
            _session = new SyncSession(Guid.Empty, _system);

            var models = new DataTypeModelFactory().CreateModels(_session, new TestDataTypeRootFolder());
            _session.DataTypes = TestDataType.Convert(models);
        }

        [Test]
        public void DataTypes_OnePerKindOfValue()
        {
            Assert.That(_session.DataTypes.Select(x => x.Name), Is.EqualTo(new[]
            {
                "Cyborgs Integer", "Cyborgs Dice", "Cyborgs Text", "Cyborgs LongText", "Cyborgs Boolean", "Cyborgs Children"
            }));

            var integer = _session.GetDataTypeByName(RpgDataTypes.Integer)!;
            Assert.That(integer.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Integer));

            //Limits are reported by the rules engine, never enforced, so they are not turned into validation
            Assert.That(integer.ConfigurationData, Is.Empty);

            var children = _session.GetDataTypeByName(RpgDataTypes.Children)!;
            Assert.That(children.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultiNodeTreePicker));
            Assert.That(children.ConfigurationData.Keys, Does.Contain("filter"));
        }

        [Test]
        public void AuthorableObjects_AreThoseWithATemplate()
        {
            Assert.That(_system.AuthorableObjects().Select(x => x.Archetype),
                Is.EquivalentTo(new[] { nameof(PlayerCharacter), nameof(MeleeWeapon), nameof(RangedWeapon), nameof(Armour) }));
        }

        [Test]
        public void DocTypeTemplate_ForACharacter()
        {
            var template = _system.AsDocTypeTemplate(_system.GetMetaObject(nameof(PlayerCharacter))!);

            //The template's values, without the name (which is the name of the content), then what the
            //character holds
            Assert.That(template.Props.Select(x => x.Prop),
                Is.EqualTo(new[] { "Strength", "Agility", "Health", "Brains", "Insight", "Charisma", "Hands", "Wearing" }));

            var strength = template.Props.Single(x => x.Prop == "Strength");
            Assert.That(strength.DataTypeName, Is.EqualTo(RpgDataTypes.Integer));
            Assert.That(strength.Group, Is.EqualTo("Stats"));
            Assert.That(strength.Alias, Is.EqualTo("strength"));

            var hands = template.Props.Single(x => x.Prop == "Hands");
            Assert.That(hands.DataTypeName, Is.EqualTo(RpgDataTypes.Children));
            Assert.That(hands.Tab, Is.EqualTo("Gear"));
        }

        [Test]
        public void DocTypeTemplate_ForAWeapon_DiceAreEnteredAsText()
        {
            var template = _system.AsDocTypeTemplate(_system.GetMetaObject(nameof(MeleeWeapon))!);

            Assert.That(template.Props.Select(x => x.Prop), Is.EqualTo(new[] { "Damage", "HitBonus" }));
            Assert.That(template.Props.Single(x => x.Prop == "Damage").DataTypeName, Is.EqualTo(RpgDataTypes.Dice));
            Assert.That(template.Props.Single(x => x.Prop == "HitBonus").Alias, Is.EqualTo("hitBonus"));
        }

        [Test]
        public void DocTypeModel_ForACharacter()
        {
            var template = _system.AsDocTypeTemplate(_system.GetMetaObject(nameof(PlayerCharacter))!);
            var model = new DocTypeModelFactory().CreateModel(_session, template);

            Assert.That(model.Alias, Is.EqualTo("Cyborgs_PlayerCharacter"));
            Assert.That(model.Properties.Count(), Is.EqualTo(8));

            //The alias is a safe form of the property name. The description carries the name itself, which
            //is what turns content back into an object.
            var strength = model.Properties.Single(x => x.Alias == "strength");
            Assert.That(strength.Description, Is.EqualTo("Strength"));
            Assert.That(strength.DataTypeKey, Is.EqualTo(_session.GetDataTypeByName(RpgDataTypes.Integer)!.Key));

            var hands = model.Properties.Single(x => x.Alias == "hands");
            Assert.That(hands.DataTypeKey, Is.EqualTo(_session.GetDataTypeByName(RpgDataTypes.Children)!.Key));

            //Tabs and groups come from the game system's property attributes
            var tabs = model.Containers.Where(x => x.Type == "Tab").Select(x => x.Name).ToArray();
            Assert.That(tabs, Does.Contain("Gear"));

            var statsGroup = model.Containers.Single(x => x.Type == "Group" && x.Name == "Stats");
            Assert.That(strength.ContainerKey, Is.EqualTo(statsGroup.Key));
        }

        [Test]
        public void DocTypeModel_ForALibrary()
        {
            var template = new DocTypeTemplate("Action Arg")
                .SetIsElement(true)
                .AddIcon("icon-rectangle-ellipsis")
                .AddProp("Arg Name", RpgDataTypes.Text)
                .AddProp("Is Nullable", RpgDataTypes.Boolean);

            var model = new DocTypeModelFactory().CreateModel(_session, template);

            Assert.That(model.Alias, Is.EqualTo("Cyborgs_ActionArg"));
            Assert.That(model.IsElement, Is.True);
            Assert.That(model.Icon, Is.EqualTo("icon-rectangle-ellipsis"));
            Assert.That(model.Properties.Select(x => x.Alias), Is.EqualTo(new[] { "argName", "isNullable" }));
            Assert.That(model.Properties.Select(x => x.Name), Is.EqualTo(new[] { "Arg Name", "Is Nullable" }));
        }

        [Test]
        public void Aliases_RoundTripToArchetypes()
        {
            Assert.That(_system.GetDocumentTypeAlias("MeleeWeapon"), Is.EqualTo("Cyborgs_MeleeWeapon"));
            Assert.That(_system.GetDocumentTypeAlias("Entity Library"), Is.EqualTo("Cyborgs_EntityLibrary"));
            Assert.That(_system.GetDocumentTypeAlias("Cyborgs"), Is.EqualTo("Cyborgs"));
            Assert.That(_system.GetArchetype("Cyborgs_MeleeWeapon"), Is.EqualTo("MeleeWeapon"));
        }

        [Test]
        public void PropAliases()
        {
            var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
            var alias = helper.CleanStringForSafeAlias("This is a string");

            Assert.That(alias, Is.Not.Null);
        }
    }
}
