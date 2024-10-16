﻿using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;

namespace Rpg.ModObjects.Tests
{
    public class ModObjectTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void TestEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(graph.GetObjects().Count(), Is.EqualTo(3));
            var mods = graph.GetActiveMods();
            Assert.That(mods.Count(), Is.EqualTo(11));

            Assert.That(entity.States.Count(), Is.EqualTo(3));
            Assert.That(entity.GetState(nameof(BuffState)), Is.Not.Null);
            Assert.That(entity.GetState(nameof(NerfState)), Is.Not.Null);
            Assert.That(entity.GetState(nameof(Testing)), Is.Not.Null);
        }

        [Test]
        public void CreateSimpleEntity_SetBonus_VerifyScore()
        {
            var entity = new SimpleModdableEntity(2, 2);

            Assert.That(entity.Score, Is.EqualTo(2));
            Assert.That(entity.Bonus, Is.EqualTo(2));

            var graph = new RpgGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));
            Assert.That(entity.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void CreateSimpleEntity_AddScoreMod_EnsureScoreUpdate()
        {
            var entity = new SimpleModdableEntity(2, 2);
            var graph = new RpgGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));

            entity.AddMod(new Permanent(), x => x.Score, 4);
            graph.Time.TriggerEvent();

            Assert.That(entity.Score, Is.EqualTo(8));
        }

        [Test]
        public void CreateModdableEntity_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Strength.Score, Is.EqualTo(14));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(2));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6 + 2"));
        }

        [Test]
        public void TestEntity_ReplaceStrengthScoreWithBaseOverrideMod_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Strength.Score, Is.EqualTo(14));

            entity.AddMod(new Override(), x => x.Strength.Score, 10);
            graph.Time.TriggerEvent();

            Assert.That(entity.Strength.Score, Is.EqualTo(10));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(0));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(2));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6"));
        }

        [Test]
        public void TestEntity_CreateDamageMod_CreateRepairMod_IsRepaired()
        {
            //var entity = new ModdableEntity();
            //var graph = new RpgGraph(entity);

            //Assert.That(entity.Health, Is.EqualTo(10));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            //entity.AddMod(new ExpireOnZero(), x => x.Health, -10);
            //graph.Time.TriggerEvent();

            //Assert.That(entity.Health, Is.EqualTo(0));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(12));

            //entity.AddMod(new ExpireOnZero(), x => x.Health, 10);
            //graph.Time.TriggerEvent();

            //Assert.That(entity.Health, Is.EqualTo(10));
            //Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
        }
    }
}
