using Rpg.ModObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs
{
    public class Injury : RpgLifecycleObject
    {
        public string Id { get; set; }
        public BodyPartType BodyPartType { get; set; }
        public int Severity { get; set; }
    }
}
