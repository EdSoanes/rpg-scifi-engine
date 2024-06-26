using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Rpg.ModObjects.Meta.Attributes
{
    public abstract class SelectUIAttribute : MinZeroUIAttribute
    {
        public string[] Values { get; protected set; } = Array.Empty<string>();

        public SelectUIAttribute(string[] values)
            : base()
        {
            Values = values;
            Max = Values.Length;
            ReturnType = nameof(Int32);
        }
    }
}
