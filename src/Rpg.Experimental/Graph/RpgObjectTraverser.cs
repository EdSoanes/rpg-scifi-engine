using System.Collections;
using System.Reflection;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public class RpgObjectTraverser
    {
        private Type[] _nonTraversibleTypes;

        private List<RpgObject> _objects = new();

        public RpgObjectTraverser()
        {
            var res = GetType().Assembly.GetTypes()
                .Where(x => x.IsClass
                    && !x.IsAssignableTo(typeof(RpgObject)))
                .ToList();

            res.AddRange([
                typeof(string),
                typeof(DateTime),
                typeof(Guid),
                //typeof(Mod),
                //typeof(ModSet),
                //typeof(State),
                //typeof(ActionTemplate)
            ]);

            res.Remove(typeof(Lifespan));

            _nonTraversibleTypes = res.ToArray();
        }

        public List<RpgObject> Build(RpgObject root, Action<RpgObject, RpgObject?> onObject)
        {
            _objects.Clear();

            Traverse(root, null, onObject);

            return _objects;
        }

        private void Traverse(object obj, RpgObject? parentObj, Action<RpgObject, RpgObject?> onObject)
        {
            if (obj is RpgObject rpgObj)
            {
                if (_objects.Any(x => x.Id == rpgObj.Id))
                    return;

                _objects.Add(rpgObj);
                onObject(rpgObj, parentObj);
            }

            var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                var items = GetPropertyObjects(obj, propertyInfo, out var isEnumerable);
                foreach (var item in items.Where(x => IsTraversibleType(x.GetType())))
                {
                    Traverse(item, obj as RpgObject, onObject);
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

            if (propertyInfo.GetMethod?.Name == "get_Item")
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
