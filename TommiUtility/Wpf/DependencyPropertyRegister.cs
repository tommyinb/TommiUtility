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

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner));
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, TProperty defaultValue)
        {
            Contract.Requires<ArgumentNullException>(property != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(defaultValue));
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, ValidateValueCallback validation)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(validation != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(default(TProperty)), validation);
        }

        public DependencyProperty Register<TProperty>(Expression<Func<TOwner, TProperty>> property, TProperty defaultValue, ValidateValueCallback validation)
        {
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(validation != null);

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            return DependencyProperty.Register(propertyInfo.Name, propertyInfo.PropertyType, typeof(TOwner), new PropertyMetadata(defaultValue), validation);
        }
    }
}
