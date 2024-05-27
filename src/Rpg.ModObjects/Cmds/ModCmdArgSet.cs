using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Cmds
{
    public class ModCmdArgSet
    {
        private readonly ModCmdArg[] _args;
        [JsonProperty] private Dictionary<string, object?> Commands { get; set; } = new Dictionary<string, object?>();

        [JsonConstructor] private ModCmdArgSet() { }

        public ModCmdArgSet(ModCmdArg[] args) 
        {
            _args = args;
            foreach (var arg in args)
                Commands.Add(arg.Name, null);
        }

        public string[] ArgNames() 
            => Commands.Keys.ToArray();

        public bool IsValid<T>(string arg, T? value)
        {
            var modArgs = _args.Where(x => x.Name == arg);
            if (modArgs.Count() != 1)
                return false;

            var modArg = modArgs.Single();
            if (!modArg.IsNullable && value == null)
                return false;

            if (modArg.Type == null)
                return false;

            return modArg.Type.IsAssignableTo(typeof(T));
        }

        public ModCmdArgSet Set<T>(string arg, T? value)
        {
            if (IsValid<T>(arg, value))
            {
                if (Commands.ContainsKey(arg))
                    Commands[arg] = value;
                else
                    Commands.Add(arg, value);
            }

            return this;
        }

        public ModCmdArgSet SetModSet(ModSet modSet)
            => Set(ModCmdArg.ModSetArg, modSet);

        public ModCmdArgSet SetInitiator<T>(T initiator)
            where T : ModObject
                => Set(ModCmdArg.InitiatorArg, initiator);

        public ModCmdArgSet SetRecipient<T>(T recipient) 
            where T : ModObject
                => Set(ModCmdArg.RecipientArg, recipient);

        public ModCmdArgSet SetTarget(int target)
            => Set(ModCmdArg.TargetArg, target);

        public ModCmdArgSet SetDiceRoll(Dice diceRoll)
            => Set(ModCmdArg.DiceRollArg, diceRoll);

        public ModCmdArgSet SetOutcome(int outcome)
            => Set(ModCmdArg.OutcomeArg, outcome);

        public object?[] ToArgs()
        {
            var res = new List<object?>();
            foreach (var cmdArg in _args)
            {
                if (Commands.ContainsKey(cmdArg.Name))
                {
                    var obj = Commands[cmdArg.Name];
                    res.Add(obj);
                }
            }

            return res.ToArray();
        }
    }
}
