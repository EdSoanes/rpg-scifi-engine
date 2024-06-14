using Rpg.ModObjects.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObj
    {
        public string Archetype {  get; set; }
        public MetaProp[] Props { get; set; }
    }

    public class MetaProp
    {
        public string Prop { get; set; }
        public List<string> Path { get; set; } = new List<string>();
        public string Type { get; set; }

        public string EditorName { get; set; }
        public string? Tab { get; set; }
        public string? Group { get; set; }

        public override string ToString()
        {
            var prop = new List<string>(Path);
            prop.Add(Prop);
            return  $"{string.Join('.', prop)} {Type} [{Tab},{Group}]";
        }
    }

    public class MetaGen
    {
        private static readonly Type[] BaseTypes =
        [
            typeof(int),
            typeof(string),
            typeof(Dice)
        ];

        public MetaObj Object(Type type)
        {
            var obj = new MetaObj();
            obj.Archetype = type.Name;
            obj.Props = Props(type);
            return obj;
        }

        public MetaProp[] Props(Type type)
        {
            var metaProps = new List<MetaProp>();
            var propStack = new Stack<string>();
            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                Prop(metaProps, propStack, propInfo, null, null);

            return metaProps.ToArray();
        }

        private void Prop(List<MetaProp> metaProps, Stack<string> propStack, PropertyInfo propInfo, string? tab, string? group)
        {
            var propUI = propInfo.GetPropUI();
            tab = !string.IsNullOrEmpty(propUI?.Tab) ? propUI.Tab : tab;
            group = !string.IsNullOrEmpty(propUI?.Group) ? propUI.Group : group;

            if (BaseTypes.Contains(propInfo.PropertyType))
            {
                var metaProp = new MetaProp();

                metaProp.Prop = propInfo.Name;
                metaProp.Type = propInfo.PropertyType.Name;
                metaProp.Path = propStack.ToList();
                metaProp.Tab = tab;
                metaProp.Group = group;
                metaProp.EditorName = !string.IsNullOrEmpty(propUI?.EditorName) ? propUI.EditorName : propInfo.Name;

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
}
