using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Mods.Behaviors;
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

        public object? GetValue()
            => Mods;

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);
        public void Expire(RpgGraph graph, TimePoint expiryTime) { }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            var toExpire = Mods.Where(x => x.Source.PropertyRef?.ObjectId == objectId && x.Expiry == LifecycleExpiry.Active);
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
                RpgPropertyType.Int => new Dice(obj.Value<int>(Prop)),
                RpgPropertyType.Dice => obj.Value<Dice>(Prop),
                _ => null,
            };

            if (dice != null && dice != Dice.Zero)
            {
                var initial = new Mod(ModType.Initial)
                    .SetTarget(obj, Prop)
                    .SetSource(dice.Value)
                    .Behavior(new Replace());

                Mods.Add(initial);
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
            }
        }

        public void OnTimeEvent(RpgGraph graph)
        {
            var updated = false;
            foreach (var mod in Mods)
            {
                var oldExpiry = mod.Expiry;

                if (mod.Source.PropertyRef != null && !graph.ChangeTracker.IsObjectUpdated(mod.Source.PropertyRef.ObjectId))
                    graph.RefreshObject(mod.Source.PropertyRef.ObjectId);

                mod.ModBehavior.OnBeforeTimeEvent(mod, graph, this);
                mod.OnTimeEvent(graph);
                mod.ModBehavior.OnAfterTimeEvent(mod, graph, this);

                updated |= oldExpiry != mod.Expiry;
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

            var value = ModCalculator.Value(graph, Mods);
            if (PropType == RpgPropertyType.Int)
            {
                var newVal = value?.Roll();
                var oldVal = obj.Value<int?>(Prop);

                if (newVal != oldVal)
                    obj.SetPropertyValue(Prop, IsNullable ? newVal : newVal ?? 0);
            }
            else if (PropType == RpgPropertyType.Dice)
            {
                var newVal = value;
                var oldVal = obj.Value<Dice?>(Prop);

                if (newVal != oldVal)
                    obj.SetPropertyValue(Prop, IsNullable ? newVal : newVal ?? Dice.Zero);
            }
        }

        public override string ToString()
        {
            return $"{PropType}{(IsNullable ? "?" : "")} {Prop}";
        }
    }
}
