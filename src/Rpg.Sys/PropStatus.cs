using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public class PropStatus
    {
        public PropStatusType StatusType { get; set; } = PropStatusType.Base;
        public Dice Base { get; set; }
        public Dice Current { get; set; }
        public bool IsPlayerModified { get; set; }
    }
}
