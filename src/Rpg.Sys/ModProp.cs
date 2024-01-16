using Newtonsoft.Json;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys
{
    public class ModProp
    {
        private Graph _graph;

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public Guid EntityId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public string ReturnType {  get; private set; }

        [JsonProperty] public Dice Value {  get; private set; }
        [JsonProperty] public Dice BaseValue { get; private set; }
        [JsonProperty] public string[] Description { get; private set; }

        [JsonProperty] private List<Modifier> Modifiers { get; set; } = new List<Modifier>();

        [JsonIgnore]
        public Modifier[] AllModifiers {  get => Modifiers.ToArray(); }

        [JsonIgnore]
        public Modifier[] BaseModifiers
        {
            get
            {
                var baseMods = Modifiers
                    .Where(x => x.Duration.GetExpiry(_graph.Turn) == ModifierExpiry.Active && x.ModifierType == ModifierType.Base)
                    .ToArray();

                return baseMods;
            }
        }

        [JsonIgnore]
        public Modifier[] FilteredModifiers
        {
            get
            {
                var activeModifiers = Modifiers.Where(x => x.Duration.GetExpiry(_graph.Turn) == ModifierExpiry.Active);

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
        }

        [JsonConstructor] private ModProp() { }

        public ModProp(Guid id, string prop, string returnType)
        {
            EntityId = id;
            Prop = prop;
            ReturnType = returnType;
        }

        public bool Add(IEnumerable<Modifier> mods)
        {
            var res = false;
            foreach (var mod in mods)
                res |= Add(mod);
            return res;
        }

        public bool Add(Modifier mod)
        {
            var existing = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
            if (existing != null)
                Modifiers.Remove(existing);

            if (mod.ModifierAction == ModifierAction.Accumulate)
            {
                Modifiers.Add(mod);
                return true;
            }

            if (mod.ModifierAction == ModifierAction.Sum)
            {
                return Sum(mod);
            }

            if (mod.ModifierAction == ModifierAction.Replace)
            {
                return Replace(mod);
            }

            return false;
        }

        private bool Sum(Modifier mod)
        {
            var matchingMods = Modifiers
                .Where(x => x.Name == mod.Name && x.ModifierType == mod.ModifierType)
                .ToArray();

            var dice = _Calculate(matchingMods) + _Calculate(new[] { mod });

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

        public Modifier[] UpdateOnTurn(int newTurn)
        {
            var updated = new List<Modifier>();
            foreach (var mod in Modifiers)
            {
                mod.OnUpdate(newTurn);

                var expiry = mod.Duration.GetExpiry(_graph.Turn);
                if (expiry == ModifierExpiry.Remove)
                    updated.Add(mod);
                else
                {
                    var oldExpiry = mod.Duration.GetExpiry(_graph.Turn - 1);
                    if (expiry != oldExpiry)
                        updated.Add(mod);
                }
            }

            foreach (var mod in updated.Where(x => x.Duration.GetExpiry(_graph.Turn) == ModifierExpiry.Remove))
                Modifiers.Remove(mod);

            return updated.ToArray();
        }

        public Modifier? Remove(Modifier mod)
        {
            var toRemove = Modifiers.FirstOrDefault(x => x.Id == mod.Id);
            if (toRemove != null)
            {
                Modifiers.Remove(toRemove);
                return toRemove;
            }

            return null;
        }

        public Modifier[] Remove(Func<Modifier, bool>? filter)
        {
            var toRemove = Modifiers
                .Where(x => filter == null || filter(x))
                .ToArray();

            foreach (var r in toRemove)
                Modifiers.Remove(r);

            return toRemove;
        }

        public void Clear()
        {
            var toRemove = Modifiers
                .Where(x => x.Duration.CanClear(_graph.Turn))
                .ToArray();

            foreach (var remove in toRemove)
                Modifiers.Remove(remove);
        }

        public bool AffectedBy(Guid id, string prop)
            => Modifiers.Any(x => x.Source.Id == id &&  x.Source.Prop == prop);

        public bool Contains(Modifier mod)
            => Modifiers.Any(x => x.Id == mod.Id);

        public void Evaluate(Graph? graph = null)
        {
            if (_graph == null && graph == null)
                throw new Exception("Graph null. Cannot evaluate modprop");

            if (_graph == null)
                _graph = graph!;

            Value = _Calculate(FilteredModifiers);
            BaseValue = _Calculate(BaseModifiers);
            Description = _Describe();
        }

        public string[] Describe(Graph graph)
        {
            var entity = graph.Entities.Get(EntityId);
            if (entity == null)
                return new string[0];

            var res = new List<string> { $"{entity.Name}.{Prop} = {Value}" };
            res.AddRange(Description.Select(x => $"  {x}"));

            return res.ToArray();
        }  

        public Dice Calculate(Modifier mod)
            => _Calculate(new[] { mod });

        private Dice _Calculate(IEnumerable<Modifier> mods)
        {
            Dice dice = "0";

            foreach (var mod in mods)
            {
                Dice modDice = mod.Source.PropType == PropType.Dice
                    ? mod.Source.Prop
                    : _graph.Entities.Get(mod.Source.Id)?.GetModdableProperty(mod.Source.Prop) ?? Dice.Zero;

                dice += _ApplyDiceCalc(modDice, mod.DiceCalc);
            }

            return dice;
        }

        private Dice _ApplyDiceCalc(Dice dice, ModifierDiceCalc diceCalc)
        {
            if (!diceCalc.IsCalc)
                return dice;

            if (diceCalc.IsStatic)
                return this.ExecuteFunction<Dice, Dice>($"{diceCalc.ClassName}.{diceCalc.FuncName}", dice);

            var entity = _graph.Entities.Get(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }

        private string[] _Describe(Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();

            var res = new List<string>();
            foreach (var mod in FilteredModifiers)
            {
                if (idStack.Contains(mod.Id))
                    throw new Exception($"Stack contains id {mod.Id}");
                else
                    idStack.Push(mod.Id);

                res.Add(_Describe(mod, idStack.Count - 1));
                var modProp = _graph.Mods.Get(mod.Source);
                if (modProp != null)
                    res.AddRange(modProp._Describe(idStack));

                idStack.Pop();
            }

            return res.ToArray();
        }

        private string _Describe(Modifier mod, int depth)
        {
            var src = mod.Source.Describe(_graph);
            if (mod.Source.PropType == PropType.Path)
                src += $" => {_graph.Mods.Get(mod.Source)?.Value ?? Dice.Zero}";
            else
                src = $"{mod.Name}.{src}";

            var calc = mod.DiceCalc.Describe(_graph);
            if (calc != null)
                src = $"{src} => {calc}() => {_Calculate(new[] { mod })}";

            return src.PadLeft((depth * 2) + src.Length);
        }
    }
}
