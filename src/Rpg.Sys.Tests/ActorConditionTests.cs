using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class ActorConditionTests
    {
        private TestEquipment _equipment;
        private Graph _graph;
        private Actor _actor;

        [SetUp]
        public void Setup()
        {
            _graph = new Graph();
            _equipment = new TestEquipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            _actor = new Actor(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                },
                Stats = new StatPointsTemplate
                {
                    Strength = 16
                },
                Movement = new MovementTemplate
                {
                    MaxSpeed = 10
                },
                Actions = new ActionPointsTemplate
                {
                    Action = 10,
                    Exertion = 10,
                    Focus = 10
                }
            });

            _graph.Initialize(_actor);
            _graph.Add.Entities(_equipment);
        }

        [Test]
        public void AddFatigued_VerifyValues()
        {
            Assert.That(_actor.Stats.Strength.Bonus, Is.EqualTo(3));
            Assert.That(_actor.Actions.Exertion.Max, Is.EqualTo(13));
            Assert.That(_actor.Movement.Speed.Max, Is.EqualTo(10));

            var fatigued = new Fatigued(_actor);
            _graph!.Add.Conditions(fatigued);

            Assert.That(_actor.Stats.Strength.Bonus, Is.EqualTo(1));
            Assert.That(_actor.Actions.Exertion.Max, Is.EqualTo(11));
            Assert.That(_actor.Movement.Speed.Max, Is.EqualTo(8));
        }
    }
}
