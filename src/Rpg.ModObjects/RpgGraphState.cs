using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public class RpgGraphState
    {
        public List<RpgEntity> Entities { get; set; } = new();
        public string? ContextId { get; set; }
        public ITimeEngine? Time {  get; set; }
    }
}
