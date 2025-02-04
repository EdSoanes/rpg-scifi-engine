namespace Rpg.Experimental.System.Props
{
    public class ChildAttribute : RpgPropertyAttribute
    {
        public ChildAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Child;
            Editor = EditorType.Child;
            IsNullable = true;
        }
    }
}
