using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Moddable
{
    public class ModGraph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [JsonProperty] protected ModObject? Context { get; private set; }
        [JsonProperty] Dictionary<Guid, ModObject> ModObjectStore = new Dictionary<Guid, ModObject>();
        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public static ModGraph Current { get; } = new ModGraph();

        public T GetContext<T>()
            where T : ModObject
                => (T)Context!;

        public void SetContext(ModObject context)
        {
            Context = context;
            ModObjectStore.Clear();
            foreach (var entity in context.Traverse())
            {
                if (!ModObjectStore.ContainsKey(entity.Id))
                    ModObjectStore.Add(entity.Id, entity);
            }
        }

        public T? GetEntity<T>(Guid? entityId)
            where T : ModObject
                => entityId != null && ModObjectStore.ContainsKey(entityId.Value)
                    ? ModObjectStore[entityId.Value] as T
                    : null;

        public IEnumerable<T> GetAffectedEntities<T>(Guid sourceId, string prop) 
            where T : ModObject
        {

        }
        public void NewEncounter()
        {
            Turn = 1;
            Update.Conditions();
            Update.Mods();
        }

        public void EndEncounter()
        {
            Turn = 0;
            Update.Conditions();
            Update.Mods();
        }

        public void NewTurn()
        {
            Turn++;
            Update.Conditions();
            Update.Mods();
        }

        public void PrevTurn()
        {
            Turn--;
            Update.Conditions();
            Update.Mods();
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            Update.Conditions();
            Update.Mods();
        }
    }
}
