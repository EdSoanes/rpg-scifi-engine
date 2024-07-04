using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Tests
{
    public class ActorMeleeWeaponTests
    {
        PlayerCharacterTemplate _benny = new PlayerCharacterTemplate
        {
            Name = "Benny",
            Strength = 1,
            Agility = 2,
            Health = 0,
            Brains = 1,
            Insight = 2,
            Charisma = 0
        };

        MeleeWeaponTemplate _sword = new MeleeWeaponTemplate
        {
            Name = "Excalibur",
            Damage = "d6",
            HitBonus = 1
        };

        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void PlayerCharacter_EnsureValues()
        {
            var room = new RpgContainer("Room");
            var pc = new PlayerCharacter(_benny);
            pc.Hands.Add(new MeleeWeapon(_sword));
            room.Add(pc);

            var graph = new RpgGraph(room);

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength, Is.EqualTo(1));
            Assert.That(pc.Agility, Is.EqualTo(2));
            Assert.That(pc.Health, Is.EqualTo(0));
            Assert.That(pc.Brains, Is.EqualTo(1));
            Assert.That(pc.Insight, Is.EqualTo(2));
            Assert.That(pc.Charisma, Is.EqualTo(0));

            Assert.That(pc.StaminaPoints, Is.EqualTo(12));
            Assert.That(pc.LifePoints, Is.EqualTo(7));
            Assert.That(pc.FocusPoints, Is.EqualTo(5));
            Assert.That(pc.LuckPoints, Is.EqualTo(1));

            Assert.That(pc.Defence, Is.EqualTo(9));
            Assert.That(pc.Reactions, Is.EqualTo(11));
            Assert.That(pc.MeleeAttack, Is.EqualTo(1));

            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
        }

        [Test]
        public void Sword_EnsureValues()
        {
            var room = new RpgContainer("Room");
            var pc = new PlayerCharacter(_benny);
            pc.Hands.Add(new MeleeWeapon(_sword));
            room.Add(pc);

            var graph = new RpgGraph(room);

            var sword = pc.Hands.Get<MeleeWeapon>()?.FirstOrDefault();
            Assert.That(sword, Is.Not.Null);
            Assert.That(sword.Name, Is.EqualTo("Excalibur"));
            Assert.That(sword.Damage.ToString(), Is.EqualTo("1d6"));
            Assert.That(sword.HitBonus, Is.EqualTo(1));
        }

        [Test]
        public void PlayerCharacter_CarryingSword_Attack()
        {
            var room = new RpgContainer("Room");
            var pc = new PlayerCharacter(_benny);
            pc.Hands.Add(new MeleeWeapon(_sword));
            room.Add(pc);

            var graph = new RpgGraph(room);

            var sword = pc.Hands.Get<MeleeWeapon>().First();
            var attack = sword.CreateActionInstance(pc, nameof(MeleeAttack), 0)!;

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);
            Assert.That(graph.Time.Current, Is.EqualTo(TimePoints.Encounter(1)));

            attack.CostArgs["focusPoints"] = 0;
            var cost = attack.Cost();

            Assert.That(pc.CurrentActions, Is.EqualTo(2));

            pc.AddModSet(cost);
            graph.Time.TriggerEvent();
            Assert.That(pc.CurrentActions, Is.EqualTo(1));

            attack.ActArgs["focusPoints"] = 0;
            var act = attack.Act();
            pc.AddModSets(act);
            graph.Time.TriggerEvent();
            Assert.That(attack.ActResult.ToString(), Is.EqualTo("2d6 + 1"));

            graph.Time.SetTime(TimePoints.Encounter(2));
            Assert.That(pc.CurrentActions, Is.EqualTo(2));
            Assert.That(attack.ActResult.ToString(), Is.EqualTo(Dice.Zero.ToString()));
        }
    }
}
