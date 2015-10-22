using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            if (expression.Body is MemberExpression == false)
                throw new ArgumentException();

            var memberExpression = (MemberExpression)expression.Body;

            var modelLambda = Expression.Lambda<Func<object>>(memberExpression.Expression);
            var modelFunc = modelLambda.Compile();
            var model = modelFunc.Invoke();

            if (memberExpression.Member is PropertyInfo == false)
                throw new ArgumentException();

            var propertyInfo = (PropertyInfo)memberExpression.Member;

            dynamic value = propertyInfo.GetValue(model);

            var genericProperties = db.GetType().GetProperties()
                .Where(t => t.PropertyType.IsGenericType);
            var dbSetProperty = genericProperties.Single(t =>
                t.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                && t.PropertyType.GetGenericArguments().First() == value.GetType());
            dynamic dbSet = dbSetProperty.GetValue(db);

            var dbValue = dbSet.Find(value.Id);
            propertyInfo.SetValue(model, dbValue);
        }
    }
}