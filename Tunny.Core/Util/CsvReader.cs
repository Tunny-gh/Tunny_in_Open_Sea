using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tunny.Core.Util
{
    [LoggingAspect]
    public class CsvReader
    {
        private readonly string _filePath;

        public CsvReader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CSV file not found", filePath);
            }

            _filePath = filePath;
        }

        public List<Dictionary<string, string>> ReadFishEggCsv()
        {
            var lines = File.ReadLines(_filePath).ToList();

            string[] headers = lines[0].Split(',');
            var result = new List<Dictionary<string, string>>();

            for (int lineIndex = 1; lineIndex < lines.Count; lineIndex++)
            {
                string[] values = lines[lineIndex].Split(',');

                if (headers.Length != values.Length)
                {
                    throw new InvalidDataException("The number of columns in the CSV file is not consistent.");
                }

                var rowDictionary = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(values[i]))
                    {
                        continue;
                    }
                    rowDictionary[headers[i]] = values[i];
                }

                result.Add(rowDictionary);
            }

            return result;
        }

        public int[] ReadSelectionCsv(CsvType csvType)
        {
            string key = csvType == CsvType.Dashboard
                ? key = "Number"
                : key = "trial_id";

            var lines = File.ReadLines(_filePath).ToList();
            if (lines.Count == 0)
            {
                return Array.Empty<int>();
            }

            string[] headers = lines[0].Split(',');
            int numberColumnIndex = Array.FindIndex(headers, h => h.Trim()
                .Equals(key, StringComparison.OrdinalIgnoreCase));

            if (numberColumnIndex == -1)
            {
                return Array.Empty<int>();
            }

            var numbers = lines.Skip(1)
                .Select(line =>
                {
                    string[] columns = line.Split(',');
                    return int.Parse(columns[numberColumnIndex], CultureInfo.InvariantCulture);
                })
                .ToList();

            return numbers.ToArray();
        }
    }

    public enum CsvType
    {
        Dashboard,
        DesignExplorer,
    }
}
