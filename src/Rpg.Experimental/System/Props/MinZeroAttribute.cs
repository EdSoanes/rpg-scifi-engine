namespace Rpg.Experimental.System.Props
{
    public class MinZeroAttribute : IntegerAttribute
    {
        public MinZeroAttribute()
            : base()
        {
            Min = 0;
        }
    }
}
