﻿using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.States
{
    public class ModState : IGraphEvents
    {
        protected RpgGraph? Graph { get; set; }

        [JsonProperty] public string EntityId { get; protected set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string? ShouldActivateMethod { get; protected set; }
        [JsonProperty] public string? OnActivateMethod { get; protected set; }

        public string InstanceName { get => $"{EntityId}.{Name}"; }

        [JsonConstructor] private ModState() { }

        public ModState(string entityId, string name, string? shouldActivateMethod = null, string? onActivateMethod = null)
        {
            EntityId = entityId;
            Name = name;
            ShouldActivateMethod = shouldActivateMethod;
            OnActivateMethod = onActivateMethod;
        }

        public void SetActive()
            => Graph!.AddMods(new ForceStateMod(this).Create());

        public void SetInactive()
        {
            var entity = Graph!.GetEntity(EntityId);
            var stateModSet = Graph!.GetModSet(entity!, InstanceName);
            stateModSet?.Lifecycle.SetExpired(Graph!.Time.Current);

            var stateMods = Graph!
                .GetMods(entity, InstanceName, mod => mod.Behavior.Type == ModType.ForceState)
                .ToArray();

            Graph!.RemoveMods(stateMods);
        }

        public bool IsActive()
        {
            var entity = Graph!.GetEntity(EntityId);
            return Graph!.GetPropValue(entity, InstanceName) != Dice.Zero;
        }

        public bool IsForcedActive()
        {
            var entity = Graph!.GetEntity(EntityId);
            return Graph!.CalculatePropValue(entity, InstanceName, mod => mod.Behavior is ForceState) != Dice.Zero;
        }

        public bool IsConditionallyActive()
        {
            var entity = Graph!.GetEntity(EntityId);
            return Graph!.CalculatePropValue(entity, InstanceName, mod => mod.Behavior is State) != Dice.Zero;
        }

        protected virtual bool ShouldActivate()
        {
            if (!string.IsNullOrEmpty(ShouldActivateMethod))
            {
                var entity = Graph!.GetEntity(EntityId);
                return entity?.ExecuteFunction<bool>(ShouldActivateMethod) ?? false;
            }

            return false;
        }

        public void OnGraphCreating(RpgGraph graph, RpgObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnObjectsCreating() { }

        public void OnUpdating(RpgGraph graph, TimePoint time) 
        {
            var entity = graph.GetEntity(EntityId);

            //Activate state if it fulfills the conditional
            var isConditionallyActive = IsConditionallyActive();
            var shouldActivate = ShouldActivate();
            if (!isConditionallyActive && shouldActivate)
                graph!.AddMods(new StateMod(this, 1).Create());
            else if (isConditionallyActive && !shouldActivate)
                graph!.AddMods(new StateMod(this, -1).Create());

            //if the state is active in any way and there is no state modset then create it
            var isActive = IsActive();
            var stateModSet = Graph!.GetModSet(entity!, InstanceName);
            if (isActive && stateModSet == null && !string.IsNullOrEmpty(OnActivateMethod))
            {
                var modSet = new ModSet(EntityId, InstanceName);
                entity.ExecuteAction(OnActivateMethod, modSet);
                if (modSet.Mods.Any())
                    entity?.AddModSet(modSet);
            }
            else if (!isActive && stateModSet != null)
                graph?.RemoveModSetByName(entity!, InstanceName);
        }
    }
}