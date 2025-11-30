using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CsvConverterTool
{
    public partial class ColumnSelectionWindow : Window
    {
        private readonly List<CheckBox> _checkBoxes = new();

        public IReadOnlyList<string> SelectedColumns { get; private set; } = Array.Empty<string>();

        public ColumnSelectionWindow(IEnumerable<string> allColumns, IEnumerable<string> initiallySelected)
        {
            InitializeComponent();

            var selectedSet = new HashSet<string>(initiallySelected ?? Enumerable.Empty<string>());

            foreach (var columnName in allColumns)
            {
                var cb = new CheckBox
                {
                    Content = columnName,
                    Margin = new Thickness(0, 2, 0, 2),
                    IsChecked = selectedSet.Count == 0 || selectedSet.Contains(columnName)
                };

                _checkBoxes.Add(cb);
                stackColumns.Children.Add(cb);
            }
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cb in _checkBoxes)
            {
                cb.IsChecked = true;
            }
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cb in _checkBoxes)
            {
                cb.IsChecked = false;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var selected = _checkBoxes
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Content?.ToString())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (selected.Count == 0)
            {
                var result = MessageBox.Show(
                    "No columns are selected. This will produce an empty output. Continue?",
                    "No Columns Selected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            SelectedColumns = selected;
            DialogResult = true;   // closes the window and returns true from ShowDialog()
        }
    }
}
