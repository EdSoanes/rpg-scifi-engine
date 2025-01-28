using Newtonsoft.Json;

namespace Rpg.Experimental.Tests.Models
{
    public class TestAction : RpgAction<TestObject>
    {
        [JsonConstructor] protected TestAction()
            : base() { }

        public TestAction(TestObject owner)
            : base(owner)
        {
        }

        public bool CanPerform(TestObject owner)
            => owner.Strength > 10;

        public bool Cost(TestObject owner, int owner_Strength)
        {
            return true;
        }

        public bool Perform(TestObject initiator, int value)
        {

            return true;
        }

        public bool Outcome(RpgActivityAction activityAction, TestObject initiator, int value)
        {
            activityAction.Result.Add(initiator.CreateStateActivation(nameof(TestState), 1, false).SetOwner(activityAction.Id, false));
            return true;
        }
    }
}
