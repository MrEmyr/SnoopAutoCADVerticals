// ErrorLogService.cs - Enhanced error logging and diagnostics service
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnifiedSnoop.Core.Helpers;

#if NET8_0_OR_GREATER
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#else
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace UnifiedSnoop.Services
{
    /// <summary>
    /// Provides centralized error logging and diagnostics for UnifiedSnoop.
    /// </summary>
    public class ErrorLogService
    {
        #region Singleton

        private static readonly object _lock = new object();
        #if NET8_0_OR_GREATER
        private static ErrorLogService? _instance;
        #else
        private static ErrorLogService _instance;
        #endif

        public static ErrorLogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ErrorLogService();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private readonly List<LogEntry> _logEntries;
        private readonly object _logLock = new object();
        #if NET8_0_OR_GREATER
        private readonly string? _logFilePath;
        #else
        private readonly string _logFilePath;
        #endif
        private bool _enableFileLogging;
        private bool _enableConsoleLogging;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the error log service.
        /// </summary>
        private ErrorLogService()
        {
            _logEntries = new List<LogEntry>();
            _enableFileLogging = true;
            _enableConsoleLogging = true;

            // Set log file path to bundle directory
            try
            {
                string bundlePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle";
                Directory.CreateDirectory(bundlePath);
                
                // Get version number for log filename
                string version = VersionHelper.GetVersionNumber();
                string versionSuffix = version != "Unknown" ? $"_v{version}" : "";
                
                _logFilePath = Path.Combine(bundlePath, $"UnifiedSnoop{versionSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            }
            catch
            {
                // Fallback to temp directory if bundle path fails
                try
                {
                    string tempPath = Path.GetTempPath();
                    _logFilePath = Path.Combine(tempPath, $"UnifiedSnoop_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                }
                catch
                {
                    _logFilePath = null;
                    _enableFileLogging = false;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Logs an error message.
        /// </summary>
        #if NET8_0_OR_GREATER
        public void LogError(string message, Exception? exception = null, string? context = null)
        #else
        public void LogError(string message, Exception exception = null, string context = null)
        #endif
        {
            Log(LogLevel.Error, message, exception, context);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        #if NET8_0_OR_GREATER
        public void LogWarning(string message, string? context = null)
        #else
        public void LogWarning(string message, string context = null)
        #endif
        {
            Log(LogLevel.Warning, message, null, context);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        #if NET8_0_OR_GREATER
        public void LogInfo(string message, string? context = null)
        #else
        public void LogInfo(string message, string context = null)
        #endif
        {
            Log(LogLevel.Info, message, null, context);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        #if NET8_0_OR_GREATER
        public void LogDebug(string message, string? context = null)
        #else
        public void LogDebug(string message, string context = null)
        #endif
        {
            Log(LogLevel.Debug, message, null, context);
        }

        /// <summary>
        /// Gets all log entries.
        /// </summary>
        public List<LogEntry> GetLogEntries()
        {
            lock (_logLock)
            {
                return new List<LogEntry>(_logEntries);
            }
        }

        /// <summary>
        /// Gets log entries filtered by level.
        /// </summary>
        public List<LogEntry> GetLogEntries(LogLevel level)
        {
            lock (_logLock)
            {
                return _logEntries.FindAll(e => e.Level == level);
            }
        }

        /// <summary>
        /// Clears all log entries.
        /// </summary>
        public void ClearLogs()
        {
            lock (_logLock)
            {
                _logEntries.Clear();
            }
        }

        /// <summary>
        /// Exports all logs to a file.
        /// </summary>
        public bool ExportLogs(string filePath)
        {
            try
            {
                lock (_logLock)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("UnifiedSnoop Error Log");
                    sb.AppendLine($"Version: {VersionHelper.GetVersionNumber()}");
                    sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"Total Entries: {_logEntries.Count}");
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine();

                    foreach (var entry in _logEntries)
                    {
                        sb.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}");
                        if (!string.IsNullOrEmpty(entry.Context))
                            sb.AppendLine($"  Context: {entry.Context}");
                        if (entry.Exception != null)
                            sb.AppendLine($"  Exception: {entry.Exception}");
                        sb.AppendLine();
                    }

                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the current log file.
        /// </summary>
        public string GetLogFilePath()
        {
            return _logFilePath ?? "[Logging disabled]";
        }

        /// <summary>
        /// Enables or disables file logging.
        /// </summary>
        public void SetFileLogging(bool enabled)
        {
            _enableFileLogging = enabled && _logFilePath != null;
        }

        /// <summary>
        /// Enables or disables console logging.
        /// </summary>
        public void SetConsoleLogging(bool enabled)
        {
            _enableConsoleLogging = enabled;
        }

        #endregion

        #region Private Methods

        #if NET8_0_OR_GREATER
        private void Log(LogLevel level, string message, Exception? exception, string? context)
        #else
        private void Log(LogLevel level, string message, Exception exception, string context)
        #endif
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message ?? "[null]",
                Context = context,
                Exception = exception
            };

            lock (_logLock)
            {
                _logEntries.Add(entry);

                // Keep only last 1000 entries in memory
                if (_logEntries.Count > 1000)
                {
                    _logEntries.RemoveAt(0);
                }
            }

            // Write to console (Debug output) and AutoCAD command line
            if (_enableConsoleLogging)
            {
                string logMessage = $"[UnifiedSnoop] [{level}] {message}";
                if (!string.IsNullOrEmpty(context))
                    logMessage += $" (Context: {context})";
                
                System.Diagnostics.Debug.WriteLine(logMessage);
                
                if (exception != null)
                    System.Diagnostics.Debug.WriteLine($"  Exception: {exception}");

                // Write to AutoCAD command line for errors and warnings
                if (level == LogLevel.Error || level == LogLevel.Warning)
                {
                    try
                    {
                        var doc = AcadApp.DocumentManager.MdiActiveDocument;
                        if (doc != null)
                        {
                            doc.Editor.WriteMessage($"\n{logMessage}\n");
                            if (exception != null)
                            {
                                doc.Editor.WriteMessage($"  Exception: {exception.Message}\n");
                                if (exception.StackTrace != null)
                                    doc.Editor.WriteMessage($"  Stack: {exception.StackTrace.Split('\n')[0]}\n");
                            }
                        }
                    }
                    catch
                    {
                        // Silently fail if AutoCAD context is not available
                    }
                }
            }

            // Write to file
            if (_enableFileLogging && _logFilePath != null)
            {
                try
                {
                    lock (_logLock)
                    {
                        using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                        {
                            writer.WriteLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                            if (!string.IsNullOrEmpty(context))
                                writer.WriteLine($"  Context: {context}");
                            if (exception != null)
                                writer.WriteLine($"  Exception: {exception}");
                            writer.WriteLine();
                        }
                    }
                }
                catch
                {
                    // Silently fail if we can't write to log file
                }
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Represents a single log entry.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        #if NET8_0_OR_GREATER
        public string? Message { get; set; }
        public string? Context { get; set; }
        public Exception? Exception { get; set; }
        #else
        public string Message { get; set; }
        public string Context { get; set; }
        public Exception Exception { get; set; }
        #endif
    }

    /// <summary>
    /// Log level enumeration.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    #endregion
}

