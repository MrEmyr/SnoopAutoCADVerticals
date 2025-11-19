# XRecord Editor

## Overview

The **XRecord Editor** is a powerful standalone tool included with UnifiedSnoop that allows you to view, edit, and delete XRecords in your AutoCAD drawings. Unlike the read-only XRecord viewer in the main Snoop tool, the XRecord Editor provides full CRUD (Create, Read, Update, Delete) capabilities.

## Launching the XRecord Editor

### Command Line
Type the following command in AutoCAD:
```
XRECORDEDIT
```

### What It Does
- Opens a dedicated window for managing XRecords
- Displays all XRecords in a tree structure organized by their parent dictionaries
- Allows full editing capabilities including add, edit, and delete operations

## User Interface

### Main Window Layout

The XRecord Editor window is divided into two main sections:

```
┌──────────────────────────────────────────────────────┐
│  XRecord Editor - UnifiedSnoop                       │
├────────────────┬─────────────────────────────────────┤
│                │  XRecord: MyApp_Settings            │
│  Tree View     │  5 entries                          │
│                │                                      │
│  ├─ Named      │  ┌───────────────────────────────┐ │
│  │  Objects    │  │  Idx  DXF  Type    Value      │ │
│  │  Dictionary │  ├───────────────────────────────┤ │
│  │  ├─ MyApp_  │  │  0    1    String  "MyApp"   │ │
│  │  │  Settings│  │  1    70   Int16   1         │ │
│  │  │  [XRec]  │  │  2    10   Point3d (100...)  │ │
│  │  └─ Config  │  │  3    40   Double  3.14159   │ │
│  │     [XRec]  │  │  4    90   Int32   12345     │ │
│  │             │  └───────────────────────────────┘ │
│  └─ All        │                                     │
│     Dictionar. │  [Add Entry] [Edit] [Delete Entry] │
│                │  [Delete XRecord]                   │
└────────────────┴─────────────────────────────────────┘
│  [Refresh]  [Close]                                  │
└──────────────────────────────────────────────────────┘
```

### Left Panel: XRecord Tree

The tree view displays all XRecords found in the drawing, organized by:

1. **Named Objects Dictionary**
   - Shows all XRecords directly in the NOD
   - Recursively displays sub-dictionaries and their XRecords

2. **All Dictionaries**
   - Scans the entire database for XRecords
   - Includes extension dictionaries attached to entities
   - Shows the parent object for each XRecord

### Right Panel: XRecord Data

When you select an XRecord from the tree, the right panel displays:

- **XRecord Name**: The dictionary key for the XRecord
- **Entry Count**: Number of TypedValue entries
- **Data ListView**: All entries with:
  - Index: Sequential position (0-based)
  - DXF Code: The type code (1-1071)
  - Type: Human-readable type name
  - Value: Formatted value display

## Operations

### 1. Adding an Entry

**Steps:**
1. Select an XRecord in the tree
2. Click **[Add Entry]**
3. In the dialog:
   - Select a DXF code from the dropdown
   - Enter the value following the format hint
   - Click **[OK]**

**Example:**
```
DXF Code: 1 - String
Value: "Hello World"
```

### 2. Editing an Entry

**Steps:**
1. Select an XRecord in the tree
2. Select an entry in the data list
3. Click **[Edit Entry]** or double-click the entry
4. Modify the value in the dialog
5. Click **[OK]**

**Note:** You can change both the DXF code and the value

### 3. Deleting an Entry

**Steps:**
1. Select an XRecord in the tree
2. Select an entry in the data list
3. Click **[Delete Entry]**
4. Confirm the deletion

**Warning:** This removes the entry from the XRecord permanently

### 4. Deleting an XRecord

**Steps:**
1. Select an XRecord in the tree
2. Click **[Delete XRecord]** (red button)
3. Confirm the deletion

**Warning:** This removes the entire XRecord and all its data from the drawing. This action cannot be undone.

### 5. Refreshing the View

**Steps:**
1. Click **[Refresh]** to reload all XRecords from the database
2. Use this after external changes or to verify changes

## Supported DXF Codes

The XRecord Editor supports all standard DXF code ranges:

