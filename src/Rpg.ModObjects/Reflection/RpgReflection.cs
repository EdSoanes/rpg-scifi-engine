using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public static class RpgReflection
    {
        internal static readonly Type[] RpgPropertyTypes =
        [
            typeof(int),
            typeof(string),
            typeof(Dice)
        ];

        private static string[] ExcludeAssembliesWith =
        {
            "Microsoft",
            "System",
            "Umbraco",
            "Newtonsoft",
            "Azure",
            "SixLabors",
            "NPoco",
            "Asp"
        };

        private static List<Assembly> _scanAssemblies = new List<Assembly>();

        public static void RegisterAssembly(Assembly assembly)
        {
            if (!_scanAssemblies.Contains(assembly))
                _scanAssemblies.Add(assembly);
        }

        internal static string[] GetArchetypes(this Type type)
        {
            var res = new List<string>();
            if (type.IsAssignableTo(typeof(RpgObject)))
            {
                while (type != null)
                {
                    res.Add(type.Name);
                    if (type == typeof(RpgObject) || type.BaseType == null)
                        break;

                    type = type.BaseType;
                }
            }

            res.Reverse();
            return res.ToArray();
        }

        public static Type? ScanForTypeByName(string name)
        {
            var type = RpgPropertyTypes.FirstOrDefault(x => x.Name == name);
            if (type != null)
                return type;

            foreach (var assembly in GetScanAssemblies())
            {
                type = assembly.GetExportedTypes().FirstOrDefault(x => x.Name == name);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static Type? ScanForType(string qualifiedTypeName)
        {
            var type = RpgPropertyTypes.FirstOrDefault(x => x.AssemblyQualifiedName == qualifiedTypeName);
            if (type != null)
                return type;

            foreach (var assembly in GetScanAssemblies())
            {
                type = assembly.GetExportedTypes().FirstOrDefault(x => x.AssemblyQualifiedName == qualifiedTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        internal static void ExecuteMethod(string staticMethod, object?[]? args = null)
        {
            var methodInfo = ScanForMethod(staticMethod);
            methodInfo.Invoke(null, args);
        }

        internal static TReturn? ExecuteMethod<TReturn>(string staticMethod, object?[]? args = null)
        {
            var methodInfo = ScanForMethod(staticMethod);
            return (TReturn?)methodInfo.Invoke(null, args);
        }

        internal static void ExecuteMethod(object obj, string methodName, object?[]? args = null)
        {
            var methodInfo = ScanForMethod(obj.GetType(), methodName);
            methodInfo.Invoke(obj, args);
        }

        internal static TReturn? ExecuteMethod<TReturn>(object obj, string methodName, object?[]? args = null)
        {
            var methodInfo = ScanForMethod(obj.GetType(), methodName);
            return (TReturn?)methodInfo.Invoke(obj, args);
        }

        internal static PropertyInfo[] ScanForModdableProperties(this RpgObject context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(IsModdableProperty)
                .ToArray();
        }

        internal static (int?, int?) GetPropertyThresholds(this PropertyInfo propInfo)
        {
            if (propInfo.PropertyType == typeof(int))
            {
                var threshold = propInfo.GetCustomAttribute<ThresholdAttribute>();
                if (threshold != null)
                    return (threshold.Min, threshold.Max);

                var select = propInfo.GetCustomAttribute<MetaSelectAttribute>();
                if (select != null)
                    return (select.Min, select.Max);

                var integer = propInfo.GetCustomAttribute<IntegerAttribute>();
                if (integer != null && (integer.Min > int.MinValue || integer.Max < int.MaxValue))
                    return (integer.Min, integer.Max);
            }

            return (null, null);
        }

        internal static PropertyInfo? ScanForProperty(this object? entity, string path, out object? pathEntity)
        {
            pathEntity = null;
            if (entity == null || string.IsNullOrEmpty(path))
                return null;

            var parts = path.Split('.');
            PropertyInfo? propInfo = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                propInfo = entity.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo == null)
                    return null;

                if (i < parts.Length - 1)
                {
                    entity = propInfo.GetValue(entity, null);
                    if (entity == null)
                        return null;
                }
            }

            if (propInfo != null)
            {
                pathEntity = entity;
                return propInfo;
            }

            pathEntity = null;
            return null;
        }

        internal static PropertyInfo? ScanForModdableProperty(this object? entity, string path, out object? pathEntity)
        {
            var propInfo = ScanForProperty(entity, path, out pathEntity);
            if (!propInfo.IsModdableProperty())
            {
                pathEntity = null;
                return null;
            }

            return propInfo;
        }

        private static bool IsModdableProperty(this PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null || !RpgPropertyTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            if (propertyInfo.PropertyType == typeof(string))
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily);
        }

        internal static MethodInfo ScanForMethod(string staticMethod)
        {
            var parts = staticMethod.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                throw new ArgumentException($"Invalid Class.Method format in {staticMethod}");

            var className = parts[0];
            var methodName = parts[1];

            Type? type = ScanForType(className);
            if (type == null)
                throw new ArgumentException($"Failed to find class for static method {staticMethod}");

            var methodInfo = ScanForMethod(type!, methodName);
            if (!methodInfo.IsStatic)
                throw new ArgumentException($"Method {staticMethod} is not static");

            return methodInfo;
        }

        internal static MethodInfo ScanForMethod(Type type, string methodName)
        {
            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (methodInfo == null)
                throw new InvalidOperationException($"{methodName}() method not found on {type.Name} class");

            return methodInfo;
        }

        internal static MethodInfo ScanForMethod<TReturn>(Type type, string methodName)
        {
            var methodInfo = ScanForMethod(type, methodName);

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(TReturn)))
                throw new InvalidOperationException($"{methodName}() method on {type.Name} class does not have return type {nameof(TReturn)}");

            return methodInfo;
        }

        public static IEnumerable<Type> ScanForTypes<T>()
        {
            try
            {
                var assemblies = GetScanAssemblies();
                return ScanForTypes<T>(assemblies);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        public static IEnumerable<Type> ScanForSubTypes(Type type)
        {
            try
            {
                var assemblies = GetScanAssemblies();
                var res = new List<Type>();

                foreach (var assembly in assemblies)
                {
                    var assemblyTypes = assembly.DefinedTypes
                        .Where(x => x.IsSubclassOf(type))
                        .Select(x => x.AsType());

                    res.AddRange(assemblyTypes);
                }

                return res;
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        internal static IEnumerable<Type> ScanForTypes<T>(IEnumerable<Assembly> assemblies)
        {
            var res = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var assemblyTypes = assembly.DefinedTypes
                    .Where(x => IsValidScanType<T>(x))
                    .Select(x => x.AsType());

                res.AddRange(assemblyTypes);
            }

            return res;
        }

        internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types
                    .Where(t => t != null)
                    .Cast<Type>();
            }
        }

        internal static List<Assembly> GetScanAssemblies()
        {
            if (_scanAssemblies.Any())
            {
                if (!_scanAssemblies.Contains(typeof(IMetaSystem).Assembly))
                    _scanAssemblies.Add(typeof(IMetaSystem).Assembly);

                return _scanAssemblies;
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !ExcludeAssembliesWith.Any(n => x.FullName?.Contains(n) ?? false))
                .ToList();
        }


        private static bool IsValidScanType<T>(TypeInfo typeInfo)
        {
            if (typeInfo.IsAbstract)
                return false;

            var baseTypeInfo = typeof(T).GetTypeInfo();

            if (baseTypeInfo.IsClass)
                return typeInfo.IsSubclassOf(baseTypeInfo.AsType());

            if (baseTypeInfo.IsInterface)
                return typeInfo.ImplementedInterfaces.Contains(typeof(T));

            return false;
        }
    }
}
