﻿using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Sys.Components.Values
{
    public class MinMaxValue : RpgComponent
    {
        [JsonProperty] public int Min { get; protected set; }
        [JsonProperty] public int Max { get; protected set; }
        [JsonProperty] public int Current { get; protected set; }

        [JsonConstructor] private MinMaxValue() { }

        public MinMaxValue(string entityId, string name, int max)
            : base(entityId, name)
        {
            Max = max;
        }

        public MinMaxValue(string entityId, string name, int min, int max)
            : this(entityId, name, max)
        {
            Min = min;
        }

        public MinMaxValue(string entityId, string name, int min, int max, int current)
            : this(entityId, name, min, max)
        {
            Current = current;
        }

        protected override void OnCreating()
        {
            this.BaseMod(x => x.Current, x => x.Max);
        }
    }
}