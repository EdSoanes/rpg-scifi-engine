using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class ReflectionEngineTests
    {
        [TestMethod]
        public void DiceCalcFunction_StaticMethod()
        {
            var diceCalc = ReflectionEngine.GetDiceCalcFunction<Dice>(() => Rules.Minus);
            Assert.AreEqual("Rules.Minus", diceCalc);
        }

        [TestMethod]
        public void DiceCalcFunction_InstanceMethod()
        {
            var gun = new Gun(10, 10);
            var diceCalc = ReflectionEngine.GetDiceCalcFunction<Dice>(() => gun.CalculateRange);
            Assert.AreEqual($"{gun.Id}.CalculateRange", diceCalc);
        }
    }
}
