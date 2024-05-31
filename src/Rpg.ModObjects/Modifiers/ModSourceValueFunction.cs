using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSourceValueFunction
    {
        [JsonProperty] public string? EntityId { get; private set; }
        [JsonProperty] public string? ClassName { get; private set; }
        [JsonProperty] public string? FuncName { get; private set; }
        [JsonProperty] public string? FullName { get; private set; }

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
                ClassName = methodInfo.DeclaringType!.Name;
                FuncName = methodInfo.Name;
                FullName = $"{ClassName}.{FuncName}";
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

            EntityId = entity?.Id;
            FuncName = methodInfo.Name;
            FullName = FuncName;

            return;
        }
    }
}
