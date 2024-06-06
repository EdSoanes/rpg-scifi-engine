using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;
using System.Diagnostics;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaGraph
    {
        private static List<Assembly> ScannableAssemblies = new List<Assembly>();
        public static List<string> ScannableNamespaces = new List<string>();

        private static Type[] ModdablePropertyTypes = new Type[]
        {
            typeof(int),
            typeof(Dice)
        };

        private static Type[] ExcludedPropertyTypes = new Type[]
        {
            typeof(RpgActionStore),
            typeof(ModSetStore),
            typeof(ModStateStore),
            typeof(PropStore),
            typeof(RpgGraph),
            typeof(RpgAction),
            typeof(ModState),
            typeof(ModSet),
            typeof(Mod),
            typeof(string),
            typeof(DateTime),
            typeof(Guid)
        };

        private static string[] RpgObjectBaseTypes = new string[]
        {
            nameof(RpgEntity),
            nameof(RpgEntityComponent)
        };

        private Dictionary<string, MetaObject> _metaObjects = new Dictionary<string, MetaObject>();

        public MetaGraph()
        {
            RegisterAssembly(GetType().Assembly);
            RegisterAssembly(Assembly.GetCallingAssembly());
        }

        public static void RegisterAssembly(Assembly assembly)
        {
            if (!ScannableAssemblies.Contains(assembly))
            {
                ScannableAssemblies.Add(assembly);

                var namespaces = assembly.GetTypes()
                    .Select(t => t.Namespace)
                    .Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("System.") && !x.StartsWith("Microsoft."))
                    .Distinct()
                    .Cast<string>();

                ScannableNamespaces.AddRange(namespaces);
                ScannableNamespaces = ScannableNamespaces.Distinct().ToList();
            }
        }

        public MetaObject[] GetObjects()
        {
            foreach (var assembly in ScannableAssemblies)
            {
                var types = FindDerivedTypes(assembly, typeof(RpgObject));
                foreach (var type in types)
                {
                    var obj = GetObject(type);
                    if (obj != null)
                    {
                        if (!_metaObjects.ContainsKey(obj.Archetype))
                            _metaObjects.Add(obj.Archetype, obj);
                        else
                            Debug.WriteLine(obj.Archetype);
                    }
                }
            }

            foreach (var metaObj in _metaObjects.Values)
                foreach (var prop in metaObj.Properties)
                {
                    prop.IsComponent = _metaObjects.ContainsKey(prop.ReturnType) && _metaObjects[prop.ReturnType].IsComponent;
                }
            return _metaObjects.Values.ToArray();
        }

        public MetaObject? GetObject(Type type)
        {
            if (RpgObjectBaseTypes.Contains(type.Name))
                return null;

            var properties = GetModdableProperties(type)
                .Select(x => new MetaProperty(x))
                .ToArray();

            var baseTypes = GetBaseTypes(type);
            var metaObject = new MetaObject()
            {
                Archetype = type.Name,
                BaseType = baseTypes.First(x => RpgObjectBaseTypes.Contains(x)),
                BaseTypes = baseTypes.Where(x => !RpgObjectBaseTypes.Contains(x)).ToArray(),
                Properties = GetProperties(type),
                States = GetStates(type),
                Actions = GetActions(type),
                IsComponent = baseTypes.Contains(nameof(RpgEntityComponent))
            };

            return metaObject;
        }

        private string[] GetBaseTypes(Type type)
        {
            var res = new List<string>();
            var t = type;
            while (t != null & t != typeof(RpgObject))
            {
                res.Add(t!.Name);
                t = t.BaseType;
            }

            res.Reverse();
            return res.ToArray();
        }

        public MetaProperty[] GetProperties(Type type)
        {
            var properties = GetModdableProperties(type)
                .Select(x => new MetaProperty(x))
                .ToArray();

            return properties;
        }

        public MetaAction[] GetActions(Type type)
        {
            var methods = type.GetMethods()
                .Where(x => x.IsActionMethod());

            var res = new List<MetaAction>();
            foreach (var method in methods)
            {
                var cmdAttr = method.GetCustomAttributes<RpgActionAttribute>(true).FirstOrDefault();
                var attrs = method.GetCustomAttributes<RpgActionArgAttribute>(true);
                var args = method.GetParameters()
                    .Select(x => new MetaActionArg(x, attrs?.FirstOrDefault(a => a.Prop == x.Name)))
                    .Where(x => x != null)
                    .Cast<MetaActionArg>()
                    .ToArray();

                res.Add(new MetaAction(method.Name, cmdAttr!, args));
            }

            return res.ToArray();
        }

        public MetaState[] GetStates(Type type)
        {
            var methods = type.GetMethods()
                .Where(x => IsStateMethod(x));

            var res = new List<MetaState>();
            foreach (var method in methods)
            {
                var stateAttr = method.GetCustomAttributes<ModStateAttribute>(true).First();
                var modState = new MetaState(stateAttr.Name ?? method.Name, stateAttr.ActiveWhen);

                res.Add(modState);
            }

            return res.ToArray();
        }

        private bool IsStateMethod(MethodInfo method)
        {
            if (method.GetCustomAttributes<ModStateAttribute>().Any())
            {
                var args = method.GetParameters();
                if (args.Count() == 1 && args.First().ParameterType.IsAssignableTo(typeof(ModSet)))
                    return true;
            }

            return false;
        }

        private IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            TypeInfo baseTypeInfo = baseType.GetTypeInfo();
            bool isClass = baseTypeInfo.IsClass, isInterface = baseTypeInfo.IsInterface;

            return
                from type in assembly.DefinedTypes
                where isClass ? type.IsSubclassOf(baseType) :
                      isInterface ? type.ImplementedInterfaces.Contains(baseTypeInfo.AsType()) : false
                select type.AsType();
        }

        private PropertyInfo[] GetModdableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => IsModdableProperty(x) || IsRpgObjectProperty(x))
                .ToArray();
        }

        private bool IsModdableProperty(PropertyInfo propertyInfo)
        {
            if (!InScannableNamespace(propertyInfo.DeclaringType))
                return false;

            if (!ModdablePropertyTypes.Any(x => x == propertyInfo.PropertyType))
                return false;

            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily);
        }

        private bool IsRpgObjectProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily)
                && (propertyInfo.PropertyType.IsAssignableTo(typeof(RpgObject)) || propertyInfo.PropertyType.IsAssignableTo(typeof(IEnumerable<RpgObject>)));
        }

        private static bool InScannableNamespace(Type? type)
        {
            return type == null
                ? false
                : ScannableNamespaces.Any(x => type.Namespace == x);
        }
    }
}
