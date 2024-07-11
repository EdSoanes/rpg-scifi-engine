using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Props
{
    public class TextAttribute : MetaPropAttribute
    {
        public TextAttribute()
            : base()
        {
            Editor = EditorType.Text;
        }
    }
}
