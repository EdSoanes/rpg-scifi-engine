using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests.Models
{
    public class SimpleModdableEntity : RpgEntity
    {
        public int Score { get; protected set; }
        public int Bonus { get; protected set; }

        public SimpleModdableEntity(int score, int bonus)
        {
            Score = score;
            Bonus = bonus;
        }

        public RpgRef<string> Ref { get; init; } = new();

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Ref.OnCreating(graph, entity);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Ref.OnTimeBegins();

            this.AddMod(new Base(), x => x.Score, x => x.Bonus);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            Ref.OnStartLifecycle();
            return base.OnStartLifecycle();
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            Ref.OnUpdateLifecycle();
            return base.OnUpdateLifecycle();
        }
    }
}
