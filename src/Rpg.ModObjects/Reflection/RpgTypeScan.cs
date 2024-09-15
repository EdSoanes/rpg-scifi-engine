using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public static class RpgTypeScan
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

        public static bool TypeNotExcluded(Type type)
            => !ExcludeAssembliesWith.Any(x => type.Name.StartsWith(x));

        public static void RegisterAssembly(Assembly assembly)
        {
            if (!_scanAssemblies.Contains(assembly))
                _scanAssemblies.Add(assembly);
        }

        public static Type? ForTypeByName(string name)
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

        public static Type? ForType(string qualifiedTypeName)
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

        internal static MethodInfo ForMethod(string staticMethod)
        {
            var parts = staticMethod.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                throw new ArgumentException($"Invalid Class.Method format in {staticMethod}");

            var className = parts[0];
            var methodName = parts[1];

            Type? type = ForType(className);
            if (type == null)
                throw new ArgumentException($"Failed to find class for static method {staticMethod}");

            var methodInfo = ForMethod(type!, methodName);
            if (!methodInfo.IsStatic)
                throw new ArgumentException($"Method {staticMethod} is not static");

            return methodInfo;
        }

        internal static MethodInfo ForMethod(Type type, string methodName)
        {
            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (methodInfo == null)
                throw new InvalidOperationException($"{methodName}() method not found on {type.Name} class");

            return methodInfo;
        }

        internal static MethodInfo ForMethod<TReturn>(Type type, string methodName)
        {
            var methodInfo = ForMethod(type, methodName);

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(TReturn)))
                throw new InvalidOperationException($"{methodName}() method on {type.Name} class does not have return type {nameof(TReturn)}");

            return methodInfo;
        }

        public static IEnumerable<Type> ForTypes<T>()
        {
            try
            {
                var assemblies = GetScanAssemblies();
                return ForTypes<T>(assemblies);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        public static IEnumerable<Type> ForSubTypes(Type type)
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

        internal static IEnumerable<Type> ForTypes<T>(IEnumerable<Assembly> assemblies)
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
