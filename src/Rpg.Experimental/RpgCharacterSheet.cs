using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.System;

namespace Rpg.Experimental
{
    public class RpgCharacterSheet : RpgGraph
    {
        [JsonProperty] public RpgObject Actor { get; private set; }

        public RpgCharacterSheet(RpgObject context, RpgSystem? metaGraph = null) 
            : base(context, metaGraph)
        {
            Actor = context;
        }

        public RpgCharacterSheet(RpgObject context, RpgObject actor, RpgSystem? rpgSystem = null)
            : base(context, rpgSystem)
        {
            Actor = actor;
        }

        public RpgCharacterSheet(RpgCharacterSheetState characterSheetState, RpgSystem rpgSystem) 
            : base(characterSheetState, rpgSystem)
        {
            Actor = (RpgObject)characterSheetState.Objects.First(x => x.Id == characterSheetState.ActorId);
        }

        public RpgCharacterSheetState GetState()
        {
            var characterSheetState = new RpgCharacterSheetState
            {
                Objects = Objects.Values.ToList(),
                ObjectData = ObjectData.Values.ToList(),
                ContextId = Context.Id,
                ActorId = Actor.Id,
                Time = Time
            };

            return characterSheetState;
        }
    }
}
