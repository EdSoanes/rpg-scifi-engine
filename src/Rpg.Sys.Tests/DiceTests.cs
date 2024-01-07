using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class DiceTests
    {
        [Test]
        public void DiceSerialization()
        {
            Dice dice = "1d8";

            var json = JsonConvert.SerializeObject(dice);
            Assert.That(json, Is.Not.Null);

            Dice dice2 = JsonConvert.DeserializeObject<Dice>(json);
            Assert.That(dice2.ToString(), Is.EqualTo("1d8"));
        }

        [Test]
        public void PositiveExpression()
        {
            Dice dice = "2d6 + 7 - d6 + 2";

            Assert.That(dice.ToString(), Is.EqualTo("1d6 + 9"));
        }

        [Test]
        public void NegativeExpression()
        {
            Dice dice = "2d6 - 3d6 + 2";

            Assert.That(dice.ToString(), Is.EqualTo("-1d6 + 2"));
        }

        [Test]
        public void WithSubExpression_Minus()
        {
            Dice dice = "-d6";
            Assert.That(dice.ToString(), Is.EqualTo("-1d6"));
        }
    }
}
