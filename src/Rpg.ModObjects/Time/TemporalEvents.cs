using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class TemporalEventArgs : EventArgs
    {
        public PointInTime Time { get; private set; }

        public TemporalEventArgs(PointInTime time)
        {
            Time = time;
        }
    }

    public delegate void NotifyTemporalEventHandler(object? sender, TemporalEventArgs e);
}
