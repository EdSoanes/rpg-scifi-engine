using Newtonsoft.Json;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Props
{
    public class Prop : RpgLifecycleObject
    {
        [JsonProperty] public string EntityId { get; protected set; }
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public RefType RefType { get; protected set; } = RefType.Value;
        [JsonProperty] public List<Mod> Mods { get; private set; } = new();
        [JsonProperty] public List<PropObjRef<string>> Refs { get; private set; } = new();

        private List<RpgObject>? _preCreatedObjects = new();

        public Prop(string entityId, string name, RefType refType)
        {
            EntityId = entityId;
            Name = name;
            RefType = refType;
        }

        #region Mods

        public Mod[] GetActive()
        {
            if (Mods.Any(x => x.IsBaseOverrideMod))
                return Mods
                    .Where(x => x.IsActive && (x.IsBaseOverrideMod || x.Behavior is Threshold))
                    .ToArray();

            return Mods
                    .Where(x => x.IsActive)
                    .ToArray();
        }

        public Mod[] Get(Func<Mod, bool> filterFunc)
            => Mods
                .Where(x => filterFunc(x))
                .ToArray();

        public bool Contains(Mod mod)
            => Mods
                .Any(x => x.Id == mod.Id);

        public void Add(Mod mod)
        {
            if (!Contains(mod))
                Mods.Add(mod);
        }

        public Mod? Remove(string id)
        {
            var toRemove = Mods.FirstOrDefault(x => x.Id == id);
            if (toRemove != null)
                Mods.Remove(toRemove);
            return toRemove;
        }

        public Mod? Remove(Mod mod)
            => Remove(mod.Id);

        public Mod[] Clear()
        {
            if (Mods.Any())
            {
                var res = Mods.ToArray();
                Mods.Clear();
                return res;
            }

            return Array.Empty<Mod>();
        }

        public bool IsAffectedBy(PropRef propRef)
            => Mods
                .Any(x => x.SourcePropRef != null && x.SourcePropRef.EntityId == propRef.EntityId && x.SourcePropRef.Prop == propRef.Prop);

        #endregion Mods

        #region Refs

        public bool IsParentObjectTo(RpgObject childObject)
            => IsParentObjectTo(childObject.Id);

        public bool IsParentObjectTo(string childObjectId)
        {
            if (RefType != RefType.Child)
                return false;

            return Refs.FirstOrDefault()?.Get() == childObjectId;
        }

        public bool Contains(string entityId)
            => Graph != null
                ? Refs.Any(x => x.Expiry == LifecycleExpiry.Active && x.Get() == entityId)
                : Refs.Any(x => x.Get() == entityId);
        
        public T? GetChildObject<T>() 
            where T : RpgObject
        {
            if (RefType != RefType.Child)
                throw new InvalidDataException("Can only GetObject for RefType.Object");

            var rpgObjId = Refs.FirstOrDefault()?.Get();
            return Graph.GetObject<T>(rpgObjId);
        }

        public RpgObject[] GetChildObjects()
        {
            if (RefType != RefType.Children)
                throw new InvalidDataException("Can only GetObjects for RefType.Objects");

            if (Graph == null)
                return [];

            var rpgObjs = Refs
                .Select(x => Graph!.GetObject(x.Get()))
                .Where(x => x != null)
                .Cast<RpgObject>()
                .ToArray();

            return rpgObjs;
        }

        public void AddRef(RpgObject? source)
        {
            if (RefType == RefType.Value)
                throw new InvalidDataException("Cannot add Ref to RefType.Value prop");

            if (RefType == RefType.Child && Refs.Count() > 1)
                throw new InvalidDataException("Too many Refs for RefType.Object");

            if (source == null)
            {
                if (RefType == RefType.Child)
                    Refs.FirstOrDefault()?.SetExpired();

                return;
            }

            var existing = RefType == RefType.Child
                ? Refs.FirstOrDefault()
                : Refs.FirstOrDefault(x => x.Get() == source.Id);

            if (existing != null)
                existing.Set(source.Id);
            else
            {
                if (Graph != null)
                {
                    existing = new PropObjRef<string>(Graph, source.Id);
                    existing.OnStartLifecycle();
                }
                else
                {
                    existing = new PropObjRef<string>(source.Id);
                    _preCreatedObjects?.Add(source);
                }

                Refs.Add(existing);
            }
        }

        public void RemoveRef(RpgObject source)
        {
            if (RefType == RefType.Value)
                throw new InvalidDataException("Cannot remove Ref from RefType.Value prop");

            if (RefType == RefType.Child && Refs.Count() > 1)
                throw new InvalidDataException("Too many Refs for RefType.Object");

            var existing = Refs.FirstOrDefault(x => x.Get() == source.Id);
            if (existing != null)
                existing.SetExpired();
        }

        #endregion Refs

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            
            if (_preCreatedObjects != null)
            {
                foreach (var preCreated in _preCreatedObjects!)
                {
                    foreach (var item in preCreated.Traverse())
                        graph.AddObject(item);
                }
                    
                _preCreatedObjects = null;
            }

            foreach (var objRef in Refs)
                objRef.OnCreating(graph, entity);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            foreach (var objRef in Refs)
                objRef.OnTimeBegins();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            var expiry = base.OnStartLifecycle();
            foreach (var mod in Mods)
                mod.OnStartLifecycle();

            foreach (var objRef in Refs)
                objRef.OnStartLifecycle();

            return expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            var expiry = base.OnUpdateLifecycle();

            foreach (var mod in Mods)
                mod.OnUpdateLifecycle();

            foreach (var objRef in Refs)
                objRef.OnUpdateLifecycle();

            return expiry;
        }
        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);
            foreach (var mod in Mods)
                mod.OnRestoring(graph);

            foreach (var objRef in Refs) 
                objRef.OnRestoring(graph);
        }

        public override string ToString()
        {
            return RefType switch
            {
                RefType.Value => $"#Mods = {Mods.Count()}",
                RefType.Child => $"#Ref = {Refs.FirstOrDefault(x => x.Expiry == LifecycleExpiry.Active)?.Get() ?? "null"}",
                _ => RefType.ToString()
            };
        }
    }
}
