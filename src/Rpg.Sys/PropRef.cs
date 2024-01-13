using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.Sys
{
    public struct PropRef
    {
        private string? _description;

        [JsonProperty] public Guid? RootId { get; private set; }
        [JsonProperty] public Guid? Id { get; private set; }
        [JsonProperty] public string Prop { get; private set; }
        [JsonProperty] public PropType PropType { get; private set; } = PropType.Path;
        [JsonProperty] public PropReturnType PropReturnType { get; private set; } = PropReturnType.Integer;

        public static bool operator ==(PropRef d1, PropRef d2) => d1.Equals(d2);
        public static bool operator !=(PropRef d1, PropRef d2) => !d1.Equals(d2);

        [JsonConstructor] public PropRef() { }

        public PropRef(Guid? rootId, Guid? id, int val)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Dice;
            PropReturnType = PropReturnType.Integer;
            Prop = val.ToString();
        }

        public PropRef(Guid? rootId, Guid? id, Dice dice)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Dice;
            PropReturnType = PropReturnType.Dice;
            Prop = dice;
        }

        public PropRef(Guid? rootId, Guid? id, string propPath, PropReturnType propReturnType)
        {
            RootId = rootId;
            Id = id;
            PropType = PropType.Path;
            PropReturnType = propReturnType;
            Prop = propPath;
        }

        internal string Describe(Graph graph)
        {
            if (string.IsNullOrEmpty(_description))
            {
                if (PropType == PropType.Dice)
                    _description = Prop;
                else
                {
                    var parts = new []
                    {
                        graph.Entities.Get(RootId!.Value)?.Name,
                        graph.Entities.Get(Id)?.Name,
                        Prop
                    };

                    _description = string.Join('.', parts.Where(x => !string.IsNullOrEmpty(x)).Distinct());
                }
            }
            
            return _description;
        }

        public static PropRef FromInt(Guid entityId, int val)
        {
            var locator = new PropRef(entityId, entityId, val);
            return locator;
        }
        public static PropRef FromInt(Guid rootId, Guid entityId, int val)
        {
            var locator = new PropRef(rootId, entityId, val);
            return locator;
        }

        public static PropRef FromDice(Dice? dice)
        {
            var locator = new PropRef(null, null, dice ?? "0");
            return locator;
        }

        public static PropRef FromDice(Guid? entityId, Dice? dice)
        {
            var locator = new PropRef(entityId, entityId, dice ?? "0");
            return locator;
        }

        public static PropRef FromDice(Guid? rootId, Guid? entityId, Dice? dice)
        {
            var locator = new PropRef(rootId, entityId, dice ?? "0");
            return locator;
        }

        public static PropRef FromPath(ModdableObject entity, string propPath, bool source = false)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var pathEntity = entity.PropertyValue<ModdableObject>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");

            var propReturnType = ToReturnType(pathEntity.GetType().GetProperty(prop)?.PropertyType);
            var locator = new PropRef(entity.Id, pathEntity.Id, prop, propReturnType);
            return locator;
        }

        public static PropRef FromPath<T, TResult>(T entity, Expression<Func<T, TResult>> expression, bool source = false)
            where T : ModdableObject
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

            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegments.Add(memberExpression.Member.Name);
            }

            pathSegments.Reverse();
            var path = string.Join(".", pathSegments);
            var pathEntity = entity.PropertyValue<ModdableObject>(path);

            var locator = new PropRef(entity.Id, pathEntity!.Id, prop, ToReturnType(propReturnType));
            return locator;
        }

        private static PropReturnType ToReturnType(Type? type) => type?.Name switch
        {
            "Dice" => PropReturnType.Dice,
            _ => PropReturnType.Integer
        };

        public override string ToString()
        {
            return $"{Id?.ToString() ?? "Dice"}.{Prop}";
        }

        public override bool Equals(object? obj)
        {
            return obj?.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
