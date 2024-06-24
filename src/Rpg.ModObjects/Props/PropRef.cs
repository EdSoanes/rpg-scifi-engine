using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;
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

            var prop = memberExpression.Member.Name;
            var path = new List<string>();
            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    path.Add(memberExpression.Member.Name);
            }

            path.Reverse();
            var entity = !path.Any()
                ? rootEntity
                : rootEntity.PropertyValue<RpgObject>(string.Join(".", path));

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
