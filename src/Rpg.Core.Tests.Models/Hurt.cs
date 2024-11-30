using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Core.Tests.Models
{
    public class Hurt : State<TestPerson>
    {
        [JsonConstructor] private Hurt() { }

        public Hurt(TestPerson owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(TestPerson owner)
            => owner.HitPoints < owner.BaseValue(x => x.HitPoints)?.Roll();
    }
}
