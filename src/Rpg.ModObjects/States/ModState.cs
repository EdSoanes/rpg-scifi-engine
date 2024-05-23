using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Modifiers;
using System.Reflection;

namespace Rpg.ModObjects.States
{
    public class ModState<T> : IModState
        where T : ModObject
    {
        protected ModGraph? Graph { get; set; }

        [JsonProperty] public Guid EntityId { get; protected set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string? ShouldActivateMethod { get; protected set; }
        [JsonProperty] public string OnActivateMethod { get; protected set; }

        public string InstanceName { get => $"{EntityId}.{Name}"; }

        [JsonConstructor] private ModState() { }

        public ModState(Guid entityId, string name, string? shouldActivateMethod, string onActivateMethod)
        {
            EntityId = entityId;
            Name = name;
            ShouldActivateMethod = shouldActivateMethod;
            OnActivateMethod = onActivateMethod;
        }

        public void SetActive()
        {
            var entity = Graph?.GetEntity<T>(EntityId);
            if (entity != null && !entity.IsStateForcedActive(Name))
                entity?.AddPermanentMod(InstanceName, 1);

            UpdateStateMods(entity);
        }

        public void SetInactive()
        {
            var entity = Graph?.GetEntity<T>(EntityId);
            entity?.RemoveMods(InstanceName, ModType.Permanent);
            UpdateStateMods(entity);
        }

        protected virtual bool ShouldActivate()
        {
            if (!string.IsNullOrEmpty(ShouldActivateMethod))
            {
                var entity = Graph?.GetEntity<T>(EntityId);
                return entity?.ExecuteFunction<bool>(ShouldActivateMethod) ?? false;
            }

            return false;
        }

        protected void UpdateActivation()
        {
            var entity = Graph?.GetEntity<T>(EntityId);
            if (entity != null)
            {
                var isConditionallyActive = entity.IsStateConditionallyActive(Name);
                var shouldActivate = ShouldActivate();

                if (!isConditionallyActive && shouldActivate)
                    entity!.AddSumMod(InstanceName, 1);
                else if (isConditionallyActive && !shouldActivate)
                    entity!.AddSumMod(InstanceName, -1);
            }

            UpdateStateMods();
        }

        private bool ShouldAddStateModSet(ModObject entity)
            => entity.IsStateActive(Name) && (entity.IsStateForcedActive(Name) || ShouldActivate()) && entity.GetModSet(InstanceName) == null;

        private bool ShouldRemoveStateModSet(ModObject entity)
            => !entity.IsStateActive(Name) && !ShouldActivate() && entity.GetModSet(InstanceName) != null;

        protected void UpdateStateMods(T? entity = null)
        {
            entity ??= Graph?.GetEntity<T>(EntityId);
            if (entity != null)
            {
                if (ShouldAddStateModSet(entity))
                {
                    var modSet = new ModSet(InstanceName);
                    entity.ExecuteAction(OnActivateMethod, modSet);
                    if (modSet.Mods.Any())
                        entity?.AddModSet(modSet);
                }
                else if (ShouldRemoveStateModSet(entity))
                    entity?.RemoveModSet(InstanceName);
            }
        }

        public void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnBeginEncounter()
            => UpdateActivation();

        public void OnEndEncounter()
            => UpdateActivation();

        public void OnTurnChanged(int turn)
            => UpdateActivation();
    }
}
