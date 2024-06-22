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
            foreach (var assembly in GetScanAssemblies())
            {
                var type = assembly.GetType(qualifiedTypeName);
                if (type != null)
                    return type;
            }

            return null;
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
