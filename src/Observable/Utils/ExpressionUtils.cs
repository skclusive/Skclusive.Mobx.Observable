using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Skclusive.Mobx.Observable
{
    public static class ExpressionUtils
    {
        public static string GetPropertySymbol<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return String.Join(".",
                GetMembersOnPath(expression.Body as MemberExpression)
                    .Select(m => m.Member.Name)
                    .Reverse());
        }

        private static IEnumerable<MemberExpression> GetMembersOnPath(MemberExpression expression)
        {
            while (expression != null)
            {
                yield return expression;
                expression = expression.Expression as MemberExpression;
            }
        }

        public static PropertyInfo GetProperty<T, P>(Expression<Func<T, P>> expr)
        {
            return ((MemberExpression)expr.Body)
                .Member as PropertyInfo;
        }

        public static string GetMethodSymbol<M>(Expression<Action<M>> expr)
        {
            return ((MethodCallExpression)expr.Body)
                .Method.Name;
        }

        public static MethodInfo GetMethod<M>(Expression<Action<M>> expr)
        {
            return ((MethodCallExpression)expr.Body)
                .Method
                .GetGenericMethodDefinition();
        }

        public static Expression<Func<TToModel, TToProperty>> Cast<TFromModel, TFromProperty, TToModel, TToProperty>(Expression<Func<TFromModel, TFromProperty>> expression)
        {
            Expression converted = Expression.Convert(expression.Body, typeof(TToProperty));

            return Expression.Lambda<Func<TToModel, TToProperty>>(converted, expression.Parameters);
        }

        public static Func<P> MakeGetDelegate<T, P>(T target, MethodInfo @get)
        {
            var f = (Func<P>)Delegate.CreateDelegate(typeof(Func<P>), target, @get);

            return () => f();
        }

        public static Func<T, P> MakeGetDelegate<T, P>(MethodInfo @get)
        {
            var f = (Func<T, P>)Delegate.CreateDelegate(typeof(Func<T, P>), @get);

            return (x) => f(x);
        }

        public static Func<T, P> GetPropertyDelegate<T, P>(Expression<Func<T, P>> expr)
        {
            return MakeGetDelegate<T, P>(GetProperty<T, P>(expr).GetGetMethod());
        }
    }
}
