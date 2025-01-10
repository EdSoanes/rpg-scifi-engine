using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Meta.Props
{
    public class RichTextAttribute : MetaPropAttribute
    {
        public RichTextAttribute()
            : base()
        {
            Editor = EditorType.RichText;
        }
    }
}
