using Rpg.SciFi.Engine.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public class CharacterSheet
    {
        public Character Character { get; set; } = new Character();
        public List<Artifact> Artifacts { get; set; } = new List<Artifact>();

        public List<Turn> Turns { get; set; } = new List<Turn>();

        public Turn CurrentTurn { get; set; }

        public void NewTurn()
        {
            if (CurrentTurn != null)
                Turns.Add(CurrentTurn);

            CurrentTurn = new Turn();
        }
    }
}
