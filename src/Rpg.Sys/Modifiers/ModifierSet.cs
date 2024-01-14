using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Modifiers
{
    public class ModifierSet
    {
        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(ModifierSet);
        [JsonProperty] public ModifierDuration Duration { get; private set; } = new ModifierDuration();

        [JsonConstructor] private ModifierSet() { }

        public ModifierSet(string name)
        {
            Name = name;
        }
        public ModifierSet(string name, params Modifier[] mods)
        {
            Name = name;
            Modifiers.AddRange(mods);
        }

        public void Add(params Modifier[] mods)
            => Modifiers.AddRange(mods);

        public Modifier[] GetModifiers()
            => Modifiers.ToArray();
    }
}
