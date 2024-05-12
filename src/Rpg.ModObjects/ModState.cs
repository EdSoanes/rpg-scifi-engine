using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public abstract class ModState<T> : ITemporal
        where T : ModObject
    {
        public string Name { get; protected set; } = nameof(ModState<T>);

        private ModGraph? _graph;
        private T? _entity;

        public bool IsApplied => _entity?.GetModSet(Name) != null;

        protected abstract bool ShouldApply(T entity);
        protected abstract ModSet CreateState(T entity);

        protected void Apply()
        {
            if (_entity != null)
            {
                if (ShouldApply(_entity))
                {
                    var modSet = CreateState(_entity);
                    modSet.Name = Name;

                    _entity.AddModSet(modSet);
                }
                else if (IsApplied)
                {
                    _entity.RemoveModSet(Name);
                }
            }
        }

        public void OnBeginEncounter()
            => Apply();

        public void OnEndEncounter()
            => Apply();

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            _graph = graph;
            _entity = entity as T;
        }

        public void OnTurnChanged(int turn)
            => Apply();
    }
}
