using Rpg.Experimental.Graph;
using Rpg.Experimental.Time;

namespace Rpg.Experimental
{
    public abstract class RpgObject : Lifespan, ILifecycle
    {
        public string Id { get; set; }

        public RpgObject()
            : base()
        {
            Id = this.NewId();
        }

        public virtual void OnCreating(RpgGraph graph, RpgObject obj) 
        { }

        public override void OnTimeEvent(TimePoint now)
            => base.OnTimeEvent(now);
    }
}
