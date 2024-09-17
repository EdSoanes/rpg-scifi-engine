using Rpg.ModObjects.Reflection;
using System.Reflection;

namespace Rpg.ModObjects.Meta.Props
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class MetaPropAttribute : Attribute
    {
        public string DataTypeName { get; set; }
        public EditorType Editor { get; set; }
        public string? DisplayName { get; set; }
        public bool Ignore { get; set; }
        public string Tab { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        private MetaPropAttr? Values { get; set; }

        public MetaPropAttr GetValues()
        {
            var attrs = new MetaPropAttr();
            foreach (var propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name != "TypeId" && (x.PropertyType.IsPrimitive || x.PropertyType.IsEnum || RpgTypeScan.TypeNotExcluded(x.PropertyType))))
                attrs.Add(propInfo.Name, propInfo.GetValue(this));

            return attrs;
        }

        protected MetaPropAttribute()
        {
            DataTypeName = GetType().Name.Replace("Attribute", "");
        }
    }
}
