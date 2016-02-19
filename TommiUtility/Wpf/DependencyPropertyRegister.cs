using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Wpf
{
    public class DependencyPropertyRegister<TOwner>
    {
        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Ensures(Contract.Result<DependencyProperty>() != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner));
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, TProperty defaultValue)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Ensures(Contract.Result<DependencyProperty>() != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var dependencyProperty = DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(defaultValue));

            Contract.Assume(dependencyProperty != null);
            return dependencyProperty;
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, ValidateValueCallback validation)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(validation != null);
            Contract.Ensures(Contract.Result<DependencyProperty>() != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var dependencyProperty = DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(default(TProperty)), validation);

            Contract.Assume(dependencyProperty != null);
            return dependencyProperty;
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, TProperty defaultValue, ValidateValueCallback validation)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(validation != null);
            Contract.Ensures(Contract.Result<DependencyProperty>() != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var dependencyProperty = DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(defaultValue), validation);

            Contract.Assume(dependencyProperty != null);
            return dependencyProperty;
        }
    }
}
