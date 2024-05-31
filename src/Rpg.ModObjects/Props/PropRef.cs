using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Props
{
    public class PropRef
    {
        [JsonProperty] public string EntityId { get; protected set; }
        [JsonProperty] public string Prop { get; protected set; }

        [JsonConstructor] protected PropRef() { }

        public PropRef(string entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public static PropRef CreatePropRef<T, TResult>(T rootEntity, Expression<Func<T, TResult>> expression)
            where T : RpgObject
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();

            //Get the prop name
            var prop = memberExpression.Member.Name;

            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();
            var path = string.Join(".", pathSegments);
            var entity = rootEntity.PropertyValue<RpgObject>(path);

            return new PropRef(entity!.Id, prop);
        }

        public static PropRef CreatePropRef(RpgObject rootEntity, string propPath)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var entity = rootEntity.PropertyValue<RpgObject>(path);
            var locator = entity != null
                ? new PropRef(entity.Id, prop)
                : new PropRef(rootEntity.Id, propPath);

            return locator;
        }

        public override string ToString()
        {
            return $"{EntityId}.{Prop}";
        }
    }
}
