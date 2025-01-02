using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Reflection
{
    public static class RpgMemberUtilities
    {
        public static string ExpressionToPath<T, TResult>(Expression<Func<T, TResult>>? expression)
        {
            if (expression == null) return null;

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegs = new List<string>();
            pathSegs.Add(memberExpression.Member.Name);
            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegs.Add(memberExpression.Member.Name);
            }

            pathSegs.Reverse();
            var path = string.Join(".", pathSegs);
            return path;
        }
    }
}
