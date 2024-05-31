using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public class RpgGraph
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        private List<PropRef> UpdatedProps = new List<PropRef>();

        [JsonProperty] public RpgObject Context { get; private set; }
        [JsonProperty] protected Dictionary<string, RpgObject> ObjectStore { get; set; } = new Dictionary<string, RpgObject>();
        [JsonProperty] public int Turn { get; private set; } = ModBehavior.BeforeEncounter;

        public RpgGraph(RpgObject context)
        {
            RpgGraphExtensions.RegisterAssembly(GetType().Assembly);

            Context = context;
            Build();
        }

        private void Build()
        {
            ObjectStore.Clear();

            foreach (var entity in Context.Traverse())
            {
                AddEntity(entity);
                entity.OnGraphCreating(this, entity);
            }

            foreach (var entity in ObjectStore.Values)
            {
                entity.OnObjectsCreating();
                var props = GetModProps(entity);
                foreach (var prop in props)
                {
                    var propsAffectedBy = GetPropsAffectedBy(prop);
                    UpdatedProps.Merge(propsAffectedBy);
                }
            }

            TriggerUpdate();
        }

        public Mod[] GetMods(PropRef? propRef, bool filtered = true)
            => GetEntity(propRef?.EntityId)?.PropStore.GetMods(propRef!.Prop, filtered) ?? Array.Empty<Mod>();

        public Mod[] GetMods(PropRef? propRef, Func<Mod, bool> filterFunc)
            => GetEntity(propRef?.EntityId)?.PropStore.GetMods(propRef!.Prop, filterFunc) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, string prop, Func<Mod, bool> filterFunc)
            => rpgObj?.PropStore.GetMods(prop, filterFunc) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, string prop, bool filtered = true)
            => rpgObj?.PropStore.GetMods(prop, filtered) ?? Array.Empty<Mod>();

        public Mod[] GetMods(RpgObject? rpgObj, bool filtered = true)
            => rpgObj?.PropStore.GetMods(filtered).ToArray() 
                ?? Array.Empty<Mod>();

        public Mod[] GetMods(bool filtered = true)
            => ObjectStore.Values
                .SelectMany(x => GetMods(x, filtered)).ToArray() 
                ?? Array.Empty<Mod>();

        public void AddMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetEntity(modGroup.Key)?.PropStore.Add(modGroup.ToArray());
        }

        public void RemoveMods(params Mod[] mods)
        {
            foreach (var modGroup in mods.GroupBy(x => x.EntityId))
                GetEntity(modGroup.Key)?.PropStore.Remove(modGroup.ToArray());
        }

        public Prop? GetModProp(PropRef? propRef, bool create = false)
        {
            var entity = GetEntity(propRef?.EntityId);
            return entity?.PropStore.Get(propRef!.Prop, create);
        }

        public Prop[] GetModProps(RpgObject? rpgObj)
            => rpgObj?.PropStore.Get() ?? Array.Empty<Prop>();

        public List<PropRef> GetPropsAffectedBy(PropRef propRef)
        {
            var res = new List<PropRef>();
            res.Merge(propRef);

            var propsAffectedBy = new List<PropRef>();
            foreach (var entity in GetEntities())
            {
                var affectedBy = GetModProps(entity)
                    .Where(x => x.IsAffectedBy(propRef))
                    .Select(x => new PropRef(entity.Id, x.Prop))
                    .Distinct();

                res.Merge(affectedBy);

                foreach (var propAffectedBy in affectedBy)
                {
                    var parentAffectedBy = GetPropsAffectedBy(propAffectedBy);
                    res.Merge(parentAffectedBy);
                }
            }

            return res;
        }

        //public List<PropRef> GetPropsThatAffect(RpgObject? rpgObj)
        //{
        //    var res = rpgObj?.PropStore.Get()
        //        .SelectMany(GetPropsThatAffect)
        //        .Distinct()
        //        .ToList() ?? new List<PropRef>();

        //    return res;
        //}

        //public List<PropRef> GetPropsThatAffect(PropRef propRef)
        //{
        //    var res = new List<PropRef>();

        //    var propsThatAffect = GetMods(propRef)
        //        .Where(x => x.SourcePropRef != null)
        //        .Select(x => x.SourcePropRef!)
        //        .GroupBy(x => $"{x.EntityId}.{x.Prop}")
        //        .Select(x => x.First());

        //    foreach (var propThatAffects in propsThatAffect)
        //    {
        //        var childPropsThatAffect = GetPropsThatAffect(propThatAffects);
        //        res.Merge(childPropsThatAffect);
        //        res.Merge(propThatAffects);
        //    }

        //    res.Merge(propRef);

        //    return res;
        //}


        public bool AddEntity(RpgObject entity)
        {
            if (!ObjectStore.ContainsKey(entity.Id))
            {
                ObjectStore.Add(entity.Id, entity);
                return true;
            }

            return false;
        }

        public bool RemoveEntity(RpgObject entity)
        {
            if (ObjectStore.ContainsKey(entity.Id))
            {
                ObjectStore.Remove(entity.Id);
                return true;
            }

            return false;
        }

        public T? GetEntity<T>(string? entityId)
            where T : RpgObject
                => entityId != null && ObjectStore.ContainsKey(entityId)
                    ? ObjectStore[entityId] as T
                    : null;

        public RpgObject? GetEntity(string? entityId)
            => entityId != null && ObjectStore.ContainsKey(entityId)
                ? ObjectStore[entityId]
                : null;

        public IEnumerable<RpgObject> GetEntities()
            => ObjectStore.Values;

        public IEnumerable<RpgObject> GetEntities(string entityId, ModScope scope)
        {
            var all = ObjectStore.Values.Where(x => x.Id == entityId || (x as RpgEntityComponent)?.EntityId == entityId);
            if (scope == ModScope.Objects)
                return all;

            if (scope == ModScope.Components)
            {
                var components = all
                    .Where(x => x is RpgEntityComponent)
                    .Select(x => x as RpgEntityComponent)
                    .Where(x => x != null)
                    .Cast<RpgObject>();

                return components;
            }

            return all.Where(x => x.Id == entityId);
        }
            

        public ModSet[] GetModSets()
            => ObjectStore.Values
                .SelectMany(x => x.ModSetStore.Get())
                .ToArray();

        public ModSet? GetModSet(RpgObject rpgObj, string name)
        {
            var modSet = rpgObj.ModSetStore.Get(name);
            if (modSet != null)
                return modSet;

            return null;
        }

        public ModSet? GetModSet(string modSetId)
        {
            foreach (var rpgObj in ObjectStore.Values)
            {
                var modSet = rpgObj.ModSetStore[modSetId];
                if (modSet != null)
                    return modSet;
            }

            return null;
        }

        public void RemoveModSet(string modSetId)
        {
            foreach (var rpgObj in ObjectStore.Values)
            {
                var modSet = rpgObj.ModSetStore[modSetId];
                if (modSet != null)
                {
                    rpgObj.ModSetStore.Remove(modSetId);
                    return;
                }
            }
        }

        public void RemoveModSet(RpgObject rpgObj, string modSetId)
        {
            var modSet = rpgObj.ModSetStore[modSetId];
            if (modSet != null)
                rpgObj.ModSetStore.Remove(modSet.Id);
        }

        public void RemoveModSetByName(RpgObject rpgObj, string name)
        {
            var modSet = rpgObj.ModSetStore.Get(name);
            if (modSet != null)
                rpgObj.ModSetStore.Remove(modSet.Id);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(PropRef propRef, Func<Mod, bool>? filterFunc = null)
        {
            var entity = GetEntity(propRef.EntityId);
            return CalculatePropValue(entity, propRef.Prop, filterFunc);
        }

        /// <summary>
        /// Shallow recalculation of a property value
        /// </summary>
        /// <param name="propRef"></param>
        /// <returns></returns>
        public Dice? CalculatePropValue(RpgObject? entity, string? prop, Func<Mod, bool>? filterFunc = null)
        {
            if (entity == null || string.IsNullOrEmpty(prop))
                return null;

            var mods = filterFunc != null
                ? GetMods(entity, prop, filterFunc)
                : GetMods(entity, prop);

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

            Dice value = mod.SourceValue ?? GetPropValue(GetEntity(mod.SourcePropRef!.EntityId), mod.SourcePropRef.Prop);

            if (mod.SourceValueFunc.IsCalc)
            {
                var funcEntity = (object?)GetEntity(mod.SourceValueFunc.EntityId)
                    ?? this;

                value = funcEntity.ExecuteFunction<Dice, Dice>(mod.SourceValueFunc.FullName!, value);
            }

            return value;
        }

        public Dice GetPropValue(RpgObject? entity, string? prop)
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
                var mods = GetMods(entity, prop);
                var dice = CalculateModsValue(mods);

                return dice;
            }

            return Dice.Zero;
        }

        public void OnPropUpdated(PropRef propRef)
            => UpdatedProps.Merge(propRef);

        public void SetPropValue(PropRef propRef)
        {
            var entity = GetEntity(propRef.EntityId);
            SetPropValue(entity, propRef.Prop);
        }

        public void SetPropValue(RpgObject? entity, string prop)
        {
            var oldValue = GetPropValue(entity, prop);
            var newValue = CalculatePropValue(entity, prop);

            if (oldValue == null || oldValue != newValue)
                entity.PropertyValue(prop, newValue);
        }

        public Dice? GetInitialPropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseInitMod);

        public Dice? GetBasePropValue(RpgObject? entity, string prop)
            => CalculatePropValue(entity, prop, mod => mod.IsBaseMod);

        public void TriggerUpdate()
        {
            //Call OnBeforeUpdate() all rpg objects to set expiry and expire/remove
            // modsets as needed
            foreach (var entity in ObjectStore.Values)
                entity?.OnBeforeUpdate(this);

            UpdateProps();

            //Call OnAfterUpdate() on each rpg object to give states a chance to update
            // after property values have been updated
            foreach (var entity in ObjectStore.Values)
                entity?.OnAfterUpdate(this);

            //If OnAfterUpdate() has caused any props to change then update the relevant props
            UpdateProps();
        }

        private void UpdateProps()
        {
            //It is IMPORTANT that Merge() adds the propRefs in the right order because
            // after this step the assumption is that they can be updated from top to bottom
            // so that all child props are updated before their parents
            var propsToUpdate = new List<PropRef>();
            foreach (var propRef in UpdatedProps)
            {
                var affectedBy = GetPropsAffectedBy(propRef);
                propsToUpdate.Merge(affectedBy);
            }

            //Clear the UpdatedProps queue.
            UpdatedProps.Clear();

            foreach (var propRef in propsToUpdate)
                SetPropValue(propRef);
        }

        public void TriggerUpdate(PropRef propRef)
        {
            var propsAffected = GetPropsAffectedBy(propRef);

            foreach (var entityId in propsAffected.Select(x => x.EntityId).Distinct())
                GetEntity(entityId)?.OnBeforeUpdate(this);

            foreach (var targetPropRef in propsAffected)
                SetPropValue(targetPropRef);
        }

        public void NewEncounter()
        {
            if (Turn > ModBehavior.EncounterStart)
                EndEncounter();

            if (Turn != ModBehavior.EncounterStart)
            {
                Turn = ModBehavior.EncounterStart;
                TriggerUpdate();
            }

            NewTurn();
        }

        public void EndEncounter()
        {
            Turn = ModBehavior.EncounterEnd;
            TriggerUpdate();
        }

        public void NewTurn()
        {
            Turn++;
            TriggerUpdate();
        }

        public void PrevTurn()
        {
            if (Turn > ModBehavior.EncounterStart + 1)
            {
                Turn--;
                TriggerUpdate();
            }
        }

        public void SetTurn(int turn)
        {
            if (turn > ModBehavior.EncounterStart && Turn < ModBehavior.EncounterEnd && turn != Turn)
            {
                Turn = turn;
                TriggerUpdate();
            }
        }
    }
}
