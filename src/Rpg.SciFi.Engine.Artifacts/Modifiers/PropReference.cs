using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public class PropReference
    {
        [JsonProperty] public Guid? RootId { get; private set; }
        [JsonProperty] public Guid? Id { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public PropType PropType { get; private set; } = PropType.Path;
        [JsonProperty] public PropReturnType PropReturnType { get; private set; } = PropReturnType.Integer;

        [JsonConstructor] private PropReference() { }

        public PropReference(Guid? rootId, Guid? id, int val)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Dice;
            PropReturnType = PropReturnType.Integer;
            Prop = val.ToString();
        }

        public PropReference(Guid? rootId, Guid? id, Dice dice)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Dice;
            PropReturnType = PropReturnType.Dice;
            Prop = dice;
        }

        public PropReference(Guid? rootId, Guid? id, string propPath, PropReturnType propReturnType)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Path;
            PropReturnType = propReturnType;
            Prop = propPath;
        }

        public static PropReference FromInt(Guid entityId, int val)
        {
            var locator = new PropReference(entityId, entityId, val);
            return locator;
        }
        public static PropReference FromInt(Guid rootId, Guid entityId, int val)
        {
            var locator = new PropReference(rootId, entityId, val);
            return locator;
        }

        public static PropReference FromDice(Dice? dice)
        {
            var locator = new PropReference(null, null, dice ?? "0");
            return locator;
        }

        public static PropReference FromDice(Guid? entityId, Dice? dice)
        {
            var locator = new PropReference(entityId, entityId, dice ?? "0");
            return locator;
        }

        public static PropReference FromDice(Guid? rootId, Guid? entityId, Dice? dice)
        {
            var locator = new PropReference(rootId, entityId, dice ?? "0");
            return locator;
        }

        public static PropReference FromPath(Entity entity, string propPath, bool source = false)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var pathEntity = entity.PropertyValue<Entity>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
            if (!source && !pathEntity.IsModdableProperty(prop))
                throw new ArgumentException($"Invalid path. Property {prop} must have the attribute {nameof(ModdableAttribute)}");

            var propReturnType = ToReturnType(pathEntity.GetType().GetProperty(prop)?.PropertyType);
            var locator = new PropReference(entity.Id, pathEntity.Id, prop, propReturnType);
            return locator;
        }

        public static PropReference FromPath<T, TResult>(T entity, Expression<Func<T, TResult>> expression, bool source = false)
            where T : Entity
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();

            //Get the prop name
            var prop = memberExpression.Member.Name;
            var propReturnType = memberExpression.Member.MemberType == MemberTypes.Property
                ? ((PropertyInfo)memberExpression.Member).PropertyType
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

            var locator = new PropReference(entity.Id, pathEntity!.Id, prop, ToReturnType(propReturnType));
            return locator;
        }

        private static PropReturnType ToReturnType(Type? type) => type?.Name switch
        {
            "Dice" => PropReturnType.Dice,
            _ => PropReturnType.Integer
        };
    }
}
