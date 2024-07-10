using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;

namespace Rpg.Sys.Attributes
{
    public class HealthAttribute : MetaSelectAttribute
    {
        public HealthAttribute()
            : base("Healthy", "Light", "Heavy", "Critical", "Fatal")
        {
            DataTypeName = "Health";
        }
    }
}
