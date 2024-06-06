using Newtonsoft.Json;
using Rpg.ModObjects.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaActionArg
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

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string TypeName { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }
        [JsonProperty] public bool IsReserved { get; private set; }
        [JsonProperty] public RpgActionArgType ArgType { get; private set; }

        [JsonConstructor] private MetaActionArg() { }

        public MetaActionArg(ParameterInfo parameterInfo, RpgActionArgAttribute? attr)
        {
            Name = parameterInfo.Name!;
            TypeName = parameterInfo.ParameterType.AssemblyQualifiedName!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            IsReserved = ReservedArgs.Contains(parameterInfo.Name);
            ArgType = attr?.ArgType ?? RpgActionArgType.Any;
        }
    }
}