| DXF Code | Type | Format Example |
|----------|------|----------------|
| 1 | String | "My text" |
| 5 | Handle | "1F4A" |
| 10 | Point3d | "100.5,200.3,0" |
| 40 | Double | "3.14159" |
| 70 | Int16 | "42" |
| 90 | Int32 | "1000" |
| 280 | Int8/Boolean | "1" (0 or 1) |
| 290 | Boolean | "1" (0 or 1) |
| 300 | String (Arbitrary) | "Custom data" |
| 330 | Handle (Soft Pointer) | "A5" |
| 1000 | String (XData) | "XData string" |
| 1010 | Point3d (XData) | "10,20,30" |
| 1040 | Double (XData) | "2.71828" |
| 1070 | Int16 (XData) | "256" |

### Value Format Guidelines

**String (DXF 1, 300, 1000):**
```
Enter plain text
Example: "My Application Data"
```

**Handle (DXF 5, 105, 330):**
```
Enter hexadecimal handle
Example: 1F4A
```

**Point3d (DXF 10-18, 1010):**
```
Enter X,Y,Z separated by commas
Example: 100.5,200.3,0
```

**Double (DXF 40, 140-147, 1040):**
```
Enter decimal number
Example: 3.14159
```

**Int16 (DXF 70, 170-175, 1070):**
```
Enter integer (-32768 to 32767)
Example: 42
```

**Int32 (DXF 90-99):**
```
Enter integer
Example: 1000000
```

**Boolean (DXF 280-293):**
```
Enter 0 (false) or 1 (true)
Example: 1
```

## Use Cases

### 1. Application Configuration Management

**Scenario:** Your application stores settings in an XRecord

```
XRecord: MyApp_Config
  [0] DXF 1    String    "MyApplication"
  [1] DXF 70   Int16     1 (version)
  [2] DXF 40   Double    1.5 (scale factor)
  [3] DXF 290  Boolean   1 (enabled)
```

**Actions:**
- View current configuration
- Update settings values
- Add new configuration entries

### 2. Debugging Application Data

**Scenario:** Troubleshooting why your application isn't loading data correctly

**Steps:**
1. Open XRECORDEDIT
2. Navigate to your application's XRecord
3. Verify all expected entries are present
4. Check data types and values
5. Edit incorrect values for testing

### 3. Data Recovery

**Scenario:** An application is no longer available but you need to access its data

**Steps:**
1. Open XRECORDEDIT
2. Locate the application's XRecords
3. Export or document the data manually
4. Optionally delete obsolete XRecords

### 4. Custom Data Management

**Scenario:** Manually adding custom metadata to drawings

**Steps:**
1. Create XRecord in Named Objects Dictionary
2. Add entries with your metadata
3. Document DXF codes for later retrieval

### 5. Bulk XRecord Cleanup

**Scenario:** Removing XRecords from obsolete applications

**Steps:**
1. Open XRECORDEDIT
2. Navigate through the tree
3. Identify obsolete XRecords
4. Delete unwanted XRecords
5. Refresh to confirm

## Safety Features

### Confirmation Dialogs

The XRecord Editor includes safety confirmations for destructive operations:

- **Delete Entry**: "Are you sure you want to delete this entry?"
- **Delete XRecord**: "Are you sure you want to delete the entire XRecord? This action cannot be undone."

### Transaction Management

- All changes are wrapped in AutoCAD transactions
- If an error occurs, changes are rolled back
- Commit only occurs when the editor closes successfully

### Error Logging

