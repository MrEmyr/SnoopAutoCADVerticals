// ExportService.cs - Service for exporting object properties to Excel/CSV
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Services
{
    /// <summary>
    /// Provides services for exporting object properties to various formats (CSV, Excel, JSON).
    /// </summary>
    public class ExportService
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private readonly CollectorRegistry? _collectorRegistry;
        #else
        private readonly CollectorRegistry _collectorRegistry;
        #endif

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ExportService.
        /// </summary>
        public ExportService()
        {
            _collectorRegistry = CollectorRegistry.Instance;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Exports a single object's properties to a CSV file.
        /// </summary>
        /// <param name="obj">The object to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the CSV file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportToCSV(object obj, Transaction trans, string? filePath = null)
        #else
        public bool ExportToCSV(object obj, Transaction trans, string filePath = null)
        #endif
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Get properties
                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");
                
                var properties = _collectorRegistry.Collect(obj, trans);

                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("CSV Files|*.csv", "Export to CSV");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                // Build CSV content
                var csv = new StringBuilder();
                csv.AppendLine("Property,Type,Value");

                foreach (var prop in properties)
                {
                    string value = prop.HasError ? $"[Error: {prop.ErrorMessage}]" : (prop.Value ?? "[null]");
                    string safeName = prop.Name ?? string.Empty;
                    string safeType = prop.Type ?? string.Empty;
                    csv.AppendLine($"{EscapeCSV(safeName)},{EscapeCSV(safeType)},{EscapeCSV(value)}");
                }

                // Write to file
                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to CSV: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Exports multiple objects' properties to a CSV file.
        /// </summary>
        /// <param name="objects">The objects to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the CSV file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportMultipleToCSV(List<object> objects, Transaction trans, string? filePath = null)
        #else
        public bool ExportMultipleToCSV(List<object> objects, Transaction trans, string filePath = null)
        #endif
        {
            if (objects == null || objects.Count == 0)
                throw new ArgumentException("No objects to export", nameof(objects));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("CSV Files|*.csv", "Export Multiple Objects to CSV");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                // Build CSV content
                var csv = new StringBuilder();
                csv.AppendLine("Object,Property,Type,Value");

                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");

                int objectIndex = 0;
                foreach (var obj in objects)
                {
                    objectIndex++;
                    string objectName = GetObjectName(obj);
                    var properties = _collectorRegistry.Collect(obj, trans);

                    foreach (var prop in properties)
                    {
                        string value = prop.HasError ? $"[Error: {prop.ErrorMessage}]" : (prop.Value ?? "[null]");
                        string safeName = prop.Name ?? string.Empty;
                        string safeType = prop.Type ?? string.Empty;
                        csv.AppendLine($"{EscapeCSV(objectName)},{EscapeCSV(safeName)},{EscapeCSV(safeType)},{EscapeCSV(value)}");
                    }
                }

                // Write to file
                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to CSV: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Exports a single object's properties to an Excel-compatible tab-delimited file.
        /// </summary>
        /// <param name="obj">The object to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportToExcel(object obj, Transaction trans, string? filePath = null)
        #else
        public bool ExportToExcel(object obj, Transaction trans, string filePath = null)
        #endif
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Get properties
                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");
                
                var properties = _collectorRegistry.Collect(obj, trans);

                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("Text Files|*.txt|All Files|*.*", "Export to Excel (Tab-Delimited)");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                // Build tab-delimited content
                var content = new StringBuilder();
                content.AppendLine("Property\tType\tValue");

                foreach (var prop in properties)
                {
                    string value = prop.HasError ? $"[Error: {prop.ErrorMessage}]" : (prop.Value ?? "[null]");
                    content.AppendLine($"{prop.Name}\t{prop.Type}\t{value}");
                }

                // Write to file
                File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Exports multiple objects to an Excel-compatible tab-delimited file.
        /// </summary>
        /// <param name="objects">The objects to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportMultipleToExcel(List<object> objects, Transaction trans, string? filePath = null)
        #else
        public bool ExportMultipleToExcel(List<object> objects, Transaction trans, string filePath = null)
        #endif
        {
            if (objects == null || objects.Count == 0)
                throw new ArgumentException("No objects to export", nameof(objects));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("Text Files|*.txt|All Files|*.*", "Export Multiple Objects to Excel");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                // Build tab-delimited content
                var content = new StringBuilder();
                content.AppendLine("Object\tProperty\tType\tValue");

                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");

                foreach (var obj in objects)
                {
                    string objectName = GetObjectName(obj);
                    var properties = _collectorRegistry.Collect(obj, trans);

                    foreach (var prop in properties)
                    {
                        string value = prop.HasError ? $"[Error: {prop.ErrorMessage}]" : (prop.Value ?? "[null]");
                        content.AppendLine($"{objectName}\t{prop.Name}\t{prop.Type}\t{value}");
                    }
                }

                // Write to file
                File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Exports a single object's properties to a JSON file.
        /// </summary>
        /// <param name="obj">The object to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the JSON file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportToJSON(object obj, Transaction trans, string? filePath = null)
        #else
        public bool ExportToJSON(object obj, Transaction trans, string filePath = null)
        #endif
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Get properties
                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");
                
                var properties = _collectorRegistry.Collect(obj, trans);

                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("JSON Files|*.json|All Files|*.*", "Export to JSON");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                // Build JSON content
                var json = new StringBuilder();
                json.AppendLine("{");
                json.AppendLine($"  \"ObjectType\": {EscapeJSON(obj.GetType().Name)},");
                json.AppendLine($"  \"ExportDate\": {EscapeJSON(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))},");
                json.AppendLine("  \"Properties\": [");

                for (int i = 0; i < properties.Count; i++)
                {
                    var prop = properties[i];
                    json.AppendLine("    {");
                    json.AppendLine($"      \"Name\": {EscapeJSON(prop.Name ?? string.Empty)},");
                    json.AppendLine($"      \"Type\": {EscapeJSON(prop.Type ?? string.Empty)},");
                    
                    if (prop.HasError)
                    {
                        json.AppendLine($"      \"Value\": null,");
                        json.AppendLine($"      \"Error\": {EscapeJSON(prop.ErrorMessage ?? "Unknown error")}");
                    }
                    else
                    {
                        json.AppendLine($"      \"Value\": {EscapeJSON(prop.Value ?? "[null]")}");
                    }
                    
                    if (!string.IsNullOrEmpty(prop.Category))
                    {
                        json.AppendLine($"      ,\"Category\": {EscapeJSON(prop.Category)}");
                    }

                    json.Append("    }");
                    if (i < properties.Count - 1)
                        json.AppendLine(",");
                    else
                        json.AppendLine();
                }

                json.AppendLine("  ]");
                json.AppendLine("}");

                // Write to file
                File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to JSON: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Exports multiple objects' properties to a JSON file.
        /// </summary>
        /// <param name="objects">The objects to export.</param>
        /// <param name="trans">The active transaction.</param>
        /// <param name="filePath">The path to save the JSON file. If null, prompts user.</param>
        /// <returns>True if export was successful; otherwise, false.</returns>
        #if NET8_0_OR_GREATER
        public bool ExportMultipleToJSON(List<object> objects, Transaction trans, string? filePath = null)
        #else
        public bool ExportMultipleToJSON(List<object> objects, Transaction trans, string filePath = null)
        #endif
        {
            if (objects == null || objects.Count == 0)
                throw new ArgumentException("No objects to export", nameof(objects));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                // Prompt for file path if not provided
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = PromptForSaveFile("JSON Files|*.json|All Files|*.*", "Export Multiple Objects to JSON");
                    if (string.IsNullOrEmpty(filePath))
                        return false; // User cancelled
                }

                if (_collectorRegistry == null)
                    throw new InvalidOperationException("CollectorRegistry is not initialized.");

                // Build JSON content
                var json = new StringBuilder();
                json.AppendLine("{");
                json.AppendLine($"  \"ExportDate\": {EscapeJSON(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))},");
                json.AppendLine($"  \"ObjectCount\": {objects.Count},");
                json.AppendLine("  \"Objects\": [");

                for (int objIndex = 0; objIndex < objects.Count; objIndex++)
                {
                    var obj = objects[objIndex];
                    string objectName = GetObjectName(obj);
                    var properties = _collectorRegistry.Collect(obj, trans);

                    json.AppendLine("    {");
                    json.AppendLine($"      \"ObjectName\": {EscapeJSON(objectName)},");
                    json.AppendLine($"      \"ObjectType\": {EscapeJSON(obj.GetType().Name)},");
                    json.AppendLine("      \"Properties\": [");

                    for (int i = 0; i < properties.Count; i++)
                    {
                        var prop = properties[i];
                        json.AppendLine("        {");
                        json.AppendLine($"          \"Name\": {EscapeJSON(prop.Name ?? string.Empty)},");
                        json.AppendLine($"          \"Type\": {EscapeJSON(prop.Type ?? string.Empty)},");
                        
                        if (prop.HasError)
                        {
                            json.AppendLine($"          \"Value\": null,");
                            json.AppendLine($"          \"Error\": {EscapeJSON(prop.ErrorMessage ?? "Unknown error")}");
                        }
                        else
                        {
                            json.AppendLine($"          \"Value\": {EscapeJSON(prop.Value ?? "[null]")}");
                        }

                        json.Append("        }");
                        if (i < properties.Count - 1)
                            json.AppendLine(",");
                        else
                            json.AppendLine();
                    }

                    json.AppendLine("      ]");
                    json.Append("    }");
                    if (objIndex < objects.Count - 1)
                        json.AppendLine(",");
                    else
                        json.AppendLine();
                }

                json.AppendLine("  ]");
                json.AppendLine("}");

                // Write to file
                File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to JSON: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Prompts the user for a file save location.
        /// </summary>
        /// <param name="filter">File filter (e.g., "CSV Files|*.csv").</param>
        /// <param name="title">Dialog title.</param>
        /// <returns>The selected file path, or null if cancelled.</returns>
        #if NET8_0_OR_GREATER
        private string? PromptForSaveFile(string filter, string title)
        #else
        private string PromptForSaveFile(string filter, string title)
        #endif
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = filter;
                saveDialog.Title = title;
                saveDialog.FileName = $"UnifiedSnoop_Export_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveDialog.FileName;
                }
            }

            return null;
        }

        /// <summary>
        /// Escapes a string for CSV format.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            // If value contains comma, newline, or quote, wrap in quotes and escape internal quotes
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        /// <summary>
        /// Gets a display name for an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The object's name or type.</returns>
        private string GetObjectName(object obj)
        {
            if (obj == null)
                return "[null]";

            // Try to get Name property
            var nameProp = obj.GetType().GetProperty("Name");
            if (nameProp != null)
            {
                var name = nameProp.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            // Fallback to type name
            return obj.GetType().Name;
        }

        /// <summary>
        /// Escapes a string for JSON format.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value wrapped in quotes.</returns>
        private string EscapeJSON(string value)
        {
            if (value == null)
                return "null";

            if (string.IsNullOrEmpty(value))
                return "\"\"";

            // Escape special JSON characters
            value = value.Replace("\\", "\\\\")  // Backslash
                        .Replace("\"", "\\\"")  // Quote
                        .Replace("\n", "\\n")   // Newline
                        .Replace("\r", "\\r")   // Carriage return
                        .Replace("\t", "\\t")   // Tab
                        .Replace("\b", "\\b")   // Backspace
                        .Replace("\f", "\\f");  // Form feed

            return $"\"{value}\"";
        }

        #endregion
    }
}

