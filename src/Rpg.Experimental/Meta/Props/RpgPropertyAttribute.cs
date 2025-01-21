using Rpg.Experimental.Reflection;
using System.Reflection;

namespace Rpg.Experimental.Meta.Props
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class RpgPropertyAttribute : Attribute
    {
        private static string[] IgnoreProps = ["Editor", "DisplayName", "Tab", "Group", "TypeId", "Values"];

        public EditorType Editor { get; set; }
        public string? DisplayName { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;

        public Dictionary<string, object?> GetValues()
        {
            var attrs = new Dictionary<string, object?>();

            foreach (var propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => IsValidProp(x)))
                attrs.Add(propInfo.Name, propInfo.GetValue(this));

            return attrs;
        }

        private bool IsValidProp(PropertyInfo propInfo)
            => !IgnoreProps.Contains(propInfo.Name) && (propInfo.PropertyType.IsPrimitive || propInfo.PropertyType.IsEnum || RpgTypeUtilities.TypeNotExcluded(propInfo.PropertyType));
    }
}