All errors are logged to:
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\UnifiedSnoop_[timestamp].log
```

Errors are also displayed in:
- Status bar (bottom of window)
- AutoCAD command line
- Message boxes (for critical errors)

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `F5` | Refresh |
| `Double-Click` | Edit selected entry |
| `Delete` | Delete selected entry |
| `Esc` | Close editor |

## Best Practices

### Before Editing

1. **Back up your drawing** before making changes
2. **Document the XRecord structure** if you plan to modify it
3. **Test in a copy** of the drawing first

### While Editing

1. **Use appropriate DXF codes** for your data types
2. **Follow format guidelines** for values
3. **Add descriptive strings** at the beginning (DXF 1) to identify XRecords

### After Editing

1. **Verify changes** by refreshing and reviewing
2. **Test your application** to ensure it still works
3. **Document changes** for future reference

## Troubleshooting

### XRecord Not Appearing in Tree

**Possible causes:**
- XRecord may be in an extension dictionary not yet scanned
- Object may be erased (deleted but not purged)
- Drawing database may need refreshing

**Solution:**
- Click [Refresh]
- Check "All Dictionaries" node
- Use SNOOPHANDLE command to access by handle

### Cannot Edit Value

**Possible causes:**
- XRecord is read-only (rare)
- Value format is incorrect
- Object is on a locked layer (shouldn't affect XRecords)

**Solution:**
- Check value format matches DXF code requirements
- Try entering a simpler test value
- Check AutoCAD command line for error messages

### Changes Not Saving

**Possible causes:**
- Drawing is read-only
- Insufficient permissions
- Transaction error

**Solution:**
- Check drawing file properties
- Check log file for detailed error
- Try closing and reopening the editor

### Error: "Cannot find parent dictionary"

**Possible causes:**
- Parent dictionary was deleted
- XRecord became orphaned

**Solution:**
- Cannot delete orphaned XRecords through editor
- May need database recovery tools

## Integration with UnifiedSnoop

The XRecord Editor works alongside the main UnifiedSnoop tool:

### Viewing vs. Editing

| Feature | UnifiedSnoop (SNOOP) | XRecord Editor (XRECORDEDIT) |
|---------|----------------------|------------------------------|
| View XRecords | ✅ Yes (read-only) | ✅ Yes |
| Edit XRecords | ❌ No | ✅ Yes |
| Delete XRecords | ❌ No | ✅ Yes |
| Add entries | ❌ No | ✅ Yes |
| Navigate database | ✅ Full tree | ⚠️ XRecords only |
| View other objects | ✅ All objects | ❌ XRecords only |

### Workflow Recommendation

1. Use **SNOOP** to explore the database and locate XRecords
2. Use **XRECORDEDIT** to modify XRecords you've identified
3. Use **SNOOP** again to verify changes in context

## Technical Details

### Implementation

- **Forms**: `XRecordEditorForm.cs`, `XRecordValueEditorForm.cs`
- **Command**: `XRECORDEDIT`
- **Transaction**: Uses `TransactionHelper` for safe database access
- **Error Logging**: Integrated with `ErrorLogService`

### Limitations

1. **Read-Write Access**: Requires drawing to be editable
2. **No Undo**: Changes are committed when editor closes
3. **No Batch Operations**: Must edit entries one at a time
4. **No XRecord Creation**: Can only edit existing XRecords (create via application code)

### Performance

- Efficient for drawings with < 1000 XRecords
- Tree loading may be slow for very large databases
- Individual XRecord editing is fast regardless of size

## Examples

### Example 1: Simple String Storage

```
XRecord: "MyApp_UserName"
  [0] DXF 1    String    "JohnDoe"
```

### Example 2: Configuration Data

```
XRecord: "MyApp_Settings"
  [0] DXF 1    String    "MyApplication v2.0"
  [1] DXF 70   Int16     2 (version)
  [2] DXF 40   Double    100.0 (tolerance)
  [3] DXF 290  Boolean   1 (auto-save enabled)
  [4] DXF 300  String    "C:\\Data\\Config.xml"
```

### Example 3: Geometric Data

```
XRecord: "CustomPoint"
  [0] DXF 1    String    "Reference Point"
  [1] DXF 10   Point3d   "1000.0,2000.0,0.0"
  [2] DXF 40   Double    0.5 (tolerance)
```

### Example 4: Object References

```
XRecord: "LinkedObjects"
  [0] DXF 1    String    "Object Links"
  [1] DXF 330  Handle    "1F4" (soft pointer to object)
  [2] DXF 330  Handle    "A52" (soft pointer to object)
```

## See Also

- [XRECORD_SUPPORT.md](XRECORD_SUPPORT.md) - XRecord viewing in UnifiedSnoop
- [USER_GUIDE.md](USER_GUIDE.md) - General UnifiedSnoop usage
- [AutoCAD DXF Reference](https://help.autodesk.com/view/OARX/2025/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3)
- [Xrecord Class Documentation](https://help.autodesk.com/view/OARX/2025/ENU/?guid=OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_Xrecord)

## Version History

- **v1.0** (Initial Release): Full XRecord editing with tree view and entry management

