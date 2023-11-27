using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Parent
    {
        public string Prop1 { get; set; }
        private string Prop2 { get; set; } = "Bananas";
    }

    public class Child : Parent
    {
        public string Child1 { get; set; }
        private string Child2 { get; set; } = "Custard";

        public int DoThing(int num)
        {
            return num + 1;
        }

        public static int DoAnotherThing(int num)
        {
            return num + 1;
        }

    }

    public static class Helper
    {
        public static T RunCalculationMethod<T>(string method, T input, object? obj = null)
        {
            var parts = method.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException($"Specified method {method} is invalid");
            
            var cls = parts[0];
            var mthd = parts[1];
            var type = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(x => x.Name == cls);

            if (type == null)
                throw new ArgumentException($"Class in {method} does not exist");

            var methodInfo = type?.GetMethod(mthd);
            if (methodInfo == null)
                throw new ArgumentException($"Method in {method} does not exist");

            var res = methodInfo.IsStatic
                ? (T?)methodInfo?.Invoke(null, new object?[] { input })
                : (T?)methodInfo?.Invoke(obj, new object?[] { input });

            return res ?? default;
        }

        public static string GetCalculationMethod<T>(Expression<Func<Func<T, T>>> expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Object!;
            var methodInfo = (MemberInfo)methodInfoExpression.Value!;
            return $"{methodInfo.DeclaringType!.Name}.{methodInfo.Name}";
        }
    }
}
