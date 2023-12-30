using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModifierStore : IDictionary<string, MetaModdableProperty>
    {
        private readonly Dictionary<Guid, Dictionary<string, MetaModdableProperty>> _store = new Dictionary<Guid, Dictionary<string, MetaModdableProperty>>();
        protected IContext Context { get; set; }

        public MetaModdableProperty this[string key]
        {
            get
            {
                TryGetValue(key, out var res);
                return res;
            }

            set => Add(key, value);
        }

        public ICollection<string> Keys => AllKeys();

        public ICollection<MetaModdableProperty> Values => AllValues();

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.Modifiers.Count()));

        public bool IsReadOnly => false;

        public MetaModdableProperty? Get(PropReference? moddableProperty, bool createMissingEntries = false)
        {
            return moddableProperty?.Id != null
                ? Get(moddableProperty.Id.Value, moddableProperty.Prop!, createMissingEntries)
                : null;
        }

        public MetaModdableProperty? Get(Guid? entityId, string? prop, bool createMissingEntries = false)
        {
            if (entityId == null  || string.IsNullOrEmpty(prop))
                return null;

            if (createMissingEntries)
            {
                if (!_store.ContainsKey(entityId.Value))
                    _store.Add(entityId.Value, new Dictionary<string, MetaModdableProperty>());

                var entityMods = _store[entityId.Value];
                if (!entityMods.ContainsKey(prop))
                    entityMods.Add(prop, new MetaModdableProperty(entityId.Value, prop));

                return entityMods[prop];
            }
            else
            {
                return TryGetValue(MakeKey(entityId.Value, prop), out var res)
                    ? res
                    : null;
            }
        }

        public void Add(Modifier[] mods)
        {
            foreach (var mod in mods)
                Add(mod);
        }

        public void Add(string key, MetaModdableProperty value)
        {
            foreach (var mod in value.Modifiers)
                Add(mod);
        }

        public void Add(KeyValuePair<string, MetaModdableProperty> item) => Add(item.Key, item.Value);

        public void Add(Modifier mod)
        {
            var modProp = Get(mod.Target, true)!;
            if (mod.ModifierAction == ModifierAction.Accumulate || (mod.Target.PropReturnType == PropReturnType.Dice && !Evaluate(mod.Target).IsConstant))
            {
                modProp.Modifiers.Add(mod);
            }

            else
            {
                var existingMods = modProp.Modifiers
                    .Where(x => x.Id == mod.Id || (x.Name == mod.Name && x.ModifierType == mod.ModifierType))
                    .ToList();

                if (mod.ModifierAction == ModifierAction.Sum)
                {
                    Dice dice = Dice.Sum(existingMods.Select(x => x.DiceCalc.Apply(SourceDice(x), Context.Get(x.Source)))) + mod.DiceCalc.Apply(SourceDice(mod), Context.Get(mod.Source));
                    mod.SetDice(dice);
                }

                foreach (var existingMod in existingMods)
                    modProp.Modifiers.Remove(existingMod);

                if (mod.ModifierType != ModifierType.Transient)
                    modProp.Modifiers.Add(mod);
                else
                {
                    var dice = Evaluate(mod);
                    if (!dice.IsConstant || dice.Roll() != 0)
                        modProp.Modifiers.Add(mod);
                }
            }

            modProp.DiceIsSet = false;
            modProp.IsDiceProperty = mod.Target.PropType == PropType.Dice;
        }

        private Dice ApplyDiceCalc(Dice dice, string? diceCalc, Entity? entity = null)
        {
            if (string.IsNullOrEmpty(diceCalc))
                return dice;

            var parts = diceCalc.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                if (Guid.TryParse(parts[0], out var id))
                    entity = Context.Get(id);
                else
                    entity = null;
            }

            var funcName = entity != null
                ? parts.Last()
                : diceCalc;

            Dice val = entity.ExecuteFunction<Dice, Dice>(funcName, dice);
            return val;
        }

        public Dice Evaluate(PropReference? moddableProperty) 
            => Evaluate(moddableProperty?.Id, moddableProperty?.Prop);

        public Dice Evaluate(Guid? id, string? prop)
        {
            var modProp = Get(id, prop);
            if (modProp == null)
                return "0";
            
            if (modProp.DiceIsSet)
                return modProp.Dice;

            modProp.Dice = Dice.Sum(modProp.Modifiers.Select(Evaluate));
            return modProp.Dice;
        }

        public Dice Evaluate(Modifier modifier)
        {
            if (modifier.Source.PropType == PropType.Dice)
                return modifier.Source.Prop;

            var sourceEntity = Context.Get(modifier.Source);
            var dice = SourceDice(modifier, sourceEntity);

            dice = ApplyDiceCalc(dice, modifier.DiceCalc, sourceEntity ?? targetEntity);

            return dice;
        }

        public int Resolve(PropReference? moddableProperty) 
            => Evaluate(moddableProperty?.Id, moddableProperty?.Prop).Roll();

        public int Resolve(Guid? id, string? prop)
            => Evaluate(id, prop).Roll();

        public string[] Describe(PropReference? moddableProperty, bool includeEntityInformation = false)
        {
            if (moddableProperty?.Id == null)
                return new string[0];

            return Describe(Context.Get(moddableProperty.Id.Value)!, moddableProperty.Prop!, includeEntityInformation);
        }

        public string[] Describe(Entity entity, string prop, bool includeEntityInformation = false)
        {
            var res = new List<string> { $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" };

            var modProp = Get(entity.Id, prop);
            var descriptions = modProp?.Modifiers
                .SelectMany(x => Describe(x, includeEntityInformation))
                .ToArray();

            res.AddRange(descriptions ?? new string[0]);

            return res.ToArray();
        }

        public string[] Describe(Modifier modifier, bool includeEntityInformation = false)
        {
            var desc = DescribeSource(modifier, includeEntityInformation);
            var res = new List<string>() { desc };

            var modProp = Get(modifier.Source);
            if (modProp != null)
            {
                foreach (var mod in modProp.Modifiers)
                    res.AddRange(Describe(mod).Select(x => $"  {x}"));
            }

            return res.ToArray();
        }

        private string DescribeSource(Modifier mod, bool includeEntityInformation = false)
        {
            if (mod.Source.PropType == PropType.Dice)
                return $"{mod.Name} => {mod.Source.Prop}";

            var rootEntity = Context.Get(mod.Source.RootId!.Value);
            var sourceEntity = Context.Get(mod.Source);

            var desc = "";
            if (includeEntityInformation)
            {
                desc += rootEntity?.Id != sourceEntity?.Id
                    ? $"{rootEntity?.Name}.{sourceEntity?.Name}"
                    : sourceEntity?.Name;

                desc = desc.Trim('.');
                if (!string.IsNullOrEmpty(desc))
                    desc += ".";
            }

            desc += $"{mod.Source.Prop ?? mod.Name} => {SourceDice(mod, sourceEntity)}";

            if (mod.DiceCalc != null)
                desc += $" => {DescribeDiceCalc(mod.DiceCalc)}() => {Evaluate(mod)}";

            return desc;
        }

        private string DescribeTarget(Modifier modifier)
        {
            var rootEntity = Context.Get(modifier.Target.RootId!.Value);
            var targetEntity = Context.Get(modifier.Target);
            
            return $"{rootEntity?.Name}.{targetEntity?.Name}.{modifier.Target.Prop}".Trim('.');
        }

        private string DescribeDiceCalc(string? diceCalc)
        {
            if (string.IsNullOrEmpty(diceCalc))
                return diceCalc!;

            var parts = diceCalc.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return diceCalc;

            if (parts.Length == 2 && Guid.TryParse(parts[0], out var id))
                return $"{Context.Get(id)?.Name}.{parts[1]}".Trim('.');

            return diceCalc;
        }

        public Dice SourceDice(Modifier mod, Entity? sourceEntity = null)
        {
            if (mod.Source.PropType == PropType.Dice)
                return mod.Source.Prop;

            sourceEntity = sourceEntity != null && sourceEntity.Id == mod.Source.Id
                ? sourceEntity
                : Context.Get(mod.Source.Id!.Value);

            return sourceEntity?.PropertyValue<string>(mod.Source!.Prop!) ?? "0";
        }

        public void Clear() => _store.Clear();

        public void Clear(Guid entityId)
        {
            if (!_store.ContainsKey(entityId))
                return;

            var entityMods = _store[entityId];
            foreach (var prop in entityMods.Keys)
            {
                var modProp = Get(entityId, prop);
                if (modProp != null)
                {
                    if (modProp.Modifiers.Any())
                    {
                        var toRemove = modProp.Modifiers.Where(x => x.CanBeCleared()).ToArray();
                        foreach (var remove in toRemove)
                            modProp.Modifiers.Remove(remove);

                        if (toRemove.Any())
                            modProp.DiceIsSet = false;
                    }
                }
            }
        }

        public bool Contains(Modifier mod) => Get(mod.Target.Id, mod.Target.Prop!)?.Modifiers.Any(x => x.Id == mod.Target.Id) ?? false;
        public bool Contains(KeyValuePair<string, MetaModdableProperty> item) => ContainsKey(item.Key);

        public bool ContainsKey(string key)
        {
            if (!TryParseKey(key, out var res))
                return false;

            return _store.ContainsKey(res.Item1) && _store[res.Item1].ContainsKey(res.Item2);
        }

        public void CopyTo(KeyValuePair<string, MetaModdableProperty>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, MetaModdableProperty>> GetEnumerator()
        {
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    yield return new KeyValuePair<string, MetaModdableProperty>(MakeKey(id, prop), _store[id][prop]);
        }

        public bool Remove(Modifier mod)
        {
            var res = false;

            if (_store.ContainsKey(mod.Target.Id!.Value))
            {
                var entityMods = _store[mod.Target.Id!.Value];
                if (!string.IsNullOrEmpty(mod.Target.Prop) && entityMods.ContainsKey(mod.Target.Prop))
                {
                    var modProp = entityMods[mod.Target.Prop];
                    var toRemove = modProp.Modifiers.FirstOrDefault(x => x.Id == mod.Id && mod.CanBeCleared());

                    if (toRemove != null)
                    {
                        modProp.Modifiers.Remove(toRemove);
                        res = true;
                    }

                    if (!modProp.Modifiers.Any())
                        entityMods.Remove(mod.Target.Prop);

                    if (!entityMods.Any())
                        _store.Remove(mod.Target.Id!.Value);
                }
            }

            return res;
        }

        public bool Remove(Guid entityId)
        {
            if (!_store.ContainsKey(entityId))
                return false;

            return _store.Remove(entityId);
        }

        public bool Remove(PropReference? moddableProperty)
            => Remove(moddableProperty?.Id, moddableProperty?.Prop);

        public bool Remove(Guid? entityId, string? prop)
        {
            var modProp = Get(entityId, prop);
            if (modProp != null)
            {
                var removed = modProp.Modifiers.Count > 0;
                modProp.Modifiers.Clear();
                modProp.DiceIsSet = false;

                return removed;
            }

            return false;
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
        }

        public bool Remove(int currentTurn)
        {
            var res = false;
            foreach (var modProp in  this.Select(x => x.Value))
            {
                var toRemove = new List<Modifier>();

                foreach (var mod in modProp.Modifiers)
                {
                    if (mod.ShouldBeRemoved(currentTurn))
                        toRemove.Add(mod);
                }

                foreach (var mod in toRemove)
                    modProp.Modifiers.Remove(mod);

                if (toRemove.Any())
                {
                    modProp.DiceIsSet = false;
                    res = true;
                }
            }

            return res;
        }

        public bool Remove(KeyValuePair<string, MetaModdableProperty> item) => Remove(item.Key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out MetaModdableProperty value)
        {
            if (TryParseKey(key, out (Guid, string) res))
            {
                Dictionary<string, MetaModdableProperty> entityMods;
                if (_store.ContainsKey(res.Item1))
                {
                    entityMods = _store[res.Item1];
                }
                else
                {
                    entityMods = new Dictionary<string, MetaModdableProperty>();
                    _store.Add(res.Item1, entityMods);
                }

                if (entityMods.ContainsKey(res.Item2))
                {
                    value = entityMods[res.Item2];
                    return true;
                }
                else
                {
                    value = new MetaModdableProperty(res.Item1, res.Item2);
                    entityMods.Add(res.Item2, value);
                    
                    return true;
                }

            }
    
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public (Guid, string) EmptyKey => (Guid.Empty, string.Empty);

        private bool TryParseKey(string key, out (Guid, string) res)
        {
            if (string.IsNullOrEmpty(key))
            {
                res = EmptyKey;
                return false;
            }

            var parts = key.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var id))
            {
                res = EmptyKey;
                return false;
            }

            res = (id, parts[1]);
            return true;
        }

        private string MakeKey(Guid id, string prop) => $"{id}.{prop}";

        private string[] AllKeys()
        {
            var res = new List<string>();
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    res.Add(MakeKey(id, prop));
            return res.ToArray();
        }

        private ICollection<MetaModdableProperty> AllValues()
        {
            var res = new List<MetaModdableProperty>();
            foreach (var id in _store.Keys)
            {
                foreach (var prop in _store[id].Keys)
                    res.Add(_store[id][prop]);
            }
            return res.ToArray();
        }
    }
}
