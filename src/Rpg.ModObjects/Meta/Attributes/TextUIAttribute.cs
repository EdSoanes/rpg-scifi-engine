using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Attributes
{
    public class TextUIAttribute : MetaPropUIAttribute
    {
        public TextUIAttribute()
        {
            ReturnType = nameof(String);
        }
    }
}
