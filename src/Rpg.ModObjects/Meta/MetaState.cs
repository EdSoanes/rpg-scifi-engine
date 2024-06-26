﻿using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Attributes;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaState
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public bool Required { get; private set; }

        [JsonConstructor] private MetaState() { }

        public MetaState(Type stateType)
        {
            Name = stateType.Name;
            Archetype = stateType.BaseType!.GenericTypeArguments[0].Name;

            var attr = stateType.GetCustomAttribute<StateAttribute>();
            if (attr != null)
                Required = attr.Required;
        }

        public override string ToString()
        {
            return $"{Name} ({Archetype})";
        }
    }
}
