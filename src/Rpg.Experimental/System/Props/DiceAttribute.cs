namespace Rpg.Experimental.System.Props
{
    public class DiceAttribute : RpgPropertyAttribute
    {
        public DiceAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Dice;
            Editor = EditorType.Dice;
        }
    }
}
