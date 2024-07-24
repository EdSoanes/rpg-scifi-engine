using Rpg.ModObjects.Reflection.ArgFactories;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethodFactory
    {
        private static IRpgArgFactory[]? _argFactories;
        private static IRpgArgFactory[] GetArgFactories()
        {
            if (_argFactories == null)
            {
                var nspc = typeof(RpgObject).Namespace!;
                var types = RpgReflection.ScanForTypes<IRpgArgFactory>()
                    .Where(x => !x.IsAbstract && !string.IsNullOrEmpty(x.Namespace) && !x.Namespace.StartsWith(nspc));

                var factories = types
                    .Select(x => Activator.CreateInstance(x) as IRpgArgFactory)
                    .Where(x => x != null)
                    .Cast<IRpgArgFactory>()
                    .ToList();

                factories.Add(Activator.CreateInstance<RpgObjectArgFactory>());
                factories.Add(Activator.CreateInstance<IntegerArgFactory>());
                factories.Add(Activator.CreateInstance<DiceArgFactory>());
                factories.Add(Activator.CreateInstance<DefaultArgFactory>());

                _argFactories = factories.ToArray();
            }

            return _argFactories;
        }

        public static RpgMethod<TOwner, TReturn> Create<TOwner, TInput, TReturn>(Expression<Func<Func<TInput, TReturn>>>? expression)
            where TOwner : class
        {
            if (expression == null)
                throw new InvalidOperationException("No method name on RpgMethod expression");

            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MethodInfo)methodInfoExpression.Value!;

            if (methodInfo.IsStatic)
            {
                var staticMethod = new RpgMethod<TOwner, TReturn>
                {
                    ClassName = methodInfo.DeclaringType!.Name,
                    MethodName = methodInfo.Name,
                    Args = CreateArgs(methodInfo.GetParameters())
                };

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

            var rpgMethod = new RpgMethod<TOwner, TReturn>
            {
                MethodName = methodInfo.Name,
                Args = CreateArgs(methodInfo.GetParameters())
            };

            return rpgMethod;
        }

        public RpgMethod<TOwner, TReturn>? Create<TOwner, TReturn>(TOwner obj, string methodName)
            where TOwner : class
        {
            var type = obj.GetType();
            var methodInfo = RpgReflection.ScanForMethod<TReturn>(type, methodName);
            if (methodInfo == null)
                return null;

            var rpgMethod = new RpgMethod<TOwner, TReturn>
            {
                MethodName = methodName,
                ClassName = methodInfo.IsStatic ? type.Name : null,
                Args = CreateArgs(methodInfo.GetParameters())
            };

            return rpgMethod;
        }

        private static RpgArg[] CreateArgs(ParameterInfo[] parameters)
        {
            var args = parameters
                .Select(x => CreateArg(x))
                .Where(x => x != null)
                .Cast<RpgArg>()
                .ToArray();

            return args;
        }

        private static RpgArg? CreateArg(ParameterInfo parameterInfo)
        {
            var factory = GetArgFactories().FirstOrDefault(x => x.CanCreate(parameterInfo));
            return factory?.Create(parameterInfo);
        }
    }
}
