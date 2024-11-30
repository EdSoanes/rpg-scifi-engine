using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Core.Tests.Models
{
    public class Attacking : State<TestPerson>
    {
        [JsonConstructor] private Attacking() { }

        public Attacking(TestPerson owner)
            : base(owner)
        { }
    }
}
