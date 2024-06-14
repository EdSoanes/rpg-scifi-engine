using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    internal static class MetaGraphExtensions
    {
        private static Type[] ModdablePropertyTypes = new Type[]
        {
            typeof(int),
            typeof(Dice)
        };

        internal static MetaObjectType GetObjectType(this Type type)
        {
            if (type.IsAssignableTo(typeof(IRpgEntityTemplate)))
                return MetaObjectType.EntityTemplate;

            if (type.IsAssignableTo(typeof(IRpgComponentTemplate)))
                return MetaObjectType.ComponentTemplate;

            if (type.IsAssignableTo(typeof(RpgComponent)))
                return MetaObjectType.Component;

            if (type.IsAssignableTo(typeof(RpgEntity)))
                return MetaObjectType.Entity;

            return MetaObjectType.None;
        }

        internal static PropertyInfo[] GetModdableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsModdableProperty() || x.IsRpgObjectProperty() || x.IsRpgTemplateProperty())
                .ToArray();
        }

        internal static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            if (!ModdablePropertyTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily);
        }

        internal static bool IsRpgObjectProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily)
                && (propertyInfo.PropertyType.IsAssignableTo(typeof(RpgObject)) || propertyInfo.PropertyType.IsAssignableTo(typeof(IEnumerable<RpgObject>)));
        }

        internal static bool IsRpgTemplateProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily)
                && (propertyInfo.PropertyType.IsAssignableTo(typeof(IRpgEntityTemplate)) || propertyInfo.PropertyType.IsAssignableTo(typeof(IRpgComponentTemplate)));
        }

        internal static MetaPropUIAttribute GetPropUI(this PropertyInfo propertyInfo)
        {
            var ui = propertyInfo.GetCustomAttributes(true)
                .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(MetaPropUIAttribute))) as MetaPropUIAttribute;

            if (ui == null)
            {
                ui = propertyInfo.PropertyType.Name switch
                {
                    nameof(Int32) => new IntegerUIAttribute(),
                    nameof(Dice) => new DiceUIAttribute(),
                    nameof(String) => new TextUIAttribute(),
                    _ => new ComponentUIAttribute()
                };
            }

            return ui!;
        }
    }
}
