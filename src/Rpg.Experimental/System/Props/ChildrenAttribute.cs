namespace Rpg.Experimental.System.Props
{
    public class ChildrenAttribute : RpgPropertyAttribute
    {
        public int MaxItems { get; set; } = int.MaxValue;

        public ChildrenAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Children;
            Editor = EditorType.Children;
            IsNullable = false;
        }
    }
}
