using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaEncounter
    {
        public int CurrentTurn { get; private set; }

        public void StartEncounter() => CurrentTurn = 1;
        public void IncrementTurn() => CurrentTurn++;
        public void SetTurn(int turn) => CurrentTurn = turn;
        public void EndEncounter() => CurrentTurn = 0;
    }
}
