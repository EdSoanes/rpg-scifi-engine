using Rpg.Experimental.Reflection;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.Experimental.Graph
{
    public class RpgPropertyRefCreator
    {
        private readonly RpgGraph Graph;

        public RpgPropertyRefCreator(RpgGraph graph)
        {
            Graph = graph;
        }

        public RpgPropertyRef? Create(string objectId, string path)
        {
            var obj = Graph.GetObject(objectId);
            return Create(obj, path);
        }

        public RpgPropertyRef? Create(RpgObject? obj, string path)
        {
            if (obj != null)
            {
                var (propObj, prop) = GetObjectForPath(obj, path);
                if (propObj != null && prop != null)
                    return new RpgPropertyRef(propObj.Id, prop);
            }

            return null;
        }

        public RpgPropertyRef? Create<T, TResult>(T obj, Expression<Func<T, TResult>> expression)
            where T : RpgObject
        {
            var path = RpgMemberUtilities.ExpressionToPath(expression);
            return Create(obj, path);
        }



        //public RpgPropertyRefValue CreateWithValue<T, TResult>(T obj, Expression<Func<T, TResult>> expression)
        //    where T : RpgObject
        //{
        //    var propRef = Create(obj, expression);

        //    var memberExpression = expression.Body as MemberExpression;
        //    if (memberExpression == null)
        //        throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

        //    var pathSegs = new List<string>();
        //    pathSegs.Add(memberExpression.Member.Name);
        //    while (memberExpression != null)
        //    {
        //        memberExpression = memberExpression.Expression as MemberExpression;
        //        if (memberExpression != null)
        //            pathSegs.Add(memberExpression.Member.Name);
        //    }

        //    pathSegs.Reverse();
        //    var path = string.Join(".", pathSegs);

        //    return CreateWithValue(obj, path);
        //}

        //public RpgPropertyRefValue CreateWithValue(object? obj, string path, Type? valueType = null)
        //{
        //    var (propObj, prop) = GetObjectByPath(obj, path);
        //    if (propObj != null && prop != null)
        //    {
        //        var propInfo = propObj.GetType().GetProperty(prop);
        //        if (propInfo != null && (valueType == null || IsPropertyTypeMatch(propInfo.PropertyType, valueType)))
        //            return new RpgPropertyRefValue(new RpgPropertyRef(propObj.Id, propInfo.Name), propObj.Value<object>(prop));
        //    }

        //    return new RpgPropertyRefValue(null, null);
        //}

        public (RpgObject?, string?) GetObjectForPath(object? obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
                return (null, null);

            var parts = path.Split('.');
            PropertyInfo? propInfo = null;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];

                propInfo = obj.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo != null)
                {
                    obj = propInfo.GetValue(obj, null);

                }
                //Could be a virtual property, in which case check the objecdata for values
                else if (obj is RpgObject rpgObj)
                {
                    var propData = Graph.GetPropertyData(rpgObj.Id, part);
                    obj = propData?.GetValue<RpgObject>(Graph);
                }

                if (obj == null)
                    return (null, null);
            }

            return (
                obj as RpgObject,
                obj is RpgObject ? parts.Last() : null
            );
        }

        private static bool IsPropertyTypeMatch(Type propertyType, Type valueType)
            => valueType.IsAssignableTo(propertyType) || (Nullable.GetUnderlyingType(propertyType)?.IsAssignableFrom(valueType) ?? false);

    }
}
