﻿using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModdableProperty
    {
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public string? Prop { get; private set; }
        [JsonProperty] public string? Method { get; private set; }
        [JsonProperty] public string Source { get; private set; }

        public ModdableProperty(Guid id, string type, string? prop, string? method)
        {
            Id = id;
            Type = type;
            Prop = prop;
            Method = method?.EndsWith("()") ?? false ? method : $"{method}()";
            Source = $"{Type}.{Prop ?? Method ?? throw new ArgumentException("Either Prop or Method must be set")}";
        }

        public override string ToString()
        {
            var metaEntity = Id.MetaData();
            return metaEntity != null
                ? $"{metaEntity.Type}[{Id}].{Prop}"
                : $"{{unknown}}[{Id}].{Prop}";
        }

        public static bool operator ==(ModdableProperty? m1, ModdableProperty? m2) => (m1 == null && m2 == null) || (m1?.Id == m2?.Id && m1?.Source == m2?.Source);

        public static bool operator !=(ModdableProperty? m1, ModdableProperty? m2) => m1?.Id != m2?.Id || m1?.Source != m2?.Source;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is ModdableProperty))
                return false;

            return this == (ModdableProperty)obj;
        }
    }
}