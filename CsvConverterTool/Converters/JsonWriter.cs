using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CsvConverterTool
{
    public class JsonWriter
    {
        /// <summary>
        /// Writes a DataTable as a JSON array of objects.
        /// </summary>
        public void Save(DataTable table, string path, Encoding encoding)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

            var rows = new List<Dictionary<string, string?>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, string?>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col]?.ToString();
                }
                rows.Add(dict);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(rows, options);
            File.WriteAllText(path, json, encoding);
        }
    }
}
