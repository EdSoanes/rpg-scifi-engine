using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests
{
    public class SerializationTests
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void RpgMethod_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var method = new RpgMethod<ScoreBonusValue, Dice>(entity.Strength, nameof(ScoreBonusValue.CalculateStatBonus));

            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(ScoreBonusValue.CalculateStatBonus)));
            Assert.That(method.ClassName, Is.Null);

            var argSet = method.CreateArgSet();
            Assert.That(argSet.Count(), Is.EqualTo(1));
            Assert.That(argSet.ArgNames, Does.Contain("dice"));

            var json = JsonConvert.SerializeObject(method, JsonSettings)!;
            var method2 = JsonConvert.DeserializeObject<RpgMethod<ScoreBonusValue, Dice>>(json, JsonSettings)!;
            Assert.That(method2, Is.Not.Null);
            Assert.That(method2.MethodName, Is.EqualTo(nameof(ScoreBonusValue.CalculateStatBonus)));
            Assert.That(method2.ClassName, Is.Null);

            var argSet2 = method2.CreateArgSet();
            Assert.That(argSet2.Count(), Is.EqualTo(1));
            Assert.That(argSet2.ArgNames, Does.Contain("dice"));
        }


        [Test]
        public void BaseMod_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var method = new RpgMethod<ScoreBonusValue, Dice>(entity.Strength, nameof(ScoreBonusValue.CalculateStatBonus));

            var baseMod = new PermanentMod()
                .SetProps(entity.Strength, x => x.Bonus, 2, () => DiceCalculations.CalculateStatBonus)
                .Create();

            Assert.That(baseMod, Is.Not.Null);
            Assert.That(baseMod.Name, Is.EqualTo(nameof(ScoreBonusValue.Bonus)));

            var json = JsonConvert.SerializeObject(baseMod, JsonSettings)!;
            var baseMod2 = JsonConvert.DeserializeObject<Mod>(json, JsonSettings)!;

            Assert.That(baseMod2, Is.Not.Null);
            Assert.That(baseMod2.Name, Is.EqualTo(nameof(ScoreBonusValue.Bonus)));
        }
    }
}
