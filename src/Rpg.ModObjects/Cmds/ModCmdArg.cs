using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Cmds
{
    public class ModCmdArg
    {
        public const string InitiatorArg = "initiator";
        public const string TargetArg = "target";
        public const string DiceRollArg = "diceRoll";
        public const string ModSetArg = "modSet";
        public const string OutcomeArg = "outcome";

        public static string[] ReservedArgs = new[]
        {
            InitiatorArg,
            TargetArg,
            DiceRollArg,
            ModSetArg, 
            OutcomeArg
        };

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string DataType { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public bool IsReserved { get; private set; }
        [JsonProperty] public ModCmdArgType ArgType { get; private set; }

        internal static ModCmdArg? Create(ParameterInfo parameterInfo, ModCmdArgAttribute? attr)
        {
            if (string.IsNullOrEmpty(parameterInfo.Name))
                return null;

            return new ModCmdArg
            {
                Name = parameterInfo.Name,
                DataType = parameterInfo.ParameterType.FullName!,
                IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null,
                IsReserved = ReservedArgs.Contains(parameterInfo.Name),
                ArgType = attr?.ArgType ?? ModCmdArgType.Any
            };
        }

        public bool IsOfType(object? obj)
        {
            if (IsNullable && obj == null)
                return true;

            if (obj != null && obj.GetType().FullName == DataType)
                return true;

            return false;
        }
    }
}
