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
            Time.Refresh();
        }

        public RpgCharacterSheet(RpgObject context, RpgObject actor, RpgSystem? rpgSystem = null)
            : base(context, rpgSystem)
        {
            Actor = actor;

            //The graph was built before the actor was known. Refresh so anything that depends on the actor
            //(e.g. whether actions can be performed) is evaluated
            Time.Refresh();
        }

        public RpgCharacterSheet(RpgCharacterSheetState characterSheetState, RpgSystem rpgSystem) 
            : base(characterSheetState, rpgSystem)
        {
            Actor = (RpgObject)characterSheetState.Objects.First(x => x.Id == characterSheetState.ActorId);
            Time.Refresh();
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
