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

        [Setup] public void Setup()
        {
            this.Mod(() => BaseIntValue, () => ModdedValue).IsBase().Apply();
            this.Mod(() => BaseIntValue, () => ModdableCalculatedValue, () => CalculateValue).Apply();
        }

        public void Buff()
        {
            this.Mod("Buff", "d6", () => ModdableValue).IsInstant().Apply();
        }

        public Dice CalculateValue(Dice dice)
        {
            return dice.Roll() + 2;
        }
    }

    [TestClass]
    public class ModdableTests
    {
        [TestMethod]
        public void Mod_Setup_Test()
        {
            var entity = new AnEntity
            {
                BaseIntValue = 1
            };

            Meta.Initialize(entity);

            Assert.AreEqual(1, entity.BaseIntValue);
            Assert.AreEqual(1, entity.ModdedValue.Roll());
            Assert.AreEqual(3, entity.ModdableCalculatedValue.Roll());
            Assert.AreEqual(0, entity.ModdableValue.Roll());
        }

        [TestMethod]
        public void Mod_ModdableValue_Test()
        {
            var entity = new AnEntity
            {
                BaseIntValue = 1
            };

            Meta.Initialize(entity);

            entity.Buff();

            Assert.AreEqual<string>("1d6", entity.ModdableValue);
        }

        [TestMethod]
        public void PropExpr_Path()
        {
            var entity = new AnEntity
            {
                BaseIntValue = 1
            };

            Meta.Initialize(entity);

            Assert.AreEqual<string>("0", entity.ModdableValue);

            entity.Mod("Buff", "d6", () => entity.ModdableValue).IsInstant().Apply();

            Assert.AreEqual<string>("1d6", entity.ModdableValue);
        }
    }
}
