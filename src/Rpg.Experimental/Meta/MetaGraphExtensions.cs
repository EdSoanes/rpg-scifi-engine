using Rpg.Experimental.Meta.Props;
using System.Collections;
using System.Reflection;

namespace Rpg.Experimental.Meta
{
    internal static class MetaGraphExtensions
    {
        private static Type[] ModdablePropertyTypes = new Type[]
        {
            typeof(int),
            typeof(Dice)
        };

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

            if (ui == null && propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                ui = new ComponentAttribute { Ignore = true };

            return ui;
        }
    }
}
