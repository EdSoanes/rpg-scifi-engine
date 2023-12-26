using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Gear;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
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

            _meta.AddMod(PlayerModifier.Create(player, 18, x => x.Stats.BaseStrength));
            _meta.AddMod(PlayerModifier.Create(player, 5, x => x.Stats.BaseDexterity));
            _meta.AddMod(PlayerModifier.Create(player, 14, x => x.Stats.BaseIntelligence));

            _meta.AddMod(PlayerModifier.Create(player, 5, x => x.Turns.BaseAction));
            _meta.AddMod(PlayerModifier.Create(player, 5, x => x.Turns.BaseExertion));
            _meta.AddMod(PlayerModifier.Create(player, 5, x => x.Turns.BaseFocus));

            _meta.AddMod(PlayerModifier.Create(_target, 18, x => x.Stats.BaseStrength));
            _meta.AddMod(PlayerModifier.Create(_target, 5, x => x.Stats.BaseDexterity));
            _meta.AddMod(PlayerModifier.Create(_target, 14, x => x.Stats.BaseIntelligence));

            _meta.AddMod(PlayerModifier.Create(_target, 5, x => x.Turns.BaseAction));
            _meta.AddMod(PlayerModifier.Create(_target, 5, x => x.Turns.BaseExertion));
            _meta.AddMod(PlayerModifier.Create(_target, 5, x => x.Turns.BaseFocus));
        }

        [TestMethod]
        public void Rules_CalculateStatBonus()
        {
            Assert.AreEqual(0, Rules.CalculateStatBonus(10).Roll());
            Assert.AreEqual(4, Rules.CalculateStatBonus(18).Roll());
            Assert.AreEqual(-4, Rules.CalculateStatBonus(3).Roll());
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
            var successDesc = action.Success.SelectMany(x => _meta.Describe(x, true));
            var failureDesc = action.Failure.SelectMany(x => _meta.Describe(x, true));

            Assert.AreEqual<string>("1d20 - 2", action.DiceRoll);
            Assert.AreEqual<string>("11", action.DiceRollTarget);
        }

        [TestMethod]
        public void Character_Gun_Fire_Success()
        {
            Assert.AreEqual(10, _target.Health.Physical);
            var action = _gun.Fire(_game.Character, _target, 3);

            var desc = _game.Character.Describe(x => x.Turns.Action);
            var descHealth = _game.Character.Describe(x => x.Health.Physical);

            Assert.AreEqual(10, _target.Health.Physical);
            Assert.AreEqual(2, _game.Character.Turns.Action);
            Assert.AreEqual(9, _game.Character.Turns.Exertion);
            Assert.AreEqual(7, _game.Character.Turns.Focus);

            var oldHealth = _target.Health.Physical;
            var nextAction = _meta.Apply(_game.Character, action, 11);
            Assert.IsNull(nextAction);

            var da = _game.Character.Describe(x => x.Turns.Action);
            var de = _game.Character.Describe(x => x.Turns.Exertion);
            var df = _game.Character.Describe(x => x.Turns.Focus);

            Assert.IsTrue(oldHealth > _target.Health.Physical);
            Assert.AreEqual(-1, _game.Character.Turns.Action);
            Assert.AreEqual(8, _game.Character.Turns.Exertion);
            Assert.AreEqual(6, _game.Character.Turns.Focus);
        }

        [TestMethod]
        public void Character_Gun_Fire_Failure()
        {
            Assert.AreEqual(10, _target.Health.Physical);
            var action = _gun.Fire(_game.Character, _target, 3);

            var desc = _game.Character.Describe(x => x.Turns.Action);
            var descHealth = _game.Character.Describe(x => x.Health.Physical);

            Assert.AreEqual(10, _target.Health.Physical);
            Assert.AreEqual(2, _game.Character.Turns.Action);
            Assert.AreEqual(9, _game.Character.Turns.Exertion);
            Assert.AreEqual(7, _game.Character.Turns.Focus);

            var oldHealth = _target.Health.Physical;
            var nextAction = _meta.Apply(_game.Character, action, 4);
            Assert.IsNull(nextAction);

            var da = _game.Character.Describe(x => x.Turns.Action);
            var de = _game.Character.Describe(x => x.Turns.Exertion);
            var df = _game.Character.Describe(x => x.Turns.Focus);

            Assert.IsTrue(oldHealth == _target.Health.Physical);
            Assert.AreEqual(-1, _game.Character.Turns.Action);
            Assert.AreEqual(8, _game.Character.Turns.Exertion);
            Assert.AreEqual(6, _game.Character.Turns.Focus);
        }
    }
}
