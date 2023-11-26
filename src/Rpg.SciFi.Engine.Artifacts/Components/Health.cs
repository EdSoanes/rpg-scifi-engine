using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Health : Entity
    {
        public Health() { }
        public Health(int basePhysical, int baseMental)
        {
            BasePhysical = basePhysical;
            BaseMental = baseMental;
        }

        [JsonProperty] public virtual int BasePhysical { get; protected set; }
        [JsonProperty] public virtual int BaseMental { get; protected set; }

        [Moddable] public virtual int Physical { get => this.Resolve(nameof(Physical)); }
        [Moddable] public virtual int Mental { get => this.Resolve(nameof(Mental)); }

        [Setup]
        public void Setup()
        {
            this.AddMod(x => x.BasePhysical, x => x.Physical);
            this.AddMod(x => x.BaseMental, x => x.Mental);
        }
    }

    public class CompositeHealth : Health
    {
        [JsonProperty] private Health[] _healths { get; set; }

        public CompositeHealth(Health[] healths)
        {
            _healths = healths;
        }

        public override int BasePhysical { get => _healths.Sum(x => x.BasePhysical); protected set => throw new ArgumentException("Cannot set BasePhysical"); }
        public override int BaseMental { get => _healths.Sum(x => x.BaseMental); protected set => throw new ArgumentException("Cannot set BaseMental"); }

        public override int Physical { get => _healths.Sum(x => x.Physical); }
        public override int Mental { get => _healths.Sum(x => x.Mental); }

    }
}
