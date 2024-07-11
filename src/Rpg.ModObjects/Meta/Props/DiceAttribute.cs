using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Props
{
    public class DiceAttribute : MetaPropAttribute
    {
        public DiceAttribute()
            : base()
        {
            Editor = EditorType.Text;
        }
    }
}
