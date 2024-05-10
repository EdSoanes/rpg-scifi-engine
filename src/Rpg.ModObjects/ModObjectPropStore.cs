using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rpg.ModObjects.Values;
using System;
using System.Collections;
using System.Reflection;

namespace Rpg.ModObjects
{
    //public class ModObjectPropStoreConverter : JsonConverter<ModObjectPropStore>
    //{
    //    public override ModObjectPropStore? ReadJson(JsonReader reader, Type objectType, ModObjectPropStore? existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        var jObj = JObject.Load(reader);
    //        var res = jObj.ToObject<Dictionary<string, ModObjectProp>>();

    //        if (res != null)
    //        {
    //            var propStore = (ModObjectPropStore)Activator.CreateInstance(typeof(ModObjectPropStore), true, res)!;
    //            return propStore;
    //        }

    //        return null;
    //    }

    //    public override void WriteJson(JsonWriter writer, ModObjectPropStore? value, JsonSerializer serializer)
    //    {
    //        if (value != null)
    //        {
    //            var modObjProps = value.GetType()
    //                .GetProperty("ModObjProps", BindingFlags.NonPublic | BindingFlags.Instance)
    //                !.GetValue(value) as Dictionary<string, ModObjectProp>;

    //            if (modObjProps != null)
    //            {
    //                var o = JObject.FromObject(modObjProps, serializer);
    //                o.WriteTo(writer);
    //            }
    //        }
    //    }
    //}

    //[JsonConverter(typeof(ModObjectPropStoreConverter))]
    public class ModObjectPropStore
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public Guid EntityId { get; set; }

        [JsonProperty] protected Dictionary<string, ModObjectProp> ModObjProps { get; set; } = new Dictionary<string, ModObjectProp>();

        public ModObjectProp? this[string prop]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(prop))
                    return null;

                if (ModObjProps.ContainsKey(prop))
                    return ModObjProps[prop];

                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value?.Prop) && !ModObjProps.ContainsKey(prop))
                    ModObjProps.Add(prop, value);
            }
        }

        private ModObjectPropStore(Dictionary<string, ModObjectProp> modObjProps)
            => ModObjProps = modObjProps;

        public ModObjectPropStore() { }

        public void Initialize(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public string[] AllProps()
            => ModObjProps.Keys.ToArray();

        public Mod[] Get(string prop)
            => ModObjProps[prop].Get(Graph!);

        //public Mod[] Get(string prop, ModType modType)
        //    => ModObjProps[prop].Get(Graph!, modType);

        public Mod[] Get(string prop, ModType modType, string modName)
            => ModObjProps[prop].Get(modType, modName);

        public bool Contains(string prop)
            => ModObjProps.ContainsKey(prop);

        public ModObjectProp Create(string prop)
        {
            if (!Contains(prop))
                this[prop] = new ModObjectProp(EntityId, prop);

            return this[prop]!;
        }

        public Dice Calculate(string prop, ModType? modifierType = null, string? modifierName = null)
        {
            var mods = !string.IsNullOrEmpty(modifierName) && modifierType != null
                ? Get(prop, modifierType.Value, modifierName)
                : Get(prop);

            Dice dice = "0";
            foreach (var mod in mods)
            {
                Dice value = mod.Source.Calculate(Graph!);
                dice += value;
            }

            return dice;
        }

        public void Remove(IEnumerable<Mod> mods)
        {
            foreach (var mod in mods)
            {
                var entity = Graph!.GetEntity<ModObject>(mod.EntityId);
                if (entity != null)
                    entity.PropStore[mod.Prop]!.Remove(mod);
            }
        }


        public void Add(params Mod[] mods)
        {
            foreach (var mod in mods)
            {
                mod.OnAdd(Graph!.Turn);

                var entity = Graph!.GetEntity<ModObject>(mod.EntityId);
                if (entity != null)
                {
                    var modProp = entity.PropStore.Create(mod.Prop);
                    if (modProp.Contains(mod))
                        modProp.Remove(mod.Id);

                    if (mod.ModifierAction == ModAction.Accumulate)
                        modProp.Add(mod);

                    else if (mod.ModifierAction == ModAction.Replace)
                        modProp.Replace(mod);

                    else if (mod.ModifierAction == ModAction.Sum)
                    {
                        var oldValue = entity.PropStore.Calculate(mod.Prop, mod.ModifierType, mod.Name);
                        var newValue = oldValue + mod.Source.Calculate(Graph!);

                        mod.SetSource(newValue);
                        modProp.Replace(mod);
                    }
                }
            }
        }

        public string[] RemoveExpiredProps()
        {
            var updatedProps = new List<string>();
            var turn = Graph!.Turn;

            foreach (var modObjProp in ModObjProps.Values)
            {
                var toRemove = new List<Mod>();
                foreach (var mod in modObjProp.Mods)
                {
                    mod.OnUpdate(turn);

                    var expiry = mod.Duration.GetExpiry(turn);
                    if (expiry == ModExpiry.Expired && mod.Duration.CanRemove(Graph!.Turn))
                        toRemove.Add(mod);
                }

                if (toRemove.Any())
                {
                    Remove(toRemove);
                    updatedProps.Add(modObjProp.Prop);
                }
            }

            return updatedProps.ToArray();
        }

        public List<ModObjectPropRef> PropsAffectedBy(ModObjectPropRef propRef)
        {
            var res = new List<ModObjectPropRef>();
            res.Merge(propRef);

            var propsAffectedBy = new List<ModObjectPropRef>();
            foreach (var entity in Graph!.GetEntities())
            {
                var affectedBy = entity.PropStore.AllProps()
                    .Where(x => entity.PropStore[x]!.IsAffectedBy(propRef))
                    .Select(x => new ModObjectPropRef(entity.Id, x))
                    .Distinct();

                res.Merge(affectedBy);

                foreach (var propAffectedBy in affectedBy)
                {
                    var parentEntity = Graph!.GetEntity<ModObject>(propAffectedBy.EntityId);
                    var parentAffects = parentEntity!.PropStore.PropsAffectedBy(propAffectedBy);

                    res.Merge(parentAffects);
                }
            }

            return res;
        }

        public List<ModObjectPropRef> AffectedByProps()
        {
            var res = ModObjProps.Keys
                .SelectMany(AffectedByProps)
                .Distinct()
                .ToList();

            return res;
        }

        public List<ModObjectPropRef> AffectedByProps(string prop)
        {
            var res = new List<ModObjectPropRef>();

            var affectedByGroup = Get(prop)
                .Where(x => x.Source.EntityId != null && !string.IsNullOrEmpty(x.Source.Prop))
                .Select(x => new ModObjectPropRef(x.Source.EntityId!.Value, x.Source.Prop!))
                .Distinct()
                .GroupBy(x => x.EntityId);

            foreach (var affectedByProp in affectedByGroup)
            {
                var entity = Graph!.GetEntity<ModObject>(affectedByProp.Key);
                if (entity != null)
                {
                    var childProps = affectedByProp
                        .SelectMany(x => entity.PropStore.AffectedByProps(x.Prop))
                        .Distinct();

                    res.Merge(childProps);
                }

                res.Merge(affectedByProp);
            }

            res.Merge(new ModObjectPropRef(EntityId, prop));
            return res;
        }
    }
}
