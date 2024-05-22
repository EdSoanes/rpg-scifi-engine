using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;

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

        [JsonProperty] public ModObject Context { get; private set; }
        [JsonProperty] protected Dictionary<Guid, ModObject> ModObjectStore { get; set; } = new Dictionary<Guid, ModObject>();
        [JsonProperty] public int Turn { get; private set; }
        public bool EncounterActive => Turn > 1;

        public ModGraph(ModObject context)
        {
            ModGraphExtensions.RegisterAssembly(GetType().Assembly);

            Context = context;
            Build();
        }

        private void Build()
        {
            ModObjectStore.Clear();

            foreach (var entity in Context.Traverse())
                AddEntity(entity);

            foreach (var entity in ModObjectStore.Values)
                entity.OnGraphCreating(this, entity);

            Context.UpdateProps();
        }

        public ModProp? GetModProp(ModPropRef? propRef)
            => GetModProp(propRef?.EntityId, propRef?.Prop);

        public ModProp? GetModProp(Guid? entityId, string? prop)
        {
            var entity = GetEntity(entityId);
            return entity?.GetModProp(prop);
        }

        public bool AddEntity(ModObject entity)
        {
            if (!ModObjectStore.ContainsKey(entity.Id))
            {
                ModObjectStore.Add(entity.Id, entity);
                return true;
            }

            return false;
        }

        public bool RemoveEntity(ModObject entity)
        {
            if (ModObjectStore.ContainsKey(entity.Id))
            {
                ModObjectStore.Remove(entity.Id);
                return true;
            }

            return false;
        }

        public T? GetEntity<T>(Guid? entityId)
            where T : ModObject
                => entityId != null && ModObjectStore.ContainsKey(entityId.Value)
                    ? ModObjectStore[entityId.Value] as T
                    : null;

        public ModObject? GetEntity(Guid? entityId)
            => entityId != null && ModObjectStore.ContainsKey(entityId.Value)
                ? ModObjectStore[entityId.Value]
                : null;

        public IEnumerable<ModObject> GetEntities()
            => ModObjectStore.Values;

        public Mod[] GetAllMods()
            => ModObjectStore.Values
                .SelectMany(x => x.GetMods(filtered: false))
                .ToArray();

        public ModSet[] GetModSets()
            => ModObjectStore.Values
                .SelectMany(x => x.GetModSets())
                .ToArray();

        public void NewEncounter()
        {
            if (Turn < ModDuration.EndEncounter)
                EndEncounter();

            Turn = ModDuration.BeginEncounter;
            Context!.OnBeginEncounter();

            NewTurn();
        }

        public void Initialize()
            => Context!.TriggerUpdate();

        public void EndEncounter()
        {
            Turn = ModDuration.EndEncounter;
            Context!.OnEndEncounter();
        }

        public void NewTurn()
        {
            Turn++;
            Context!.OnTurnChanged(Turn);
        }

        public void PrevTurn()
        {
            if (Turn > 1)
            {
                Turn--;
                Context!.OnTurnChanged(Turn);
            }
        }

        public void SetTurn(int turn)
        {
            if (turn > 0 && turn != Turn)
            {
                Turn = turn;
                Context!.OnTurnChanged(Turn);
            }
        }
    }
}
