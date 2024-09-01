using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Tests
{
    public class TransferActionTests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private Room _room;

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            _room = new Room();
            _room.Contents.Add(_pc);

            _graph = new RpgGraph(_room);
        }

        [Test]
        public void Benny_Drops_Sword_OutsideTurn()
        {
            var activity = _graph.CreateActivity(_pc, _sword, nameof(Transfer));
            Assert.That(activity.ActionInstance, Is.Not.Null);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            activity
                .SetAll("from", _pc.Hands)
                .SetAll("to", (_graph.Context as Room)!.Contents)
                .AutoComplete();

            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword_OnTurn2()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(2);

            _graph.CreateActivity(_pc, _sword, nameof(Transfer))
                .SetAll("from", _pc.Hands)
                .SetAll("to", (_graph.Context as Room)!.Contents)
                .AutoComplete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
        }

        [Test]
        public void Benny_Drops_Sword_OnTurn2_RevertToTurn1()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(2);

            _graph.CreateActivity(_pc, _sword, nameof(Transfer))
                .SetAll("from", _pc.Hands)
                .SetAll("to", (_graph.Context as Room)!.Contents)
                .AutoComplete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            _graph.Time.Transition(1);

            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Tries_Two_Transfers_One_Turn()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(PointInTimeType.EncounterBegins);

            _graph.CreateActivity(_pc, _sword, nameof(Transfer))
                .SetAll("from", _pc.Hands)
                .SetAll("to", (_graph.Context as Room)!.Contents)
                .AutoComplete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            var pickupActivity = _graph.CreateActivity(_pc, _sword, nameof(Transfer))
                .SetAll("from", (_graph.Context as Room)!.Contents)
                .SetAll("to", _pc.Hands);

            Assert.Throws<InvalidOperationException>(() => pickupActivity.AutoComplete());
        }
    }
}
