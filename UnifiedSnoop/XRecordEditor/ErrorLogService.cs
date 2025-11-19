// ErrorLogService.cs - Error logging service for XRecord Editor
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if NET8_0_OR_GREATER
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#else
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace UnifiedSnoop.XRecordEditor
{
    /// <summary>
    /// Provides centralized error logging for XRecord Editor.
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

        private ErrorLogService()
        {
            _logEntries = new List<LogEntry>();
            _enableFileLogging = true;
            _enableConsoleLogging = true;

            // Set log file path
            try
            {
                string bundlePath = @"C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle";
                Directory.CreateDirectory(bundlePath);
                _logFilePath = Path.Combine(bundlePath, $"XRecordEditor_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            }
            catch
            {
                try
                {
                    string tempPath = Path.GetTempPath();
                    _logFilePath = Path.Combine(tempPath, $"XRecordEditor_{DateTime.Now:yyyyMMdd_HHmmss}.log");
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

        #if NET8_0_OR_GREATER
        public void LogError(string message, Exception? exception = null, string? context = null)
        #else
        public void LogError(string message, Exception exception = null, string context = null)
        #endif
        {
            Log(LogLevel.Error, message, exception, context);
        }

        #if NET8_0_OR_GREATER
        public void LogWarning(string message, string? context = null)
        #else
        public void LogWarning(string message, string context = null)
        #endif
        {
            Log(LogLevel.Warning, message, null, context);
        }

        #if NET8_0_OR_GREATER
        public void LogInfo(string message, string? context = null)
        #else
        public void LogInfo(string message, string context = null)
        #endif
        {
            Log(LogLevel.Info, message, null, context);
        }

        public string GetLogFilePath()
        {
            return _logFilePath ?? "[Logging disabled]";
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

                // Keep only last 1000 entries
                if (_logEntries.Count > 1000)
                {
                    _logEntries.RemoveAt(0);
                }
            }

            // Write to console
            if (_enableConsoleLogging)
            {
                string logMessage = $"[XRecordEditor] [{level}] {message}";
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
                        // Silently fail if AutoCAD context not available
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
                    // Silently fail if we can't write
                }
            }
        }

        #endregion
    }

    #region Supporting Classes

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

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    #endregion
}

