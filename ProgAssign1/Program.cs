using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace Assignment1
{
    public class Program
    {
        private int skippedRowsCount = 0;     // Counter for rows with missing data
        private int validRowsCount = 0;       // Counter for valid rows
        private const string outputFilePath = "Output/Output.csv";
        private const string logFilePath = "Log/log.txt";  // Log file path

        public void DirWalker()
        {
            // Ensure Logs directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            File.WriteAllText(outputFilePath, "FirstName,LastName,StreetNumber,Street,City,Province,PostalCode,Country,PhoneNumber,EmailAddress,Date\n");
            
        }


        public void WalkAndProcessFiles(string rootPath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            TraverseDirectories(rootPath);

            stopwatch.Stop();
            LogExecutionDetails(stopwatch.Elapsed);
        }

        private void TraverseDirectories(string path)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    TraverseDirectories(dir);  // Recursively traverse directories
                }

                foreach (string filePath in Directory.GetFiles(path, "CustomerData*.csv"))
                {
                    string dateFromPath = ExtractDateFromPath(filePath);
                    if (dateFromPath != null)
                    {
                        ProcessCsvFile(filePath, dateFromPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory {path}: {ex.Message}");
            }
        }

        private string ExtractDateFromPath(string filePath)
        {
            var match = Regex.Match(filePath, @"(\d{4})[/\\](\d{2})[/\\](\d{2})");
            if (match.Success)
            {
                return $"{match.Groups[1].Value}/{match.Groups[2].Value}/{match.Groups[3].Value}";
            }
            return null;
        }

        private void ProcessCsvFile(string filePath, string date)
        {
            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Skip header line if present
                    if (!parser.EndOfData) parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        // Ensure all columns have data (10 expected fields)
                        if (fields.Length == 10 && Array.TrueForAll(fields, field => !string.IsNullOrWhiteSpace(field)))
                        {
                            File.AppendAllText(outputFilePath, $"{string.Join(",", fields)},{date}\n");
                            validRowsCount++;
                        }
                        else
                        {
                            skippedRowsCount++;
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        private void LogExecutionDetails(TimeSpan executionTime)
        {
            string logMessage = $"Execution Time: {executionTime}\n" +
                                $"Total Valid Rows: {validRowsCount}\n" +
                                $"Total Skipped Rows: {skippedRowsCount}\n" +
                                $"Timestamp: {DateTime.Now}\n\n";

            File.AppendAllText(logFilePath, logMessage);
            Console.WriteLine("Execution details logged successfully.");
        }

        
         public static void Main()
         {
             Program walker = new Program();
             walker.WalkAndProcessFiles(@"C:\Users\koush\source\repos\Add\Sample Data\");
         }
    }
}
