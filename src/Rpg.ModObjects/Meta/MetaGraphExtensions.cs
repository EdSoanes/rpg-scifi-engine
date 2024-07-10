using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;
using System.Collections;
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

        internal static MetaPropAttribute? GetPropUI(this PropertyInfo propertyInfo)
        {
            var ui = propertyInfo.GetCustomAttributes(true)
                .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(MetaPropAttribute))) as MetaPropAttribute;

            if (ui == null)
            {
                ui = propertyInfo.PropertyType.Name switch
                {
                    nameof(Int32) => new IntegerAttribute { Ignore = true },
                    nameof(Dice) => new DiceAttribute { Ignore = true },
                    nameof(String) => new TextAttribute { Ignore = true },
                    _ => null
                };
            }

            if (ui == null)
            {
                if (propertyInfo.PropertyType.IsAssignableTo(typeof(RpgContainer)))
                    ui = new ContainerAttribute { Ignore = true };
                else if (propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                    ui = new ComponentAttribute { Ignore = true };
            }

            return ui;
        }
    }
}
