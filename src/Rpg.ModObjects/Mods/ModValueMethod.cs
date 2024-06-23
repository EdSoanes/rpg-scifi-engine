using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Mods
{
    internal class ModValueMethod : RpgMethod<RpgObject, Dice>
    {
        [JsonConstructor] internal ModValueMethod() { }

        [JsonProperty] public string? EntityId { get; private set; }

        public bool IsCalc => 
            !string.IsNullOrWhiteSpace(MethodName)
            && (EntityId != null || !string.IsNullOrWhiteSpace(ClassName));

        public Dice Execute(RpgGraph graph, Dice dice)
        {
            var args = Create();
            args["dice"] = dice;

            if (IsStatic)
                return Execute(args);

            var entity = graph.GetEntity(EntityId);
            if (entity == null)
                throw new InvalidOperationException($"Could not find entity {EntityId} for {nameof(ModValueMethod)}");

            return Execute(entity, args);
        }

        public void Set<T>(Expression<Func<Func<T, T>>>? expression)
        {
            if (expression == null)
                return;

            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MethodInfo)methodInfoExpression.Value!;

            if (methodInfo.IsStatic)
            {
                DiscoverMethod(methodInfo.DeclaringType!, methodInfo.Name);
                return;
            }

            RpgObject? entity;
            var memberExpression = methodCallExpression.Arguments?.Last() as MemberExpression;
            if (memberExpression != null)
            {
                var constantExpression = memberExpression.Expression as ConstantExpression;
                var fieldInfo = memberExpression?.Member as FieldInfo;
                entity = fieldInfo?.GetValue(constantExpression?.Value) as RpgObject;
            }
            else
            {
                var constantExpression = methodCallExpression.Arguments?.Last() as ConstantExpression;
                entity = constantExpression?.Value as RpgObject;
            }

            DiscoverMethod(entity!.GetType(), methodInfo.Name);
            EntityId = entity!.Id;
        }

        public void Set(ModValueMethod? valueFunc)
        {
            if (valueFunc != null)
            {
                EntityId = valueFunc.EntityId;
                ClassName = valueFunc.ClassName;
                MethodName = valueFunc.MethodName;
            }
        }
    }
}
