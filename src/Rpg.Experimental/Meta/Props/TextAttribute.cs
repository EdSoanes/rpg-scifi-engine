using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Meta.Props
{
    public class TextAttribute : RpgPropertyAttribute
    {
        public TextAttribute()
            : base()
        {
            Editor = EditorType.Text;
        }
    }
}
