using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    public class AnEntity : ModdableObject
    {
        public int BaseIntValue { get; set; }

        [Moddable] public Dice ModdedValue { get => Evaluate(); }
        [Moddable] public Dice ModdableValue { get => Evaluate(); }
        [Moddable] public Dice ModdableCalculatedValue { get => Evaluate(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, x => x.BaseIntValue, x => x.ModdedValue),
                BaseModifier.Create(this, x => x.BaseIntValue, x => x.ModdableCalculatedValue, () => this.CalculateValue)
            };
        }

        public Dice CalculateValue(Dice dice)
        {
            return dice.Roll() + 2;
        }
    }

    [TestClass]
    public class ModdableTests
    {
        private AnEntity _anEntity;
        private EntityManager<AnEntity> _meta;

        [TestInitialize]
        public void Initialize()
        {
            _anEntity = new AnEntity
            {
                BaseIntValue = 1
            };

            _meta = new EntityManager<AnEntity>();
            _meta.Initialize(_anEntity);
        }

        [TestMethod]
        public void Mod_Setup_Test()
        {
            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue.Roll());
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue.Roll());
            Assert.AreEqual(0, _anEntity.ModdableValue.Roll());
        }

        [TestMethod]
        public void PropExpr_Path()
        {
            Assert.AreEqual<string>("0", _anEntity.ModdableValue);

            _meta.Mods.Add(BaseModifier.Create(_anEntity, "Buff", "d6", (x) => x.ModdableValue));

            Assert.AreEqual<string>("1d6", _anEntity.ModdableValue);
        }

        [TestMethod]
        public void PropertyValue_By_Path()
        {
            var entity = new AnEntity
            {
                BaseIntValue = 2
            };

            Assert.AreEqual(2, entity.PropertyValue<int>(nameof(AnEntity.BaseIntValue)));
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_Turn3()
        {
            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.Turns.StartEncounter();
            _meta.Mods.Add(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            
            Assert.AreEqual(1, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.Turns.Increment();

            Assert.AreEqual(2, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.Turns.Increment();

            Assert.AreEqual(3, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_Turn3_WithTurnMod()
        {
            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _meta.Turns.StartEncounter();
            _meta.Mods.Add(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            _meta.Mods.Add(TurnModifier.Create("Noise", 10, _anEntity, x => x.ModdedValue));

            Assert.AreEqual(1, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(11, _anEntity.ModdedValue);

            _meta.Turns.Increment();

            Assert.AreEqual(2, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _meta.Turns.Increment();

            Assert.AreEqual(3, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_EndEncounter()
        {
            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.Turns.StartEncounter();
            _meta.Mods.Add(TimedModifier.Create(RemoveTurn.Encounter, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));

            Assert.AreEqual(1, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.Turns.Increment();

            Assert.AreEqual(2, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.Turns.Increment();

            Assert.AreEqual(3, _meta.Turns.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.Turns.EndEncounter();

            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_Remove_WhenZero()
        {
            _meta.Mods.Add(BaseModifier.Create(_anEntity, 1, x => x.ModdableValue));

            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _meta.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _meta.Turns.Increment();
            _meta.Mods.Add(DamageModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(1, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(2, _meta.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _meta.Turns.Increment();

            Assert.AreEqual(2, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.Turns.Increment();

            Assert.AreEqual(3, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.Turns.EndEncounter();

            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.Mods.Add(HealingModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(0, _meta.Turns.Current);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _meta.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());
        }
    }
}
