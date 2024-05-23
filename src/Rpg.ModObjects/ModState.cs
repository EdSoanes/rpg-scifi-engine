using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects
{
    public abstract class ModState : ITemporal
    {
        protected ModGraph? Graph { get; set; }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public Guid? EntityId { get; protected set; }

        [JsonProperty] public string Name { get; protected set; } = nameof(ModState);
        
        public string InstanceName { get => $"{Id}.{Name}"; }

        public void SetActive()
        {
            var entity = Graph?.GetEntity(EntityId);
            if (entity != null && !entity.IsStateForcedActive(Name))
                entity?.AddPermanentMod(InstanceName, 1);

            UpdateStateMods(entity);
        }

        public void SetInactive()
        {
            var entity = Graph?.GetEntity(EntityId);
            entity?.RemoveMods(InstanceName, ModType.Permanent);
            UpdateStateMods(entity);
        }

        protected virtual bool ShouldActivate()
            => false;

        protected void UpdateActivation()
        {
            var entity = Graph?.GetEntity(EntityId);
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

        protected void UpdateStateMods(ModObject? entity = null)
        {
            entity ??= Graph?.GetEntity(EntityId);
            if (entity != null)
            {
                if (ShouldAddStateModSet(entity))
                {
                    var modSet = new ModSet(InstanceName);
                    OnActivate(modSet);
                    if (modSet.Mods.Any())
                        entity?.AddModSet(modSet);
                }
                else if (ShouldRemoveStateModSet(entity))
                    entity?.RemoveModSet(InstanceName);
            }
        }


        protected virtual void OnActivate(ModSet modSet) { }

        public void OnBeginEncounter()
            => UpdateActivation();

        public void OnEndEncounter()
            => UpdateActivation();

        public void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnTurnChanged(int turn)
            => UpdateActivation();
    }
}
