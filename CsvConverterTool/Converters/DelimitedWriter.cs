using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvConverterTool
{
    public class DelimitedWriter
    {
        /// <summary>
        /// Writes a DataTable as a delimited file (CSV/TSV/etc.).
        /// </summary>
        public void Save(DataTable table, string path, char delimiter, Encoding encoding)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

            using var writer = new StreamWriter(path, false, encoding);

            // Header
            string[] headers = table.Columns.Cast<DataColumn>()
                .Select(c => EscapeField(c.ColumnName, delimiter))
                .ToArray();
            writer.WriteLine(string.Join(delimiter, headers));

            // Rows
            foreach (DataRow row in table.Rows)
            {
                string[] fields = table.Columns.Cast<DataColumn>()
                    .Select(c => EscapeField(row[c]?.ToString() ?? string.Empty, delimiter))
                    .ToArray();

                writer.WriteLine(string.Join(delimiter, fields));
            }
        }

        private static string EscapeField(string value, char delimiter)
        {
            bool mustQuote = value.Contains(delimiter)
                             || value.Contains('"')
                             || value.Contains('\n')
                             || value.Contains('\r');

            if (mustQuote)
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
