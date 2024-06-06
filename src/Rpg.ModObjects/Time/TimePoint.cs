using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TimePoint
    {
        public string Type { get; private set; }
        public int Tick { get; private set; }

        [JsonConstructor] private TimePoint() { }

        public TimePoint(string type, int tick) 
        {
            Type = type;
            Tick = tick;
        }
    }
}
