using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Tests.Factories;

namespace Rpg.Sys.Tests
{
    public class HumanTransferItemTests
    {
        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(human.LeftHand, Has.No.Member(equipment));
            Assert.That(human.RightHand, Has.Member(equipment));

            var action = human.Transfer(human.RightHand, human.LeftHand, equipment);
            action.Resolve(human, graph);

            Assert.That(human.LeftHand, Has.Member(equipment));
            Assert.That(human.RightHand, Has.No.Member(equipment));
        }

        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand_VerifyCosts()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();
            
            var actionPoints = human.Actions.Action.Current;

            graph.NewEncounter();

            var action = human.Transfer(human.RightHand, human.LeftHand, equipment);
            Assert.That(action.Cost.Action, Is.EqualTo(1));
            Assert.That(action.Cost.Exertion, Is.EqualTo(0));
            Assert.That(action.Cost.Focus, Is.EqualTo(0));

            action.Resolve(human, graph);

            Assert.That(human.Actions.Action.Current, Is.EqualTo(actionPoints - 1));
        }

        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand_VerifyPropsChanged()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            var actionPoints = human.Actions.Action.Current;

            var rhPropNames = new List<string>();
            human.RightHand.PropertyChanged += (s, e) => rhPropNames.Add(e.PropertyName!);

            var lhPropNames = new List<string>();
            human.LeftHand.PropertyChanged += (s, e) => lhPropNames.Add(e.PropertyName!);

            var action = human.Transfer(human.RightHand, human.LeftHand, equipment);
            action.Resolve(human, graph);

            Assert.That(rhPropNames.Count(), Is.EqualTo(2));
            Assert.That(rhPropNames, Has.Exactly(1).Matches<string>(x => x == nameof(Container.CurrentEncumbrance)));
            Assert.That(rhPropNames, Has.Exactly(1).Matches<string>(x => x == nameof(Container.CurrentItems)));

            Assert.That(lhPropNames.Count(), Is.EqualTo(2));
            Assert.That(lhPropNames, Has.Exactly(1).Matches<string>(x => x == nameof(Container.CurrentEncumbrance)));
            Assert.That(lhPropNames, Has.Exactly(1).Matches<string>(x => x == nameof(Container.CurrentItems)));
        }

        [Test]
        public void TransferEquipment_Drop_FromRightHand()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(human.LeftHand, Has.No.Member(equipment));
            Assert.That(human.RightHand, Has.Member(equipment));

            var action = human.Transfer(human.RightHand, null, equipment);
            action.Resolve(human, graph);

            Assert.That(human.LeftHand, Has.No.Member(equipment));
            Assert.That(human.RightHand, Has.No.Member(equipment));
        }

        [Test]
        public void TransferEquipment_Pickup_ToLeftHand()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            human.RightHand.Remove(equipment.Id);

            Assert.That(human.LeftHand, Has.No.Member(equipment));
            Assert.That(human.RightHand, Has.No.Member(equipment));

            var action = human.Transfer(null, human.LeftHand, equipment);
            action.Resolve(human, graph);

            Assert.That(human.LeftHand, Has.Member(equipment));
            Assert.That(human.RightHand, Has.No.Member(equipment));
        }
    }
}
