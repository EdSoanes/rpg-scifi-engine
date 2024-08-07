﻿using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Models
{
    public class MaxCurrentValue : RpgComponent
    {
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MaxCurrentValue() { }

        public MaxCurrentValue(string name, int max)
            : base(name)
        {
            Max = max;
        }

        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.Current, x => x.Max);
        }
    }
}
