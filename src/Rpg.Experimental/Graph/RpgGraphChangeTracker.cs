namespace Rpg.Experimental.Graph
{
    public class RpgGraphChangeTracker
    {
        //True when props have been added to UpdatedProps since their dependents were last added
        private bool _dependentsPending;

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
            if (UpdatedProps.Merge(propRefs))
                _dependentsPending = true;
        }

        public void AllPropsUpdated(RpgGraph graph)
        {
            foreach (var objData in graph.ObjectData.Values)
                foreach (var prop in objData.Props)
                    UpdatedProps.Merge(new RpgPropertyRef(prop.ObjectId, prop.Prop));

            //Every prop is in the list so there are no dependents left to add
            _dependentsPending = false;
        }

        public bool UnsyncedProperties(RpgGraph graph, string objectId)
        {
            AddDependentProps(graph);
            return UpdatedProps.Any(x => x.ObjectId == objectId);
        }

        public void SyncProperties(RpgGraph graph)
        {
            AddDependentProps(graph);

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
            _dependentsPending = false;
        }

        public void SyncProperties(RpgGraph graph, string? objectId)
        {
            if (objectId == null) return;

            //Dependents on other objects stay in UpdatedProps until those objects are synced
            AddDependentProps(graph);

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

        /// <summary>
        /// When a prop changes, every prop that is derived from it (has a mod that uses it as a source) has
        /// changed as well, and so on down the chain. Add those dependents to UpdatedProps so that their
        /// values are synced to their objects too.
        /// </summary>
        private void AddDependentProps(RpgGraph graph)
        {
            if (!_dependentsPending)
                return;

            _dependentsPending = false;

            if (!UpdatedProps.Any())
                return;

            //source prop => the props that have a mod reading from it
            var dependents = new Dictionary<(string, string), List<RpgPropertyRef>>();
            foreach (var objData in graph.ObjectData.Values)
                foreach (var propData in objData.Props.OfType<RpgPropertyDataModdable>())
                    foreach (var mod in propData.Mods)
                    {
                        var source = mod.Source?.PropRef;
                        if (source == null)
                            continue;

                        var key = (source.ObjectId, source.Path);
                        if (!dependents.TryGetValue(key, out var targets))
                        {
                            targets = new List<RpgPropertyRef>();
                            dependents.Add(key, targets);
                        }

                        if (!targets.Any(x => x.ObjectId == propData.ObjectId && x.Path == propData.Prop))
                            targets.Add(new RpgPropertyRef(propData.ObjectId, propData.Prop));
                    }

            if (dependents.Count == 0)
                return;

            //Props already in the list are never queued twice, so circular references cannot loop forever
            var known = UpdatedProps.Select(x => (x.ObjectId, x.Path)).ToHashSet();
            var queue = new Queue<RpgPropertyRef>(UpdatedProps);
            while (queue.Count > 0)
            {
                var propRef = queue.Dequeue();
                if (!dependents.TryGetValue((propRef.ObjectId, propRef.Path), out var targets))
                    continue;

                foreach (var target in targets)
                    if (known.Add((target.ObjectId, target.Path)))
                    {
                        UpdatedProps.Add(target);
                        queue.Enqueue(target);
                    }
            }
        }
    }

    public static class RpgPropertyChangeTrackerExtensions
    {
        /// <returns>True if the prop was not already in the list</returns>
        internal static bool Merge(this List<RpgPropertyRef> target, RpgPropertyRef propRef)
        {
            if (target.Any(x => x.ObjectId == propRef.ObjectId && x.Path == propRef.Path))
                return false;

            target.Add(propRef);
            return true;
        }

        /// <returns>True if any prop was not already in the list</returns>
        internal static bool Merge(this List<RpgPropertyRef> target, IEnumerable<RpgPropertyRef?> source)
        {
            var added = false;
            foreach (var a in source.Where(x => x != null))
                added |= target.Merge(a!);

            return added;
        }
    }
}
