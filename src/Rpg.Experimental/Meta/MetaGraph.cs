using Rpg.Experimental.Activities;
using Rpg.Experimental.Meta.Props;
using Rpg.Experimental.Reflection;
using System.Data;
using System.Reflection;

namespace Rpg.Experimental.Meta
{
    public class MetaGraph
    {
        public IMetaSystem Build()
        {
            var system = DiscoverMetaSystems().FirstOrDefault();
            if (system == null)
                throw new InvalidOperationException("No IMetaSystem types found");

            return Build(system);
        }

        public IMetaSystem Build(IMetaSystem system)
        {
            var systemAssemblies = DiscoverSystemAssemblies(system);

            var propAttrs = RpgTypeUtilities.ForTypes<RpgPropertyAttribute>(systemAssemblies)
                .Select(x => (RpgPropertyAttribute)Activator.CreateInstance(x)!)
                .ToArray();

            var actions = RpgTypeUtilities.ForTypes<RpgAction>(systemAssemblies)
                .Select(x => CreateAction(x))
                .ToArray();

            var states = RpgTypeUtilities.ForTypes<States.State>(systemAssemblies)
                .Select(x => new MetaState(x))
                .ToArray();

            var objectTypes = RpgTypeUtilities.ForTypes<RpgObject>(systemAssemblies);
            var res = objectTypes
                .Where(x => !x.IsAssignableTo(typeof(RpgAction)))
                .Select(x => CreateObject(x, actions, states))
                .ToArray();

            system.Objects = res;
            system.Actions = actions;
            system.States = states;
            system.PropertyAttributes = propAttrs.Select(x => x.GetValues()).ToArray();
            system.Namespaces = Namespaces(objectTypes);
           
            return system;
        }

        public MetaObject CreateObject(Type type, MetaAction[] actions, MetaState[] states)
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

        private string[] Namespaces(IEnumerable<Type> objectTypes)
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

        public List<MetaProperty> CreateProperties(Type type)
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

        private MetaProperty? CreateProperty(Stack<string> propStack, PropertyInfo propInfo)
        {
            var propAttr = GetRpgPropertyAttribute(propInfo);
            if (propAttr == null)
                return null;

            var metaProp = new MetaProperty
            {
                Path = propStack.ToList(),
                Prop = propInfo.Name,
                Editor = propAttr.Editor,
                DisplayName = propAttr.DisplayName ?? string.Join(" ", [.. propStack, propInfo.Name]),
                Tab = propAttr.Tab,
                Group = propAttr.Group,
                Properties = propAttr.GetValues()
            };

            return metaProp;
        }

        private MetaAction CreateAction(Type actionType)
        {
            var action = (RpgAction)Activator.CreateInstance(actionType, true)!;
            var metaAction = new MetaAction
            {
                Name = actionType.Name,
                //OwnerArchetype =
                Cost = action.CostMethod,
                Perform = action.PerformMethod,
                Outcome = action.OutcomeMethod,
            };

            return metaAction;
        }

        private RpgPropertyAttribute? GetRpgPropertyAttribute(PropertyInfo propertyInfo)
        {
            var propAttr = propertyInfo.GetCustomAttributes(true)
                .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(RpgPropertyAttribute))) as RpgPropertyAttribute;

            if (propAttr == null)
            {
                propAttr = propertyInfo.PropertyType.Name switch
                {
                    nameof(Int32) => new IntegerAttribute { Editor = EditorType.None },
                    nameof(Dice) => new DiceAttribute { Editor = EditorType.None },
                    nameof(String) => new TextAttribute { Editor = EditorType.None },
                    _ => null
                };
            }

            if (propAttr == null && propertyInfo.PropertyType.IsAssignableTo(typeof(RpgObject)))
                propAttr = new ComponentAttribute { Editor = EditorType.None };

            return propAttr;
        }

        private Assembly[] DiscoverSystemAssemblies(IMetaSystem system)
        {
            var assemblies = new List<Assembly>() { system.GetType().Assembly };
            var libAssembly = GetType().Assembly;
            if (!assemblies.Contains(libAssembly))
                assemblies.Add(libAssembly);

            return assemblies.ToArray();
        }

        public static IMetaSystem[] DiscoverMetaSystems()
        {
            var systems = new List<IMetaSystem>();

            var types = RpgTypeUtilities.ForTypes<IMetaSystem>();
            foreach (var type in types)
            {
                var system = Activator.CreateInstance(type) as IMetaSystem;
                if (system == null)
                    throw new InvalidOperationException($"Could not create instance of IMetaSystem {type.Name}");

                systems.Add(system);
            }

            return systems.ToArray();
        }
    }
}
