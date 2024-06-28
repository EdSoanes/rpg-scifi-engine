using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.Sys.Attributes
{
    public class HealthUIAttribute : SelectUIAttribute
    {
        public HealthUIAttribute()
            : base("Healthy", "Light", "Heavy", "Critical", "Fatal")
        {
            DataType = "Select";
            DataTypeName = "Health";
        }
    }
}
