﻿using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Actions
{
    [Action(Required = true)]
    public class TakeDamageAction : ModObjects.Actions.Action<TestHuman>
    {
        [JsonConstructor] private TakeDamageAction() { }

        public TakeDamageAction(TestHuman owner)
            : base(owner) { }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            => true;

        public ModSet OnCost(TestHuman owner)
            => new ModSet(owner);

        public ModSet OnAct(TestHuman owner, int damage)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), owner, x => x.Health, -damage);
        }

        public ModSet[] OnOutcome(TestHuman owner)
            => Array.Empty<ModSet>();
    }
}
