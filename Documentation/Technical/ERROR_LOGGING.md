# UnifiedSnoop - Error Logging & Diagnostics

## Overview

UnifiedSnoop includes comprehensive error logging that automatically captures all errors and displays them both in the AutoCAD command line and in persistent log files.

---

## Features

### 1. AutoCAD Command Line Mirroring

All errors and warnings are automatically displayed in the AutoCAD command line for immediate visibility.

**Example Output:**
```
[UnifiedSnoop] [Error] Error opening UnifiedSnoop UI
  Exception: SplitterDistance must be between Panel1MinSize and Width - Panel2MinSize
  Stack: at System.Windows.Forms.SplitContainer.set_SplitterDistance...
Log file: C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\UnifiedSnoop_20251117_100530.log
```

**What's Included:**
- Error/warning level indicator
- Descriptive message
- Exception details
- First line of stack trace
- Log file path for detailed information

### 2. Persistent Log Files

**Location:**
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
```

**Filename Format:**
```
UnifiedSnoop_YYYYMMDD_HHMMSS.log
```
Example: `UnifiedSnoop_20251117_100530.log`

**Log File Contents:**
- Timestamp for each entry
- Log level (Error, Warning, Info, Debug)
- Descriptive message
- Context information (command, method, etc.)
- Full exception details
- Complete stack traces

**Sample Log Entry:**
```
[2025-11-17 10:05:30] [Error] Error opening UnifiedSnoop UI
  Context: SNOOP command
  Exception: System.ArgumentException: SplitterDistance must be between Panel1MinSize and Width - Panel2MinSize.
   at System.Windows.Forms.SplitContainer.set_SplitterDistance(Int32 value)
   at UnifiedSnoop.UI.MainSnoopForm.InitializeForm()
   at UnifiedSnoop.UI.MainSnoopForm..ctor(Database database, TransactionHelper transactionHelper)
```

### 3. Error Handling Coverage

UnifiedSnoop has error handling in all critical areas:

**Commands:**
- `SNOOP` - Main snoop UI command
- `SNOOPHANDLE` - Snoop by handle command
- `SNOOPVERSION` - Version information

**UI Components:**
- Form initialization
- Splitter distance calculation
- Object property collection
- Export operations
- Comparison operations
- Bookmark management

**Services:**
- Database operations
- Property collection
- Export service
- Bookmark service

### 4. Automatic Fallbacks

When errors occur, UnifiedSnoop attempts to recover gracefully:

**Example - Splitter Distance Error:**
1. Attempts to calculate optimal splitter position
2. If error occurs, logs the error
3. Falls back to safe default (300 pixels)
4. If that fails, continues without setting distance
5. User sees error in command line and log

---

## Using the Logs

### Finding Log Files

1. Navigate to: `C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\`
2. Look for files named `UnifiedSnoop_*.log`
3. Most recent log has the latest timestamp in filename

### Reading Log Files

Log files are plain text and can be opened with any text editor:
- Notepad
- Notepad++
- Visual Studio Code
- Or any text editor

### Sharing Log Files

When reporting issues:
1. Reproduce the error
2. Note the log file name shown in command line
3. Locate the log file in the bundle directory
4. Attach the log file to your bug report

---

## Log Levels

### Error
Critical errors that prevent operations from completing.
- **Display:** AutoCAD command line + log file
- **Action Required:** Review and fix

### Warning
Non-critical issues that don't prevent operation.
- **Display:** AutoCAD command line + log file
- **Action Required:** Review if persistent

### Info
Informational messages about operations.
- **Display:** Log file only
- **Action Required:** None, for reference

### Debug
Detailed diagnostic information.
- **Display:** Log file only
- **Action Required:** Used for troubleshooting

---

## Configuration

### Enabling/Disabling Logging

By default, all logging is enabled. To change:

```csharp
// Disable file logging
ErrorLogService.Instance.SetFileLogging(false);

// Disable console (command line) logging
ErrorLogService.Instance.SetConsoleLogging(false);
```

### Viewing Log Path

To see the current log file path:
```
Type: SNOOPVERSION
```

Or programmatically:
```csharp
string logPath = ErrorLogService.Instance.GetLogFilePath();
```

---

## Troubleshooting

### Log File Not Created

**Possible Causes:**
1. Insufficient permissions to bundle directory
2. Bundle directory doesn't exist
3. Disk full

**Fallback:**
If bundle directory fails, logs will be created in:
```
C:\Users\[YourUser]\AppData\Local\Temp\
```

### Errors Not Showing in Command Line

**Check:**
1. AutoCAD command line window is visible
2. Console logging is enabled
3. Error severity is Error or Warning level

### Log Files Growing Too Large

**Solution:**
Old log files can be safely deleted. They are not referenced by UnifiedSnoop after creation.

**Cleanup:**
```powershell
# Delete log files older than 30 days
Get-ChildItem "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\UnifiedSnoop_*.log" | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-30)} | 
    Remove-Item
```

---

## Best Practices

### For Users

1. **Keep Recent Logs:** Don't delete logs from the last few days
2. **Check Command Line:** Errors appear immediately there
3. **Report Issues:** Include log files when reporting bugs
4. **Monitor Patterns:** Recurring errors may indicate configuration issues

### For Developers

1. **Use Descriptive Messages:** Make errors understandable
2. **Include Context:** Specify where the error occurred
3. **Handle Gracefully:** Attempt recovery when possible
4. **Test Error Paths:** Verify error handling works correctly

---

## Error Categories

### UI Errors
- Form initialization failures
- Control layout issues
- Event handler exceptions

### Database Errors
- Transaction failures
- Object access errors
- Property collection failures

### Export Errors
- File access denied
- Invalid export format
- Data serialization issues

### Comparison Errors
- Object type mismatches
- Property access failures

### Bookmark Errors
- File I/O failures
- Serialization errors
- Invalid bookmark data

---

## Additional Resources

- **User Guide:** `USER_GUIDE.md` - Feature documentation
- **Deployment Guide:** `DEPLOYMENT_GUIDE.md` - Installation instructions
- **Implementation Report:** `IMPLEMENTATION_REPORT.md` - Technical details

---

**Document Version:** 1.0  
**Last Updated:** November 17, 2025

