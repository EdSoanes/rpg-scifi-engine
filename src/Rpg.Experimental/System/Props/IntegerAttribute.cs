namespace Rpg.Experimental.System.Props
{
    public class IntegerAttribute : RpgPropertyAttribute
    {
        public string Unit { get; protected set; } = nameof(Int32);
        public int Min { get; set; } = int.MinValue;
        public int Max { get; set; } = int.MaxValue;

        public IntegerAttribute()
            : base()
        {
            PropertyType = RpgPropertyType.Int;
            Editor = EditorType.Int32;
            IsNullable = false;
        }
    }
}
