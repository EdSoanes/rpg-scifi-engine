using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgPropertyDataModdable : IRpgPropertyData
    {
        [JsonProperty] public string ObjectId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public RpgPropertyType PropType { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }

        [JsonProperty] public List<Mod> Mods { get; private set; } = new();

        [JsonConstructor] private RpgPropertyDataModdable() { }

        public RpgPropertyDataModdable(string objectId, string prop, RpgPropertyType propType, bool isNullable)
        {
            ObjectId = objectId;
            Prop = prop;
            PropType = propType;
            IsNullable = isNullable;
        }

        public T? GetValue<T>(RpgGraph graph)
        {
            var dice = ModCalculator.Value(graph, Mods);
            if (typeof(T) == typeof(int))
                return (T)(object)(dice?.Roll() ?? 0);
            
            if (typeof(T) == typeof(int?))
                return (T?)(object?)dice?.Roll();

            if (typeof(T) == typeof(Dice) || typeof(T) == typeof(Dice?))
                return (T?)(object?)dice;

            if (typeof(T) == typeof(object))
                return (T?)(object?)dice;

            return default;
        }

        public void Expire(TimePoint expiryTime) { }

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);

        public void Expire(RpgGraph graph, TimePoint expiryTime) { }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            var toExpire = Mods.Where(x => x.Source?.PropRef?.ObjectId == objectId && x.Expiry == LifecycleExpiry.Active);
            foreach (var mod in toExpire)
            {
                mod.Expire(graph, expiryTime);
                mod.OnTimeEvent(graph);
            }

            if (toExpire.Any())
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
        }

        public void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            Dice? dice = PropType switch
            {
                RpgPropertyType.Int => new Dice(graph.GetPropertyValue<int>(obj, Prop)),
                RpgPropertyType.Dice => graph.GetPropertyValue<Dice>(obj, Prop),
                _ => null,
            };

            if (obj != null && dice != null && dice != Dice.Zero)
            {
                var initial = new Initial(graph.PropertyRefs.Create(obj.Id, Prop)!, dice.Value);

                Mods.Add(initial);
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
            }
        }

        public void OnRestoring(RpgGraph graph) { }

        public void OnCreatingVirtual(RpgGraph graph, object? value)
        {
            var obj = graph.GetObject(ObjectId);
            if (obj == null) return;

            var mod = new Base(new RpgPropertyRef(ObjectId, Prop));
            var propRef = ResolvePropertyNameToPropRef(graph, obj);
            if (propRef != null)
                mod.SetSource(propRef);
            else if (value is int val)
                mod.SetSource(val);
            else if (value is Dice dice)
                mod.SetSource(dice);

            if (mod.Source != null)
                Mods.Add(mod);
        }

        public void OnTimeEvent(RpgGraph graph)
        {
            var updated = false;
            foreach (var mod in Mods)
            {
                var oldExpiry = mod.Expiry;
                mod.OnTimeEvent(graph);
                updated |= oldExpiry != mod.Expiry;
            }
            
            if (!graph.Time.Now.IsEncounterTime)
            {
                CombineMods(graph);
                ReplaceMods(graph);
            }

            Mods = Mods
                .Where(x => x.Expiry != LifecycleExpiry.Destroyed)
                .ToList();

            if (updated)
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
        }

        public void OnSyncProperty(RpgGraph graph)
        {
            var obj = graph.GetObject(ObjectId)!;
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.Id != ObjectId)
                throw new ArgumentException($"Invalid object id {obj.Id}", "obj");

            if (PropType == RpgPropertyType.Int)
            {
                var newVal = GetValue<int?>(graph);
                var oldVal = graph.GetPropertyValue<int?>(obj, Prop);
                if (newVal != oldVal)
                    graph.SetPropertyValue(obj, Prop, IsNullable ? newVal : newVal ?? 0);
            }
            else if (PropType == RpgPropertyType.Dice)
            {
                var newVal = GetValue<Dice?>(graph);
                var oldVal = graph.GetPropertyValue<Dice?>(obj, Prop);
                if (newVal != oldVal)
                    graph.SetPropertyValue(obj, Prop, IsNullable ? newVal : newVal ?? Dice.Zero);
            }
        }

        private RpgPropertyRef? ResolvePropertyNameToPropRef(RpgGraph graph, RpgObject obj)
        {
            var propParts = Prop.Split('_');
            var propName = propParts[0];
            var argObj = obj.ResolvePropertyNameToObject(graph, propName);

            if (argObj is RpgObject rpgObj && propParts.Length > 1)
            {
                var path = string.Join('.', propParts.Skip(1));
                return graph.PropertyRefs.Create(argObj.Id, path);
            }

            return null;
        }

        private void CombineMods(RpgGraph graph)
        {
            var combineMods = ModFilters.Active(Mods)
                .Where(x => x is Combine && x.Type == ModType.Standard)
                .ToList();

            var val = ModCalculator.Value(graph, combineMods);
            foreach (var mod in combineMods)
                mod.Expire(graph);

            if (val != null && val != Dice.Zero)
                graph.Add(new Combine()
                    .SetTarget(ObjectId, Prop)
                    .SetSource(val!.Value));
        }

        private void ReplaceMods(RpgGraph graph)
        {
            var mods = ModFilters.FilterReplacements(Mods);
            foreach (var mod in mods.Where(x => x is Replace))
                mod.SetVersion(0);

            Mods = mods.ToList();
        }

        public override string ToString()
        {
            return $"{PropType}{(IsNullable ? "?" : "")} {Prop}";
        }
    }
}
