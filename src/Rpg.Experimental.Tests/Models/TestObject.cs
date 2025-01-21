using Rpg.Experimental.Graph;
using Rpg.Experimental.Meta.Props;

namespace Rpg.Experimental.Tests.Models
{
    public class TestObject : RpgObject
    {
        public int Strength { get; protected set; } = 10;

        [Integer(DisplayName = "Str", Min = 3, Max = 18)]
        public int? Intelligence { get; protected set; }

        public Dice Damage { get; protected set; } = "d6 + 1";
        public Dice? Initiative { get; protected set; }
        public RpgObject? Child { get; set; }
        public List<RpgObject> Children { get; protected set; } = new();

        public TestObject() : base() { }

        public TestObject(string name)
            : base(name) { }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            graph
                .Add(this, x => x.Intelligence, 3)
                .Add(this, x => x.Damage, x => x.Strength);
        }
    }
}
