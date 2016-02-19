using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace iPanel.Utility
{
    public static class DbContextUtil
    {
        public static void RefillById<T>(this DbContext db, Expression<Func<T>> expression)
        {
            Contract.Requires<ArgumentNullException>(db != null);
            Contract.Requires<ArgumentNullException>(expression != null);

            if (expression.Body is MemberExpression == false) throw new ArgumentException();
            var memberExpression = (MemberExpression)expression.Body;

            if (memberExpression.Expression == null) throw new ArgumentException();
            var modelLambda = Expression.Lambda<Func<object>>(memberExpression.Expression);
            var modelFunc = modelLambda.Compile();
            var model = modelFunc.Invoke();

            if (memberExpression.Member is PropertyInfo == false) throw new ArgumentException();
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            var dbType = db.GetType();
            Contract.Assume(dbType != null);
            var dbProperties = dbType.GetProperties();
            if (dbProperties.Any() == false) throw new ArgumentException();

            var dbSetProperty = dbProperties.SingleOrDefault(t =>
                t.PropertyType.IsGenericType
                && t.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && t.PropertyType.GetGenericArguments().Count() == 1
                && t.PropertyType.GetGenericArguments().First() == propertyInfo.PropertyType);
            if (dbSetProperty == null) throw new ArgumentException();

            dynamic dbSet = dbSetProperty.GetValue(db);

            dynamic item = propertyInfo.GetValue(model);

            var dbItem = dbSet.Find(item.Id);

            propertyInfo.SetValue(model, dbItem);
        }
    }
}