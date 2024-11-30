using Newtonsoft.Json;
using Rpg.ModObjects;

namespace Rpg.Core.Tests.Models
{
    public class TestEnhancement : RpgEntity
    {
        [JsonConstructor] protected TestEnhancement() { }

        public TestEnhancement(string name)
            : base(name)
        {
        }
    }
}
