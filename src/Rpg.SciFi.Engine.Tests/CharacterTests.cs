using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class CharacterTests
    {
        private Meta<Game> _meta;
        private Game _game = new Game();
        private Gun _gun;
        private Character _target;

        [TestInitialize]
        public void Initialize()
        {
            _gun = new Gun(10, 2)
            {
                Name = "Blaster"
            };

            var player = new Character("The Player");
            player.Equipment.Add(_gun);

            _target = new Character("The Target");

            _game.Character = player;
            _game.Environment.Contains.Add(_target);

            _meta = new Meta<Game>();
            _meta.Initialize(_game);
        }

        [TestMethod]
        public void Rules_CalculateStatBonus()
        {
            Assert.AreEqual<int>(0, Rules.CalculateStatBonus(10).Roll());
            Assert.AreEqual<int>(4, Rules.CalculateStatBonus(18).Roll());
            Assert.AreEqual<int>(-4, Rules.CalculateStatBonus(3).Roll());
        }

        [TestMethod]

        public void Character_Init_DamageBuff()
        {
            Assert.AreEqual<string>("1d6", _game.Character.Damage.BaseImpact);
            Assert.AreEqual<string>("1d6 + 4", _game.Character.Damage.Impact);
        }

        [TestMethod]
        public void Character_Gun_Fire()
        {
            var action = _gun.Fire(_game.Character, _target, 3);

            var diceRollDesc = action.Describe(nameof(TurnAction.DiceRoll));
            var diceRollTargetDesc = action.Describe(nameof(TurnAction.DiceRollTarget));
            var successDesc = action.Success.SelectMany(x => x.Describe(true));
            var failureDesc = action.Failure.SelectMany(x => x.Describe(true));

            Assert.AreEqual<string>("1d20 - 1", action.DiceRoll);
            Assert.AreEqual<string>("11", action.DiceRollTarget);
        }

        [TestMethod]
        public void Character_Gun_Fire_Success()
        {
            var action = _gun.Fire(_game.Character, _target, 3);
            Assert.AreEqual(8, _game.Character.Turns.Action);
            Assert.AreEqual(10, _target.Health.Physical);

            var oldHealth = _target.Health.Physical;
            var nextAction = _meta.Apply(_game.Character, action, 11);
            Assert.IsNull(nextAction);

            Assert.AreEqual(5, _game.Character.Turns.Action);
            Assert.IsTrue(oldHealth > _target.Health.Physical);
        }

        [TestMethod]
        public void Character_Gun_Fire_Failure()
        {
            var action = _gun.Fire(_game.Character, _target, 3);
            Assert.AreEqual(8, _game.Character.Turns.Action);
            Assert.AreEqual(10, _target.Health.Physical);

            var oldHealth = _target.Health.Physical;
            var nextAction = _meta.Apply(_game.Character, action, 4);
            Assert.IsNull(nextAction);

            Assert.AreEqual(5, _game.Character.Turns.Action);
            Assert.IsTrue(oldHealth == _target.Health.Physical);
        }
    }
}
