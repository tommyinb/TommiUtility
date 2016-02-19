using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TommiUtility.Wpf
{
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string == false) return DependencyProperty.UnsetValue;

            var matches = Regex.Matches((string)parameter, @"\((?<false>[^\s,]+)\)|(?<true>[^,\s]+)");
            var enumValues = matches.Cast<Match>().ToLookup(
                keySelector: t => t.Groups["true"].Success,
                elementSelector: t => t.Groups["true"].Success ? t.Groups["true"].Value : t.Groups["false"].Value);

            if (targetType == null) return DependencyProperty.UnsetValue;

            if (targetType.IsEnum)
            {
                if (value is bool == false) return DependencyProperty.UnsetValue;

                var outputValues = enumValues[(bool)value];
                if (outputValues.Any() == false) return DependencyProperty.UnsetValue;

                var outputValue = outputValues.First();
                Contract.Assume(outputValue != null);

                return Enum.Parse(targetType, outputValue);
            }
            else if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (value == null) return DependencyProperty.UnsetValue;

                var inputType = value.GetType();
                if (inputType.IsEnum == false) return DependencyProperty.UnsetValue;

                var trueValues = enumValues[true];
                var valueText = value.ToString();
                return trueValues.Contains(valueText);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
