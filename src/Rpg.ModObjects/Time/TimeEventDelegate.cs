using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public class NotifyTimeEventEventArgs : EventArgs
    {
        public TimePoint Time { get; private set; }

        public NotifyTimeEventEventArgs(TimePoint time)
        {
            Time = time;
        }
    }

    public delegate void NotifyTimeEventHandler(object? sender, NotifyTimeEventEventArgs e);
}
