using Newtonsoft.Json;
using Rpg.ModObjects.Reflection.Args;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgMethod
    {
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
                Args = RpgMethodArgs.CreateArgs(methodInfo)
            };

            var returnType = methodInfo.ReturnType != null
                ? Nullable.GetUnderlyingType(methodInfo.ReturnType) ?? methodInfo.ReturnType
                : null;

            rpgMethod.ReturnTypeName = returnType?.Name;
            rpgMethod.ReturnQualifiedTypeName = returnType?.AssemblyQualifiedName;

            return rpgMethod;
        }
    }

    public class RpgMethod<TOwner, TReturn> : RpgMethod
        where TOwner : class
    {
        [JsonProperty] public string? EntityId { get; internal set; }
        [JsonProperty] public string? ClassName { get; internal set; }
        [JsonProperty] public string MethodName { get; internal set; }
        [JsonProperty] public string? ReturnTypeName { get; internal set; }
        [JsonProperty] public string? ReturnQualifiedTypeName { get; internal set; }
        [JsonProperty] public bool ReturnIsNullable { get; internal set; }

        [JsonProperty] public RpgArg[] Args { get; internal set; } = Array.Empty<RpgArg>();

        public RpgMethod() { }

        public string FullName { get => IsStatic ? $"{ClassName}.{MethodName}" : MethodName; }
        public bool IsStatic { get => !string.IsNullOrEmpty(ClassName); }

        //public RpgArgSet CreateArgSet()
        //{
        //    var res = new RpgArgSet();
        //    foreach (var arg in Args)
        //    {
        //        res.Args.Add(arg.Name, arg);
        //        res.ArgValues.Add(arg.Name, null);
        //    }

        //    return res;
        //}
        //public TReturn Execute(RpgArgSet argSet)
        //    => ExecuteMethod(FullName, argSet.ToArgs())!;

        public TReturn Execute(Dictionary<string, object?> args)
            => ExecuteMethod(FullName, args.Values.ToArray())!;

        //public TReturn Execute(TOwner owner, RpgArgSet argSet)
        //    => ExecuteMethod(owner, MethodName, argSet.ToArgs())!;

        public TReturn Execute(TOwner owner, Dictionary<string, object?> args)
            => ExecuteMethod(owner, MethodName, args.Values.ToArray())!;

        //internal static void ExecuteMethod(string staticMethod, object?[]? args = null)
        //{
        //    var methodInfo = RpgTypeScan.ScanForMethod(staticMethod);
        //    methodInfo.Invoke(null, args);
        //}

        internal static TReturn? ExecuteMethod(string staticMethod, object?[]? args = null)
        {
            var methodInfo = RpgTypeScan.ForMethod(staticMethod);
            return (TReturn?)methodInfo.Invoke(null, args);
        }

        //internal static void ExecuteMethod(object obj, string methodName, object?[]? args = null)
        //{
        //    var methodInfo = RpgTypeScan.ScanForMethod(obj.GetType(), methodName);
        //    methodInfo.Invoke(obj, args);
        //}

        internal static TReturn? ExecuteMethod(object obj, string methodName, object?[]? args = null)
        {
            var methodInfo = RpgTypeScan.ForMethod(obj.GetType(), methodName);
            return (TReturn?)methodInfo.Invoke(obj, args);
        }
    }
}
