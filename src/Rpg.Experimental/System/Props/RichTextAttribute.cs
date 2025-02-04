namespace Rpg.Experimental.System.Props
{
    public class RichTextAttribute : RpgPropertyAttribute
    {
        public RichTextAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Text;
            Editor = EditorType.RichText;
        }
    }
}
