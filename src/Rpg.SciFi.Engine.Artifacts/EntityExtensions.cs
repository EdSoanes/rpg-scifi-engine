﻿using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class EntityExtensions
    {
        public static MetaEntity? MetaData(this Guid id)
        {
            if (Meta.MetaEntities == null)
                throw new InvalidOperationException($"{nameof(Meta)} not initialized");

            return Meta.MetaEntities.SingleOrDefault(x => x.Id == id);
        }

        public static string[] Describe(this Entity? entity, string prop)
        {
            var res = new List<string>();
            res.Add($"{entity.GetType().Name}.{prop}");
            res.AddRange(entity.Describe(prop, 1));
            return res.ToArray();
        }
        private static string[] Describe(this Entity? entity, string prop, int level = 0)
        {
            var res = new List<string>();

            if (entity != null)
            {
                foreach (var mod in entity.Mods(prop))
                {
                    res.Add($"{(level == 0 ? "" : level.ToString())} {mod.Source?.Source ?? mod.Name} => {mod.Evaluate()}");
                    if (mod.Source?.Prop != null)
                    {
                        var subs = mod.Source.Id.MetaData()
                            ?.Entity
                            ?.Describe(mod.Source.Prop, level + 1) 
                            ?? new string[0];

                        res.AddRange(subs);
                    }
                }
            }

            return res.ToArray();
        }

        public static T? PropertyValue<T>(this Guid id, string path)
            => (id.MetaData()?.Entity).PropertyValue<T>(path);

        public static T? PropertyValue<T>(this Entity? entity, string path)
        {
            if (entity == null)
                return default;

            if (string.IsNullOrEmpty(path))
                return entity is T
                    ? (T?)(object)entity
                    : default;

            object? res = entity;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                var propInfo = res.MetaProperty(part);
                res = propInfo?.GetValue(res, null);
                if (res == null)
                    break;
            }

            if (res is T)
                return (T?)res;

            if (typeof(T) == typeof(string))
                return (T?)(object?)res?.ToString();

            return default;
        }

        public static Modifier Mod<T1>(
            this Entity entity,
            string name,
            Dice dice,
            string targetPropPath,
            Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
        {
            var tgt = entity.ToModdableProperty(targetPropPath);
            var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

            return new Modifier(name, dice, tgt, calc);
        }

        public static Modifier Mod<TSource, T1>(
            this TSource entity, 
            string name,
            Dice dice,
            Expression<Func<TSource, T1>> targetExpr,
            Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TSource : Entity
        {
            var tgt = entity.ToModdableProperty(targetExpr);
            var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

            return new Modifier(name, dice, tgt, calc);
        }

        public static Modifier Mod<TSource, T1, T2>(
            this TSource entity,
            Expression<Func<TSource, T1>> sourceExpr,
            Expression<Func<TSource, T2>> targetExpr,
            Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TSource : Entity
            => entity.Mod(null, sourceExpr, entity, targetExpr, diceCalcExpr);

        public static Modifier Mod<TSource, TTarget, T1, T2>(
            this TSource entity,
            Expression<Func<TSource, T1>> sourceExpr,
            TTarget target,
            Expression<Func<TTarget, T2>> targetExpr,
            Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TSource : Entity
            where TTarget : Entity
            => entity.Mod(null, sourceExpr, target, targetExpr, diceCalcExpr);

        private static Modifier Mod<TSource, TTarget, T1, T2>(
            this TSource entity,
            string? name,
            Expression<Func<TSource, T1>> sourceExpr,
            TTarget target,
            Expression<Func<TTarget, T2>> targetExpr,
            Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TSource : Entity
            where TTarget : Entity
        {
            var src = entity.ToModdableProperty(sourceExpr, true);
            var tgt = target.ToModdableProperty(targetExpr);
            var calc = ReflectionEngine.GetDiceCalcFunction(diceCalcExpr);

            name ??= src.Prop;
            return new Modifier(name, src, tgt, calc);
        }

        public static ModdableProperty ToModdableProperty(this Entity entity, string propPath, bool source = false)
        {
            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var pathEntity = entity.PropertyValue<Entity>(path) ?? throw new ArgumentException($"Invalid path. Property path {path} is not an Entity object");
            if (!source && !pathEntity.IsModdableProperty(prop))
                throw new ArgumentException($"Invalid path. Property {prop} must have the attribute {nameof(ModdableAttribute)}");

            var locator = new ModdableProperty(pathEntity.Id, pathEntity.GetType().Name, prop, null);
            return locator;
        }

        public static ModdableProperty ToModdableProperty<T, TResult>(this T sourceEntity, Expression<Func<T, TResult>> expression, bool source = false)
            where T : Entity
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegments = new List<string>();

            //Get the prop name
            var prop = memberExpression.Member.Name;
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
            var pathEntity = sourceEntity.PropertyValue<Entity>(path);

            var locator = new ModdableProperty(pathEntity!.Id, pathEntity!.GetType().Name, prop, null);
            return locator;
        }
    }
}