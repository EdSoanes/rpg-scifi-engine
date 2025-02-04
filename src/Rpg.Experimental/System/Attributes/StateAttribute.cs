namespace Rpg.Experimental.System.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StateAttribute : Attribute
    {
        public bool Required { get; set; }
        public bool Hidden { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }

        public StateAttribute() { }
    }
}
