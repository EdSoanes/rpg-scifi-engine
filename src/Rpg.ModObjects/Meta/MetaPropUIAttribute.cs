using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MetaPropUIAttribute : Attribute
    {
        public string Unit { get; set; }
        public int Min {  get; set; }
        public int Max { get; set; }
        public string Editor { get; set; }
        public string[] Keys { get; set; }
        public bool Ignore {  get; set; }
    }
}
