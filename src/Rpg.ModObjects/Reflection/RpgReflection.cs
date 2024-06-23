using Rpg.ModObjects.Meta;
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

        internal static Type? ScanForType(string qualifiedTypeName)
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

        internal static PropertyInfo? ScanForModdableProperty(this object context, string prop)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == prop && x.IsModdableProperty());
        }

        private static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            if (!RpgPropertyTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            if (propertyInfo.PropertyType == typeof(string))
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

        internal static IEnumerable<Type> ScanForTypes<T>()
        {
            var res = new List<Type>();

            foreach (var assembly in GetScanAssemblies())
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
