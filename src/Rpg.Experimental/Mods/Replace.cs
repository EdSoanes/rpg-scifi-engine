using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Replace : Mod
    {
        [JsonProperty] public int Order { get; private set; }

        public Replace()
            : base(ModType.Standard)
        { }

        protected Replace(ModType modType)
            : base(modType)
        { }

        public Replace(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Replace(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Replace(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Replace(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Replace(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);
            Order = graph.GetPropertyData<RpgPropertyDataModdable>(Target!.ObjectId, Target!.Path)
                ?.Mods
                .Where(x => x is Replace && x.ModType == ModType)
                .Max(x => (x as Replace)?.Order ?? 0) + 1 ?? 0;
        }

        public Replace SetOrder(int order)
        {
            Order = order;
            return this;
        }
    }
}
