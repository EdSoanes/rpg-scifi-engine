using Newtonsoft.Json;
using Rpg.Sys.Moddable;
using System.Linq.Expressions;

namespace Rpg.Sys
{
    public class PropRef
    {
        [JsonProperty] public Guid EntityId { get; protected set; }
        [JsonProperty] public string? Path { get; protected set; }
        [JsonProperty] public string Prop { get; protected set; }
        [JsonProperty] public Guid? RootEntityId { get; protected set; }

        [JsonConstructor] public PropRef() { }

        public PropRef(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public PropRef(Guid entityId, string path, string prop)
            : this(entityId, prop)
                => Path = path;

        public PropRef(Guid entityId, string path, string prop, Guid rootEntityId)
            : this(entityId, path, prop)
                => RootEntityId = rootEntityId;

        public static PropRef Create(ModObject rootEntity, string propPath)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var entity = rootEntity.PropertyValue<ModObject>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");

            var locator = new PropRef(entity.Id, path, prop, rootEntity.Id);
            return locator;
        }

        public static PropRef Create<T, TResult>(T rootEntity, Expression<Func<T, TResult>> expression)
            where T : ModObject
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
            var entity = rootEntity.PropertyValue<ModObject>(path);

            var locator = new PropRef(entity!.Id, path, prop, rootEntity.Id);
            return locator;
        }

        public override string ToString()
        {
            return $"{EntityId}.{Prop}";
        }
    }
}
