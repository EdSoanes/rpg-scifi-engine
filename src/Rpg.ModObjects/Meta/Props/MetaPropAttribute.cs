using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Meta.Props
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class MetaPropAttribute : Attribute
    {
        public string DataTypeName { get; set; }
        public EditorType Editor { get; set; }
        public ReturnType Returns { get; set; }
        public string? DisplayName { get; set; }
        public bool Ignore { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        private Dictionary<string, object?>? Values { get; set; }

        public Dictionary<string, object?> GetValues()
        {
            if (Values == null)
            {
                Values = new Dictionary<string, object?>();
                foreach (var propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    Values.Add(propInfo.Name, propInfo.GetValue(this));
            }

            return Values;
        }

        public T Value<T>(string prop, T def)
        {
            if (Values != null && Values.TryGetValue(prop, out var val) && val != null)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)(val.ToString() ?? string.Empty);

                if (val is T)
                    return (T)val;
            }

            return def;
        }

        protected MetaPropAttribute()
        {
            DataTypeName = GetType().Name.Replace("Attribute", "");
        }
    }
}
