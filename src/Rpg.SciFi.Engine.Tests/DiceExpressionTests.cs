using Rpg.SciFi.Engine.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    public class DiceContext
    {
        public int Num { get; set; }
        public string DiceExpr { get; set; }
    }

    [TestClass]
    public class DiceExpressionTests
    {
        [TestMethod]
        public void Simplified()
        {
            var diceContext = new DiceContext();
            var dice = new DiceExpression("2d6 + 7 - d6 + 2", DiceExpressionOptions.SimplifyStringValue);
            var expr = dice.Expression(diceContext);

            Assert.AreEqual("1d6 + 9", expr);
        }

        [TestMethod]
        public void WithSubExpression()
        {
            var diceContext = new DiceContext
            {
                Num = 3,
                DiceExpr = "d6"
            };

            var dice = new DiceExpression("2d6 + [Num] + 2", DiceExpressionOptions.None);
            var expr = dice.Expression(diceContext);

            Assert.AreEqual("2d6 + 3 + 2", expr);
        }

        [TestMethod]
        public void WithSubExpression_Simplified()
        {
            var diceContext = new DiceContext
            {
                Num = 3,
                DiceExpr = "d6"
            };

            var dice = new DiceExpression("2d6 + [Num] + 2", DiceExpressionOptions.SimplifyStringValue);
            var expr = dice.Expression(diceContext);

            Assert.AreEqual("2d6 + 5", expr);
        }

        [TestMethod]
        public void WithSubExpression_DiceExpr()
        {
            var diceContext = new DiceContext
            {
                Num = 3,
                DiceExpr = "d6"
            };

            var dice = new DiceExpression("2d6 + [DiceExpr] + 2", DiceExpressionOptions.SimplifyStringValue);
            var expr = dice.Expression(diceContext);

            Assert.AreEqual("3d6 + 2", expr);
        }
    }
}
