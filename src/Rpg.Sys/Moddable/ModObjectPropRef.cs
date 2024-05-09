using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Rpg.Sys.Moddable
{
    public class ModObjectPropRef
    {
        [JsonProperty] public Guid EntityId { get; protected set; }
        [JsonProperty] public string Prop { get; protected set; }

        [JsonConstructor] protected ModObjectPropRef() { }

        public ModObjectPropRef(Guid entityId, string prop)
        {
            EntityId = entityId;
            Prop = prop;
        }

        public static bool operator ==(ModObjectPropRef? d1, ModObjectPropRef? d2)
            => d1?.EntityId == d2?.EntityId && d1?.Prop == d2?.Prop;
        
        public static bool operator !=(ModObjectPropRef? d1, ModObjectPropRef? d2) 
            => d1?.EntityId != d2?.EntityId || d1?.Prop != d2?.Prop;

        public static ModObjectPropRef CreatePropRef<T, TResult>(T rootEntity, Expression<Func<T, TResult>> expression)
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

            return new ModObjectPropRef(entity!.Id, prop);
        }

        public static PropRef CreatePropRef(ModObject rootEntity, string propPath)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var entity = rootEntity.PropertyValue<ModObject>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");

            var locator = new PropRef(entity.Id, path, prop, rootEntity.Id);
            return locator;
        }
    }
}
