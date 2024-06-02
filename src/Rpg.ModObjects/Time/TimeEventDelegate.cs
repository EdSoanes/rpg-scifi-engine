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
        public Time Time { get; private set; }

        public NotifyTimeEventEventArgs(Time time)
        {
            Time = time;
        }
    }

    public delegate void NotifyTimeEventHandler(object? sender, NotifyTimeEventEventArgs e);
}
