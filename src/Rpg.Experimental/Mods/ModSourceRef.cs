using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Mods
{
    public sealed class ModSource
    {
        public RpgPropertyRef? PropRef { get; set; }
        public Dice? Value { get; set; }
        public RpgMethod<RpgObject, Dice>? Calc { get; set; }

        [JsonConstructor] private ModSource() { }

        public ModSource(RpgPropertyRef propRef, Expression<Func<Func<Dice, Dice>>>? calc)
        {
            PropRef = propRef;
            Calc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(calc);
        }

        public ModSource(Dice dice, Expression<Func<Func<Dice, Dice>>>? calc)
        {
            Value = dice;
            Calc = RpgMethodFactory.Create<RpgObject, Dice, Dice>(calc);
        }
    }
}
