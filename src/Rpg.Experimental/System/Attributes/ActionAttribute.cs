namespace Rpg.Experimental.System.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public bool Required { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string[]? NextActionHints { get; set; }

        public ActionAttribute() { }
    }
}
