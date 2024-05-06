using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rpg.Sys.GraphOperations;
using Rpg.Sys.Modifiers;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rpg.Sys
{
    public class PropertyModifiers
    {
        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }

        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore] public Modifier[] AllModifiers { get => Modifiers.ToArray(); }

        public Modifier[] BaseModifiers(Graph graph)
        {
            var baseMods = Modifiers
                .Where(x => x.Duration.GetExpiry(graph.Turn) == ModifierExpiry.Active && x.ModifierType == ModifierType.Base)
                .ToArray();

            return baseMods;
        }

        public Modifier[] FilteredModifiers(Graph graph)
        {
            var activeModifiers = Modifiers.Where(x => x.Duration.GetExpiry(graph.Turn) == ModifierExpiry.Active);

            var res = activeModifiers
                .Where(x => x.ModifierType != ModifierType.Base && x.ModifierType != ModifierType.BaseOverride)
                .ToList();

            var baseMods = activeModifiers
                .Where(x => x.ModifierType == ModifierType.BaseOverride)
                .ToList();

            if (!baseMods.Any())
                baseMods = activeModifiers
                    .Where(x => x.ModifierType == ModifierType.Base)
                    .ToList();

            res.AddRange(baseMods);
            return res.ToArray();
        }

        public Modifier[] MatchingModifiers(string name, ModifierType modifierType)
            => Modifiers
                .Where(x => x.Name == name && x.ModifierType == modifierType)
                .ToArray();

        public PropertyModifiers(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public bool Add(ModdableObject entity, params Modifier[] mods)
        {
            var res = false;
            foreach (var mod in mods.Where(x => x.Target.EntityId == EntityId))
            {
                var existing = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
                if (existing != null)
                {
                    Modifiers.Remove(existing);
                    res |= true;
                }

                if (mod.ModifierAction == ModifierAction.Accumulate)
                {
                    Modifiers.Add(mod);
                    res |= true;
                }

                else if (mod.ModifierAction == ModifierAction.Sum)
                {
                    res |= Sum(entity, mod);
                }

                else if (mod.ModifierAction == ModifierAction.Replace)
                {
                    res |= Replace(mod);
                }
            }

            return res;
        }

        private bool Sum(ModdableObject entity, Modifier mod)
        { 
            var oldValue = entity.EvaluateProp(mod.Target.Prop, mod.Name, mod.ModifierType);
            var oldMods = MatchingModifiers(mod.Name, mod.ModifierType);

            Modifiers = Modifiers.Except(oldMods).ToList();
            Modifiers.Add(mod);

            var newValue = entity.EvaluateProp(mod.Target.Prop, mod.Name, mod.ModifierType) + oldValue;
            if (newValue == null || (newValue == Dice.Zero && mod.Duration.EndTurn == RemoveTurn.WhenZero))
                Modifiers.Remove(mod);
            else
                mod.SetDice(newValue.Value);

            return true;
        }

        private bool Replace(Modifier mod)
        {
            var matchingMods = Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();

            Modifiers = Modifiers.Except(matchingMods).ToList();
            Modifiers.Add(mod);

            return true;
        }

        public Modifier[] UpdateOnTurn(Graph graph, int newTurn)
        {
            var updated = new List<Modifier>();
            foreach (var mod in Modifiers)
            {
                mod.OnUpdate(newTurn);

                var expiry = mod.Duration.GetExpiry(graph.Turn);
                if (expiry == ModifierExpiry.Remove)
                    updated.Add(mod);
                else
                {
                    var oldExpiry = mod.Duration.GetExpiry(graph.Turn - 1);
                    if (expiry != oldExpiry)
                        updated.Add(mod);
                }
            }

            foreach (var mod in updated.Where(x => x.Duration.GetExpiry(graph.Turn) == ModifierExpiry.Remove))
                Modifiers.Remove(mod);

            return updated.ToArray();
        }

        public Modifier[] Remove(params Modifier[] mods)
        {
            var toRemove = Modifiers
                .Where(x => mods.Select(m => m.Id).Contains(x.Id))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var mod in toRemove)
                    Modifiers.Remove(mod);
            }

            return toRemove;
        }

        public Modifier[] Clear(Graph graph)
        {
            if (Modifiers.Any())
            { 
                var res = Modifiers.ToArray();
                Modifiers.Clear();
                return res;
            }

            return new Modifier[0];
        }

        public bool Clean(Graph graph)
        {
            var toRemove = Modifiers
                .Where(x => x.Duration.CanRemove(graph.Turn))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var remove in toRemove)
                    Modifiers.Remove(remove);

                return true;
            }

            return false;
        }

        public bool AffectedBy(Guid id, string prop)
            => Modifiers.Any(x => x.Source != null && x.Source.EntityId == id && x.Source.Prop == prop);

        public bool Contains(Modifier mod)
            => Modifiers.Any(x => x.Id == mod.Id);
    }

    public static class ModdableObjectExtensions
    {
        private class PropertyRef
        {
            public ModdableObject? Entity { get; set; }
            public string? Prop { get; set; }
        }

        //public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Expression<Func<TEntity, Dice>> expression)
        //    where TEntity : ModdableObject
        //{
        //    var propertyRef = rootEntity.GetPropertyRef(expression);
        //    return !string.IsNullOrWhiteSpace(propertyRef.Prop)
        //        ? propertyRef.Entity?.GetPropValue(propertyRef.Prop)
        //        : null;
        //}

        //public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Modifier mod)
        //    where TEntity : ModdableObject
        //{
        //    var entity = rootEntity.FindModdableObject(mod.Source!.EntityId);
        //    return entity != null && !string.IsNullOrWhiteSpace(mod.Source.Prop)
        //        ? entity.GetPropValue(mod.Source.Prop)
        //        : null;
        //}

        public static Modifier[]? GetMods<TEntity, TResult>(this TEntity rootEntity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModdableObject
        {
            var propertyRef = rootEntity.GetPropertyRef(expression);
            return propertyRef.Entity?.GetMods(propertyRef.Prop);
        }

        private static PropertyRef GetPropertyRef<T, TResult>(this T rootEntity, Expression<Func<T, TResult>> expression)
            where T : ModdableObject
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();

            //Get the prop name
            var prop = memberExpression.Member.Name;

            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();
            var path = string.Join(".", pathSegments);
            var entity = rootEntity.PropertyValue<ModdableObject>(path);

            return new PropertyRef
            {
                Entity = entity,
                Prop = prop
            };
        }
    }

    public abstract class ModdableObject : INotifyPropertyChanged, IModSubscriber
    {
        #region Mod Store
        [JsonProperty] protected bool IsInitialized { get; set; }
        [JsonProperty] protected Dictionary<string, PropertyModifiers> PropertyModifiers { get; set; } = new Dictionary<string, PropertyModifiers>();

        private PropertyModifiers? GetPropertyModifiers(string? prop)
        {
            return !string.IsNullOrWhiteSpace(prop) && PropertyModifiers.ContainsKey(prop)
                ? PropertyModifiers[prop]
                : null;
        }

        public void AddMods(params Modifier[] mods)
            => AddMods(true, mods);

        protected ModdableObject Init(params Modifier[] mods)
        {
            AddMods(false, mods);
            return this;
        }

        private void AddMods(bool evaluate, params Modifier[] mods)
            => this.ForEach(mods, (entity, prop, propMods) =>
            {
                var propertyModifiers = entity.GetPropertyModifiers(prop);
                if (propertyModifiers == null)
                {
                    propertyModifiers = new PropertyModifiers(entity.Id, prop);
                    entity.PropertyModifiers.Add(prop, propertyModifiers);
                }

                if (propertyModifiers.Add(entity, propMods) && evaluate)
                    entity.SetProp(prop);
            });


        public void RemoveMods(params Modifier[] mods)
            => this.ForEach(mods, (entity, prop, propMods) =>
            {
                var removed = entity.GetPropertyModifiers(prop)
                    ?.Remove(propMods);

                if (removed != null && removed.Any())
                    entity.SetProp(prop);
            });

        public Modifier[] GetMods(string? prop)
            => GetPropertyModifiers(prop)?.AllModifiers ?? new Modifier[0];

        public void SetProps()
            => this.ForEachReversed((entity, prop) => entity.SetProp(prop));

        public void SetProp(string prop)
        {
            var oldValue = GetModdableProperty(prop);
            var newValue = EvaluateProp(prop) ?? Dice.Zero;

            if (oldValue == null || oldValue != newValue)
                SetModdableProperty(prop, newValue);
        }

        public Dice? EvaluateProp(string prop, string? modifierName = null, ModifierType? modifierType = null)
        {
            var propertyModifiers = GetPropertyModifiers(prop);
            if (propertyModifiers != null)
            {
                var mods = !string.IsNullOrEmpty(modifierName) && modifierType != null
                    ? propertyModifiers.MatchingModifiers(modifierName, modifierType.Value)
                    : propertyModifiers.AllModifiers;

                var newValue = Graph.Current.Evaluate.Mod(mods);
                return newValue;
            }

            return null;
        }

        #endregion

        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string[] Is { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ModdableObject()
        {
            Id = Guid.NewGuid();
            Name = GetType().Name;
            Is = this.GetBaseTypes();
        }

        public bool IsA(string type) => Is.Contains(type);

        public virtual void OnAdd(Graph graph) { }

        public void Initialize()
        {
            Graph.Current.SetContext(this);

            foreach (var entity in this.Traverse(true).Where(x => !x.IsInitialized))
            {
                entity.InitializeBaseValues();
                entity.OnInitialize();
                entity.IsInitialized = true;
            }

            SetProps();
        }

        protected virtual void OnInitialize() { }

        private void InitializeBaseValues()
        {
            foreach (var propInfo in this.ModdableProperties())
            {
                var val = this.PropertyValue(propInfo.Name);
                if (val != null)
                {
                    if (val is Dice && ((Dice)val) != Dice.Zero)
                        Init(BaseValueModifier.Create(this, (Dice)val, propInfo.Name));
                    else if (val is int && ((int)val) != 0)
                        Init(BaseValueModifier.Create(this, (int)val, propInfo.Name));
                }
            }
        }

        public virtual Modifier[] OnSetup()
        {
            var mods = new List<Modifier>();

            foreach (var propInfo in this.ModdableProperties())
            {
                var val = this.PropertyValue(propInfo.Name);
                if (val != null)
                {
                    if (val is Dice && ((Dice)val) != Dice.Zero)
                        mods.Add(BaseValueModifier.Create(this, (Dice)val, propInfo.Name));
                    else if (val is int && ((int)val) != 0)
                        mods.Add(BaseValueModifier.Create(this, (int)val, propInfo.Name));
                }
            }

            return mods.ToArray();
        }

        public void CallPropertyChanged(string prop) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public Dice? GetModdableProperty(string prop)
        {
            var val = this.PropertyValue(prop);
            if (val != null)
            {
                if (val is Dice)
                    return (Dice)val;
                else if (val is int)
                    return (int)val;
            }

            return null;
        }

        public void SetModdableProperty(string prop, Dice dice)
        {
            this.PropertyValue(prop, dice);
            CallPropertyChanged(prop);
        }

        protected void NotifyPropertyChanged(string prop)
            => Graph.Current?.Notify.Send(Id, prop);
    }
}
