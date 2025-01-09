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

        public void PropsUpdated(params RpgPropertyRef?[] propRefs)
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
            var groups = UpdatedProps.GroupBy(x => x.ObjectId);
            foreach (var byObjId in groups)
            {
                var objData = graph.GetObjectData(byObjId.Key);
                if (objData != null)
                    foreach (var propRef in byObjId)
                        objData.OnSyncProperty(graph, propRef.Path);
            }

            foreach (var byObjId in groups)
            {
                var obj = graph.GetObject(byObjId.Key);
                if (obj != null)
                    foreach (var propRef in byObjId)
                        obj.OnSyncProperty(graph, propRef.Path);
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
                if (objData != null)
                    foreach (var propRef in propRefs)
                    {
                        objData?.OnSyncProperty(graph, propRef.Path);
                        UpdatedProps.Remove(propRef);
                    }

                foreach (var propRef in propRefs)
                    graph.GetObject(objectId)?.OnSyncProperty(graph, propRef.Path);

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

        internal static void Merge(this List<RpgPropertyRef> target, IEnumerable<RpgPropertyRef?> source)
        {
            foreach (var a in source.Where(x => x != null))
                target.Merge(a!);
        }
    }
}
