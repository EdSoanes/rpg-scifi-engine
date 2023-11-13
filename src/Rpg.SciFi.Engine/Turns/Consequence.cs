using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Turns
{
    public class Consequence
    {
        public Artifact Target { get; set; }
        public string DurationType { get; set; } = "Permanent"; //Permanent (like damage), Encounter (until end of encounter), Turn (needs to specify number of turns below
        public string? Turns { get; set; } = "-1"; //null = permanent, otherwise should evaluate to +ve number
        public string BindsTo { get; set; }
        public object? Value { get; set; }
    }
}
