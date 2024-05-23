using Rpg.ModObjects.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    public class HumanShootsGunTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void HumanShootsGun_InLocation_EnsureValues()
        {
            var initiator = new TestHuman();
            var target = new TestHuman();
            var gun = new TestGun();

            var location = new ModObjectContainer("Room");
            location.Add(initiator);
            location.Add(target);
            location.Add(gun);

            var graph = new ModGraph(location);

            Assert.That(gun.Commands.Count(), Is.EqualTo(2));
            Assert.That(gun.Commands.SingleOrDefault(x => x.CommandName == "Shoot"), Is.Not.Null);
            Assert.That(gun.Commands.SingleOrDefault(x => x.CommandName == "InflictDamage"), Is.Not.Null);
        }
    }
}
