﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components.Values
{
    public class DefenseValue : ModdableObject
    {
        [JsonProperty] public int Value { get; protected set; }
        [JsonProperty] public int Shielding { get; protected set; }

        [JsonConstructor] private DefenseValue() { }

        public DefenseValue(int value, int shielding)
        {
            Value = value;
            Shielding = shielding;
        }
    }
}
