using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class TestEquipment : Equipment
    {
        public TestEquipment(ArtifactTemplate template)
            : base(template)
        {
            States.Add(new TestActorState(Id));
        }
    }

    public class TestActorState : State<TestEquipment>
    {
        public TestActorState(Guid id) : base(id, "Enhance") { }

        protected override Modifier[] Effects(Actor actor, TestEquipment artifact)
        {
            return new Modifier[]
            {
                StateModifier.Create(artifact.Id, Name, actor, 3, x => x.Health.Physical.Current)
            };
        }
    }
}
