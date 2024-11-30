using Newtonsoft.Json;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.States
{
    public sealed class StateActivation : Lifespan
    {
        [JsonProperty] public StateInstanceType InstanceType { get; internal set; }
        [JsonProperty] public string? InitiatorId { get; internal set; }

        [JsonConstructor] private StateActivation() { }

        public StateActivation(string? initiatorId, Lifespan lifespan)
            : base(lifespan.Start, lifespan.End)
        {
            InitiatorId = initiatorId;
            InstanceType = StateInstanceType.Timed;
        }

        public StateActivation(StateInstanceType instanceType)
        {
            InstanceType = instanceType;
        }

        public bool Matches(StateActivation stateActivation)
            => InstanceType == stateActivation.InstanceType && 
                InitiatorId == stateActivation.InitiatorId && 
                Start == stateActivation.Start && 
                End == stateActivation.End;

        public bool Matches(string initiatorId, Lifespan? lifespan)
            => InitiatorId == initiatorId && 
                (lifespan == null || (lifespan.Start == Start && lifespan.End == End));
    }
}
