using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Time
{
    public class TemporalEventArgs : EventArgs
    {
        public TimePoint Time { get; private set; }

        public TemporalEventArgs(TimePoint time)
        {
            Time = time;
        }
    }

    public delegate void NotifyTemporalEventHandler(object? sender, TemporalEventArgs e);
}
