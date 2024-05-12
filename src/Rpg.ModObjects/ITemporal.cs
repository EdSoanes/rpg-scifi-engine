using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public interface ITemporal
    {
        void OnTurnChanged(int turn);
        void OnEncounterStarted();
        void OnEncounterEnded();
    }
}
