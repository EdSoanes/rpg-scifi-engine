using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethod<TOwner, TReturn> 
        where TOwner : class
    {
        [JsonProperty] public string? ClassName { get; protected set; }
        [JsonProperty] public string MethodName { get; protected set; }
        [JsonProperty] protected RpgArgSet ArgSet { get; private set; }

        public string FullName { get => IsStatic ? MethodName : $"{ClassName}.{MethodName}"; }
        public bool IsStatic { get => !string.IsNullOrEmpty(ClassName); }

        [JsonConstructor] protected RpgMethod() { }

        public RpgMethod(string qualifiedClassName, string methodName)
        {
            var type = RpgReflection.ScanForType(qualifiedClassName)!;
            ClassName = type.Name;

            DiscoverMethod(type, methodName);
        }

        public RpgMethod(TOwner entity, string methodName)
        {
            DiscoverMethod(entity.GetType(), methodName);
        }

        public static RpgMethod<TOwner, TReturn> Create<T>(Expression<Func<Func<T, T>>>? expression)
        {
            if (expression == null)
                throw new InvalidOperationException("No method name on RpgMethod expression");

            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MethodInfo)methodInfoExpression.Value!;

            if (methodInfo.IsStatic)
            {
                var staticMethod = new RpgMethod<TOwner, TReturn>(methodInfo.DeclaringType!.Name, methodInfo.Name);
                return staticMethod;
            }

            TOwner? entity;
            var memberExpression = methodCallExpression.Arguments?.Last() as MemberExpression;
            if (memberExpression != null)
            {
                var constantExpression = memberExpression.Expression as ConstantExpression;
                var fieldInfo = memberExpression?.Member as FieldInfo;
                entity = fieldInfo?.GetValue(constantExpression?.Value) as TOwner;
            }
            else
            {
                var constantExpression = methodCallExpression.Arguments?.Last() as ConstantExpression;
                entity = constantExpression?.Value as TOwner;
            }

            var rpgMethod = new RpgMethod<TOwner, TReturn>(entity!, methodInfo.Name);

            return rpgMethod;
        }

        public RpgArgSet Create()
            => ArgSet.Clone();

        public TReturn Execute(RpgArgSet argSet)
        {
            if (!IsStatic)
                throw new InvalidOperationException($"{FullName} is not static");

            var res = this.ExecuteFunction<TReturn>(FullName, argSet.ToArgs())!;
            return res;
        }

        public TReturn Execute(TOwner owner, RpgArgSet argSet)
        {
            var res = owner.ExecuteFunction<TReturn>(MethodName, argSet.ToArgs())!;
            return res;
        }

        private void DiscoverMethod(Type type, string methodName)
        {
            var methodInfo = type.GetMethod(methodName);
            if (methodInfo == null)
                throw new InvalidOperationException($"{methodName}() method not found on {type.Name} class");

            if (!methodInfo.ReturnType.IsAssignableTo(typeof(TReturn)))
                throw new InvalidOperationException($"{methodName}() method on {type.Name} class does not have return type {nameof(TReturn)}");

            MethodName = methodName;
            ArgSet = new RpgArgSet(methodInfo.GetParameters());
        }
    }
}
