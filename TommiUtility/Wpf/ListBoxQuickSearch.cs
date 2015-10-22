using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TommiUtility.Wpf
{
    public sealed class ListBoxQuickSearch : IDisposable
    {
        public ListBoxQuickSearch(ListBox listBox, Func<object, string> wordSelector)
        {
            this.listBox = listBox;
            listBox.KeyDown += KeyDown;

            this.wordSelector = wordSelector;
        }
        public void Dispose()
        {
            listBox.KeyDown -= KeyDown;
        }

        private ListBox listBox;
        private Func<object, string> wordSelector;

        private StringBuilder text = new StringBuilder();
        private DateTime lastTime = DateTime.Now;

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (DateTime.Now - lastTime > new TimeSpan(0, 0, 2))
            {
                text.Clear();
            }

            if (e.Key < Key.A)
                return;
            if (e.Key > Key.Z)
                return;

            text.Append(e.Key.ToString());
            lastTime = DateTime.Now;

            var searchText = text.ToString();
            var startItems = listBox.Items.Cast<object>().Where(t =>
                wordSelector(t).StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
            var containItems = listBox.Items.Cast<object>().Where(t =>
                wordSelector(t).ToUpper().Contains(searchText));

            var searchItem = startItems.Concat(containItems).FirstOrDefault();
            if (searchItem == null)
                return;

            listBox.SelectedItem = searchItem;
            listBox.ScrollIntoView(searchItem);
        }
    }
}
