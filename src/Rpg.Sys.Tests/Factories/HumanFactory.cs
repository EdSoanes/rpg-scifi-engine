using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests.Factories
{
    internal class HumanFactory
    {
        public static Graph Create()
        {
            var graph = new Graph();
            var equipment = new TestEquipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            var human = new Human(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                },
                Presence = new PresenceTemplate
                {
                    Weight = 80,
                    HeatMax = 36,
                    HeatCurrent = 36
                }
            });

            human.RightHand.Add(equipment);
            graph.SetContext(human);

            return graph;
        }
    }
}
