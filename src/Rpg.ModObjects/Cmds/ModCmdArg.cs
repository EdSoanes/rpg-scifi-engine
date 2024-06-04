using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Cmds
{
    public class ModCmdArg
    {
        public const string InitiatorArg = "initiator";
        public const string RecipientArg = "recipient";
        public const string TargetArg = "target";
        public const string DiceRollArg = "diceRoll";
        public const string ModSetArg = "modSet";
        public const string OutcomeArg = "outcome";

        public static string[] ReservedArgs = new[]
        {
            InitiatorArg,
            RecipientArg,
            TargetArg,
            DiceRollArg,
            ModSetArg,  
            OutcomeArg
        };

        private Type? _type;
        [JsonIgnore]
        public Type? Type
        {
            get
            {
                if (_type == null || (!string.IsNullOrEmpty(TypeName) && _type.AssemblyQualifiedName != TypeName))
                    _type = Type.GetType(TypeName);

                return _type;
            }
        }

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string TypeName { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public bool IsReserved { get; private set; }
        [JsonProperty] public ModCmdArgType ArgType { get; private set; }

        [JsonConstructor] private ModCmdArg() { }

        internal ModCmdArg(ParameterInfo parameterInfo, ModCmdArgAttribute? attr)
        {
            Name = parameterInfo.Name!;
            TypeName = parameterInfo.ParameterType.AssemblyQualifiedName!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            IsReserved = ReservedArgs.Contains(parameterInfo.Name);
            ArgType = attr?.ArgType ?? ModCmdArgType.Any;
        }

        public bool IsOfType(object? obj)
        {
            if (IsNullable && obj == null)
                return true;

            if (obj != null && obj.GetType().FullName == TypeName)
                return true;

            return false;
        }
    }
}
