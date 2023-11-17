using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Score : Modifiable
    {
        public const int NotApplicable = -1;

        public Score() { }

        public Score(string name)
        {
            Name = name;
        }

        public Score(string name, string description, int baseValue)
        {
            Name = name;
            Description = description;
            BaseValue = baseValue;
        }

        [JsonProperty] public virtual int BaseValue { get; protected set; } = 0;
        public virtual int Value => BaseValue + ModifierRoll("Value");
    }

    public class CompositeScore : Score
    {
        [JsonProperty] protected Score[] Scores { get; private set; }

        public CompositeScore(Score[] scores) 
        { 
            Scores = scores;
        }

        public override int BaseValue 
        { 
            get => Scores.Sum(x => x.BaseValue); 
            protected set => throw new ArgumentException($"Cannot set {nameof(CompositeScore)}.{nameof(BaseValue)}"); 
        }

        public override int Value { get => BaseValue + Scores.Sum(x => x.Value); }
    }
}
