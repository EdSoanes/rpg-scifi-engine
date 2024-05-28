using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects.States
{
    public class ModState : ITemporal
    {
        protected ModGraph? Graph { get; set; }

        [JsonProperty] public Guid EntityId { get; protected set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string? ShouldActivateMethod { get; protected set; }
        [JsonProperty] public string? OnActivateMethod { get; protected set; }

        public string InstanceName { get => $"{EntityId}.{Name}"; }

        [JsonConstructor] private ModState() { }
        public ModState(Guid entityId, string name, string? shouldActivateMethod = null, string? onActivateMethod = null)
        {
            EntityId = entityId;
            Name = name;
            ShouldActivateMethod = shouldActivateMethod;
            OnActivateMethod = onActivateMethod;
        }

        public void SetActive()
            => Graph?.GetEntity(EntityId)?.AddMod<ForceStateBehavior, ModObject>(InstanceName, 1);

        public void SetInactive()
        {
            var entity = Graph?.GetEntity(EntityId);
            if (entity != null)
            {
                var stateMods = Graph?.GetEntity(EntityId)?
                    .GetMods(InstanceName, false)
                    .Where(x => x.Name == InstanceName && x.Behavior.Type == ModType.ForceState)
                    .ToArray();

                if (stateMods != null)
                    entity.RemoveMods(stateMods);
            }
        }

        protected virtual bool ShouldActivate()
        {
            if (!string.IsNullOrEmpty(ShouldActivateMethod))
            {
                var entity = Graph?.GetEntity(EntityId);
                return entity?.ExecuteFunction<bool>(ShouldActivateMethod) ?? false;
            }

            return false;
        }

        protected void UpdateActivation()
        {
            var entity = Graph?.GetEntity(EntityId);
            if (entity != null)
            {
                var isConditionallyActive = entity.IsStateConditionallyActive(Name);
                var shouldActivate = ShouldActivate();

                if (!isConditionallyActive && shouldActivate)
                    entity!.AddMod<ExpireOnZeroBehavior, ModObject>(InstanceName, 1);
                else if (isConditionallyActive && !shouldActivate)
                    entity!.AddMod<ExpireOnZeroBehavior, ModObject>(InstanceName, -1);
            }

            UpdateStateMods();
        }

        private bool ShouldAddStateModSet(ModObject entity)
            => entity.IsStateActive(Name) && (entity.IsStateForcedActive(Name) || ShouldActivate()) && entity.GetModSet(InstanceName) == null;

        private bool ShouldRemoveStateModSet(ModObject entity)
            => !entity.IsStateActive(Name) && !ShouldActivate() && entity.GetModSet(InstanceName) != null;

        protected void UpdateStateMods(ModObject? entity = null)
        {
            entity ??= Graph?.GetEntity(EntityId);
            if (entity != null)
            {
                if (ShouldAddStateModSet(entity))
                {
                    if (!string.IsNullOrEmpty(OnActivateMethod))
                    {
                        var modSet = new ModSet(EntityId, InstanceName);
                        entity.ExecuteAction(OnActivateMethod, modSet);
                        if (modSet.Mods.Any())
                            entity?.AddModSet(modSet);
                    }
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
