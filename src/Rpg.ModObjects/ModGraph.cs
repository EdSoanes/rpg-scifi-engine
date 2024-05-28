using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.ComponentModel;

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

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(ModPropRef propRef, Func<Mod, bool>? filterFunc = null)
        {
            var entity = GetEntity(propRef.EntityId);
            return CalculatePropValue(entity, propRef.Prop, filterFunc);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(ModObject? entity, string? prop, Func<Mod, bool>? filterFunc = null)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return null;

            var mods = filterFunc != null
                ? entity.GetMods(prop, filterFunc)
                : entity.GetMods(prop);

            var dice = CalculateModsValue(mods);
            return dice;
        }

        public Dice CalculateModsValue(Mod[] mods)
        {
            var dice = Dice.Zero;
            foreach (var mod in mods)
                dice += CalculateModValue(mod);

            return dice;
        }

        public Dice CalculateModValue(Mod? mod)
        {
            if (mod == null)
                return Dice.Zero;

            Dice value = mod.Source.Value ?? GetPropValue(GetEntity(mod.Source.EntityId), mod.Source.Prop);

            if (mod.Source.ValueFunc.IsCalc)
            {
                var funcEntity = (object?)GetEntity(mod.Source.ValueFunc.EntityId)
                    ?? this;

                value = funcEntity.ExecuteFunction<Dice, Dice>(mod.Source.ValueFunc.FullName!, value);
            }

            return value;
        }

        public Dice GetPropValue(ModObject? entity, string? prop)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return Dice.Zero;

            var val = entity.PropertyValue(prop, out var propExists);
            if (propExists && val != null)
            {
                if (val is Dice)
                    return (Dice)val;
                else if (val is int)
                    return (int)val;
            }

            if (!propExists)
            {
                var mods = entity.GetMods(prop);
                var dice = CalculateModsValue(mods);

                return dice;
            }

            return Dice.Zero;
        }

        public void SetPropValue(ModPropRef propRef)
        {
            var entity = GetEntity(propRef.EntityId);
            SetPropValue(entity, propRef.Prop);
        }

        public void SetPropValue(ModObject? entity, string prop)
        {
            var oldValue = GetPropValue(entity, prop);
            var newValue = CalculatePropValue(entity, prop);

            if (oldValue == null || oldValue != newValue)
                entity.PropertyValue(prop, newValue);
        }

        public Dice? GetInitialPropValue(ModObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseInitMod);

        public Dice? GetBasePropValue(ModObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseMod);

        public void NewEncounter()
        {
            if (Turn < ModDuration.EndEncounter)
                EndEncounter();

            Turn = ModDuration.BeginEncounter;
            Context!.OnBeginEncounter();

            NewTurn();
        }

        public void TriggerUpdate()
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
