using Newtonsoft.Json;

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

        [JsonProperty] public ModObject? Context { get; private set; }
        [JsonProperty] protected Dictionary<Guid, ModObject> ModObjectStore { get; set; } = new Dictionary<Guid, ModObject>();
        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public ModGraph(ModObject context)
        {
            GraphExtensions.RegisterAssembly(GetType().Assembly);

            Context = context;
            ModObjectStore.Clear();
            foreach (var entity in context.Traverse())
            {
                if (!ModObjectStore.ContainsKey(entity.Id))
                    ModObjectStore.Add(entity.Id, entity);
            }
        }

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

        public IEnumerable<ModObject> GetEntities()
            => ModObjectStore.Values;

        public Mod[] GetMods()
            => ModObjectStore.Values
                .SelectMany(x => x.PropStore.AllProps().SelectMany(prop => x.PropStore[prop]!.Modifiers))
                .ToArray();

        public void NewEncounter()
        {
            Turn = 1;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void EndEncounter()
        {
            Turn = 0;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void NewTurn()
        {
            Turn++;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void PrevTurn()
        {
            Turn--;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }

        public void SetTurn(int turn)
        {
            Turn = turn;
            Context?.UpdateGraph();
            //Update.Conditions();
            //Update.Mods();
        }
    }
}
