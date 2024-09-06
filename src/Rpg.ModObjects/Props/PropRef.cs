using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Props
{
    public class PropRef
    {
        [JsonInclude] public string EntityId { get; protected set; }
        [JsonInclude] public string Prop { get; protected set; }
        [JsonInclude] public RefType RefType { get; protected set; } = RefType.Value;

        [JsonConstructor] protected PropRef() { }

        public PropRef(string entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public PropRef(string entityId, string prop, RefType refType)
        {
            EntityId = entityId;
            Prop = prop;
            RefType = refType;
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
            var (entity, prop) = rootEntity.FromPath(propPath);
            var locator = entity != null && prop != null
                ? new PropRef(entity.Id, prop)
                : new PropRef(rootEntity.Id, propPath);

            return locator;
        }

        public override string ToString()
        {
            return $"{RefType}.{EntityId}.{Prop}";
        }

        public static PropRef FromString(string str)
        {
            var parts = !string.IsNullOrEmpty(str) ? str.Split('.') : [];
            var refType = Enum.Parse<Props.RefType>(parts[0]);
            var entityId = parts[1];
            var prop = string.Join('.', parts.Skip(2));

            return new PropRef(entityId, prop, refType);
        }
    }
}
