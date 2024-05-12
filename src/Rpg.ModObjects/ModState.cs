using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public abstract class ModState : ITemporal
    {
        public string Name { get; protected set; } = nameof(ModState);

        private ModGraph? _graph;
        private ModObject? _entity;

        public bool IsApplied => _entity?.GetModSet(Name) != null;

        protected abstract bool ShouldApply();
        protected abstract ModSet CreateState();

        protected void Apply()
        {
            if (ShouldApply())
            {
                var modSet = CreateState();
                modSet.Name = Name;

                _entity!.AddModSet(modSet);
            }
            else if (IsApplied)
            {
                _entity.RemoveModSet(Name);
            }
        }

        public void OnBeginEncounter()
            => Apply();

        public void OnEndEncounter()
            => Apply();

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            _graph = graph;
            _entity = entity;
        }

        public void OnTurnChanged(int turn)
            => Apply();
    }
}
