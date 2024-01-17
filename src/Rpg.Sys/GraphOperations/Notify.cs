using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.GraphOperations
{
    public class Notify
    {
        private readonly Graph _graph;
        private readonly List<ModProp> _changedProperties = new List<ModProp>();

        public bool Restoring { get; set; }

        public Notify(Graph graph) 
        {
            _graph = graph;
        }

        public void Queue(params ModProp?[] modProps)
        {
            if (!Restoring && modProps != null)
            {
                foreach (var modProp in modProps.Where(x => x != null))
                {
                    if (!_changedProperties.Any(x => x.EntityId == modProp!.EntityId && x.Prop == modProp.Prop))
                        _changedProperties.Add(modProp!);
                }
            }
        }

        public void Send()
        {
            if (!Restoring)
            {
                foreach (var modProp in _changedProperties)
                {
                    var affected = _graph.Mods.GetAffectedModProps(modProp);
                    Queue(affected.ToArray());
                }

                foreach (var group in _changedProperties.GroupBy(x => x!.EntityId))
                {
                    var entity = _graph.Entities.Get(group.Key);
                    foreach (var modProp in group)
                    {
                        modProp.Evaluate(_graph);
                        entity?.SetModdableProperty(modProp.Prop, modProp.Value);
                    }
                }

                _changedProperties.Clear();
            }
        }

        public void Send(Guid entityId, string prop)
        {
            var modProp = _graph.Mods.Get(entityId, prop);
            if (modProp != null)
            {
                Queue(modProp);
                Send();
            }
        }
    }
}
