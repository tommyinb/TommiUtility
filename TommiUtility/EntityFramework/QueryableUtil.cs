using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(property1 != null);
            Contract.Requires<ArgumentNullException>(property2 != null);
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            var propertyName1 = PropertyName(property1);
            var propertyName2 = PropertyName(property2);

            var result = source.Include(propertyName1 + "." + propertyName2);
            Contract.Assume(result != null);
            return result;
        }
        public static IQueryable<T> Include<T, TProperty1, TProperty2>(
            this IQueryable<T> source,
            Expression<Func<T, TProperty1>> property1,
            Expression<Func<TProperty1, TProperty2>> property2) where T : class
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(property1 != null);
            Contract.Requires<ArgumentNullException>(property2 != null);
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            var propertyName1 = PropertyName(property1);
            var propertyName2 = PropertyName(property2);

            var result = source.Include(propertyName1 + "." + propertyName2);
            Contract.Assume(result != null);
            return result;
        }

        private static string PropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (expression.Body is MemberExpression == false) throw new ArgumentException();

            MemberExpression memberExpression = (MemberExpression)expression.Body;
            MemberInfo memberInfo = memberExpression.Member;
            return memberInfo.Name;
        }
    }
}
