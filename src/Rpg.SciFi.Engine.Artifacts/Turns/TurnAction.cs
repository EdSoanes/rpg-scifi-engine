using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class TurnAction
    {
        public string Type { get; set; } = "Immediate";
        public int ActionPoints { get; set; } = 1;
        public int Exertion { get; set; } = 1;
        
        public int Focus { get; set; } = 1;
        public Modifier[] Modifiers = new Modifier[0];
        [Moddable] public Dice DiceRoll { get => this.Evaluate(nameof(DiceRoll)); }

        //public Artifact? UsingArtifact { get; set; }
        //public Consequence[]? SuccessConsequences { get; set; }
        //public Consequence[]? FailConsequences { get; set; }
        //public int ActionPointCost { get; set; }
        //public int ExertionCost { get; set; }
        //public int SuccessRoll {  get; set; }
    }
}
