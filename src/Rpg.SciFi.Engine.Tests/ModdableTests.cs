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
                this.Mod((x) => x.BaseIntValue, (x) => x.ModdedValue),
                this.Mod((x) => x.BaseIntValue, (x) => x.ModdableCalculatedValue, () => CalculateValue)
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

            _meta.AddMod(_anEntity.Mod("Buff", "d6", (x) => x.ModdableValue).IsAdditive());

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
    }
}
