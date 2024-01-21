using Rpg.Sys.Archetypes;
using Rpg.Sys.Tests.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class HumanDescriptionTests
    {
        [Test]
        public void Human_MaxSpeed_Description()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            var desc = graph.Describe.Prop(human, x => x.Movement.Speed.Max);
            Assert.That(desc, Is.Not.Null);
        }
    }
}
