using Newtonsoft.Json;

namespace Rpg.ModObjects
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
            ModGraphExtensions.RegisterAssembly(GetType().Assembly);

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
                .SelectMany(x => x.PropStore.AllProps().SelectMany(prop => x.PropStore[prop]!.Mods))
                .ToArray();

        public void NewEncounter()
        {
            if (Turn > 0)
            {
                Turn = 0;
                Context?.RemoveExpiredProps();
            }

            Turn = 1;
            Context?.OnPropsUpdated();
        }

        public void EndEncounter()
        {
            Turn = 0;
            Context?.RemoveExpiredProps();
            Context?.OnPropsUpdated();
        }

        public void NewTurn()
        {
            Turn++;
            Context?.OnPropsUpdated();
        }

        public void PrevTurn()
        {
            if (Turn > 1)
            {
                Turn--;
                Context?.OnPropsUpdated();
            }
        }

        public void SetTurn(int turn)
        {
            if (turn > 0)
            {
                Turn = turn;
                Context?.OnPropsUpdated();
            }
        }
    }
}
