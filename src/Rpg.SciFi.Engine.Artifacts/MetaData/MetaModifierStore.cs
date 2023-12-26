using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModifierStore : IDictionary<string, List<Modifier>>
    {
        private readonly Dictionary<Guid, Dictionary<string, List<Modifier>>> _store = new Dictionary<Guid, Dictionary<string, List<Modifier>>>();
        protected IContext Context { get; set; }

        public List<Modifier> this[string key]
        {
            get
            {
                TryGetValue(key, out var res);
                return res ?? new List<Modifier>();
            }

            set => Add(key, value);
        }

        public ICollection<string> Keys => AllKeys();

        public ICollection<List<Modifier>> Values => AllValues();

        public int Count => _store.Sum(x => x.Value.Sum(y => y.Value.Count()));

        public bool IsReadOnly => false;

        public List<Modifier>? Get(ModdableProperty? moddableProperty, bool createMissingEntries = false)
        {
            return moddableProperty != null
                ? Get(moddableProperty.Id, moddableProperty.Prop!, createMissingEntries)
                : null;
        }

        public List<Modifier>? Get(Guid entityId, string prop, bool createMissingEntries = false)
        {
            if (createMissingEntries)
            {
                if (!_store.ContainsKey(entityId))
                    _store.Add(entityId, new Dictionary<string, List<Modifier>>());

                var entityMods = _store[entityId];
                if (!entityMods.ContainsKey(prop))
                    entityMods.Add(prop, new List<Modifier>());

                return entityMods[prop];
            }
            else
            {
                return TryGetValue(MakeKey(entityId, prop), out var res)
                    ? res
                    : null;
            }
        }

        public void Add(Modifier[] mods)
        {
            foreach (var mod in mods)
                Add(mod);
        }

        public void Add(string key, List<Modifier> value)
        {
            foreach (var mod in value)
                Add(mod);
        }

        public void Add(KeyValuePair<string, List<Modifier>> item) => Add(item.Key, item.Value);

        public void Add(Modifier mod)
        {
            var mods = Get(mod.Target, true)!;
            if (mod.ModifierAction == ModifierAction.Accumulate || (mod.Target.IsDiceProperty && !Evaluate(mod.Target).IsConstant))
            {
                mods.Add(mod);
            }

            else
            {
                var existingMods = mods
                    .Where(x => x.Id == mod.Id || (x.Name == mod.Name && x.ModifierType == mod.ModifierType))
                    .ToList();

                if (mod.ModifierAction == ModifierAction.Sum)
                {
                    Dice dice = Dice.Sum(existingMods.Select(x => ApplyDiceCalc(SourceDice(x), x.DiceCalc))) + ApplyDiceCalc(SourceDice(mod), mod.DiceCalc);
                    mod.SetDice(dice);
                }

                foreach (var existingMod in existingMods)
                    mods.Remove(existingMod);

                if (mod.ModifierType != ModifierType.Transient || Evaluate(mod) != Dice.Zero)
                    mods.Add(mod);
            }
        }

        public Dice Evaluate(Modifier modifier)
        {
            var sourceEntity = Context.Get(modifier.Source);
            var targetEntity = Context.Get(modifier.Target);

            var dice = SourceDice(modifier, sourceEntity);
            dice = ApplyDiceCalc(dice, modifier.DiceCalc, sourceEntity ?? targetEntity);

            return dice;
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

        public Dice Evaluate(ModdableProperty? moddableProperty)
        {
            if (moddableProperty == null)
                return "0";

            return Evaluate(moddableProperty.Id, moddableProperty.Prop!);
        }

        public Dice Evaluate(Guid id, string prop)
        {
            var mods = Get(id, prop);

            if (mods != null)
            {
                Dice dice = Dice.Sum(mods.Select(x => Evaluate(x)));
                return dice;
            }

            return "0";
        }

        public int Resolve(ModdableProperty? moddableProperty)
        {
            if (moddableProperty == null)
                return 0;

            return Resolve(moddableProperty.Id, moddableProperty.Prop!);
        }

        public int Resolve(Guid id, string prop)
        {
            Dice dice = Evaluate(id, prop);
            return dice.Roll();
        }

        public string[] Describe(ModdableProperty? moddableProperty, bool includeEntityInformation = false)
        {
            if (moddableProperty == null)
                return new string[0];

            return Describe(Context.Get(moddableProperty.Id)!, moddableProperty.Prop!, includeEntityInformation);
        }

        public string[] Describe(Entity entity, string prop, bool includeEntityInformation = false)
        {
            var res = new List<string> { $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" };

            var mods = Get(entity.Id, prop);
            var descriptions = mods?
                .SelectMany(x => Describe(x, includeEntityInformation))
                .ToArray();

            res.AddRange(descriptions ?? new string[0]);

            return res.ToArray();
        }

        public string[] Describe(Modifier modifier, bool includeEntityInformation = false)
        {
            var desc = DescribeSource(modifier, includeEntityInformation);

            //if (includeEntityInformation && Context.Get(modifier.Target.Id)?.MetaData != null)
            //    desc += $" to {DescribeTarget(modifier)}";

            var res = new List<string>() { desc };

            var mods = Get(modifier.Source);
            if (mods != null)
            {
                foreach (var mod in mods)
                    res.AddRange(Describe(mod).Select(x => $"  {x}"));
            }

            return res.ToArray();
        }

        private string DescribeSource(Modifier mod, bool includeEntityInformation = false)
        {
            if (mod.Source == null)
                return $"{mod.Name} => {mod.Dice ?? "0"}";

            var rootEntity = Context.Get(mod.Source.RootId);
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

            desc += $"{mod.Source.Prop ?? mod.Source.Method ?? mod.Name} => {SourceDice(mod, sourceEntity)}";

            if (mod.DiceCalc != null)
                desc += $" => {DescribeDiceCalc(mod.DiceCalc)}() => {Evaluate(mod)}";

            return desc;
        }

        private string DescribeTarget(Modifier modifier)
        {
            var rootEntity = Context.Get(modifier.Target.RootId);
            var targetEntity = Context.Get(modifier.Target);
            
            return $"{rootEntity?.Name}.{targetEntity?.Name}.{modifier.Target.Prop}".Trim('.');
        }

        private string DescribeDiceCalc(string? diceCalc)
        {
            if (string.IsNullOrEmpty(diceCalc))
                return diceCalc;

            var parts = diceCalc.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return diceCalc;

            if (parts.Length == 2 && Guid.TryParse(parts[0], out var id))
                return $"{Context.Get(id)?.Name}.{parts[1]}".Trim('.');

            return diceCalc;
        }

        public Dice SourceDice(Modifier mod, Entity? sourceEntity = null)
        {
            var dice = mod.Dice != null
                ? mod.Dice.Value
                : null;

            if (dice == null)
            {
                sourceEntity ??= mod.Source != null
                    ? Context.Get(mod.Source.Id)
                    : null;

                dice = sourceEntity?.PropertyValue<string>(mod.Source!.Prop!) ?? "0";
            }

            return dice;
        }

        public void Clear() => _store.Clear();

        public void Clear(Guid entityId)
        {
            if (!_store.ContainsKey(entityId))
                return;

            var entityMods = _store[entityId];
            foreach (var prop in entityMods.Keys)
            {
                var mods = Get(entityId, prop);
                if (mods?.Any() ?? false)
                {
                    var toRemove = mods.Where(x => x.CanBeCleared()).ToArray();
                    foreach (var remove in toRemove)
                        mods.Remove(remove);

                    if (!mods.Any())
                        entityMods.Remove(prop);

                    if (!entityMods.Any())
                        _store.Remove(entityId);
                }
            }
        }

        public bool Contains(Modifier mod) => Get(mod.Target.Id, mod.Target.Prop!)?.Any(x => x.Id == mod.Target.Id) ?? false;
        public bool Contains(KeyValuePair<string, List<Modifier>> item) => ContainsKey(item.Key);

        public bool ContainsKey(string key)
        {
            if (!TryParseKey(key, out var res))
                return false;

            return _store.ContainsKey(res.Item1) && _store[res.Item1].ContainsKey(res.Item2);
        }

        public void CopyTo(KeyValuePair<string, List<Modifier>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, List<Modifier>>> GetEnumerator()
        {
            foreach (var id in _store.Keys)
                foreach (var prop in _store[id].Keys)
                    yield return new KeyValuePair<string, List<Modifier>>(MakeKey(id, prop), _store[id][prop]);
        }

        public bool Remove(Modifier mod)
        {
            var res = false;

            if (_store.ContainsKey(mod.Target.Id))
            {
                var entityMods = _store[mod.Target.Id];
                if (!string.IsNullOrEmpty(mod.Target.Prop) && entityMods.ContainsKey(mod.Target.Prop))
                {
                    var mods = entityMods[mod.Target.Prop];
                    var toRemove = mods.FirstOrDefault(x => x.Id == mod.Id && mod.CanBeCleared());

                    if (toRemove != null)
                    {
                        mods.Remove(toRemove);
                        res = true;
                    }

                    if (!mods.Any())
                        entityMods.Remove(mod.Target.Prop);

                    if (!entityMods.Any())
                        _store.Remove(mod.Target.Id);
                }
            }

            return res;
        }

        public bool Remove(Guid entityId)
        {
            var res = false;

            if (!_store.ContainsKey(entityId))
                return false;

            return _store.Remove(entityId);
        }

        public bool Remove(ModdableProperty? moddableProperty)
        {
            if (moddableProperty == null)
                return false;

            return Remove(moddableProperty.Id, moddableProperty.Prop!);
        }

        public bool Remove(Guid entityId, string prop)
        {
            var res = false;

            if (!_store.ContainsKey(entityId))
                return false;

            var entityMods = _store[entityId];

            if (entityMods != null)
            {
                res = entityMods.Remove(prop);
                if (!entityMods.Any())
                    _store.Remove(entityId);
            }

            return res;
        }

        public bool Remove(string key)
        {
            return TryParseKey(key, out var res)
                ? Remove(res.Item1, res.Item2)
                : false;
        }

        public bool Remove(int currentTurn)
        {
            var toRemove = new List<Modifier>();
            foreach (var mod in this.SelectMany(x => x.Value))
            {
                if (mod.ShouldBeRemoved(currentTurn))
                    toRemove.Add(mod);
            }

            foreach (var mod in toRemove)
                Remove(mod);

            return toRemove.Any();
        }

        public bool Remove(KeyValuePair<string, List<Modifier>> item) => Remove(item.Key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out List<Modifier> value)
        {
            if (TryParseKey(key, out (Guid, string) res) && _store.ContainsKey(res.Item1))
            {
                var entityMods = _store[res.Item1] ?? new Dictionary<string, List<Modifier>>();
                if (entityMods.ContainsKey(res.Item2))
                {
                    value = entityMods[res.Item2];
                    return true;
                }
            }

            value = new List<Modifier>();
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

        private ICollection<List<Modifier>> AllValues()
        {
            var res = new List<List<Modifier>>();
            foreach (var id in _store.Keys)
            {
                foreach (var prop in _store[id].Keys)
                    res.Add(_store[id][prop]);
            }
            return res.ToArray();
        }
    }
}
