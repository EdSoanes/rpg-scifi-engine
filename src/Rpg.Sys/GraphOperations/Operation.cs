using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.GraphOperations
{
    public abstract class Operation
    {
        protected readonly Graph Graph;
        private readonly List<ModProp> _changedProperties = new List<ModProp>();

        public Operation(Graph graph) => Graph = graph;

        public bool Restoring { get; set; }

        public virtual void Execute(params ModdableObject[] entities) { }
        public virtual void Execute(params Condition[] conditions) { }
        public virtual void Execute(params Modifier[] mods) { }

        protected void AddPropertyChanged(params ModProp?[] modProps)
        {
            if (modProps != null)
            {
                foreach (var modProp in modProps.Where(x => x != null))
                {
                    if (!_changedProperties.Any(x => x.EntityId == modProp!.EntityId && x.Prop == modProp.Prop))
                        _changedProperties.Add(modProp!);
                }
            }
        }

        protected List<ModdableObject> Descendants(object obj)
        {
            var res = new List<ModdableObject>();
            var entity = obj as ModdableObject;
            if (entity != null)
                res.Add(entity);

            if (!obj.GetType().IsPrimitive)
            {
                foreach (var propertyInfo in obj.GetType().GetProperties())
                {
                    var items = obj.PropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                    foreach (var item in items)
                    {
                        var childEntities = Descendants(item);
                        res.AddRange(childEntities);
                    }
                }
            }

            return res;
        }

        public void NotifyPropertyChanged()
        {
            if (!Restoring)
            {
                foreach (var modProp in _changedProperties)
                {
                    var affected = Graph.Mods.GetAffectedModProps(modProp);
                    AddPropertyChanged(affected.ToArray());
                }

                PropertyChanged();
            }
        }

        public void NotifyPropertyChanged(Guid entityId, string prop)
        {
            var modProp = Graph.Mods.Get(entityId, prop);
            if (modProp != null)
            {
                AddPropertyChanged(modProp);
                NotifyPropertyChanged();
            }
        }

        private void PropertyChanged()
        {
            if (!Restoring)
            {
                foreach (var mp in _changedProperties.Where(x => x != null).GroupBy(x => x!.EntityId))
                {
                    var entity = Graph!.Entities.Get(mp.Key);
                    foreach (var p in mp)
                    {
                        p.Evaluate(Graph);
                        entity?.SetModdableProperty(p.Prop, p.Value);
                    }
                }
            }

            _changedProperties.Clear();
        }
    }
}
