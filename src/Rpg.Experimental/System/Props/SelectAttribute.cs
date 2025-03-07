namespace Rpg.Experimental.System.Props
{
    public class SelectAttribute : MinZeroAttribute
    {
        public string[] Values { get; protected set; } = Array.Empty<string>();

        public SelectAttribute(params string[] values)
            : base()
        {
            Values = values;
            Max = Values.Length;
            Editor = EditorType.Select;
        }
    }
}
