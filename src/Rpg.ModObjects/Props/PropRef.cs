﻿using System.Linq.Expressions;
using Newtonsoft.Json;

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
            var (entity, prop) = rootEntity.FromPath(propPath);
            var locator = entity != null && prop != null
                ? new PropRef(entity.Id, prop)
                : new PropRef(rootEntity.Id, propPath);

            return locator;
        }

        public override string ToString()
        {
            return $"{EntityId}.{Prop}";
        }

        public static PropRef FromString(string str)
        {
            var parts = !string.IsNullOrEmpty(str) ? str.Split('.') : [];
            var entityId = parts[0];
            var prop = string.Join('.', parts.Skip(1));

            return new PropRef(entityId, prop);
        }
    }
}
