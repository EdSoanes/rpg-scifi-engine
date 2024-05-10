using Newtonsoft.Json;
using Rpg.ModObjects.Values;

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
    public class ModSetStore
    {
        [JsonIgnore] private ModGraph? Graph { get; set; }
        [JsonIgnore] public Guid EntityId { get; set; }
        [JsonProperty] public List<ModSet> ModSets { get; private set; } = new List<ModSet>();

        public ModSetStore() { }

        public void Initialize(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void Add(ModSet modSet)
        {
            if (!ModSets.Contains(modSet))
                ModSets.Add(modSet);
        }

        public void UpdatePropExpiry()
        {
            foreach (var modSet in ModSets)
                modSet.UpdatePropExpiry(Graph!.Turn);
        }

        public void RemoveExpiredMods()
        {
            foreach (var modSet in ModSets)
                modSet.RemoveExpiredMods(Graph!.Turn);
        }
    }
}
