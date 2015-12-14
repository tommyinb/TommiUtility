﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TommiUtility.Wpf
{
    public class StringArrayTextConverter : IValueConverter
    {
        public IEnumerable<string> Seperators { get; set; }
        public StringArrayTextConverter()
        {
            Seperators = new[] { ", ", "," };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is IEnumerable)
            {
                var items = (IEnumerable)value;

                var seperator = Seperators.First();

                return string.Join(seperator, items.Cast<object>().ToArray());
            }
            else
            {
                return value.ToString();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var text = value.ToString();

            return text.Split(Seperators.ToArray(), StringSplitOptions.None);
        }

        [ContractInvariantMethod]
        private void ContractInvariant()
        {
            Contract.Invariant(Seperators != null);
        }
    }
}
