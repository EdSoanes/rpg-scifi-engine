using Newtonsoft.Json;
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
        public Dice Value { get; private set; }

        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore] public Modifier[] AllModifiers { get => Modifiers.ToArray(); }

        public Modifier[] BaseModifiers(Graph graph)
        {
            var baseMods = Modifiers
                .Where(x => x.Duration.GetExpiry(graph.Turn) == ModifierExpiry.Active && x.ModifierType == ModifierType.Base)
                .ToArray();

            return baseMods;
        }

        public void Evaluate(Graph graph)
        {
            var newVal = graph.Evaluate.Mod(Modifiers.ToArray());
            if (Value != newVal)
            {
                Value = newVal;
                graph.Notify.Send(EntityId, Prop);
            }
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

        public bool Add(Graph graph, params Modifier[] mods)
        {
            var res = false;
            foreach (var mod in mods.Where(x => x.Target.EntityId == EntityId))
            {
                if (mod.Target.EntityId != EntityId)
                    return false;

                var existing = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
                if (existing != null)
                    Modifiers.Remove(existing);

                if (mod.ModifierAction == ModifierAction.Accumulate)
                {
                    Modifiers.Add(mod);
                    res |= true;
                }

                if (mod.ModifierAction == ModifierAction.Sum)
                {
                    res |= Sum(graph, mod);
                }

                if (mod.ModifierAction == ModifierAction.Replace)
                {
                    res |= Replace(mod);
                }
            }

            if (res)
                Evaluate(graph);

            return res;
        }

        private bool Sum(Graph graph, Modifier mod)
        {
            var matchingMods = MatchingModifiers(mod.Name, mod.ModifierType);
            var dice = graph.Evaluate.Mod(matchingMods) + graph.Evaluate.Mod(mod);

            Modifiers = Modifiers.Except(matchingMods).ToList();
            if (dice != Dice.Zero || mod.Duration.EndTurn != RemoveTurn.WhenZero)
            {
                mod.SetDice(dice);
                Modifiers.Add(mod);
            }

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

        public Modifier[] Remove(Graph graph, params Modifier[] mods)
        {
            var toRemove = Modifiers
                .Where(x => mods.Select(m => m.Id).Contains(x.Id))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var mod in toRemove)
                    Modifiers.Remove(mod);

                Evaluate(graph);
            }

            return toRemove;
        }

        public Modifier[] Clear(Graph graph)
        {
            if (Modifiers.Any())
            { 
                var res = Modifiers.ToArray();
                Modifiers.Clear();
                Evaluate(graph);
                return res;
            }

            return new Modifier[0];
        }

        public void Clean(Graph graph)
        {
            var toRemove = Modifiers
                .Where(x => x.Duration.CanRemove(graph.Turn))
                .ToArray();

            if (toRemove.Any())
            {
                foreach (var remove in toRemove)
                    Modifiers.Remove(remove);

                Evaluate(graph);
            }
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

        public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Expression<Func<TEntity, Dice>> expression)
            where TEntity : ModdableObject
        {
            var propertyRef = rootEntity.GetPropertyRef(expression);
            return !string.IsNullOrWhiteSpace(propertyRef.Prop)
                ? propertyRef.Entity?.GetPropValue(propertyRef.Prop)
                : null;
        }

        public static Dice? GetPropValue<TEntity>(this TEntity rootEntity, Modifier mod)
            where TEntity : ModdableObject
        {
            var entity = rootEntity.FindModdableObject(mod.Source!.EntityId);
            return entity != null && !string.IsNullOrWhiteSpace(mod.Source.Prop)
                ? entity.GetPropValue(mod.Source.Prop)
                : null;
        }

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
        [JsonProperty] protected Dictionary<string, PropertyModifiers> PropertyModifiers { get; set; } = new Dictionary<string, PropertyModifiers>();

        private void ModsByEntityProp(Modifier[] mods, Action<ModdableObject, string, Modifier[]> onMatch)
        {
            var modsByEntity = mods.GroupBy(x => x.Target.EntityId);
            foreach (var entityMods in modsByEntity)
            {
                var entity = this.FindModdableObject(entityMods.Key);
                if (entity != null)
                {
                    foreach (var modsByProp in entityMods.GroupBy(x => x.Target.Prop))
                        onMatch(entity, modsByProp.Key, modsByProp.ToArray());   
                } 
            }
        }

        public void AddMods(params Modifier[] mods)
            => ModsByEntityProp(mods, (entity, prop, propMods) =>
            {
                if (!string.IsNullOrWhiteSpace(prop) && !entity.PropertyModifiers.ContainsKey(prop))
                    entity.PropertyModifiers.Add(prop, new PropertyModifiers(entity.Id, prop));

                entity.PropertyModifiers[prop].Add(Graph, propMods);
            });


        public void RemoveMods(params Modifier[] mods)
            => ModsByEntityProp(mods, (entity, prop, propMods) =>
            {
                if (entity.PropertyModifiers.ContainsKey(prop))
                    entity.PropertyModifiers[prop].Remove(Graph, propMods);
            });

        public Modifier[] GetMods(string? prop)
            => !string.IsNullOrWhiteSpace(prop) && PropertyModifiers.ContainsKey(prop) 
                ? PropertyModifiers[prop].AllModifiers 
                : new Modifier[0];

        public Dice CalcPropValue(string prop)
            => EvaluateOp.Mod(this, prop);

        public Dice? GetPropValue(string prop)
            => PropertyModifiers.ContainsKey(prop)
                ? PropertyModifiers[prop].Value
                : null;

        #endregion

        protected Graph Graph { get; set; }

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

        public virtual void OnAdd(Graph graph) => Graph = graph;

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
            => Graph?.Notify.Send(Id, prop);
    }
}
