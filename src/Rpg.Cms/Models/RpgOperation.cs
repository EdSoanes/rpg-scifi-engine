using Rpg.ModObjects;

namespace Rpg.Cms.Models
{
    public class RpgOperation<T>
        where T : class
    {
        public RpgGraphState GraphState { get; set; }
        public T Operation { get; set; }
    }
}
