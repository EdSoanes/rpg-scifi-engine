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
            nameof(RpgComponent)
        };

        private Dictionary<string, MetaObject> _metaObjects = new Dictionary<string, MetaObject>();
        private List<MetaPropUIAttribute> _propUIAttributes = new List<MetaPropUIAttribute>();

        private static List<Assembly> _scanAssemblies = new List<Assembly>();
        public static void RegisterAssembly(Assembly assembly)
        {
            if (!_scanAssemblies.Contains(assembly))
                _scanAssemblies.Add(assembly);
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

        public MetaGraph()
        {
        }

        public IMetaSystem Build()
        {
            var sysType = DiscoverMetaSystems().FirstOrDefault();
            if (sysType == null)
                throw new InvalidOperationException("No IMetaSystem types found");

            LoadPropTypes();
            LoadMetaObjects();
            InitializeMetaObjects();

            var system = Activator.CreateInstance(sysType) as IMetaSystem;
            if (system == null)
                throw new InvalidOperationException($"Could not create instance of IMetaSystem {sysType.Name}");

            system.Objects = _metaObjects.Values.ToArray();
            system.PropUIAttributes = _propUIAttributes.ToArray();

            return system;
        }

        private void InitializeMetaObjects()
        {
            foreach (var metaObject in _metaObjects.Values)
                metaObject.ParentTo = metaObject.Properties
                    .Where(x => _metaObjects.Keys.Contains(x.ReturnType))
                    .Select(x => x.ReturnType)
                    .Distinct()
                    .ToArray();

            var archetypes = GetOrderedArchetypes();
            archetypes.Reverse();

            foreach (var archetype in archetypes)
            {
                var obj = _metaObjects[archetype];
                foreach (var prop in obj.Properties)
                {
                    if (_metaObjects.ContainsKey(prop.ReturnType))
                    {
                        var propObj = _metaObjects[prop.ReturnType];
                        propObj.Inherit(prop);
                    }
                }
            }
        }

        private void LoadMetaObjects()
        {
            var templateTypes = FindDerivedTypes<IRpgEntityTemplate>()
                .Union(FindDerivedTypes<IRpgComponentTemplate>());

            var objectTypes = FindDerivedTypes<RpgObject>();
            foreach (var objectType in objectTypes)
            {
                var obj = GetObject(objectType, templateTypes);
                if (obj != null)
                {
                    if (!_metaObjects.ContainsKey(obj.Archetype))
                        _metaObjects.Add(obj.Archetype, obj);
                    else
                        Debug.WriteLine(obj.Archetype);
                }
            }
        }

        private void LoadPropTypes()
        {
            _propUIAttributes = FindDerivedTypes<MetaPropUIAttribute>()
                .GroupBy(x => x.Name)
                .Select(x => Activator.CreateInstance(x.First()) as MetaPropUIAttribute)
                .Where(x => x != null)
                .Cast<MetaPropUIAttribute>()
                .ToList();
        }

        private List<string> GetOrderedArchetypes()
        {
            var archetypes = new List<string>();

            foreach (var metaObject in _metaObjects.Values)
                Merge(archetypes, metaObject);

            return archetypes;
        }

        private void Merge(List<string> target, MetaObject metaObject)
        {
            var archetypes = new List<string>() { metaObject.Archetype };
            archetypes.AddRange(metaObject.ParentTo);

            Merge(target, archetypes);
        }


        private void Merge(List<string> target, IEnumerable<string> source)
        {
            var intersect = target.Intersect(source);
            if (!intersect.Any())
            {
                target.AddRange(source);
                return;
            }

            bool intersected = false;
            foreach (var srcArchetype in source)
            {
                if (target.Contains(srcArchetype))
                {
                    intersected = true;
                    continue;
                }

                var idxs = intersect.Select(x => target.IndexOf(x)).OrderBy(x => x);
                var i = !intersected
                    ? idxs.First()
                    : idxs.Last() + 1;

                if (i >= target.Count)
                    target.Add(srcArchetype);
                else
                    target.Insert(i, srcArchetype);
            }
        }

        public MetaObject? GetObject(Type type, IEnumerable<Type> templateTypes)
        {
            var templateType = GetTemplate(type, templateTypes);
            if (templateType != null)
            {
                var properties = GetModdableProperties(templateType)
                    .Select(x => new MetaProperty(x))
                    .ToArray();

                var baseTypes = GetBaseTypes(type);
                var metaObject = new MetaObject()
                {
                    Archetype = type.Name,
                    TemplateType = templateType.Name,
                    BaseType = baseTypes.First(x => RpgObjectBaseTypes.Contains(x)),
                    BaseTypes = baseTypes.Where(x => !RpgObjectBaseTypes.Contains(x) && x != type.Name).ToArray(),
                    Properties = GetProperties(templateType),
                    States = GetStates(type),
                    Actions = GetActions(type)
                };

                return metaObject;
            }


            return null;
        }

        private Type? GetTemplate(Type type, IEnumerable<Type> templateTypes)
        {
            if (!templateTypes.Any())
                return type;

            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                var templateParam = constructor.GetParameters().FirstOrDefault(x => templateTypes.Contains(x.ParameterType));
                if (templateParam != null)
                    return templateParam.ParameterType;
            }

            return null;
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

        private IEnumerable<Type> FindDerivedTypes<T>()
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

        private PropertyInfo[] GetModdableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => IsModdableProperty(x) || IsRpgObjectProperty(x) || IsRpgTemplateProperty(x))
                .ToArray();
        }

        private bool IsModdableProperty(PropertyInfo propertyInfo)
        {
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

        private bool IsRpgTemplateProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod != null
                && (propertyInfo.GetMethod.IsPublic || propertyInfo.GetMethod.IsFamily)
                && (propertyInfo.PropertyType.IsAssignableTo(typeof(IRpgEntityTemplate)) || propertyInfo.PropertyType.IsAssignableTo(typeof(IRpgComponentTemplate)));
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
    }
}
