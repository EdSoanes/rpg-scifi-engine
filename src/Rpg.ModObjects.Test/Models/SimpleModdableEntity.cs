using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class SimpleModdableEntity : ModObject
    {
        public int Score { get; protected set; }
        public int Bonus { get; protected set; }

        public SimpleModdableEntity(int score, int bonus)
        {
            Score = score;
            Bonus = bonus;
        }

        protected override void OnCreate()
        {
            this.AddBaseMod(x => x.Score, x => x.Bonus);
        }


    }
}
