//using Newtonsoft.Json;
//using Rpg.ModObjects.Mods;
//using Rpg.ModObjects.Values;

//namespace Rpg.ModObjects.Actions
//{
//    public class RpgActionArgSet
//    {
//        private readonly RpgActionArg[] _args;
//        [JsonProperty] private Dictionary<string, object?> Args { get; set; } = new Dictionary<string, object?>();

//        [JsonConstructor] private RpgActionArgSet() { }

//        public RpgActionArgSet(RpgActionArg[] args) 
//        {
//            _args = args;
//            foreach (var arg in args)
//                Args.Add(arg.Name, null);
//        }

//        public string[] ArgNames() 
//            => Args.Keys.ToArray();

//        public bool IsValid<T>(string arg, T? value)
//        {
//            var modArgs = _args.Where(x => x.Name == arg);
//            if (modArgs.Count() != 1)
//                return false;

//            var modArg = modArgs.Single();
//            if (!modArg.IsNullable && value == null)
//                return false;

//            if (modArg.Type == null)
//                return false;

//            return modArg.Type.IsAssignableTo(typeof(T));
//        }

//        public RpgActionArgSet Set<T>(string arg, T? value)
//        {
//            if (IsValid<T>(arg, value))
//            {
//                if (Args.ContainsKey(arg))
//                    Args[arg] = value;
//                else
//                    Args.Add(arg, value);
//            }

//            return this;
//        }

//        public RpgActionArgSet SetModSet(ModSet modSet)
//            => Set(RpgActionArg.ModSetArg, modSet);

//        public RpgActionArgSet SetInitiator<T>(T initiator)
//            where T : RpgObject
//                => Set(RpgActionArg.InitiatorArg, initiator);

//        public RpgActionArgSet SetRecipient<T>(T recipient) 
//            where T : RpgObject
//                => Set(RpgActionArg.RecipientArg, recipient);

//        public RpgActionArgSet SetTarget(int target)
//            => Set(RpgActionArg.TargetArg, target);

//        public RpgActionArgSet SetDiceRoll(Dice diceRoll)
//            => Set(RpgActionArg.DiceRollArg, diceRoll);

//        public RpgActionArgSet SetOutcome(int outcome)
//            => Set(RpgActionArg.OutcomeArg, outcome);

//        public object?[] ToArgs()
//        {
//            var res = new List<object?>();
//            foreach (var cmdArg in _args)
//            {
//                if (Args.ContainsKey(cmdArg.Name))
//                {
//                    var obj = Args[cmdArg.Name];
//                    res.Add(obj);
//                }
//            }

//            return res.ToArray();
//        }
//    }
//}
