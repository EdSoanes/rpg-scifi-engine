using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rpg.Experimental.States;
using Rpg.Experimental.ModSets;

namespace Rpg.Experimental.Tests.Models
{
    public class TestState : State<TestObject>
    {
        [JsonConstructor] private TestState() { }

        public TestState(TestObject owner)
            : base(owner)
        {
            this.Add(owner, x => x.Initiative, 1);
        }

        protected override bool IsOnWhen(TestObject owner)
            => owner.Intelligence > 3;
    }
}
