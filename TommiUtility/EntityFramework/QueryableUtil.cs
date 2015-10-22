using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.EntityFramework
{
    public static class QueryableUtil
    {
        public static IQueryable<T> Include<T, TProperty1, TProperty2>(
            this IQueryable<T> source,
            Expression<Func<T, ICollection<TProperty1>>> property1,
            Expression<Func<TProperty1, TProperty2>> property2) where T : class
        {
            var propertyName1 = PropertyName(property1);
            var propertyName2 = PropertyName(property2);

            return source.Include(propertyName1 + "." + propertyName2);
        }
        public static IQueryable<T> Include<T, TProperty1, TProperty2>(
            this IQueryable<T> source,
            Expression<Func<T, TProperty1>> property1,
            Expression<Func<TProperty1, TProperty2>> property2) where T : class
        {
            var propertyName1 = PropertyName(property1);
            var propertyName2 = PropertyName(property2);

            return source.Include(propertyName1 + "." + propertyName2);
        }

        private static string PropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            MemberInfo memberInfo = memberExpression.Member;
            return memberInfo.Name;
        }
    }
}
