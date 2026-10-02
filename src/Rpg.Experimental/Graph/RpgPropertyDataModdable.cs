using Newtonsoft.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgPropertyDataModdable : IRpgPropertyData
    {
        private MetaProperty? _metaProperty;

        [JsonProperty] public string ObjectId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public RpgPropertyType PropType { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public bool IsVirtual { get; private set; }
        [JsonProperty] public List<Mod> Mods { get; private set; } = new();

        /// <summary>
        /// The property is wanted as a number (a whole number property, a whole number input of an action
        /// step, or a roll requested by rules code). If its value has dice in it, a roll is needed.
        /// </summary>
        [JsonProperty] public bool NeedsNumber { get; internal set; }

        /// <summary>
        /// The stored result of rolling the dice in the property's value
        /// </summary>
        [JsonProperty] public RpgRoll? Roll { get; private set; }

        [JsonConstructor] private RpgPropertyDataModdable() { }

        public RpgPropertyDataModdable(string objectId, MetaProperty metaProperty)
        {
            ObjectId = objectId;
            Prop = metaProperty.Prop;
            PropType = metaProperty.PropertyType;
            IsNullable = metaProperty.IsNullable;
            NeedsNumber = PropType == RpgPropertyType.Int;
        }

        public RpgPropertyDataModdable(string objectId, string prop, RpgPropertyType propType, bool isNullable, bool isVirtual)
        {
            ObjectId = objectId;
            Prop = prop;
            PropType = propType;
            IsNullable = isNullable;
            IsVirtual = isVirtual;
            NeedsNumber = PropType == RpgPropertyType.Int;
        }

        public RpgProperty GetProperty(RpgGraph graph)
        {
            var rpgProperty = _metaProperty?.CloneAsProperty() ?? new RpgProperty
            {
                Prop = Prop,
                Editor = PropType == RpgPropertyType.Int ? EditorType.Int32 : EditorType.Dice,
                DisplayName = Prop,
            };

            rpgProperty.ObjectId = ObjectId;
            rpgProperty.IsNullable = IsNullable;
            rpgProperty.Value = GetValue<Dice>(graph);
            rpgProperty.Expression = GetExpression(graph);
            rpgProperty.Roll = Roll;
            rpgProperty.IsRollPending = IsRollPending(graph);
            rpgProperty.BaseValue = ModCalculator.BaseValue(graph, Mods) ?? Dice.Zero;
            rpgProperty.OriginalBaseValue = ModCalculator.OriginalBaseValue(graph, Mods) ?? Dice.Zero;

            return rpgProperty;
        }

        /// <summary>
        /// What the mods of the property add up to. A stored roll does not affect it.
        /// </summary>
        public Dice? GetExpression(RpgGraph graph)
            => ModCalculator.Value(graph, Mods);

        /// <summary>
        /// True if the property is wanted as a number, its value has dice in it and no stored roll applies
        /// </summary>
        public bool IsRollPending(RpgGraph graph)
        {
            if (!NeedsNumber)
                return false;

            var expression = GetExpression(graph);
            return expression != null
                && !expression.Value.IsConstant
                && !(Roll?.AppliesTo(expression) ?? false);
        }

        /// <summary>
        /// The expression with its dice replaced by the stored roll, if there is one for those dice
        /// </summary>
        private Dice? Resolve(Dice? expression)
            => Roll != null && Roll.AppliesTo(expression)
                ? new Dice(Roll.Result + expression!.Value.Bonus)
                : expression;

        /// <summary>
        /// Store the result of rolling the dice of the property's value. Returns null if the value has no
        /// dice in it.
        /// </summary>
        internal RpgRoll? SetRoll(RpgGraph graph, RpgRollSource suppliedBy, int? result = null)
        {
            var expression = GetExpression(graph);
            if (expression == null || expression.Value.IsConstant)
                return null;

            var dicePart = expression.Value.DicePart;
            int[]? dice = null;
            if (result == null)
            {
                dice = dicePart.RollDice(graph.DiceRoller);
                result = dice.Sum();
            }

            Roll = new RpgRoll(dicePart, result.Value, dice, suppliedBy);
            graph.ChangeTracker.PropUpdated(ObjectId, Prop);

            return Roll;
        }

        internal bool ClearRoll(RpgGraph graph)
        {
            if (Roll == null)
                return false;

            Roll = null;
            graph.ChangeTracker.PropUpdated(ObjectId, Prop);

            return true;
        }

        /// <summary>
        /// The value of the property. Reading it never rolls dice: a number is only available if the value
        /// has no dice in it or a roll has been stored for them.
        /// </summary>
        public T? GetValue<T>(RpgGraph graph)
        {
            var dice = Resolve(GetExpression(graph));
            if (typeof(T) == typeof(int))
            {
                if (dice != null && !dice.Value.IsConstant)
                    throw new RpgUnrolledDiceException($"{ObjectId}.{Prop} is '{dice}'. Its dice have not been rolled");

                return (T)(object)(dice?.Number ?? 0);
            }

            //No number yet
            if (typeof(T) == typeof(int?))
                return dice != null && dice.Value.IsConstant
                    ? (T?)(object?)dice.Value.Number
                    : default;

            if (typeof(T) == typeof(Dice))
                return (T)(object)(dice ?? Dice.Zero);

            if (typeof(T) == typeof(Dice?))
                return (T?)(object?)dice;

            if (typeof(T) == typeof(object))
                return (T?)(object?)dice;

            return default;
        }

        public void Expire(TimePoint expiryTime) 
        { }

        public void ResetToInitial(TimePoint expiryTime)
        {
            foreach (var mod in Mods.Where(x => !(x is Initial)))
                mod.Expire(expiryTime);
        }

        public void ResetToBase(RpgGraph graph)
        {
            var toExpire = Mods
                .Where(x => !(x is Initial) && !(x is Base) && !ModFilters.IsExpired(x))
                .ToArray();

            //Expire through the graph so each mod's Expiry is recalculated immediately and the value is correct
            //without having to wait for the next time event
            foreach (var mod in toExpire)
                mod.Expire(graph, graph.Time.Now);

            if (toExpire.Any())
                graph.ChangeTracker.PropUpdated(ObjectId, Prop);
        }

        public void Expire(RpgGraph graph)
            => Expire(graph, graph.Time.Now);

        public void Expire(RpgGraph graph, TimePoint expiryTime) { }

        public void ExpireRefsTo(RpgGraph graph, TimePoint expiryTime, string objectId)
        {
            //An object's own props keep the mods that are derived from its other props
            if (ObjectId == objectId)
                return;

            var toExpire = Mods
                .Where(x => x.Source?.PropRef?.ObjectId == objectId && x.Expiry == LifecycleExpiry.Active)
                .ToArray();
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
            _metaProperty = graph.GetMetaProperty(ObjectId, Prop);

            //Virtual properties have no class property to take an initial value from. See OnCreatingVirtual()
            if (IsVirtual)
                return;

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

        public void OnRestoring(RpgGraph graph) 
        {
            _metaProperty = graph.GetMetaProperty(ObjectId, Prop);
        }

        public void OnCreatingVirtual(RpgGraph graph, object? value)
        {
            var obj = graph.GetObject(ObjectId);
            if (obj == null) return;

            var propRef = ResolvePropertyNameToPropRef(graph, obj);
            if (propRef != null)
            {
                var mod = new Base(new RpgPropertyRef(obj.Id, Prop), propRef);
                Mods.Add(mod);
            }
            else if (value is int val)
            {
                var mod = new Initial(new RpgPropertyRef(ObjectId, Prop), val);
                Mods.Add(mod);
            }
            else if (value is Dice dice)
            {
                var mod = new Initial(new RpgPropertyRef(ObjectId, Prop), dice);
                Mods.Add(mod);
            }
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

        public void OnSyncProperty(RpgGraph graph, string prop)
        {
            var obj = graph.GetObject(ObjectId)!;
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.Id != ObjectId)
                throw new ArgumentException($"Invalid object id {obj.Id}", "obj");

            if (PropType == RpgPropertyType.Int)
            {
                //A value with unrolled dice has no number yet. The property keeps what it had until the roll is settled.
                if (IsRollPending(graph))
                    return;

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
