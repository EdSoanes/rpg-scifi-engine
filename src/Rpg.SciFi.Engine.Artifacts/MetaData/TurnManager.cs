using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class TurnManager
    {
        private ModStore? _modStore;
        private EntityStore? _entityStore;
        
        [JsonProperty] public List<TurnAction> Actions { get; private set; } = new List<TurnAction>();

        public void Initialize(ModStore modStore, EntityStore entityStore)
        {
            _modStore = modStore;
            _entityStore = entityStore;
        }

        public int Current { get; private set; }

        public void StartEncounter()
        {
            Current = 1;
            _modStore?.Remove(Current);
        }
        
        public void Increment()
        {
            Current++;
            _modStore?.Remove(Current);
        }

        public void SetTurn(int turn)
        {
            Current = turn;
            _modStore?.Remove(Current);
        }

        public void EndEncounter()
        {
            Current = 0;
            _modStore?.Remove(Current);
        }
    }
}
