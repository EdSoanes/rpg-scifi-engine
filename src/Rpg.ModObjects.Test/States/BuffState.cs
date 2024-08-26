using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class BuffState : State<ModdableEntity>
    {
        [JsonConstructor] private BuffState() { }

        public BuffState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() >= 10;

        protected override void OnFillStateSet(StateModSet modSet, ModdableEntity owner)
            => modSet.Add(owner, x => x.Health, 10);
    }
}
