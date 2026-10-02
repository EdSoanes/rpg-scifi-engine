using Rpg.Experimental.Reflection;
using System.Collections;
using System.Reflection;

namespace Rpg.Experimental.System
{
    /// <summary>
    /// Creates game system objects from authored values, e.g. content from a content management system or
    /// an item handed over from another character sheet. The values are matched by name to the properties
    /// of the object's template (see MetaTemplate).
    /// </summary>
    public static class RpgObjectFactory
    {
        /// <summary>
        /// True if objects of the archetype can be created from authored values
        /// </summary>
        public static bool CanCreate(RpgSystem system, string archetype)
            => system.GetMetaObject(archetype)?.Template != null;

        /// <summary>
        /// Create an object. Values with no matching template property are ignored, and template properties
        /// with no value keep what the template class gives them. The engine does not refuse a value for
        /// being out of range.
        /// </summary>
        public static RpgObject Create(RpgSystem system, string archetype, IReadOnlyDictionary<string, object?> values)
        {
            var metaObject = system.Objects.FirstOrDefault(x => x.Archetype == archetype);
            if (metaObject == null)
                throw new ArgumentException($"{archetype} is not an object of the {system.Identifier} system");

            if (metaObject.Template == null)
                throw new ArgumentException($"{archetype} cannot be created from authored values. It has no constructor that takes a template");

            var objectType = ResolveType(metaObject.QualifiedTypeName)
                ?? throw new InvalidOperationException($"The .net type of {archetype} could not be found");

            var templateType = ResolveType(metaObject.Template.QualifiedTypeName)
                ?? throw new InvalidOperationException($"The .net type of {metaObject.Template.TypeName} could not be found");

            var template = Activator.CreateInstance(templateType)!;
            foreach (var propInfo in TemplateProperties(templateType))
            {
                var key = values.Keys.FirstOrDefault(x => string.Equals(x, propInfo.Name, StringComparison.OrdinalIgnoreCase));
                if (key == null)
                    continue;

                var converted = ConvertValue(values[key], propInfo.PropertyType, out var ok);
                if (ok)
                    propInfo.SetValue(template, converted);
            }

            var ctor = objectType.GetConstructor([templateType])!;
            return (RpgObject)ctor.Invoke([template]);
        }

        /// <summary>
        /// Put a child into a property of its parent before either is added to a sheet, e.g. a sword into
        /// a character's hands. Returns false if the parent has no such property.
        /// </summary>
        public static bool AddChild(RpgObject parent, string prop, RpgObject child)
        {
            var propInfo = parent.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo == null)
                return false;

            if (propInfo.GetValue(parent) is IList list)
            {
                if (!list.Contains(child))
                    list.Add(child);

                return true;
            }

            var setter = propInfo.GetSetMethod(true);
            if (setter != null && propInfo.PropertyType.IsInstanceOfType(child))
            {
                setter.Invoke(parent, [child]);
                return true;
            }

            return false;
        }

        /// <summary>
        /// The template class of an object type: the single parameter of a public constructor, where that
        /// parameter is a plain class with a public parameterless constructor
        /// </summary>
        internal static Type? TemplateTypeOf(Type objectType)
        {
            foreach (var ctor in objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length != 1)
                    continue;

                var type = parameters[0].ParameterType;
                if (type.IsClass
                    && type != typeof(string)
                    && !type.IsAbstract
                    && !type.IsAssignableTo(typeof(RpgLifecycleObject))
                    && type.GetConstructor(Type.EmptyTypes) != null)
                    return type;
            }

            return null;
        }

        internal static IEnumerable<PropertyInfo> TemplateProperties(Type templateType)
            => templateType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetSetMethod() != null && IsAuthorable(x.PropertyType));

        private static bool IsAuthorable(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(int) || type == typeof(Dice) || type == typeof(string) || type == typeof(bool);
        }

        private static Type? ResolveType(string? qualifiedTypeName)
        {
            if (string.IsNullOrEmpty(qualifiedTypeName))
                return null;

            return RpgTypeUtilities.ForType(qualifiedTypeName) ?? Type.GetType(qualifiedTypeName);
        }

        private static object? ConvertValue(object? value, Type targetType, out bool ok)
        {
            ok = true;
            var isNullable = Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType;
            var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var text = value?.ToString();
            if (value == null || (type != typeof(string) && string.IsNullOrWhiteSpace(text)))
            {
                //Nothing authored: a nullable property is cleared, anything else keeps what the template gives it
                ok = isNullable;
                return null;
            }

            if (type == typeof(string))
                return text;

            if (type == typeof(bool))
            {
                if (value is bool b) return b;
                if (bool.TryParse(text, out var parsed)) return parsed;
                if (int.TryParse(text, out var number)) return number != 0;
            }
            else if (type == typeof(int))
            {
                if (value is int i) return i;
                if (value is bool flag) return flag ? 1 : 0;
                if (int.TryParse(text, out var parsed)) return parsed;
                if (decimal.TryParse(text, global::System.Globalization.NumberStyles.Any, global::System.Globalization.CultureInfo.InvariantCulture, out var dec))
                    return (int)Math.Round(dec);
            }
            else if (type == typeof(Dice))
            {
                if (value is Dice dice) return dice;
                if (Dice.TryParse(text, out var parsed)) return parsed;
            }

            ok = false;
            return null;
        }
    }
}
