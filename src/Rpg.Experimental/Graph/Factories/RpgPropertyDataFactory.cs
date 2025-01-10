using Rpg.Experimental.Reflection;
using System.Reflection;

namespace Rpg.Experimental.Graph.Factories
{
    public class RpgPropertyDataFactory
    {
        public IRpgPropertyData[] CreatePropertyData(RpgObject obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(x => CreatePropertyData(obj.Id, x))
                .Where(x => x != null)
                .Cast<IRpgPropertyData>()
                .ToArray();
        }

        private IRpgPropertyData? CreatePropertyData(string objectId, PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
                return null;

            if (propertyInfo.SetMethod == null)
                return null;

            return CreatePropertyData(objectId, propertyInfo.Name, propertyInfo.PropertyType);
        }

        public IRpgPropertyData? CreatePropertyData(string objectId, string prop, Type type)
        {
            if (RpgTypeUtilities.PropertyOfType(type, typeof(int)))
                return new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Int, RpgTypeUtilities.PropertyOfNullableType(type, typeof(int)));

            if (RpgTypeUtilities.PropertyOfType(type, typeof(Dice)))
                return new RpgPropertyDataModdable(objectId, prop, RpgPropertyType.Dice, RpgTypeUtilities.PropertyOfNullableType(type, typeof(Dice)));

            if (RpgTypeUtilities.PropertyOfType(type, typeof(RpgObject)))
                return new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Child);

            if (RpgTypeUtilities.PropertyOfType(type, typeof(ICollection<RpgObject>)))
                return new RpgPropertyDataObject(objectId, prop, RpgPropertyType.Children);

            return null;
        }
    }
}
