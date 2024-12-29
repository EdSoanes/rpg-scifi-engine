using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rpg.Experimental.Graph
{
    public class RpgGraphChangeTracker
    {
        public List<RpgPropertyRef> UpdatedProps = new();
        public List<string> UpdatedObjects = new();

        public void PropUpdated(string objectId, string prop)
            => PropsUpdated(new RpgPropertyRef(objectId, prop));

        public void PropsUpdated(params RpgPropertyRef[] propRefs)
        {
            UpdatedProps.Merge(propRefs);
            foreach (var propRef in propRefs)
            {
                if (UpdatedObjects.Contains(propRef.ObjectId))
                    UpdatedObjects.Remove(propRef.ObjectId);
            }
        }

        public void AllPropsUpdated(RpgGraph graph)
        {
            foreach (var objData in graph.ObjectData.Values)
                foreach (var prop in objData.Props)
                    UpdatedProps.Merge(new RpgPropertyRef(prop.ObjectId, prop.Prop));
        }

        public bool ObjectNeedsUpdating(string objectId)
            => UpdatedProps.Any(x => x.ObjectId == objectId);

        public bool IsObjectUpdated(string objectId)
            => UpdatedObjects.Any(x => x == objectId);

        public void ObjectUpdated(string objectId)
        {
            if (!IsObjectUpdated(objectId))
                UpdatedObjects.Add(objectId);
        }

        public void Update(RpgGraph graph)
        {
            foreach (var byObjId in UpdatedProps.GroupBy(x => x.ObjectId))
            {
                var objData = graph.GetObjectData(byObjId.Key);
                foreach (var propRef in byObjId)
                    objData?.GetPropData(propRef.Prop)?.OnSyncProperty(graph);
            }

            Clear();
        }

        public void Clear()
        {
            UpdatedProps.Clear();
            UpdatedObjects.Clear();
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
