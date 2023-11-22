using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public static class MetaDiscovery
    {
        private static object? _context;
        public static Game? Context { get; private set; }
        public static MetaEntity[]? MetaEntities { get; private set; }

        public static void Initialize(Game game)
        {
            Context = game;
            MetaEntities = Discover(game)
                .OrderBy(x => x.Path)
                .ToArray();
        }
        public static Dice? GetDice(MetaModLocator? locator)
        {
            if (locator == null)
                return null;

            var entity = Find(locator.Id);
            Dice dice = entity!.GetType().GetProperty(locator.Prop)!.GetValue(entity)?.ToString() ?? "0";

            return dice;
        }

        public static MetaModLocator GetModLocator<T, TResult>(object? obj, Expression<Func<T, TResult>> expression)
        {
            var locator = new MetaModLocator();

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var path = new List<string>();
            int pos = 0;
            while (memberExpression != null)
            {
                path.Add(memberExpression.Member.Name);

                //The first member (last property in the expression) must be moddable
                if (pos == 0)
                {
                    var moddable = memberExpression.Member.GetCustomAttribute<ModdableAttribute>() != null;
                    if (!moddable)
                        throw new ArgumentException($"Invalid path. Property {memberExpression.Member.Name} must have the attribute {nameof(ModdableAttribute)}");

                    locator.Prop = memberExpression.Member.Name;
                }
                //The direct parent of the last property must be on a class inheriting from the Modifiable class
                else if (pos == 1)
                {
                    var modifiable = memberExpression.Type.IsAssignableTo(typeof(Modifiable));
                    if (!modifiable)
                        throw new ArgumentException($"Invalid path. Parent object {memberExpression.Member.Name} must inherit from {nameof(Modifiable)}");
                }

                memberExpression = memberExpression.Expression as MemberExpression;
                pos++;
            }

            path.Reverse();
            if (obj != null)
            {
                var val = obj;
                foreach (var prop in path.Take(path.Count() - 1))
                {
                    if (val == null)
                        break;

                    val = val.GetType().GetProperty(prop)!.GetValue(val);
                }

                locator.Id = (val as Modifiable)!.Id;
            }

            return locator;
        }

        public static MetaEntity? Find(Guid contextId)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException("MetaDiscovery not initialized");

            return MetaEntities.SingleOrDefault(x => x.Id == contextId);
        }

        public static MetaEntity Find(string path, out string property)
        {
            if (MetaEntities == null)
                throw new InvalidOperationException("MetaDiscovery not initialized");

            var parts = path.Split('.');
            var entityPath = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            var metaEntity = MetaEntities.SingleOrDefault(x => x.Path == entityPath && x.ModifiableProperties.Any(x => x == prop));
            if (metaEntity == null)
                throw new ArgumentException($"{path} is invalid");

            property = prop;
            return metaEntity;
        }

        private static List<MetaEntity> Discover(object context, string basePath = "{context}")
        {
            var meta = new List<MetaEntity>();

            var metaEntity = MetaEntity(context);
            if (metaEntity != null)
            {
                metaEntity.Path = basePath;
                meta.Add(metaEntity);
            }

            if (metaEntity?.EntityType == nameof(Artifact))
            {
                foreach (var methodInfo in context.GetType().GetMethods())
                {
                    var metaAction = AbilityMethod(methodInfo);
                    if (metaAction != null)
                        metaEntity?.Actions?.Add(metaAction);
                }
            }

            var propertyInfos = GetFilteredProperties(context);
            foreach (var propertyInfo in propertyInfos)
            {
                if (IsModifiableProperty(propertyInfo) && metaEntity != null)
                {
                    metaEntity.ModifiableProperties?.Add(propertyInfo.Name);
                }

                var items = GetPropertyObjects(propertyInfo, context, out var isEnumerable);
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? "[]" : "")}";
                foreach (var item in items)
                {
                    var subEntities = Discover(item, path);
                    meta.AddRange(subEntities);
                }
            }

            return meta;
        }

        private static MetaEntity? MetaEntity(object? obj)
        {
            if (obj == null)
                return null;

            if (obj.GetType().IsAssignableTo(typeof(Modifiable)))
            {
                var modifiable = (Modifiable)obj;
                return new MetaEntity
                {
                    Id = modifiable.Id,
                    Name = modifiable.Name,
                    EntityType = nameof(Modifiable),
                    Entity = obj
                };
            }

            if (obj.GetType().IsAssignableTo(typeof(Artifact)))
            {
                var artifact = (Artifact)obj;
                return new MetaEntity
                {
                    Id = artifact.Id,
                    Name = artifact.Name,
                    EntityType = nameof(Artifact),
                    Entity = obj
                };
            }

            return null;
        }

        private static IEnumerable<object> GetPropertyObjects(PropertyInfo propertyInfo, object entity, out bool isEnumerable)
        {
            isEnumerable = false;

            var obj = propertyInfo.GetValue(entity, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            if (obj == null || obj is string || obj.GetType().IsPrimitive)
                return Enumerable.Empty<object>();

            if (obj is IEnumerable)
            {
                isEnumerable = true;
                return (obj as IEnumerable)!.Cast<object>();
            }

            return new List<object> { obj };
        }

        private static bool IsModifiableProperty(PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<ModdableAttribute>();
            return attr != null;
        }

        private static MetaAction? AbilityMethod(MethodInfo methodInfo)
        {
            var attr = methodInfo.GetCustomAttribute<AbilityAttribute>();
            if (attr != null && methodInfo.ReturnType == typeof(TurnAction))
            {
                var metaAction = new MetaAction
                {
                    Name = attr.Name
                };

                var inputAttrs = methodInfo.GetCustomAttributes<InputAttribute>();

                foreach (var parameter in methodInfo.GetParameters())
                {
                    var inputAttr = inputAttrs.FirstOrDefault(x => x.Param == parameter.Name);
                    if (inputAttr == null)
                        throw new ArgumentException($"{methodInfo.Name} missing matching Input attribute");

                    var metaActionInput = new MetaActionInput
                    {
                        Name = inputAttr.Param,
                        BindsTo = inputAttr.BindsTo,
                        InputSource = inputAttr.InputSource
                    };

                    metaAction.Inputs.Add(metaActionInput);
                }

                return metaAction;
            }

            return null;
        }

        private static PropertyInfo[] GetFilteredProperties(object context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => (c.GetMethod != null && (c.GetMethod.IsPublic || c.GetMethod.IsFamily)) || (c.SetMethod != null && (c.SetMethod.IsPublic || c.SetMethod.IsFamily)))
                .Where(x => !(x.PropertyType.Namespace!.StartsWith("System") && x.PropertyType.Name.StartsWith("Func")))
                .ToArray();
        }
    }
}