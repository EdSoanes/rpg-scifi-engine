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

        public Operation(Graph graph) 
            => Graph = graph;

        public bool Restoring { get; set; }

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
    }
}
