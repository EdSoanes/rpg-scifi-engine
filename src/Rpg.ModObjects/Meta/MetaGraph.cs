using System.Collections;
using System.Data;
using System.Reflection;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Meta
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

            var propUIs = RpgReflection.ScanForTypes<MetaPropAttribute>(systemAssemblies)
                .Select(x => (MetaPropAttribute)Activator.CreateInstance(x)!)
                .ToArray();

            var actions = RpgReflection.ScanForTypes<Actions.Action>(systemAssemblies)
                .Select(x => new MetaAction(x))
                .ToArray();

            var states = RpgReflection.ScanForTypes<States.State>(systemAssemblies)
                .Select(x => new MetaState(x))
                .ToArray();

            var objectTypes = RpgReflection.ScanForTypes<RpgEntity>(systemAssemblies);
            var res = objectTypes
                .Select(x => Object(x, actions, states))
                .ToArray();

            system.Objects = res;
            system.Actions = actions;
            system.States = states;
            system.PropUIs = propUIs;

            return system;
        }

        public MetaObj Object(Type type, MetaAction[] actions, MetaState[] states)
        {
            var obj = new MetaObj(type);
            FillMetaObject(obj, type, actions, states);

            return obj;
        }

        private void FillMetaObject(MetaObj obj, Type type, MetaAction[] actions, MetaState[] states)
        {
            var props = Props(type);
            obj.Props.AddRange(Props(type));
            obj.Containers.AddRange(Containers(type));
            obj.AllowedActions.AddRange(actions.Where(x => obj.Archetypes.Contains(x.OwnerArchetype)));
            obj.AllowedStates.AddRange(states.Where(x => obj.Archetypes.Contains(x.Archetype)));
        }

        public List<MetaContainer> Containers(Type type)
        {
            var containers = new List<MetaContainer>();
            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.PropertyType.IsAssignableTo(typeof(RpgContainer))))
            {
                var container = new MetaContainer(propInfo.PropertyType, propInfo.Name);
                FillMetaObject(container, propInfo.PropertyType, Array.Empty<MetaAction>(), Array.Empty<MetaState>());
                containers.Add(container);
            }

            return containers;
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
            if (propUI == null)
                return;

            tab = (!string.IsNullOrEmpty(propUI.Tab) ? propUI.Tab : tab) ?? string.Empty;
            group = (!string.IsNullOrEmpty(propUI.Group) ? propUI.Group : group) ?? string.Empty;

            var metaProp = new MetaProp(propInfo, propUI, propStack, tab, group);
            metaProps.Add(metaProp);

            if (propUI is ComponentAttribute)
            {
                propStack.Push(propInfo.Name);

                foreach (var childPropInfo in propInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    Prop(metaProps, propStack, childPropInfo, tab, group);

                propStack.Pop();
            }
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
            var types = new List<Type>();

            foreach (var assembly in RpgReflection.GetScanAssemblies())
            {
                var assTypes = RpgReflection.GetLoadableTypes(assembly)
                    .Where(x => x != typeof(IMetaSystem) && x.IsAssignableTo(typeof(IMetaSystem)))
                    .ToList();

                if (assTypes != null)
                    types.AddRange(assTypes);
            }

            var systems = new List<IMetaSystem>();
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
