using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;

namespace Rpg.Sys.Tests
{
    public class HumanTransferItemTests
    {
        private TestEquipment _equipment;
        private Graph _graph;
        private Human _human;

        [SetUp]
        public void Setup()
        {
            _graph = new Graph();
            _equipment = new TestEquipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            _human = new Human(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                }
            });

            _human.RightHand.Add(_equipment);
            _graph.Initialize(_human);
        }

        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand()
        {
            Assert.That(_human.LeftHand, Has.No.Member(_equipment));
            Assert.That(_human.RightHand, Has.Member(_equipment));

            var action = _human.Transfer(_human.RightHand, _human.LeftHand, _equipment);
            action.Resolve(_human, _graph);

            Assert.That(_human.LeftHand, Has.Member(_equipment));
            Assert.That(_human.RightHand, Has.No.Member(_equipment));
        }

        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand_VerifyCosts()
        {
            var actionPoints = _human.Actions.Action.Current;

            _graph.NewEncounter();

            var action = _human.Transfer(_human.RightHand, _human.LeftHand, _equipment);
            Assert.That(action.Cost.Action, Is.EqualTo(1));
            Assert.That(action.Cost.Exertion, Is.EqualTo(0));
            Assert.That(action.Cost.Focus, Is.EqualTo(0));

            action.Resolve(_human, _graph);

            Assert.That(_human.Actions.Action.Current, Is.EqualTo(actionPoints - 1));
        }

        [Test]
        public void TransferEquipment_FromRightHand_ToLeftHand_VerifyPropsChanged()
        {
            var actionPoints = _human.Actions.Action.Current;

            var rhPropNames = new List<string>();
            _human.RightHand.PropertyChanged += (s, e) => rhPropNames.Add(e.PropertyName!);

            var lhPropNames = new List<string>();
            _human.LeftHand.PropertyChanged += (s, e) => lhPropNames.Add(e.PropertyName!);

            var action = _human.Transfer(_human.RightHand, _human.LeftHand, _equipment);
            action.Resolve(_human, _graph);

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
            Assert.That(_human.LeftHand, Has.No.Member(_equipment));
            Assert.That(_human.RightHand, Has.Member(_equipment));

            var action = _human.Transfer(_human.RightHand, null, _equipment);
            action.Resolve(_human, _graph);

            Assert.That(_human.LeftHand, Has.No.Member(_equipment));
            Assert.That(_human.RightHand, Has.No.Member(_equipment));
        }

        [Test]
        public void TransferEquipment_Pickup_ToLeftHand()
        {
            _human.RightHand.Remove(_equipment.Id);

            Assert.That(_human.LeftHand, Has.No.Member(_equipment));
            Assert.That(_human.RightHand, Has.No.Member(_equipment));

            var action = _human.Transfer(null, _human.LeftHand, _equipment);
            action.Resolve(_human, _graph);

            Assert.That(_human.LeftHand, Has.Member(_equipment));
            Assert.That(_human.RightHand, Has.No.Member(_equipment));
        }
    }
}
