using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta.Props
{
    public class ContainerAttribute : MetaPropAttribute
    {
        public ContainerAttribute()
            : base()
        {
            Ignore = true;
            Returns = ReturnType.Container;
        }
    }
}
