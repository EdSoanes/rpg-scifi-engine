using System.Collections;
using System.Reflection;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaGraph
    {

        public IMetaSystem Build()
        {
            var sysType = DiscoverMetaSystems().FirstOrDefault();
            if (sysType == null)
                throw new InvalidOperationException("No IMetaSystem types found");

            var objectTypes = RpgReflection.ScanForTypes<RpgEntity>();
            var res = objectTypes
                .Select(Object)
                .ToArray();

            var propUIs = RpgReflection.ScanForTypes<MetaPropUIAttribute>()
                .Select(x => (MetaPropUIAttribute)Activator.CreateInstance(x)!)
                .ToArray();

            var actions = RpgReflection.ScanForTypes<Actions.Action>()
                .Select(x => (Actions.Action)Activator.CreateInstance(x)!)
                .ToArray();

            var states = RpgReflection.ScanForTypes<States.State>()
                .Select(x => new MetaState(x))
                .ToArray();

            var system = Activator.CreateInstance(sysType) as IMetaSystem;
            if (system == null)
                throw new InvalidOperationException($"Could not create instance of IMetaSystem {sysType.Name}");

            system.Objects = res;
            system.Actions = actions;
            system.States = states;
            system.PropUIs = propUIs;

            return system;
        }

        public MetaObj Object(Type type)
        {
            var obj = new MetaObj(type.Name);
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

                if (RpgReflection.RpgPropertyTypes.Contains(propInfo.PropertyType))
                {
                    var metaProp = new MetaProp();

                    metaProp.Prop = propInfo.Name;
                    metaProp.DataType = propUI?.DataType ?? propInfo.PropertyType.Name;
                    metaProp.ReturnType = propInfo.PropertyType.Name;
                    metaProp.Path = propStack.ToList();
                    metaProp.Path.Reverse();

                    metaProp.Tab = tab;
                    metaProp.Group = group;
                    metaProp.DisplayName = !string.IsNullOrEmpty(propUI?.DisplayName) ? propUI.DisplayName : string.Join('.', new List<string>(metaProp.Path) { metaProp.Prop });
                    metaProp.Ignore = propUI?.Ignore ?? false;

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

        private List<Type> DiscoverMetaSystems()
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

            return types;
        }
    }
}
