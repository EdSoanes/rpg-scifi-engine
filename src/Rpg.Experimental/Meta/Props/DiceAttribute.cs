namespace Rpg.Experimental.Meta.Props
{
    public class DiceAttribute : RpgPropertyAttribute
    {
        public DiceAttribute()
            : base()
        {
            Editor = EditorType.Text;
        }
    }
}
