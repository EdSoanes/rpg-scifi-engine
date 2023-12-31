﻿using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Archetypes.Templates;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class CharacterTests
    {
        private EntityGraph _graph;
        private Game _game = new Game();
        private Gun _gun;
        private Character _target;

        [TestInitialize]
        public void Initialize()
        {
            _gun = new Gun(new Rifle { Attack = 2 })
            {
                Name = "Blaster"
            };

            var player = new Character("The Player");
            player.AddArtifact(Container.LeftHand, _gun);

            _target = new Character("The Target");

            _game.Character = player;
            _game.Environment.GetContainer(Container.Environment)!.Add(_target);

            _graph = new EntityGraph();
            _graph.Initialize(_game);

            _graph.Mods.Add(PlayerModifier.Create(player, 18, x => x.Stats.Strength));
            _graph.Mods.Add(PlayerModifier.Create(player, 5, x => x.Stats.Dexterity));
            _graph.Mods.Add(PlayerModifier.Create(player, 14, x => x.Stats.Intelligence));

            _graph.Mods.Add(PlayerModifier.Create(player, 5, x => x.Turns.Action));
            _graph.Mods.Add(PlayerModifier.Create(player, 5, x => x.Turns.Exertion));
            _graph.Mods.Add(PlayerModifier.Create(player, 5, x => x.Turns.Focus));

            _graph.Mods.Add(PlayerModifier.Create(_target, 18, x => x.Stats.Strength));
            _graph.Mods.Add(PlayerModifier.Create(_target, 5, x => x.Stats.Dexterity));
            _graph.Mods.Add(PlayerModifier.Create(_target, 14, x => x.Stats.Intelligence));

            _graph.Mods.Add(PlayerModifier.Create(_target, 5, x => x.Turns.Action));
            _graph.Mods.Add(PlayerModifier.Create(_target, 5, x => x.Turns.Exertion));
            _graph.Mods.Add(PlayerModifier.Create(_target, 5, x => x.Turns.Focus));
        }

        [TestMethod]
        public void CharacterGraph_Serialize()
        {
            var state = _graph.Serialize<Game>();
            Assert.IsNotNull(state);
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
            Assert.AreEqual<string>("1d6 + 4", _game.Character.Damage.Impact);
        }

        [TestMethod]
        public void CharacterStats_BaseStrengthUpdate_PropertyNotification()
        {
            var propNames = new List<string>();
            _game.Character.Stats.PropertyChanged += (sender, e) =>
            {
                propNames.Add($"Stats.{e.PropertyName}");
            };
            _game.Character.Turns.PropertyChanged += (sender, e) =>
            {
                propNames.Add($"Turns.{e.PropertyName}");
            };
            _game.Character.Damage.PropertyChanged += (sender, e) =>
            {
                propNames.Add($"Damage.{e.PropertyName}");
            };

            _graph.Mods.Add(PlayerModifier.Create(_game.Character, 1, x => x.Stats.Strength));

            Assert.IsTrue(propNames.Contains("Stats.Strength"));
            Assert.IsTrue(propNames.Contains("Stats.StrengthBonus"));
            Assert.IsTrue(propNames.Contains("Stats.MeleeAttackBonus"));
            Assert.IsTrue(propNames.Contains("Turns.Exertion"));
            Assert.IsTrue(propNames.Contains("Damage.Impact"));

        }

        [TestMethod]
        public void Character_Gun_Fire()
        {
            var action = _gun.Fire(_game.Character, _target, 3);

            var diceRollDesc = action.Describe(nameof(TurnAction.DiceRoll));
            var diceRollTargetDesc = action.Describe(nameof(TurnAction.DiceRollTarget));
            var successDesc = action.Success.SelectMany(x => _graph.Evaluator.Describe(x, true));
            var failureDesc = action.Failure.SelectMany(x => _graph.Evaluator.Describe(x, true));

            Assert.AreEqual<string>("1d20 - 2", action.DiceRoll);
            Assert.AreEqual<string>("11", action.DiceRollTarget);
        }

        [TestMethod]
        public void Character_Gun_Fire_Success()
        {
            var baseImpactModProp = _graph.Mods.Get(_gun, x => x.Damage.Impact);
            Assert.IsNotNull(baseImpactModProp);
            Assert.AreEqual(1, baseImpactModProp.Modifiers.Count);

            Assert.AreEqual(10, _target.Health.Physical);
            var action = _gun.Fire(_game.Character, _target, 3);

            var desc = _game.Character.Describe(x => x.Turns.Action);
            var descHealth = _game.Character.Describe(x => x.Health.Physical);

            Assert.AreEqual(10, _target.Health.Physical);
            Assert.AreEqual(2, _game.Character.Turns.Action);
            Assert.AreEqual(9, _game.Character.Turns.Exertion);
            Assert.AreEqual(7, _game.Character.Turns.Focus);

            var oldHealth = _target.Health.Physical;
            var nextAction = action.Act(_game.Character, 11);
            Assert.IsNull(nextAction);

            var x1 = _graph.Mods.Get(_game, x => x.Character.Health.Physical);

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
            var nextAction = action.Act(_game.Character, 4);
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
