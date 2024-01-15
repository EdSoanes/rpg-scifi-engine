using Newtonsoft.Json;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Condition
    {
        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; } = nameof(Condition);
        [JsonProperty] public ModifierDuration Duration { get; private set; } = new ModifierDuration();

        [JsonConstructor] protected Condition() { }

        public Condition(string name)
            => Name = name;

        public Condition(string name, ModifierDuration duration)
            : this(name)
                => Duration.Set(duration);

        public Condition(string name, ModifierDuration duration, params Modifier[] mods)
            : this(name, duration)
                => Add(mods);

        public Condition Add(params Modifier[] mods)
        {
            Modifiers.AddRange(mods);
            Modifiers.ForEach(x => x.ModifierSet = Name);
            return this;
        }

        public Modifier[] GetModifiers()
            => Modifiers.ToArray();
    }
}
