﻿using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests.States
{
    public class NerfState : State<ModdableEntity>
    {
        [JsonConstructor] private NerfState() { }

        public NerfState(ModdableEntity owner)
            : base(owner)
        { }

        protected override bool IsOnWhen(ModdableEntity owner)
            => owner.Melee.Roll() < 1;

        protected override void OnFillStateSet(StateModSet modSet, ModdableEntity owner)
            => modSet.Add(owner, x => x.Health, -10);
    }
}
