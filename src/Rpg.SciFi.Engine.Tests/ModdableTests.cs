using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    public class AnEntity : ModdableObject
    {
        [Moddable] public int BaseIntValue { get => Resolve(); }
        [Moddable] public Dice ModdedValue { get => Evaluate(); }
        [Moddable] public Dice ModdableValue { get => Evaluate(); }
        [Moddable] public Dice ModdableCalculatedValue { get => Evaluate(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, "1", x => x.BaseIntValue),
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
        private EntityGraph _graph;

        [TestInitialize]
        public void Initialize()
        {
            _anEntity = new AnEntity();
            _graph = new EntityGraph();
            _graph.Initialize(_anEntity);
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

            _graph.Mods.Add(BaseModifier.Create(_anEntity, "Buff", "d6", (x) => x.ModdableValue));

            Assert.AreEqual<string>("1d6", _anEntity.ModdableValue);
        }

        [TestMethod]
        public void PropertyValue_By_Path()
        {
            _graph.Mods.Add(BaseModifier.Create(_anEntity, "2", x => x.BaseIntValue));

            Assert.AreEqual(2, _anEntity.PropertyValue<int>(nameof(AnEntity.BaseIntValue)));
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_Turn3()
        {
            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _graph.Actions.StartEncounter();
            _graph.Mods.Add(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            
            Assert.AreEqual(1, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _graph.Actions.Increment();

            Assert.AreEqual(2, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _graph.Actions.Increment();

            Assert.AreEqual(3, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_Turn3_WithTurnMod()
        {
            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _graph.Actions.StartEncounter();
            _graph.Mods.Add(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            _graph.Mods.Add(TurnModifier.Create(_anEntity, "Noise", 10, x => x.ModdedValue));

            Assert.AreEqual(1, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(11, _anEntity.ModdedValue);

            _graph.Actions.Increment();

            Assert.AreEqual(2, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _graph.Actions.Increment();

            Assert.AreEqual(3, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_EndEncounter()
        {
            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _graph.Actions.StartEncounter();
            _graph.Mods.Add(TimedModifier.Create(RemoveTurn.Encounter, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));

            Assert.AreEqual(1, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _graph.Actions.Increment();

            Assert.AreEqual(2, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _graph.Actions.Increment();

            Assert.AreEqual(3, _graph.Actions.Current);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _graph.Actions.EndEncounter();

            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_Remove_WhenZero()
        {
            _graph.Mods.Add(BaseModifier.Create(_anEntity, 1, x => x.ModdableValue));

            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _graph.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _graph.Actions.Increment();
            _graph.Mods.Add(DamageModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(1, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(2, _graph.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _graph.Actions.Increment();

            Assert.AreEqual(2, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _graph.Actions.Increment();

            Assert.AreEqual(3, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _graph.Actions.EndEncounter();

            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _graph.Mods.Add(HealingModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(0, _graph.Actions.Current);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _graph.Mods.GetMods(_anEntity, x => x.ModdableValue)!.Count());
        }

        [TestMethod]
        public void BaseIntValue_Update_Notify_RemoveTransientMods_Notify()
        {
            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);

            var propNames = new List<string>();
            _anEntity.PropertyChanged += (s, e) =>
            {
                propNames.Add(e.PropertyName!);
            };

            _graph.Mods.Add(TurnModifier.Create(_anEntity, 2, x => x.BaseIntValue));

            Assert.AreEqual(3, propNames.Count);
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.BaseIntValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdedValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdableCalculatedValue)));

            Assert.AreEqual(3, _anEntity.BaseIntValue);
            Assert.AreEqual(3, _anEntity.ModdedValue);
            Assert.AreEqual(5, _anEntity.ModdableCalculatedValue);

            propNames.Clear();

            _graph.Mods.Remove(_anEntity.Id, nameof(AnEntity.BaseIntValue), ModifierType.Transient);

            Assert.AreEqual(3, propNames.Count);
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.BaseIntValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdedValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdableCalculatedValue)));

            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);
        }

        [TestMethod]
        public void BaseIntValue_SetBaseModZero_NotifyAndUpdateOtherProps()
        {
            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);

            var propNames = new List<string>();
            _anEntity.PropertyChanged += (s, e) =>
            {
                propNames.Add(e.PropertyName!);
            };

            _graph.Mods.Add(BaseModifier.Create(_anEntity, 0, x => x.BaseIntValue));

            Assert.AreEqual(3, propNames.Count);
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.BaseIntValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdedValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdableCalculatedValue)));

            Assert.AreEqual(0, _anEntity.BaseIntValue);
            Assert.AreEqual(0, _anEntity.ModdedValue);
            Assert.AreEqual(2, _anEntity.ModdableCalculatedValue);
        }

        [TestMethod]
        public void BaseIntValue_RemoveMod_NotifyAndUpdateOtherProps()
        {
            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);

            var propNames = new List<string>();
            _anEntity.PropertyChanged += (s, e) =>
            {
                propNames.Add(e.PropertyName!);
            };

            _graph.Mods.Add(TurnModifier.Create(_anEntity, 3, x => x.BaseIntValue));

            Assert.AreEqual(3, propNames.Count);
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.BaseIntValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdedValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdableCalculatedValue)));

            Assert.AreEqual(4, _anEntity.BaseIntValue);
            Assert.AreEqual(4, _anEntity.ModdedValue);
            Assert.AreEqual(6, _anEntity.ModdableCalculatedValue);

            propNames.Clear();
            _graph.Mods.Remove(_anEntity.Id, nameof(AnEntity.BaseIntValue));

            Assert.AreEqual(3, propNames.Count);
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.BaseIntValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdedValue)));
            Assert.IsTrue(propNames.Contains(nameof(AnEntity.ModdableCalculatedValue)));

            Assert.AreEqual(0, _anEntity.BaseIntValue);
            Assert.AreEqual(0, _anEntity.ModdedValue);
            Assert.AreEqual(2, _anEntity.ModdableCalculatedValue);
        }

        [TestMethod]
        public void BaseIntValue_ReplaceBaseWithPlayer_VerifyValues()
        {
            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);

            _graph.Mods.Add(PlayerModifier.Create(_anEntity, 3, x => x.BaseIntValue));

            Assert.AreEqual(3, _anEntity.BaseIntValue);
            Assert.AreEqual(3, _anEntity.ModdedValue);
            Assert.AreEqual(5, _anEntity.ModdableCalculatedValue);

            _graph.Mods.Remove(_anEntity.Id, nameof(AnEntity.BaseIntValue), ModifierType.Player);

            Assert.AreEqual(1, _anEntity.BaseIntValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
            Assert.AreEqual(3, _anEntity.ModdableCalculatedValue);
        }
    }
}
