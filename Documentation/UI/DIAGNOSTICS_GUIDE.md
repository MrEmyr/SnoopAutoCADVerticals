# UnifiedSnoop - Diagnostics Guide

**Last Updated:** November 19, 2025

---

## Debugging the Database Tree Loading Issue

### Issue
The database tree is not pre-populating on startup. It only shows content after using "Select Object" button.

### Diagnostic Steps

#### 1. Check DebugView Output

Use **DebugView** (Sysinternals tool) or Visual Studio's Debug Output window to see diagnostic messages:

1. Download and run [DebugView](https://docs.microsoft.com/en-us/sysinternals/downloads/debugview)
2. Enable "Capture Win32" and "Capture Global Win32"
3. Launch Civil 3D
4. Run `SNOOP` command
5. Watch for these diagnostic messages:

```
LoadDatabaseTree: Starting...
LoadDatabaseTree: TreeView cleared
LoadDatabaseTree: Root node created
LoadDatabaseTree: Got X AutoCAD collections
LoadDatabaseTree: Added AutoCAD node with X children
LoadDatabaseTree: Got X Civil 3D collections
LoadDatabaseTree: Added Civil 3D node with X children
LoadDatabaseTree: Root node added with X children. Tree now has 1 root nodes
LoadDatabaseTree: Complete!
```

#### 2. Check for Errors

**Look for ERROR messages:**
- `LoadDatabaseTree: ERROR - _treeView is null!`
- `LoadDatabaseTree: ERROR - _database is null!`
- `LoadDatabaseTree: ERROR - Transaction not available!`
- `LoadDatabaseTree: EXCEPTION - ...`

**Check Status Bar:**
The bottom status bar should show:
- ✅ Good: `"Loaded database tree with X AutoCAD collections. Civil 3D: True/False"`
- ❌ Bad: `"Error: TreeView not initialized"` or `"Error: Transaction not available"`

#### 3. Check Error Log

If an exception occurs, check the error log file:

**Location:** `%LOCALAPPDATA%\UnifiedSnoop\Logs\ErrorLog.txt`

**Path Example:** `C:\Users\YourName\AppData\Local\UnifiedSnoop\Logs\ErrorLog.txt`

Open the file and look for:
```
[TIMESTAMP] ERROR in LoadDatabaseTree
Exception: ...
Stack Trace: ...
```

---

## Common Issues and Solutions

### Issue 1: Transaction Not Available

**Symptom:** Status shows "Error: Transaction not available"

**Cause:** The TransactionHelper.Transaction property is null when LoadDatabaseTree() is called.

**Solution:**
1. Check that `trHelper.Start()` is called before creating MainSnoopForm
2. Verify TransactionHelper.Transaction property is accessible
3. Check App/SnoopCommands.cs line 45-50

```csharp
using (TransactionHelper trHelper = new TransactionHelper(db))
{
    trHelper.Start();  // ← Must be called BEFORE MainSnoopForm constructor
    var mainForm = new UI.MainSnoopForm(db, trHelper);
    // ...
}
```

### Issue 2: TreeView Not Visible

**Symptom:** No errors in log, but TreeView appears empty

**Possible Causes:**
1. TreeView is hidden behind other controls
2. TreeView has zero size
3. SplitContainer Panel1 has zero width

**Solution:** Add temporary diagnostic code to check TreeView state:

```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    
    // DIAGNOSTIC: Check TreeView state
    System.Diagnostics.Debug.WriteLine($"TreeView Visible: {_treeView.Visible}");
    System.Diagnostics.Debug.WriteLine($"TreeView Size: {_treeView.Size}");
    System.Diagnostics.Debug.WriteLine($"TreeView Location: {_treeView.Location}");
    System.Diagnostics.Debug.WriteLine($"TreeView Nodes.Count: {_treeView.Nodes.Count}");
    System.Diagnostics.Debug.WriteLine($"SplitContainer Panel1 Width: {_splitContainer.Panel1.ClientSize.Width}");
    
    // ... rest of OnLoad
}
```

### Issue 3: Collections Empty

**Symptom:** Tree loads but shows "AutoCAD Collections" with no children

**Cause:** DatabaseService.GetDatabaseCollections() returns empty list

**Solution:**
1. Check that the drawing file has objects (not empty drawing)
2. Verify Database object is valid
3. Check Transaction is active

---

## Manual Testing Checklist

### Pre-Test Setup
- [ ] Build solution in Debug mode
- [ ] Start DebugView or attach Visual Studio debugger
- [ ] Open Civil 3D with a drawing that has objects (Corridors, Alignments, etc.)

### Test Steps

1. **Run SNOOP Command**
   ```
   Command: SNOOP
   ```

2. **Check Initial State**
   - [ ] Form opens successfully
   - [ ] Top panel shows "Ready" or property count
   - [ ] Bottom status bar shows successful load message
   - [ ] TreeView (left pane) shows "Database" root node
   - [ ] TreeView shows "AutoCAD Collections" node
   - [ ] TreeView shows "Civil 3D Collections" node (if Civil 3D)

3. **Expand Tree Nodes**
   - [ ] Click on "Database" node → expands
   - [ ] Click on "AutoCAD Collections" → expands to show tables
   - [ ] Click on "Layers" (or any table) → expands to show layer list
   - [ ] Click on a Layer → properties display in right panel

4. **Check ListView Headers**
   - [ ] Headers show: "Property | Type | Value"
   - [ ] Headers do NOT show data (e.g., "StyleName | String | ...")

5. **Test Select Object Button**
   - [ ] Click "Select Object" button
   - [ ] Select an object from drawing
   - [ ] Object properties display in right panel
   - [ ] TreeView updates (optional, depends on implementation)

### Expected vs. Actual

| Component | Expected | Actual (Your Result) |
|-----------|----------|----------------------|
| TreeView Root | "Database" node visible | _________________ |
| TreeView Collections | AutoCAD + Civil 3D nodes | _________________ |
| ListView Headers | "Property | Type | Value" | _________________ |
| Status Bar | "Loaded database tree..." | _________________ |
| Property Count | "Ready" or object count | _________________ |

---

## Debugging Tools

### Visual Studio Debugger

1. Set breakpoints in:
   - `MainSnoopForm` constructor (line 108)
   - `InitializeForm()` (line 151)
   - `LoadDatabaseTree()` (line 528)
   - `TreeView_AfterSelect` (line 621)

2. Attach to Civil 3D process:
   - Debug → Attach to Process
   - Select `acad.exe` or `Civil3D.exe`
   - Ensure "Managed (v4.x)" code type is selected

3. Run `SNOOP` command and step through code

### DebugView (Sysinternals)

Best for release builds where debugger isn't attached:

1. Run DebugView as Administrator
2. Enable: Capture → Capture Win32, Capture Global Win32
3. Filter for "LoadDatabaseTree" or "UnifiedSnoop"
4. Run SNOOP command and watch output

### Error Log File

Automatically created at: `%LOCALAPPDATA%\UnifiedSnoop\Logs\ErrorLog.txt`

Contains all exceptions logged by ErrorLogService.

---

## Next Steps

Based on diagnostic output:

### If TreeView is Empty
→ Check Transaction availability
→ Check DatabaseService.GetDatabaseCollections()
→ Verify nodes are being created and added

### If Headers are Wrong
→ Already fixed in UI refactor
→ Rebuild and redeploy

### If Exception Occurs
→ Check stack trace in DebugView or error log
→ Fix the underlying cause
→ Rebuild and retest

---

## Contact & Support

If issues persist after following this guide:

1. Collect diagnostic output (DebugView + Error Log)
2. Note the "Expected vs. Actual" table results
3. Provide Civil 3D version and drawing details
4. Share diagnostic files

**Log Location:** `%LOCALAPPDATA%\UnifiedSnoop\Logs\`

---

**END OF DIAGNOSTICS GUIDE**

