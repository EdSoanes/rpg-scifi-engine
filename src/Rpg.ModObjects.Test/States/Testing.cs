using Newtonsoft.Json;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class Testing : State<ModdableEntity>
    {
        [JsonConstructor] private Testing() { }

        public Testing(ModdableEntity owner)
            : base(owner)
        { }
    }
}
