using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;
using System.Xml.Linq;

namespace Rpg.ModObjects.Description
{
    public class ObjectPropDescriber
    {
        public static ObjectPropInfo? Describe(RpgGraph graph, RpgObject rpgObj, string propPath)
        {
            var objPropInfo = new ObjectPropInfo(rpgObj, propPath);
            objPropInfo.PropInfo = GetPropInfo(graph, rpgObj.Id, propPath);

            return objPropInfo;
        }

        public static ModSetValues Values(RpgGraph graph, State state)
        {
            var instance = state.GetInstances().FirstOrDefault();
            if (instance == null)
            {
                instance = new StateModSet(state.OwnerId, state.Name, StateInstanceType.Manual);
                instance.OnCreating(graph);
                state.FillStateSet(instance);
            }

            return Values(graph, instance);
        }

        public static ModSetValues Values(RpgGraph graph, ModSet modSet)
        {
            var res = new ModSetValues(modSet.Name);
            var entity = graph.GetObject(modSet.OwnerId);
            if (entity != null)
            {
                var isAdded = entity.ModSets.ContainsKey(modSet.Id);
                var isApplied = modSet.IsApplied;

                if (!isAdded)
                {
                    entity.AddModSet(modSet);
                    graph.Time.TriggerEvent();
                }

                if (!isApplied)
                {
                    modSet.Apply();
                    graph.Time.TriggerEvent();
                }

                foreach (var modGroup in modSet.Mods.GroupBy(x => x.Target))
                {
                    var val = ModCalculator.Value(graph, modGroup) ?? Dice.Zero;
                    res.Set(modGroup.Key, val);
                }

                if (!isApplied)
                {
                    modSet.Unapply();
                    graph.Time.TriggerEvent();
                }

                if (!isAdded)
                {
                    entity.RemoveModSet(modSet.Id);
                    graph.Time.TriggerEvent();
                }
            }


            return res;
        }

        private static PropInfo GetPropInfo(RpgGraph graph, string entityId, string propPath)
        {
            var entity = graph.GetObject(entityId);
            var (targetEntity, targetProp) = entity.FromPath(propPath);
            if (targetEntity == null || targetProp == null)
                throw new InvalidOperationException($"Could not find prop on obj {entityId}.{propPath}");

            var propInfo = new PropInfo(targetEntity, targetProp);
            propInfo.Value = ModCalculator.Value(graph, targetEntity.GetMods(targetProp)) ?? Dice.Zero;

            var modInfos = ModFilters.Active(targetEntity.GetMods(targetProp))
                .Select(x => GetModInfo(graph, x))
                .Where(x => x != null)
                .Cast<ModInfo>()
                .ToList();

            return propInfo;
        }

        public static ModInfo? GetModInfo(RpgGraph graph, Mod mod)
        {
            var value = mod.Value() ?? Dice.Zero;
            var modInfo = new ModInfo(mod, value);

            if (mod.Source != null)
                modInfo.Source = GetPropInfo(graph, mod.Source.EntityId, mod.Source.Prop);

            if (mod.Behavior is Threshold threshold)
            {
                modInfo.AdditionalInfo.Add("Min", threshold.Min);
                modInfo.AdditionalInfo.Add("Max", threshold.Max);
            }

            return modInfo;
        }
    }
}
