using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Tests
{
    public class DiceContext : Entity
    {
        public int Num { get; set; }
        public string DiceExpr { get; set; }
    }

    [TestClass]
    public class DiceExpressionTests
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
        public void WithSubExpression()
        {
            Meta.Initialize(new DiceContext
            {
                Num = 3,
                DiceExpr = "d6"
            });

            Dice dice = "2d6 + [Num] + 2";
            Assert.AreEqual("2d6 + 5", dice.ToString());
        }

        [TestMethod]
        public void WithSubExpression_Simplified()
        {
            Meta.Initialize(new DiceContext
            {
                Num = 3,
                DiceExpr = "d6"
            });

            Dice dice = "2d6 + [DiceExpr] + 2";
            Assert.AreEqual("3d6 + 2", dice.ToString());
        }

        [TestMethod]
        public void WithSubExpression_AvgMinMax()
        {
            Meta.Initialize(new DiceContext
            {
                Num = 3,
                DiceExpr = "d6+2"
            });

            Dice dice = "2d6 + [DiceExpr] + 2 - [Num]";
            Assert.AreEqual("3d6 + 1", dice.ToString());
            Assert.AreEqual(4, dice.Min());
            Assert.AreEqual(19, dice.Max());
            Assert.AreEqual(11.5, dice.Avg());
        }

        [TestMethod]
        public void WithSubExpression_Minus()
        {
            Dice dice = "-d6";
            Assert.AreEqual("-1d6", dice.ToString());
        }
    }
}
