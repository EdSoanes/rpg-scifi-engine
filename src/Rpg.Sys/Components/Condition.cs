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
        [JsonProperty] public Guid? OwningEntityId { get; private set; }
        [JsonProperty] public string Name { get; private set; } = nameof(Condition);
        [JsonProperty] public ModifierDuration Duration { get; private set; } = new ModifierDuration();

        [JsonConstructor] protected Condition() { }

        public Condition(string name)
            => Name = name;

        public Condition(string name, ModifierDuration duration)
            : this(name)
                => Duration.Set(duration);

        public Condition(Guid owningEntityId, string name, ModifierDuration duration)
            : this(name, duration)
                => OwningEntityId = owningEntityId;

        public Condition Add(params Modifier[] mods)
        {
            Modifiers.AddRange(mods);
            return this;
        }

        public Modifier[] GetModifiers()
            => Modifiers.ToArray();
    }
}
