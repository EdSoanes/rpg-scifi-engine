using Rpg.ModObjects.Values;
using System.Collections;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaGraph
    {
        private static readonly Type[] BaseTypes =
        [
            typeof(int),
            typeof(string),
            typeof(Dice)
        ];

        public IMetaSystem Build()
        {
            var sysType = DiscoverMetaSystems().FirstOrDefault();
            if (sysType == null)
                throw new InvalidOperationException("No IMetaSystem types found");

            var objectTypes = GetMetaTypes<RpgEntity>();
            var res = objectTypes
                .Select(Object)
                .ToArray();

            var system = Activator.CreateInstance(sysType) as IMetaSystem;
            if (system == null)
                throw new InvalidOperationException($"Could not create instance of IMetaSystem {sysType.Name}");

            system.Objects = res;
            return system;
        }

        public MetaObj Object(Type type)
        {
            var obj = new MetaObj();
            obj.Archetype = type.Name;
            obj.Props = Props(type);
            return obj;
        }

        public List<MetaProp> Props(Type type)
        {
            var metaProps = new List<MetaProp>();
            var propStack = new Stack<string>();
            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                Prop(metaProps, propStack, propInfo, null, null);

            return metaProps;
        }

        private void Prop(List<MetaProp> metaProps, Stack<string> propStack, PropertyInfo propInfo, string? tab, string? group)
        {
            var propUI = propInfo.GetPropUI();
            if (!(propUI?.Ignore ?? false))
            {
                tab = !string.IsNullOrEmpty(propUI?.Tab) ? propUI.Tab : tab;
                group = !string.IsNullOrEmpty(propUI?.Group) ? propUI.Group : group;

                if (BaseTypes.Contains(propInfo.PropertyType))
                {
                    var metaProp = new MetaProp();

                    metaProp.Prop = propInfo.Name;
                    metaProp.Type = propUI?.GetType().Name.Replace("UIAttribute", "") ?? propInfo.PropertyType.Name;
                    metaProp.Path = propStack.ToList();
                    metaProp.Path.Reverse();

                    metaProp.Tab = tab;
                    metaProp.Group = group;
                    metaProp.DisplayName = !string.IsNullOrEmpty(propUI?.EditorName) ? propUI.EditorName : propInfo.Name;

                    SetPropUIValues(metaProp, propUI);
                    metaProps.Add(metaProp);
                }
                else if (propInfo.PropertyType.IsClass && !propInfo.PropertyType.IsAssignableTo(typeof(IEnumerable)))
                {
                    propStack.Push(propInfo.Name);

                    foreach (var childPropInfo in propInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        Prop(metaProps, propStack, childPropInfo, tab, group);

                    propStack.Pop();
                }
            }
        }

        public void SetPropUIValues(MetaProp metaProp, MetaPropUIAttribute? attr)
        {
            metaProp.Values.Clear();

            if (attr != null)
            {
                foreach (var propInfo in attr.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (BaseTypes.Contains(propInfo.PropertyType) || propInfo.PropertyType == typeof(bool))
                        metaProp.Values.Add(propInfo.Name, propInfo.GetValue(attr));
                }
            }
        }

        #region Discovery

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

        private IEnumerable<Type> GetMetaTypes<T>()
        {
            var res = new List<Type>();

            TypeInfo baseTypeInfo = typeof(T).GetTypeInfo();
            bool isClass = baseTypeInfo.IsClass, isInterface = baseTypeInfo.IsInterface;

            foreach (var assembly in GetScanAssemblies())
            {
                var assemblyTypes =
                    from type in assembly.DefinedTypes
                    where isClass ? type.IsSubclassOf(typeof(T)) :
                          isInterface ? type.ImplementedInterfaces.Contains(baseTypeInfo.AsType()) : false
                    select type.AsType();

                res.AddRange(assemblyTypes);
            }

            return res;
        }

        private List<Assembly> GetScanAssemblies()
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

        private List<Type> DiscoverMetaSystems()
        {
            var types = new List<Type>();

            foreach (var assembly in GetScanAssemblies())
            {
                var assTypes = GetLoadableTypes(assembly)
                    .Where(x => x != typeof(IMetaSystem) && x.IsAssignableTo(typeof(IMetaSystem)))
                    .ToList();

                if (assTypes != null)
                    types.AddRange(assTypes);
            }

            return types;
        }

        private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
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

        #endregion
    }
}
