using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.Sys.Modifiers
{
    public class ModifierDiceCalc
    {
        [JsonProperty] public Guid? EntityId { get; private set; }
        [JsonProperty] public string? ClassName { get; private set; }
        [JsonProperty] public string? FuncName { get; private set; }

        public bool IsStatic => !string.IsNullOrWhiteSpace(ClassName)
            && !string.IsNullOrWhiteSpace(FuncName);

        public bool IsCalc => !string.IsNullOrWhiteSpace(FuncName)
            && (EntityId != null || !string.IsNullOrWhiteSpace(ClassName));

        public void Clear()
        {
            EntityId = null;
            ClassName = null;
            FuncName = null;
        }

        public void SetDiceCalc<T>(Expression<Func<Func<T, T>>>? expression)
        {
            if (expression == null)
                return;

            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MethodInfo)methodInfoExpression.Value!;

            if (methodInfo.IsStatic)
            {
                ClassName = methodInfo.DeclaringType!.Name;
                FuncName = methodInfo.Name;
                return;
            }

            ModdableObject? entity = null;
            var memberExpression = methodCallExpression.Arguments?.Last() as MemberExpression;
            if (memberExpression != null)
            {
                var constantExpression = memberExpression.Expression as ConstantExpression;
                var fieldInfo = memberExpression?.Member as FieldInfo;
                entity = fieldInfo?.GetValue(constantExpression?.Value) as ModdableObject;
            }
            else
            {
                var constantExpression = methodCallExpression.Arguments?.Last() as ConstantExpression;
                entity = constantExpression?.Value as ModdableObject;
            }

            EntityId = entity?.Id;
            FuncName = methodInfo.Name;
            return;
        }

        public string? Describe(Graph graph)
        {
            if (!IsCalc)
                return null;

            var src = EntityId != null
                ? graph.Entities.Get(EntityId.Value)?.Name
                : ClassName;

            return $"{src}.{FuncName}".Trim('.');
        }
    }
}
