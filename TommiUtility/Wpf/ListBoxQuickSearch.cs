﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(listBox != null);
            Contract.Requires<ArgumentNullException>(wordSelector != null);

            this.listBox = listBox;
            listBox.KeyDown += KeyDown;

            this.wordSelector = wordSelector;
        }
        public void Dispose()
        {
            listBox.KeyDown -= KeyDown;
        }

        private readonly ListBox listBox;
        private readonly Func<object, string> wordSelector;
        private readonly StringBuilder text = new StringBuilder();
        private DateTime lastTime = DateTime.Now;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(listBox != null);
            Contract.Invariant(wordSelector != null);
            Contract.Invariant(text != null);
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (DateTime.Now - lastTime > new TimeSpan(0, 0, 2))
            {
                text.Clear();
            }

            if (e.Key < Key.A) return;
            if (e.Key > Key.Z) return;

            text.Append(e.Key.ToString());
            lastTime = DateTime.Now;

            var searchText = text.ToString();
            var startItems = listBox.Items.Cast<object>().Where(t =>
                wordSelector(t).StartsWith(searchText, StringComparison.OrdinalIgnoreCase));
            var containItems = listBox.Items.Cast<object>().Where(t =>
                wordSelector(t).ToUpper().Contains(searchText.ToUpper()));

            var searchItem = startItems.Concat(containItems).FirstOrDefault();
            if (searchItem == null) return;

            listBox.SelectedItem = searchItem;
            listBox.ScrollIntoView(searchItem);
        }
    }
}
