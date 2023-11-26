using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Collections;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts.Meta
{
    internal static class ReflectionEngine
    {
        internal static bool IsModdableProperty(this PropertyInfo propertyInfo)
        {
            var attr = propertyInfo.GetCustomAttribute<ModdableAttribute>();
            return attr != null;
        }

        internal static List<MetaEntity> TraverseMetaGraph(this object context, Action<MetaEntity, string, PropertyInfo> processContext, string basePath = "{}")
        {
            var entities = new List<MetaEntity>();

            var metaEntity = new MetaEntity(context, basePath);
            entities.Add(metaEntity);

            foreach (var propertyInfo in context.MetaProperties())
            {
                var items = context.PropertyObjects(propertyInfo, out var isEnumerable);
                var path = $"{basePath}.{propertyInfo.Name}{(isEnumerable ? "[]" : "")}";

                processContext.Invoke(metaEntity, path, propertyInfo);

                foreach (var item in items)
                {
                    var childEntities = TraverseMetaGraph(item, processContext, path);
                    entities.AddRange(childEntities);
                }
            }

            return entities;
        }

        internal static string GetEntityClass(this object obj)
        {
            if (obj.GetType().IsAssignableTo(typeof(Artifact)))
                return nameof(Artifact);

            if (obj.GetType().IsAssignableTo(typeof(Entity)))
                return nameof(Entity);

            return nameof(Object);
        }

        internal static string[] GetSetupMethods(this object obj)
        {
            var setupMethods = obj.GetType().GetMethods()
                .Where(x => x.GetCustomAttribute<SetupAttribute>() != null)
                .Select(x => x.Name)
                .ToArray();

            return setupMethods;
        }

        internal static MetaAction[] GetAbilityMethods(this object context)
        {
            var metaAbilityMethods = new List<MetaAction>();

            foreach (var methodInfo in context.GetType().GetMethods())
            {
                var attr = methodInfo.GetCustomAttribute<AbilityAttribute>();
                if (attr == null || methodInfo.ReturnType != typeof(TurnAction))
                    continue;

                var metaAbilityMethod = new MetaAction
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

                    metaAbilityMethod.Inputs.Add(metaActionInput);
                }

                metaAbilityMethods.Add(metaAbilityMethod);
            }

            return metaAbilityMethods.ToArray();
        }

        internal static PropertyInfo? MetaProperty(this object context, string prop)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => (c.GetMethod != null && (c.GetMethod.IsPublic || c.GetMethod.IsFamily)) || (c.SetMethod != null && (c.SetMethod.IsPublic || c.SetMethod.IsFamily)))
                .FirstOrDefault(x => x.Name == prop);
        }

        internal static PropertyInfo[] MetaProperties(this object context)
        {
            return context.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => (c.GetMethod != null && (c.GetMethod.IsPublic || c.GetMethod.IsFamily)) || (c.SetMethod != null && (c.SetMethod.IsPublic || c.SetMethod.IsFamily)))
                .Where(x => !(x.PropertyType.Namespace!.StartsWith("System") && x.PropertyType.Name.StartsWith("Func")))
                .ToArray();
        }

        private static IEnumerable<object> PropertyObjects(this object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            isEnumerable = false;

            var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            if (obj == null || obj is string || obj.GetType().IsPrimitive)
                return Enumerable.Empty<object>();

            if (obj is IEnumerable)
            {
                isEnumerable = true;
                return (obj as IEnumerable)!.Cast<object>();
            }

            return new List<object> { obj };
        }
    }
}
