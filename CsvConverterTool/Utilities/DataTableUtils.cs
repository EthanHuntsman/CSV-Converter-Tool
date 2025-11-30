using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CsvConverterTool
{
    public static class DataTableUtils
    {
        /// <summary>
        /// Returns a new DataTable containing only the selected columns.
        /// If selectedColumns is null/empty, the original table is returned.
        /// </summary>
        public static DataTable BuildFilteredTable(DataTable source, IEnumerable<string>? selectedColumns)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var selected = selectedColumns?.ToList() ?? new List<string>();
            if (selected.Count == 0)
            {
                return source;
            }

            var clone = new DataTable();

            // Add selected columns
            foreach (string colName in selected)
            {
                if (source.Columns.Contains(colName))
                {
                    clone.Columns.Add(colName, typeof(string));
                }
            }

            // Copy rows
            foreach (DataRow row in source.Rows)
            {
                var newRow = clone.NewRow();
                foreach (DataColumn col in clone.Columns)
                {
                    newRow[col.ColumnName] = row[col.ColumnName];
                }
                clone.Rows.Add(newRow);
            }

            return clone;
        }

        /// <summary>
        /// Returns all column names in order.
        /// </summary>
        public static List<string> GetColumnNames(DataTable table)
        {
            return table.Columns
                        .Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .ToList();
        }
    }
}
