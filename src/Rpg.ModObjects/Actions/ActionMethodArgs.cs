//using Newtonsoft.Json;

//namespace Rpg.ModObjects.Actions
//{
//    public class ActionMethodArgs
//    {
//        public const string OwnerArg = "owner";
//        public const string InitiatorArg = "initiator";
//        public const string RecipientArg = "recipient";
//        public const string TargetArg = "target";
//        //public const string DiceRollArg = "diceRoll";
//        //public const string ModSetArg = "modSet";
//        //public const string OutcomeArg = "outcome";

//        public static string[] ReservedArgs = new[]
//        {
//            OwnerArg,
//            InitiatorArg,
//            RecipientArg,
//            TargetArg,
//            //DiceRollArg,
//            //ModSetArg,
//            //OutcomeArg
//        };

//        private readonly RpgActionArg[] _args;
//        [JsonProperty] private Dictionary<string, object?> Args { get; set; } = new Dictionary<string, object?>();

//        public object? this[string key]
//        {
//            get
//            {
//                if (Args.ContainsKey(key))
//                    return Args[key];

//                return null;
//            }
//        }

//        [JsonConstructor] private ActionMethodArgs() { }

//        public ActionMethodArgs(RpgActionArg[] args) 
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

//        public ActionMethodArgs Set<T>(string arg, T? value)
//        {
//            if (IsValid(arg, value))
//            {
//                if (Args.ContainsKey(arg))
//                    Args[arg] = value;
//                else
//                    Args.Add(arg, value);
//            }

//            return this;
//        }

//        public ActionMethodArgs SetOwner<T>(T owner)
//            where T : RpgObject
//                => Set(OwnerArg, owner);

//        public ActionMethodArgs SetInitiator<T>(T initiator)
//            where T : RpgObject
//                => Set(InitiatorArg, initiator);

//        public ActionMethodArgs SetRecipient<T>(T recipient)
//            where T : RpgObject
//                => Set(RecipientArg, recipient);

//        public ActionMethodArgs SetTarget(int target)
//            => Set(RpgActionArg.TargetArg, target);

//        //public ActionMethodArgs SetModSet(ModSet modSet)
//        //    => Set(RpgActionArg.ModSetArg, modSet);

//        //public ActionMethodArgs SetDiceRoll(Dice diceRoll)
//        //    => Set(RpgActionArg.DiceRollArg, diceRoll);

//        //public ActionMethodArgs SetOutcome(int outcome)
//        //    => Set(RpgActionArg.OutcomeArg, outcome);

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
