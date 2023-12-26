using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    public class AnEntity : Entity
    {
        public int BaseIntValue { get; set; }

        [Moddable] public Dice ModdedValue { get => this.Evaluate(nameof(ModdedValue)); }
        [Moddable] public Dice ModdableValue { get => this.Evaluate(nameof(ModdableValue)); }
        [Moddable] public Dice ModdableCalculatedValue { get => this.Evaluate(nameof(ModdableCalculatedValue)); }

        [Setup] public Modifier[] Setup()
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
        private Meta<AnEntity> _meta;

        [TestInitialize]
        public void Initialize()
        {
            _anEntity = new AnEntity
            {
                BaseIntValue = 1
            };

            _meta = new Meta<AnEntity>();
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

            _meta.AddMod(BaseModifier.Create(_anEntity, "Buff", "d6", (x) => x.ModdableValue));

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
            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.StartEncounter();
            _meta.AddMod(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            
            Assert.AreEqual(1, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.IncrementTurn();

            Assert.AreEqual(2, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.IncrementTurn();

            Assert.AreEqual(3, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_Turn3_WithTurnMod()
        {
            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _meta.StartEncounter();
            _meta.AddMod(TimedModifier.Create(3, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));
            _meta.AddMod(TurnModifier.Create("Noise", 10, _anEntity, x => x.ModdedValue));

            Assert.AreEqual(1, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(11, _anEntity.ModdedValue);

            _meta.IncrementTurn();

            Assert.AreEqual(2, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);

            _meta.IncrementTurn();

            Assert.AreEqual(3, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(1, _anEntity.ModdedValue);
        }

        [TestMethod]
        public void ModdableValue_RemoveOn_EndEncounter()
        {
            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.StartEncounter();
            _meta.AddMod(TimedModifier.Create(RemoveTurn.Encounter, _anEntity, "Weakened", 1, x => x.ModdableValue, () => Rules.Minus));

            Assert.AreEqual(1, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.IncrementTurn();

            Assert.AreEqual(2, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.IncrementTurn();

            Assert.AreEqual(3, _meta.CurrentTurn);
            Assert.AreEqual(-1, _anEntity.ModdableValue);

            _meta.EndEncounter();

            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);
        }

        [TestMethod]
        public void ModdableValue_Remove_WhenZero()
        {
            _meta.AddMod(BaseModifier.Create(_anEntity, 1, x => x.ModdableValue));

            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _meta.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _meta.StartEncounter();
            _meta.AddMod(DamageModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(1, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);
            Assert.AreEqual(2, _meta.GetMods(_anEntity, x => x.ModdableValue)!.Count());

            _meta.IncrementTurn();

            Assert.AreEqual(2, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.IncrementTurn();

            Assert.AreEqual(3, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.EndEncounter();

            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(0, _anEntity.ModdableValue);

            _meta.AddMod(HealingModifier.Create(1, _anEntity, x => x.ModdableValue));

            Assert.AreEqual(0, _meta.CurrentTurn);
            Assert.AreEqual(1, _anEntity.ModdableValue);
            Assert.AreEqual(1, _meta.GetMods(_anEntity, x => x.ModdableValue)!.Count());
        }
    }
}
