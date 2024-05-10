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
                OnEndEncounter();
            }

            Turn = 1;
            OnTurnChanged();
        }

        public void Initialize()
        {
            OnTurnChanged();
        }

        public void EndEncounter()
        {
            Turn = 0;
            OnEndEncounter();
            OnTurnChanged();
        }

        public void NewTurn()
        {
            Turn++;
            OnTurnChanged();
        }

        public void PrevTurn()
        {
            if (Turn > 1)
            {
                Turn--;
                OnTurnChanged();
            }
        }

        public void SetTurn(int turn)
        {
            if (turn > 0 && turn != Turn)
            {
                Turn = turn;
                OnTurnChanged();
            }
        }

        private void OnTurnChanged()
        {
            var affectedBy = new List<ModPropRef>();

            foreach (var entity in Context!.Traverse(true))
            {
                entity.ModSetStore.UpdatePropExpiry();
                affectedBy.Merge(entity.PropStore.AffectedByProps());
            }

            foreach (var propRef in affectedBy)
            {
                var entity = GetEntity<ModObject>(propRef.EntityId);
                entity?.SetPropValue(propRef.Prop);
            }
        }

        private void OnEndEncounter()
        {
            foreach (var entity in Context!.Traverse(true))
            {
                entity.PropStore.RemoveExpiredMods();
                entity.ModSetStore.RemoveExpiredMods();
            }
        }
    }
}
