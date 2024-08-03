using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.Attributes
{
    public class ArgSelectAttribute : ArgAttribute
    {
        public string[]? Options { get; set; }
        public Type? Enum { get; set; }

        public ArgSelectAttribute() 
            : base() 
        {
            if (Enum != null && !Enum.GetType().IsEnum)
                throw new ArgumentException("Enum");

            if (Enum != null && Options == null)
                Options = Enum.GetEnumNames();
        }
    }
}
