using Rpg.ModObjects.Reflection.Args;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethodFactory
    {
        public static Type[] PrimitiveArgTypes => [typeof(int), typeof(Dice), typeof(string)];

        private static IRpgArgFactory[]? _argFactories;

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
                return CreateMethod<TOwner, TReturn>(methodInfo, null);

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

            return CreateMethod<TOwner, TReturn>(methodInfo, entity);
        }

        public static RpgMethod<TOwner, TReturn>? Create<TOwner, TReturn>(TOwner obj, string methodName)
            where TOwner : class
        {
            var type = obj.GetType();
            var methodInfo = RpgTypeScan.ForMethod<TReturn>(type, methodName);
            if (methodInfo == null)
                return null;

            return CreateMethod<TOwner, TReturn>(methodInfo, obj);
        }


        private static RpgMethod<TOwner, TReturn> CreateMethod<TOwner, TReturn>(MethodInfo methodInfo, object? entity)
            where TOwner : class
        {
            var rpgMethod = new RpgMethod<TOwner, TReturn>
            {
                EntityId = (entity as RpgObject)?.Id,
                ClassName = methodInfo.IsStatic ? methodInfo.DeclaringType!.Name : null,
                MethodName = methodInfo.Name,
                ReturnIsNullable = methodInfo.ReturnType != null ? Nullable.GetUnderlyingType(methodInfo.ReturnType) != null : false,
                Args = CreateArgs(methodInfo)
            };

            var returnType = methodInfo.ReturnType != null
                ? Nullable.GetUnderlyingType(methodInfo.ReturnType) ?? methodInfo.ReturnType
                : null;

            rpgMethod.ReturnTypeName = returnType?.Name;
            rpgMethod.ReturnQualifiedTypeName = returnType?.AssemblyQualifiedName;

            return rpgMethod;
        }

        private static IRpgArgFactory[] GetArgFactories()
        {
            if (_argFactories == null)
            {
                var nspc = typeof(RpgObject).Namespace!;
                var types = RpgTypeScan.ForTypes<IRpgArgFactory>()
                    .Where(x => !x.IsAbstract && !string.IsNullOrEmpty(x.Namespace) && !x.Namespace.StartsWith(nspc));

                var factories = types
                    .Select(x => Activator.CreateInstance(x) as IRpgArgFactory)
                    .Where(x => x != null)
                    .Cast<IRpgArgFactory>()
                    .ToList();

                factories.Add(Activator.CreateInstance<RpgObjectArgFactory>());
                factories.Add(Activator.CreateInstance<DefaultObjectFactory>());
                factories.Add(Activator.CreateInstance<IntegerArgFactory>());
                factories.Add(Activator.CreateInstance<DiceArgFactory>());
                factories.Add(Activator.CreateInstance<DefaultArgFactory>());

                _argFactories = factories.ToArray();
            }

            return _argFactories;
        }

        private static RpgArg[] CreateArgs(MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters()
                .Select(CreateArg)
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
