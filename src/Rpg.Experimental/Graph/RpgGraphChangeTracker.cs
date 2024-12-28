using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Graph
{
    public class RpgPropertyChangeTracker
    {
        public List<RpgPropertyRef> UpdatedProps = new List<RpgPropertyRef>();

        public void OnPropUpdated(string objectId, string prop)
            => OnPropUpdated(new RpgPropertyRef(objectId, prop));

        public void OnPropUpdated(params RpgPropertyRef[] propRefs)
            => UpdatedProps.Merge(propRefs);

        public void OnPropsUpdated(RpgGraph graph)
        {
            foreach (var objData in graph.ObjectData.Values)
                foreach (var prop in objData.Props)
                    UpdatedProps.Merge(new RpgPropertyRef(prop.ObjectId, prop.Prop));
        }
    }

    public static class RpgPropertyChangeTrackerExtensions
    {
        internal static void Merge(this List<RpgPropertyRef> target, RpgPropertyRef propRef)
        {
            if (!target.Any(x => x == propRef))
                target.Add(propRef);
        }

        internal static void Merge(this List<RpgPropertyRef> target, IEnumerable<RpgPropertyRef> source)
        {
            foreach (var a in source)
                target.Merge(a);
        }
    }
}
