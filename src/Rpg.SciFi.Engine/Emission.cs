using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public abstract class Emission
    {
        public Emission(string? name = null, string? description = null, string? value = null, string? radius = null)
        {
            Name = name ?? GetType().Name;
            Description = description;
            Value = value ?? "0";
            Radius = radius ?? "100";
        }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string Value { get; set; } = "0";

        public string Radius { get; set; } = "100";
    }
}
