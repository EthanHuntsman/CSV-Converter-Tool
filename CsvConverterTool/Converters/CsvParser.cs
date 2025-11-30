using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvConverterTool
{
    public class CsvParser
    {
        /// <summary>
        /// Auto-detects a delimiter based on the first non-empty line.
        /// </summary>
        public char? DetectDelimiter(string filePath, char[]? candidates = null)
        {
            candidates ??= new[] { ',', ';', '\t', '|' };

            string? firstLine = File.ReadLines(filePath)
                .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));

            if (string.IsNullOrEmpty(firstLine))
                return null;

            char? bestDelimiter = null;
            int bestCount = 0;

            foreach (char c in candidates)
            {
                int count = firstLine.Count(ch => ch == c);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestDelimiter = c;
                }
            }

            return bestCount > 0 ? bestDelimiter : null;
        }

        /// <summary>
        /// Parses a delimited text file into a DataTable using TextFieldParser.
        /// </summary>
        public DataTable ParseToDataTable(string filePath, char delimiter, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            var dt = new DataTable();

            using (var parser = new TextFieldParser(filePath, encoding))
            {
                parser.SetDelimiters(delimiter.ToString());
                parser.HasFieldsEnclosedInQuotes = true;

                // Header row
                if (!parser.EndOfData)
                {
                    string[]? headers = parser.ReadFields();
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            string columnName = string.IsNullOrWhiteSpace(header)
                                ? "Column" + (dt.Columns.Count + 1)
                                : header;

                            // Ensure unique column names
                            if (dt.Columns.Contains(columnName))
                            {
                                int suffix = 1;
                                string baseName = columnName;
                                while (dt.Columns.Contains(columnName))
                                {
                                    columnName = $"{baseName}_{suffix++}";
                                }
                            }

                            dt.Columns.Add(columnName);
                        }
                    }
                }

                // Data rows
                while (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();
                    if (fields == null)
                        continue;

                    var row = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        row[i] = i < fields.Length ? fields[i] : string.Empty;
                    }
                    dt.Rows.Add(row);
                }
            }

            return dt;
        }
    }
}
