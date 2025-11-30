using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CsvConverterTool
{
    public partial class MainWindow : Window
    {
        private readonly CsvParser _csvParser = new CsvParser();
        private readonly DelimitedWriter _delimitedWriter = new DelimitedWriter();
        private readonly JsonWriter _jsonWriter = new JsonWriter();

        private DataTable? _currentTable;
        private List<string> _selectedColumns = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        // -------------------------
        // Event handlers
        // -------------------------

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv;*.tsv;*.txt)|*.csv;*.tsv;*.txt|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtSourcePath.Text = openFileDialog.FileName;
                LoadCsv(openFileDialog.FileName);
            }
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null || _currentTable.Rows.Count == 0)
            {
                MessageBox.Show("No data to save. Please load a CSV first.",
                                "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_selectedColumns == null || _selectedColumns.Count == 0)
            {
                MessageBox.Show("No columns selected. Please select at least one column before saving.",
                                "No Columns Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog();

            string format = GetSelectedOutputFormat();
            switch (format)
            {
                case "TSV":
                    saveDialog.Filter = "TSV Files (*.tsv)|*.tsv|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "tsv";
                    break;
                case "JSON":
                    saveDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "json";
                    break;
                default:
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "csv";
                    break;
            }

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    txtStatus.Text = "Saving...";

                    string encodingName = GetSelectedEncodingName();
                    Encoding encoding = encodingName switch
                    {
                        "Shift-JIS" => Encoding.GetEncoding("shift_jis"),
                        "Windows-1252" => Encoding.GetEncoding(1252),
                        _ => new UTF8Encoding(false) // UTF-8 without BOM
                    };

                    string outputPath = saveDialog.FileName;

                    // Filter to selected columns
                    DataTable toSave = DataTableUtils.BuildFilteredTable(_currentTable, _selectedColumns);

                    if (format == "JSON")
                    {
                        _jsonWriter.Save(toSave, outputPath, encoding);
                    }
                    else
                    {
                        char delimiter = format == "TSV" ? '\t' : ',';
                        _delimitedWriter.Save(toSave, outputPath, delimiter, encoding);
                    }

                    txtStatus.Text = "Saved: " + outputPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtStatus.Text = "Error saving file";
                }
            }
        }

        private void btnSelectColumns_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTable == null || _currentTable.Columns.Count == 0)
            {
                MessageBox.Show("No columns to select. Please load a CSV first.",
                                "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var allColumns = DataTableUtils.GetColumnNames(_currentTable);

            var dialog = new ColumnSelectionWindow(allColumns, _selectedColumns)
            {
                Owner = this
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _selectedColumns = dialog.SelectedColumns.ToList();

                // Update preview to show only selected columns
                var previewTable = DataTableUtils.BuildFilteredTable(_currentTable, _selectedColumns);
                dataGridPreview.ItemsSource = previewTable.DefaultView;

                txtStatus.Text = $"Showing {previewTable.Columns.Count} of {_currentTable.Columns.Count} columns, {_currentTable.Rows.Count} rows.";
            }
        }

        // -------------------------
        // Core logic
        // -------------------------

        private void LoadCsv(string filePath)
        {
            try
            {
                txtStatus.Text = "Loading CSV...";

                char? delimiter = GetSelectedDelimiter();
                if (delimiter == null)
                {
                    delimiter = _csvParser.DetectDelimiter(filePath);
                }

                if (delimiter == null)
                {
                    MessageBox.Show($"Could not detect delimiter for:\n{filePath}\n\nPlease choose one manually.",
                                    "Delimiter Detection",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    txtStatus.Text = "Delimiter detection failed";
                    return;
                }

                var encoding = GetSelectedEncoding();
                _currentTable = _csvParser.ParseToDataTable(filePath, delimiter.Value, encoding);

                // By default, all columns selected
                _selectedColumns = DataTableUtils.GetColumnNames(_currentTable);

                // Initial preview shows all columns
                dataGridPreview.ItemsSource = _currentTable.DefaultView;

                Title = $"CSV Converter - {System.IO.Path.GetFileName(filePath)}";
                txtStatus.Text = $"Loaded {_currentTable.Rows.Count} rows, {_currentTable.Columns.Count} columns.";
            }
            catch (Exception ex)
            {
                _currentTable = null;
                dataGridPreview.ItemsSource = null;
                _selectedColumns.Clear();

                MessageBox.Show($"Error loading CSV: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error loading CSV";
            }
        }

        // -------------------------
        // Helper methods
        // -------------------------

        private char? GetSelectedDelimiter()
        {
            if (cmbDelimiter.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag as string;
                if (string.IsNullOrEmpty(tag))
                    return null;

                return tag switch
                {
                    "\\t" => '\t',
                    _ => tag[0]
                };
            }

            return null;
        }

        private string GetSelectedOutputFormat()
        {
            if (cmbOutputFormat.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? "CSV";
            }
            return "CSV";
        }

        private string GetSelectedEncodingName()
        {
            if (cmbEncoding.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? "UTF-8";
            }
            return "UTF-8";
        }

        private Encoding GetSelectedEncoding()
        {
            string encodingName = GetSelectedEncodingName();
            return encodingName switch
            {
                "Shift-JIS" => Encoding.GetEncoding("shift_jis"),
                "Windows-1252" => Encoding.GetEncoding(1252),
                _ => new UTF8Encoding(false)
            };
        }

        private void SetStatus(string message) => txtStatus.Text = message;
    }
}
