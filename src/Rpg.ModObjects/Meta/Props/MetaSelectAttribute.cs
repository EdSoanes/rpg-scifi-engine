using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Props
{
    public abstract class MetaSelectAttribute : MinZeroAttribute
    {
        public string[] Values { get; protected set; } = Array.Empty<string>();

        public MetaSelectAttribute(params string[] values)
            : base()
        {
            Values = values;
            Max = Values.Length;
            Editor = EditorType.Select;
        }
    }
}
