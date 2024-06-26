using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Attributes
{
    public class RichTextUIAttribute : MetaPropUIAttribute
    {
        public RichTextUIAttribute()
        {
            ReturnType = nameof(String);
        }
    }
}
