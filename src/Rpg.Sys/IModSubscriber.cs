using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys
{
    public interface IModSubscriber
    {
        public void SetModdableProperty(string prop, Dice dice);
        public Dice? GetModdableValue(string prop);
    }
}
