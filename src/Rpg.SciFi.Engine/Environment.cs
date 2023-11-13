using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public class Environment
    {
        public virtual string Name { get; set; } = "Default";
        public virtual string Atmosphere { get; set; } = "Air";
        public virtual Condition Light => new Condition { Name = "Sunlight", Unit = "lumen", Value = 93 };
        public virtual Condition Temperature => new Condition { Name = "Temperature", Unit = "celcius", Value = 20 };
        public virtual Condition Visibility => new Condition { Name = "Visibility", Unit = "percent", Value = 90 };
        public virtual Condition Radiation => new Condition { Name = "Radiation", Unit = "Bq", Value = 0.5 };
        public virtual Condition Pressure => new Condition { Name = "Pressure", Unit = "psi", Value = 14.7 };
    }
}
