using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;

namespace Rpg.Sys.Components.Values
{
    public class MinMaxValue : RpgComponent
    {
        [JsonProperty] 
        [MinZero(Ignore = true)]
        public int Min { get; protected set; }

        [JsonProperty] 
        [MinZero]
        public int Max { get; protected set; }

        [JsonProperty]
        [MinZero]
        public int Current { get; protected set; }

        [JsonConstructor] protected MinMaxValue() { }

        public MinMaxValue(string name, int max)
            : base(name)
        {
            Max = max;
        }

        public MinMaxValue(string name, int min, int max)
            : this(name, max)
        {
            Min = min;
        }

        public MinMaxValue(string name, int min, int max, int current)
            : this(name, min, max)
        {
            Current = current;
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            this.BaseMod(x => x.Current, x => x.Max);
        }
    }
}
