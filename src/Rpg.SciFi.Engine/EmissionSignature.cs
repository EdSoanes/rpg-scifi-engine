using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rpg.SciFi.Engine.Emissions;

namespace Rpg.SciFi.Engine
{
    public class BaseEmissionSignature
    {
        public BaseEmissionSignature(Emission? visibleLight = null, Emission? heat = null, Emission? radiation = null, Emission? sound = null, Emission? eletromagnetic = null)        {
            VisibleLight = visibleLight ?? new VisibleLightEmission();
            Heat = heat ?? new HeatEmission();
            Radiation = radiation ?? new RadiationEmission();
            Sound = sound ?? new SoundEmission();
            Eletromagnetic = eletromagnetic ?? new ElectromagneticEmission();
        }

        public Emission VisibleLight { get; }
        public Emission Heat { get; }
        public Emission Radiation { get; }
        public Emission Sound { get; }
        public Emission Eletromagnetic { get; }
    }

    public class EmissionSignature
    {
        public EmissionSignature(BaseEmissionSignature? baseSignature = null, Emission? visibleLight = null, Emission? heat = null, Emission? radiation = null, Emission? sound = null, Emission? eletromagnetic = null)        {
            BaseSignature = baseSignature ?? new BaseEmissionSignature();
            VisibleLight = visibleLight ?? new VisibleLightEmission();
            Heat = heat ?? new HeatEmission();
            Radiation = radiation ?? new RadiationEmission();
            Sound = sound ?? new SoundEmission();
            Eletromagnetic = eletromagnetic ?? new ElectromagneticEmission();
        }

        [JsonProperty]
        protected BaseEmissionSignature BaseSignature { get; private set; }

        public Emission VisibleLight { get; protected set; }
        public Emission Heat { get; protected set; }
        public Emission Radiation { get; protected set; }
        public Emission Sound { get; protected set; }
        public Emission Eletromagnetic { get; protected set; }
    }
}
