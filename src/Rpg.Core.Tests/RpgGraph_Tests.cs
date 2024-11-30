using Rpg.ModObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Core.Tests
{
    public class RpgGraph_Tests
    {
        [Test]
        public void IsTraversible_RpgObjectCollection()
        {
            var builder = new RpgGraphBuilder();
            Assert.That(builder.IsTraversibleType(typeof(RpgObjectCollection)), Is.True);
        }
    }
}
