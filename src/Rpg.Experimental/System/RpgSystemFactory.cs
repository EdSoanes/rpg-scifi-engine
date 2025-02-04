using Rpg.Experimental.Reflection;
using Rpg.Experimental.System.Props;
using System.Data;
using System.Reflection;

namespace Rpg.Experimental.System
{
    public static class RpgSystemFactory
    {
        public static RpgSystem Build()
        {
            var system = DiscoverMetaSystems().FirstOrDefault();
            if (system == null)
                throw new InvalidOperationException("No IMetaSystem types found");

            return Build(system);
        }

        public static RpgSystem Build(IRpgSystem system)
        {
            var metaGraph = new RpgSystem();
            var systemAssemblies = DiscoverSystemAssemblies(system);
            var objectTypes = RpgTypeUtilities.ForTypes<RpgObject>(systemAssemblies);

            metaGraph.Namespaces = Namespaces(objectTypes);
            metaGraph.PropertyAttributes = RpgTypeUtilities.ForTypes<RpgPropertyAttribute>(systemAssemblies)
                .Select(x => (RpgPropertyAttribute)Activator.CreateInstance(x)!)
                .ToArray();

            metaGraph.Actions = RpgTypeUtilities.ForTypes<RpgAction>(systemAssemblies)
                .Select(x => CreateAction(x))
                .ToArray();

            metaGraph.States = RpgTypeUtilities.ForTypes<RpgState>(systemAssemblies)
                .Select(x => new MetaState(x))
                .ToArray();

            metaGraph.Objects = objectTypes
                .Where(x => !x.IsAssignableTo(typeof(RpgAction)))
                .Select(x => CreateObject(x, metaGraph.Actions, metaGraph.States))
                .ToArray();

            return metaGraph;
        }

        public static MetaObject CreateObject(Type type, MetaAction[] actions, MetaState[] states)
        {
            var obj = new MetaObject
            {
                Archetype = type.Name,
                Archetypes = RpgTypeUtilities.GetArchetypes(type)
            };

            var props = CreateProperties(type);
            obj.Properties.AddRange(props);
            obj.AllowedActions.AddRange(actions.Where(x => obj.Archetypes.Contains(x.OwnerArchetype)));
            obj.AllowedStates.AddRange(states.Where(x => obj.Archetypes.Contains(x.Archetype)));

            return obj;
        }

        private static string[] Namespaces(IEnumerable<Type> objectTypes)
        {
            var ns = objectTypes
                .Select(x => x.Namespace)
                .Where(x => x != null)
                .Cast<string>()
                .Distinct()
                .ToList();

            var thisNs = typeof(RpgObject).Namespace;
            if (thisNs != null)
                ns.Add(thisNs);

            return ns.ToArray();
        }

        public static List<MetaProperty> CreateProperties(Type type)
        {
            var metaProps = new List<MetaProperty>();
            var propStack = new Stack<string>();
            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var metaProp = CreateProperty(propStack, propInfo);
                if (metaProp != null)
                    metaProps.Add(metaProp);
            }

            return metaProps;
        }

        private static MetaProperty? CreateProperty(Stack<string> propStack, PropertyInfo propInfo)
        {
            var propAttr = GetRpgPropertyAttribute(propInfo);
            if (propAttr == null)
                return null;

            var metaProp = new MetaProperty
            {
                Path = propStack.ToList(),
                Prop = propInfo.Name,
                PropertyType = propAttr.PropertyType,
                Editor = propAttr.Editor,
                DisplayName = propAttr.DisplayName ?? string.Join(" ", [.. propStack, propInfo.Name]),
                Tab = propAttr.Tab,
                Group = propAttr.Group,
                IsNullable = propAttr.IsNullable,
                Attributes = propAttr.GetValues()
            };

            return metaProp;
        }

        private static MetaAction? CreateAction(Type actionType)
        {
            var action = (RpgAction)Activator.CreateInstance(actionType, true)!;
            var metaAction = new MetaAction
            {
                Name = actionType.Name,
                OwnerArchetype = action.OwnerArchetype,
                Cost = action.CostMethod,
                Perform = action.PerformMethod,
                Outcome = action.OutcomeMethod,
            };

            return metaAction;
        }

        private static RpgPropertyAttribute? GetRpgPropertyAttribute(PropertyInfo propertyInfo)
        {
            var isNullableValueType = RpgTypeUtilities.PropertyIsNullableValueType(propertyInfo.PropertyType);

            var propAttr = propertyInfo.GetCustomAttributes(true)
                .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(RpgPropertyAttribute))) as RpgPropertyAttribute;

            if (propAttr == null)
            {
                var propType = isNullableValueType
                    ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)!
                    : propertyInfo.PropertyType;

                propAttr = propType.Name switch
                {
                    nameof(Int32) => new IntegerAttribute { IsNullable = isNullableValueType },
                    nameof(Dice) => new DiceAttribute { IsNullable = isNullableValueType },
                    nameof(String) => new TextAttribute(),
                    _ => null
                };
            }

            if (propAttr == null)
            {
                if (RpgTypeUtilities.PropertyOfType(propertyInfo.PropertyType, typeof(RpgObject)))
                    propAttr = new ChildAttribute();

                else if (RpgTypeUtilities.PropertyIsEnumerableOfType(propertyInfo.PropertyType, typeof(RpgObject)))
                    propAttr = new ChildrenAttribute();
            }

            if (propAttr != null && isNullableValueType)
                propAttr.IsNullable = isNullableValueType;

            return propAttr;
        }

        private static Assembly[] DiscoverSystemAssemblies(IRpgSystem system)
        {
            var assemblies = new List<Assembly>() { system.GetType().Assembly };
            var libAssembly = typeof(RpgSystemFactory).Assembly;
            if (!assemblies.Contains(libAssembly))
                assemblies.Add(libAssembly);

            return assemblies.ToArray();
        }

        public static IRpgSystem[] DiscoverMetaSystems()
        {
            var systems = new List<IRpgSystem>();

            var types = RpgTypeUtilities.ForTypes<IRpgSystem>();
            foreach (var type in types)
            {
                var system = Activator.CreateInstance(type) as IRpgSystem;
                if (system == null)
                    throw new InvalidOperationException($"Could not create instance of IMetaSystem {type.Name}");

                systems.Add(system);
            }

            return systems.ToArray();
        }
    }
}
