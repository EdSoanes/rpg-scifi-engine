using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;

namespace Rpg.Experimental.Tests.Models
{
    /// <summary>
    /// A test object for time keeping: it has a per turn budget (Actions), a permanent resource (Ammo),
    /// something an effect can change (Bonus) and a state that needs turns counted (TestBleeding)
    /// </summary>
    public class TestCreature : RpgObject
    {
        public int Health { get; protected set; } = 5;
        public int Actions { get; protected set; } = 1;
        public int Ammo { get; protected set; } = 10;
        public int Bonus { get; protected set; }

        /// <summary>
        /// Set above zero before the creature is added to a sheet and it asks for a 1d4 roll every turn
        /// </summary>
        public int RollEachTurn { get; set; }

        /// <summary>
        /// The time events this creature has reacted to, in order
        /// </summary>
        public string EventsSeen { get; protected set; } = string.Empty;

        public TestCreature() : base() { }

        public TestCreature(string name)
            : base(name) { }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);

            if (graph.Time.CurrentEvent != null)
                EventsSeen += $"{graph.Time.CurrentEvent};";

            if (RollEachTurn > 0 && graph.Time.Now.Type == Rpg.Experimental.TimePointType.Turn)
                graph.RequestRoll(this, $"bleed/{graph.Time.Turn}", "1d4");
        }
    }

    /// <summary>
    /// An action with a roll: 2d6 plus an aim bonus against a target number. On a hit the total of the
    /// roll is added to the creature's Bonus for good, so a test can see what the roll came to.
    /// </summary>
    public class TestSwing : RpgAction<TestCreature>
    {
        [JsonConstructor] protected TestSwing()
            : base() { }

        public TestSwing(TestCreature owner)
            : base(owner) { }

        public override void OnCreatingActivityAction(RpgGraph graph, RpgActivityAction activityAction)
        {
            base.OnCreatingActivityAction(graph, activityAction);
            graph.Add(new Initial(activityAction, "hitRoll", "2d6"));
        }

        public bool Perform(RpgGraph graph, RpgActivityAction activityAction, int aim)
        {
            graph.Reset(activityAction, "hitRoll");
            if (aim != 0)
                graph.Add(new Standard(), activityAction, "hitRoll", aim);

            return true;
        }

        public bool Outcome(RpgActivityAction activityAction, TestCreature owner, int hitRoll, int target)
        {
            if (hitRoll >= target)
                activityAction.Result
                    .Add(new Combine()
                        .SetTarget(owner, x => x.Bonus)
                        .SetSource(hitRoll));

            return true;
        }
    }

    /// <summary>
    /// A dice roller that returns the numbers it is given, in order, and counts how often it is asked
    /// </summary>
    public class TestDiceRoller : IRpgDiceRoller
    {
        private readonly Queue<int> _results;

        public int Rolled { get; private set; }

        public TestDiceRoller(params int[] results)
            => _results = new Queue<int>(results);

        public int Roll(int sides)
        {
            Rolled++;
            return _results.Count > 0 ? _results.Dequeue() : 1;
        }
    }

    public class TestBleeding : RpgState<TestCreature>
    {
        [JsonConstructor] private TestBleeding() { }

        public TestBleeding(TestCreature owner)
            : base(owner)
        {
            NeedsTurnTracking = true;
        }

        protected override bool IsOnWhen(TestCreature owner)
            => owner.Health < 3;
    }

    public class TestStrike : RpgAction<TestCreature>
    {
        [JsonConstructor] protected TestStrike()
            : base() { }

        public TestStrike(TestCreature owner)
            : base(owner) { }

        public bool Cost(RpgActivityAction activityAction, TestCreature owner)
        {
            //One of this turn's actions...
            activityAction.CostSet
                .Add(new Temporal(1), owner, x => x.Actions, -1);

            //...and a round of ammunition, which is gone for good
            activityAction.CostSet
                .Add(new Combine()
                    .SetTarget(owner, x => x.Ammo)
                    .SetSource(-1));

            return true;
        }

        public bool Outcome(RpgActivityAction activityAction, TestCreature owner, int effectTurns, int trivial)
        {
            if (effectTurns > 0)
            {
                var effect = new Temporal(effectTurns)
                    .SetTarget(owner, x => x.Bonus)
                    .SetSource(2);

                if (trivial > 0)
                    effect.NoTurnTracking();

                activityAction.Result.Add(effect);
            }

            return true;
        }
    }
}
