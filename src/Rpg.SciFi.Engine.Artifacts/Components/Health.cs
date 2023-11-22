using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Health : Modifiable
    {
        public Health() 
        {
            Name = nameof(Health);
        }

        public Health(int basePhysical, int baseMental)
            : this()
        {
            BasePhysical = basePhysical;
            BaseMental = baseMental;
        }

        [JsonProperty] public virtual int BasePhysical { get; protected set; }
        [JsonProperty] public virtual int BaseMental { get; protected set; }

        [Moddable] public virtual int Physical { get => BasePhysical + ModifierRoll(nameof(Physical)); }
        [Moddable] public virtual int Mental { get => BaseMental + ModifierRoll(nameof(Mental)); }
    }

    public class CompositeHealth : Health
    {
        [JsonProperty] private Health[] _healths { get; set; }

        public CompositeHealth(Health[] healths)
        {
            Name = nameof(Health);
            _healths = healths;
        }

        public override int BasePhysical { get => _healths.Sum(x => x.BasePhysical); protected set => throw new ArgumentException("Cannot set BasePhysical"); }
        public override int BaseMental { get => _healths.Sum(x => x.BaseMental); protected set => throw new ArgumentException("Cannot set BaseMental"); }

        public override int Physical { get => _healths.Sum(x => x.Physical); }
        public override int Mental { get => _healths.Sum(x => x.Mental); }

    }
}
