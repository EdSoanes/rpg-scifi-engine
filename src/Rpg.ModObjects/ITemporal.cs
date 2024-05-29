using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public interface ITemporal
    {
        void OnGraphCreating(RpgGraph graph, RpgObject entity);
        void OnTurnChanged(int turn);
        void OnBeginEncounter();
        void OnEndEncounter();
    }
}
