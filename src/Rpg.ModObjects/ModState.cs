using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
            => IsForcedActive = true;

        public void SetInactive()
            => IsForcedActive = false;

        protected virtual bool ShouldApply()
            => false;

        protected abstract ModSet CreateState();

        protected void Apply()
        {
            if (Graph != null && EntityId != null)
            {
                var entity = Graph.GetEntity(EntityId);
                if ((IsForcedActive || ShouldApply()) && !IsApplied)
                {
                    var modSet = CreateState();
                    modSet.Name = Name;

                    entity?.AddModSet(modSet);
                }
                else if (IsApplied)
                {
                    entity?.RemoveModSet(Name);
                }
            }
        }

        public void OnBeginEncounter()
            => Apply();

        public void OnEndEncounter()
            => Apply();

        public void OnGraphCreating(ModGraph graph, ModObject entity)
        {
            Graph = graph;
            EntityId = entity.Id;
        }

        public void OnTurnChanged(int turn)
            => Apply();
    }

    public abstract class ModState<T> : ModState
        where T : ModObject
    {
        [JsonConstructor] private ModState() { }

        protected ModState(string name)
            => Name = name;

        protected T? Entity
        {
            get
            {
                if (Graph != null && EntityId != null)
                    return Graph.GetEntity(EntityId) as T;

                return null;
            }
        }

        protected override ModSet<T> CreateState()
        {
            var modSet = new ModSet<T>();
            OnCreateState(modSet);
            return modSet;
        }

        protected abstract void OnCreateState(ModSet<T> modSet);
    }
}
