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

        private static string[] RpgObjectBaseTypes =
        [
            nameof(RpgObject),
            nameof(RpgEntity),
            nameof(RpgComponent)
        ];

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

            foreach (var metaObject in _metaObjects.Values)
                metaObject.ParentTo = metaObject.Properties
                    .Where(x => _metaObjects.Keys.Contains(x.ReturnType))
                    .Select(x => x.ReturnType)
                    .Distinct()
                    .ToArray();

            var system = Activator.CreateInstance(sysType) as IMetaSystem;
            if (system == null)
                throw new InvalidOperationException($"Could not create instance of IMetaSystem {sysType.Name}");

            system.Objects = GetOrderedArchetypes()
                .Select(x => _metaObjects[x])
                .ToArray();

            system.PropUIAttributes = _propUIAttributes.ToArray();

            return system;
        }

        private void LoadMetaObjects()
        {
            var templateTypes = FindDerivedTypes<IRpgEntityTemplate>()
                .Union(FindDerivedTypes<IRpgComponentTemplate>());

            var objectTypes = FindDerivedTypes<RpgObject>()
                .Union(templateTypes)
                .Where(x => !RpgObjectBaseTypes.Contains(x.Name));

            foreach (var objectType in objectTypes)
            {
                var objectTemplateType = GetTemplate(objectType, templateTypes);
                var obj = GetObject(objectType, objectTemplateType);
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

        public MetaObject? GetObject(Type type, Type? objectTemplateType)
        {
            var properties = type.GetModdableProperties()
                .Select(x => new MetaProperty(x))
                .ToArray();

            var metaObject = new MetaObject()
            {
                Archetype = type.Name,
                ObjectType = type.GetObjectType(),
                TemplateType = objectTemplateType?.Name,
                Properties = GetProperties(type),
                States = GetStates(type),
                Actions = GetActions(type)
            };

            return metaObject;
        }

        private Type? GetTemplate(Type type, IEnumerable<Type> templateTypes)
        {
            if (!templateTypes.Any())
                return null;

            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                var templateParam = constructor.GetParameters().FirstOrDefault(x => templateTypes.Contains(x.ParameterType));
                if (templateParam != null)
                    return templateParam.ParameterType;
            }

            return null;
        }

        public MetaProperty[] GetProperties(Type type)
        {
            var properties = type.GetModdableProperties()
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
