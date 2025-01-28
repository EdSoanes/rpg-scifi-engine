using Rpg.Experimental.Graph;
using Rpg.Experimental.Meta.Props;
using Rpg.Experimental.Mods;

namespace Rpg.Experimental.Tests.Models
{
    public class TestObject : RpgObject
    {
        [Integer(DisplayName = "Str", Min = 3, Max = 18)]
        public int Strength { get; protected set; } = 10;
        public int StrengthBonus { get; protected set; }

        [Integer(DisplayName = "Int", Min = 3, Max = 18)]
        public int? Intelligence { get; protected set; }
        public int IntelligenceBonus { get; protected set; }

        public Dice Damage { get; protected set; } = "d6 + 1";
        public Dice? Initiative { get; protected set; }
        public RpgObject? Child { get; set; }
        public List<RpgObject> Children { get; protected set; } = new();

        public TestObject() : base() { }

        public TestObject(string name)
            : base(name) { }

        public Dice CalculateBonus(Dice score) 
            => (int)Math.Floor((double)(score.Roll() - 10) / 2);

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            graph
                .Add(new Base(), this, x => x.StrengthBonus, x => x.Strength, () => CalculateBonus)
                .Add(new Base(), this, x => x.Intelligence, 3)
                .Add(new Base(), this, x => x.IntelligenceBonus, x => x.Intelligence, () => CalculateBonus)
                .Add(new Base(), this, x => x.Damage, x => x.Strength);
        }
    }
}
