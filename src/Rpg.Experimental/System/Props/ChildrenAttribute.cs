namespace Rpg.Experimental.System.Props
{
    public class ChildrenAttribute : RpgPropertyAttribute
    {
        public ChildrenAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Children;
            Editor = EditorType.Children;
            IsNullable = false;
        }
    }
}
