using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Rpg.ModObjects
{
    public class RpgGraphBuilder
    {
        private Type[] _nonTraversibleTypes;

        private List<RpgObject> _objects = new();

        public RpgGraphBuilder() 
        {
            var res = this.GetType().Assembly.GetTypes()
                .Where(x => x.IsClass
                    && !x.IsAssignableTo(typeof(RpgLifecycleObject))
                    && !x.IsAssignableTo(typeof(ObjectsDictionary)))
                .ToList();

            res.AddRange([
                typeof(string),
                typeof(DateTime),
                typeof(Guid),
                typeof(Mod),
                typeof(ModSet),
                typeof(State),
                typeof(ActionTemplate)
            ]);

            _nonTraversibleTypes = res.ToArray();
        }

        public List<RpgObject> Build(params RpgObject[] roots)
        {
            _objects.Clear();
            
            foreach (var root in roots)
                Traverse(root);

            return _objects;
        }

        private void Traverse(object obj)
        {
            if (obj is RpgObject rpgObj)
            {
                if (_objects.Any(x => x.Id == rpgObj.Id))
                {
                    //Debug.WriteLine($"{rpgObj.GetType().Name} {rpgObj.Id}. Already added to builder");
                    return;
                }
                else
                {
                    //Debug.WriteLine($"{rpgObj.GetType().Name} {rpgObj.Id}. Found");
                    _objects.Add(rpgObj);
                }
            }


            var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                var items = GetPropertyObjects(obj, propertyInfo, out var isEnumerable);
                //if (items.Any(x => IsTraversibleType(x.GetType())))
                //    Debug.WriteLine($"{propertyInfo.PropertyType.Name} {obj.GetType().Name}.{propertyInfo.Name}. Contains traversible values...");

                foreach (var item in items.Where(x => IsTraversibleType(x.GetType())))
                {
                    Traverse(item);
                }
            }
        }

        public bool IsTraversibleType(Type type)
        {
            if (!type.IsClass)
                return false;

            if (string.IsNullOrEmpty(type.Namespace))
                return false;

            if (type.Namespace.StartsWith("System.") && !type.IsAssignableTo(typeof(IEnumerable)))
                return false;

            if (_nonTraversibleTypes.Any(x => type.IsAssignableTo(x)))
                return false;

            return true;            
        }

        private IEnumerable<object> GetPropertyObjects(object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            isEnumerable = false;

            if (propertyInfo.GetMethod?.Name == "get_Item" )
                return Enumerable.Empty<object>();

            var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

            if (obj == null)
                return Enumerable.Empty<object>();

            var items = GetPropObjects(obj!, out isEnumerable);
            return items;
        }

        private List<object> GetPropObjects(object? obj, out bool isEnumerable)
        {
            isEnumerable = false;

            var res = new List<object>();
            var items = new List<object?>();
            if (obj is IDictionary)
            {
                items = (obj as IDictionary)!.Values.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj is IEnumerable)
            {
                items = (obj as IEnumerable)!.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj != null)
                res.Add(obj);

            foreach (var item in items.Where(x => x != null))
                res.AddRange(GetPropObjects(item, out var _));

            return res;
        }
    }
}
