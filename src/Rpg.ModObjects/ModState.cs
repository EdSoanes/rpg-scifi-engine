using Newtonsoft.Json;
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

        protected ModGraph? _graph;
        protected Guid? _entityId;

        public bool IsApplied
        {
            get
            {
                if (_graph != null && _entityId != null)
                {
                    return _graph.GetEntity(_entityId)
                        ?.GetModSet(Name) != null;
                }

                return false;
            }
        }

        protected abstract bool ShouldApply();

        protected abstract ModSet CreateState();

        protected void Apply()
        {
            if (_graph != null && _entityId != null)
            {
                var entity = _graph.GetEntity(_entityId);
                if (ShouldApply())
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

        public void OnGraphCreating(ModGraph graph, ModObject? entity = null)
        {
            _graph = graph;
            _entityId = entity?.Id;
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
                if (_graph != null && _entityId != null)
                    return _graph.GetEntity(_entityId) as T;

                return null;
            }
        }
    }
}
