using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Tests
{
    public class DiceContext : ModdableObject
    {
        public int Num { get; set; }
        public string DiceExpr { get; set; }
    }

    [TestClass]
    public class DiceTests
    {
        [TestMethod]
        public void DiceSerialization()
        {
            Dice dice = "1d8";

            var json = JsonConvert.SerializeObject(dice);
            Assert.IsNotNull(json);

            Dice dice2 = JsonConvert.DeserializeObject<Dice>(json);
            Assert.AreEqual("1d8", dice2.ToString());
        }

        [TestMethod]
        public void PositiveExpression()
        {
            Dice dice = "2d6 + 7 - d6 + 2";

            Assert.AreEqual("1d6 + 9", dice.ToString());
        }

        [TestMethod]
        public void NegativeExpression()
        {
            Dice dice = "2d6 - 3d6 + 2";

            Assert.AreEqual("-1d6 + 2", dice.ToString());
        }

        [TestMethod]
        public void WithSubExpression_Minus()
        {
            Dice dice = "-d6";
            Assert.AreEqual("-1d6", dice.ToString());
        }
    }
}
