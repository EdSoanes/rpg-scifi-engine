namespace Rpg.Experimental.Graph
{
    public class RpgGraphChangeTracker
    {
        public List<string> TimeEventObjects = new();
        public List<RpgPropertyRef> UpdatedProps = new();

        public bool AddTimeEventObject(string objectId)
        {
            if (!TimeEventObjects.Contains(objectId))
            {
                TimeEventObjects.Add(objectId);
                return true;
            }

            return false;
        }

        public void PropUpdated(string objectId, string prop)
            => PropsUpdated(new RpgPropertyRef(objectId, prop));

        public void PropsUpdated(params RpgPropertyRef[] propRefs)
        {
            UpdatedProps.Merge(propRefs);
        }

        public void AllPropsUpdated(RpgGraph graph)
        {
            foreach (var objData in graph.ObjectData.Values)
                foreach (var prop in objData.Props)
                    UpdatedProps.Merge(new RpgPropertyRef(prop.ObjectId, prop.Prop));
        }

        public bool UnsyncedProperties(string objectId)
            => UpdatedProps.Any(x => x.ObjectId == objectId);

        public void SyncProperties(RpgGraph graph)
        {
            foreach (var byObjId in UpdatedProps.GroupBy(x => x.ObjectId))
            {
                var objData = graph.GetObjectData(byObjId.Key);
                foreach (var propRef in byObjId)
                    objData?.GetPropData(propRef.Path)?.OnSyncProperty(graph);
            }

            UpdatedProps.Clear();
            TimeEventObjects.Clear();
        }

        public void SyncProperties(RpgGraph graph, string? objectId)
        {
            if (objectId == null) return;

            var propRefs = UpdatedProps.Where(x => x.ObjectId == objectId).ToList();
            if (propRefs.Any())
            {
                var objData = graph.GetObjectData(objectId);
                foreach (var propRef in propRefs)
                {
                    objData?.GetPropData(propRef.Path)?.OnSyncProperty(graph);
                    UpdatedProps.Remove(propRef);
                }

                if (TimeEventObjects.Contains(objectId))
                    TimeEventObjects.Remove(objectId);
            }
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
