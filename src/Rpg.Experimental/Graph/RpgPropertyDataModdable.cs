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

        public object? GetValue()
            => Mods;

        public bool ExpireRefsTo(string objectId, TimePoint now)
        {
            var toExpire = Mods.Where(x => x.Source.PropertyRef?.ObjectId == objectId && x.Expiry == LifecycleExpiry.Active);
            foreach (var mod in toExpire)
            {
                mod.Expire(now);
                mod.OnTimeEvent(now);
            }

            return toExpire.Any();
        }

        public void OnCreating(RpgGraph graph, RpgObject obj)
        {
            Dice? dice = PropType switch
            {
                RpgPropertyType.Int => new Dice(obj.Value<int>(Prop)),
                RpgPropertyType.Dice => obj.Value<Dice>(Prop),
                _ => null,
            };

            if (dice != null && dice != Dice.Zero)
            {
                var initial = new Mod(ModType.Initial).Set(obj, Prop, dice.Value);
                Mods.Add(initial);
                graph.ChangeTracker.OnPropUpdated(ObjectId, Prop);
            }
        }

        public void OnTimeEvent(TimePoint now)
        {
            //Update 
            foreach (var mod in Mods)
                mod.OnTimeEvent(now);

            Mods = Mods
                .Where(x => x.Expiry != LifecycleExpiry.Destroyed)
                .ToList();
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
