using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class ModdableProperty
    {
        [JsonProperty] public Guid RootId { get; private set; }
        [JsonProperty] public Guid Id { get; private set; }
        [JsonProperty] public string Type { get; private set; }
        [JsonProperty] public string? Prop { get; private set; }
        [JsonProperty] public string? PropType { get; private set; }
        [JsonProperty] public string? Method { get; private set; }
        [JsonProperty] public string Source { get; private set; }

        public bool IsDiceProperty { get => PropType == nameof(Dice); }

        public ModdableProperty(Guid rootId, Guid id, string type, string? prop, string? propType, string? method)
        {
            RootId = rootId;
            Id = id;
            Type = type;
            Prop = prop;
            PropType = propType;
            Method = method?.EndsWith("()") ?? false ? method : $"{method}()";
            Source = $"{Type}.{Prop ?? Method ?? throw new ArgumentException("Either Prop or Method must be set")}";
        }

        public static ModdableProperty Create(Entity entity, string propPath, bool source = false)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var pathEntity = entity.PropertyValue<Entity>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
            if (!source && !pathEntity.IsModdableProperty(prop))
                throw new ArgumentException($"Invalid path. Property {prop} must have the attribute {nameof(ModdableAttribute)}");

            var propType = pathEntity.GetType().GetProperty(prop)?.PropertyType.Name;
            var locator = new ModdableProperty(entity.Id, pathEntity.Id, pathEntity.GetType().Name, prop, propType, null);
            return locator;
        }

        public static ModdableProperty Create<T, TResult>(T entity, Expression<Func<T, TResult>> expression, bool source = false)
            where T : Entity
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();

            //Get the prop name
            var prop = memberExpression.Member.Name;
            var propType = memberExpression.Member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)memberExpression.Member).PropertyType.Name
                : null;

            var moddable = memberExpression.Member.GetCustomAttribute<ModdableAttribute>() != null;
            if (!moddable && !source)
                throw new ArgumentException($"Invalid path. Property {memberExpression.Member.Name} must have the attribute {nameof(ModdableAttribute)}");

            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();
            var path = string.Join(".", pathSegments);
            var pathEntity = entity.PropertyValue<Entity>(path);

            var locator = new ModdableProperty(entity.Id, pathEntity!.Id, pathEntity!.GetType().Name, prop, propType, null);
            return locator;
        }
    }
}
