using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Meta.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class MetaPropUIAttribute : Attribute
    {
        public string DataType { get; private set; }
        public string ReturnType { get; protected set; }
        public string? DisplayName { get; set; }
        public bool Ignore { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        private Dictionary<string, object?> Values { get; set; }

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
        public T GetValue<T>(string prop, T def)
        {
            var values = GetValues();
            if (values.TryGetValue(prop, out var val) && val != null)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)(val.ToString() ?? string.Empty);

                if (val is T)
                    return (T)val;
            }

            return def;
        }

        protected MetaPropUIAttribute()
        {
            DataType = GetType().Name.Replace("UIAttribute", "");
        }
    }
}
