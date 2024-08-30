using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Refs
{
    public class RefStore : RpgLifecycleObject
    {
        [JsonProperty] private Dictionary<string, Dictionary<string, List<RpgObjectRef>>> Refs { get; set; } = new();

        public void Add(RpgObject target, string prop, RpgObject source, RelationshipType relType)
        {
            InternalAdd(target, prop, source, relType);
            InternalAdd(source, prop, target, Reverse(relType));
        }

        private RelationshipType Reverse(RelationshipType relType)
        {
            return relType switch
            {
                RelationshipType.Parent => RelationshipType.Child,
                RelationshipType.Child => RelationshipType.Parent,
                RelationshipType.Descendant => RelationshipType.Ancestor,
                RelationshipType.Ancestor => RelationshipType.Descendant,
                RelationshipType.Contains => RelationshipType.Contained,
                RelationshipType.Contained => RelationshipType.Contains,
                _ => throw new InvalidOperationException($"Cannot reverse {relType}")
            };
        }

        private void InternalAdd(RpgObject target, string prop, RpgObject source, RelationshipType relType)
        {
            var propRefs = Get(target.Id, prop, relType, create: true)!;

            RemoveSource(source.Id, relType);

            var sourceRef = propRefs.FirstOrDefault(x => x.Get()?.SourceId == source.Id);
            if (sourceRef == null)
            {
                sourceRef = new RpgObjectRef(target.Id, relType);
                propRefs.Add(sourceRef);
            }

            sourceRef.SetSource(source);
        }

        private List<RpgObjectRef>? Get(string targetId, string prop, RelationshipType relType, bool create = false)
        {
            Dictionary<string, List<RpgObjectRef>>? propRefs = null;
            if (Refs.ContainsKey(targetId))
                propRefs = Refs[targetId];
            else if (create)
            {
                propRefs = new Dictionary<string, List<RpgObjectRef>>();
                Refs.Add(targetId, propRefs);
            }

            if (propRefs == null)
                return [];

            List<RpgObjectRef>? objRefs = null;
            if (propRefs.ContainsKey(prop))
                objRefs = propRefs[prop];
            else if (create)
            {
                objRefs = new List<RpgObjectRef>();
                propRefs.Add(prop, objRefs);
            }

            return objRefs?
                .Where(x => x.RelType == relType)?
                .ToList();
        }

        private void RemoveSource(string sourceId, RelationshipType? relType = null)
        {
            foreach (var r in Refs.Values)
                foreach (var propRefs in r.Values)
                {
                    var toRemove = new List<RpgObjectRef>();
                    foreach (var propRef in propRefs)
                    {
                        if ((relType == null || propRef.RelType == relType) && propRef.Get()?.SourceId == sourceId)
                            toRemove.Add(propRef);
                    }

                    foreach (var remove in toRemove)
                        remove.Set(null);
                }
        }

        //private void AddRef(RpgObject target, RpgObject source, RelationshipType relationshipType)
        //{
        //    var sourceRefs = ObjectRefs.ContainsKey(source.Id)
        //        ? ObjectRefs[source.Id].Where(x =>
        //        {
        //            var r = x.Get();
        //            return r?.RefType == relationshipType &&

        //}
    }
}
