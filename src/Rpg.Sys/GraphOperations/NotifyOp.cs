using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Sys.Moddable;

namespace Rpg.Sys.GraphOperations
{
    public class NotifyOp
    {
        private readonly Graph Graph;
        private readonly List<Guid> _changedProperties = new List<Guid>();

        public bool Restoring { get; set; }

        public NotifyOp(Graph graph) 
        {
            Graph = graph;
        }

        public void Queue(params Guid[] modPropIds)
        {
            if (!Restoring && modPropIds != null)
            {
                foreach (var modPropId in modPropIds)
                {
                    if (!_changedProperties.Any(x => x == modPropId))
                        _changedProperties.Add(modPropId);
                }
            }
        }

        public void Queue(params ModProp?[] modProps)
            => Queue(modProps.Select(x => x.Id).ToArray());

        public void Send()
        {
            if (!Restoring)
            {
                var affected = new List<ModProp>();
                foreach (var modPropId in _changedProperties)
                {
                    var modProp = Graph.Get.ModProp(modPropId);
                    if (modProp != null)
                        affected.AddRange(Graph.Get.AffectedModProps(modProp));
                }
                Queue(affected.ToArray());

                if (affected != null)
                {
                    foreach (var group in affected!.GroupBy(x => x!.EntityId))
                    {
                        var entity = Graph.Get.Entity<ModObject>(group.Key);
                        foreach (var modProp in group)
                            entity?.SetModdableValue(modProp.Prop);
                    }
                }

                _changedProperties.Clear();
            }
        }

        public void Send(PropRef propRef)
            => Send(propRef.EntityId, propRef.Prop);

        public void Send(Guid entityId, string prop)
        {
            var modProp = Graph.Get.ModProp(entityId, prop);
            if (modProp != null)
            {
                Queue(modProp);
                Send();
            }
        }
    }
}
