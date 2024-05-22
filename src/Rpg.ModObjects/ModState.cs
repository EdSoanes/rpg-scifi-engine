using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;

namespace Rpg.ModObjects
{
    public abstract class ModState : ITemporal
    {
        protected ModGraph? Graph { get; set; }
        protected Guid? EntityId { get; set; }

        [JsonProperty] public string Name { get; protected set; } = nameof(ModState);
        [JsonProperty] protected bool IsForcedActive { get; set; }

        public bool IsApplied
        {
            get
            {
                if (Graph != null && EntityId != null)
                {
                    return Graph.GetEntity(EntityId)
                        ?.GetModSet(Name) != null;
                }

                return false;
            }
        }

        public void SetActive()
        {
            var entity = Graph!.GetEntity(EntityId)!;
            entity.AddMod(TurnMod.Create(entity, Name, 1));
            IsForcedActive = true;
        }

        public void SetInactive()
        {
            var entity = Graph!.GetEntity(EntityId)!;
            entity.RemoveModProp(Name);
            IsForcedActive = false;
        }

        protected virtual bool ShouldActivate()
            => false;

        protected void Activate()
        {
            if (Graph != null && EntityId != null)
            {
                var entity = Graph.GetEntity(EntityId);
                if ((IsForcedActive || ShouldActivate()) && !IsApplied)
                {
                    var modSet = new ModSet(Name);
                    OnActivate(modSet);

                    entity?.AddModSet(modSet);
                }
                else if (IsApplied)
                {
                    entity?.RemoveModSet(Name);
                }
            }
        }

        protected abstract void OnActivate(ModSet modSet);

        public void OnBeginEncounter()
            => Activate();

        public void OnEndEncounter()
            => Activate();

        public void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnTurnChanged(int turn)
            => Activate();
    }
}
