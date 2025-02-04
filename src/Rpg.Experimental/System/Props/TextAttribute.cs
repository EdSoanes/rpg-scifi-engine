namespace Rpg.Experimental.System.Props
{
    public class TextAttribute : RpgPropertyAttribute
    {
        public TextAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Text;
            Editor = EditorType.Text;
        }
    }
}
