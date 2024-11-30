using Newtonsoft.Json;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.States
{
    public sealed class StateRef
    {
        [JsonProperty] public string OwnerId { get; private set; }
        [JsonProperty] public string State { get; private set; }
        [JsonProperty] public Lifespan Lifespan { get; private set; }

        [JsonConstructor] private StateRef() { }

        public StateRef(string ownerId, string state, Lifespan lifespan)
        {  
            OwnerId = ownerId; 
            State = state;
            Lifespan = lifespan;
        }
    }
}
