# XRecord Support

## Overview

UnifiedSnoop now includes comprehensive support for viewing and inspecting AutoCAD XRecord objects. XRecords are a fundamental mechanism for storing custom application data within AutoCAD drawings.

## What are XRecords?

XRecords are database objects that store custom data as typed values in a `ResultBuffer`. They are commonly used by AutoCAD applications to:
- Store application-specific settings and preferences
- Cache computed values
- Maintain custom object data
- Store extended information that doesn't fit into extended data (XData)

Reference: [Autodesk XRecord Documentation](https://help.autodesk.com/view/OARX/2025/ENU/?guid=OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_Xrecord)

## Features

### Automatic Detection
- XRecords are automatically detected when browsing dictionaries in the tree view
- Typically found in:
  - Named Object Dictionary (NOD)
  - Extension Dictionaries attached to objects
  - Custom application dictionaries

### Property Display
The XRecordCollector provides detailed information about:

1. **General Information**
   - Object Type
   - Handle
   - Data Entry Count
   - Cloneable status
   - Translation table requirements

2. **Data Display**
   - Each entry in the ResultBuffer is displayed with:
     - DXF code (Type code)
     - Data type (interpreted from DXF code)
     - Formatted value

### Supported Data Types

The collector recognizes and formats all standard DXF code ranges:

| DXF Code Range | Data Type | Example |
|----------------|-----------|---------|
| 0 | Entity Type | "LINE", "CIRCLE" |
| 1-9 | Text String | "My custom data" |
| 5, 105 | Handle | "1F4" |
| 10-18 | Point3d | "(10.5000, 20.3000, 0.0000)" |
| 38-59 | Double | "123.456789" |
| 60-79 | Int16 | "42" |
| 90-99 | Int32 | "1000" |
| 100-102 | String (Subclass) | "AcDbEntity" |
| 140-147 | Double | "3.141593" |
| 170-175 | Int16 | "256" |
| 280-289 | Int8 | "1" |
| 290-293 | Boolean | "True" |
| 300-319 | Arbitrary String | "Application data" |
| 320-369 | Handle/Soft Pointer | "A5" |
| 370-399 | Int16/String | "25" |
| 420-429 | Int32 (Color) | "255" |
| 430-439 | String (Color Name) | "Red" |
| 440-449 | Int32 (Transparency) | "128" |
| 1000+ | Extended Data | Various XData types |

## Usage

### Finding XRecords

1. **Open UnifiedSnoop**
   ```
   Command: SNOOP
   ```

2. **Navigate to Dictionaries**
   - Expand "Database" → "AutoCAD Collections"
   - Look for "Named Object Dictionary" or other dictionaries
   - XRecords will appear as entries within these dictionaries

3. **View XRecord Data**
   - Click on any XRecord in the tree
   - The properties panel will display all typed values with formatting

### Example: Finding Application Data

Many applications store data in the Named Object Dictionary (NOD):

```
Database
└── AutoCAD Collections
    └── Named Object Dictionary
        ├── ACAD_GROUP (Dictionary)
        ├── ACAD_LAYOUT (Dictionary)
        ├── ACAD_MATERIAL (Dictionary)
        └── MyApp_Settings (XRecord) ← Custom application data
```

### Example: Extension Dictionary

Objects can have extension dictionaries containing XRecords:

```
Model Space
└── BlockReference [Handle: 123]
    └── Extension Dictionary
        └── CustomData (XRecord) ← Object-specific data
```

## Property Display Format

When viewing an XRecord, properties are displayed as:

```
Object Type: XRecord (Custom Application Data)
Handle: 1F4A
Data Entry Count: 5

--- XRecord Data ---
  [0] DXF 1    String           "MyApplication"
  [1] DXF 70   Int16            "1"
  [2] DXF 10   Point3d          "(100.0000, 200.0000, 0.0000)"
  [3] DXF 40   Double           "3.141593"
  [4] DXF 90   Int32            "12345"

Is Cloneable: True
Translation Table Required: False
```

## Technical Details

### Implementation

- **Collector**: `XRecordCollector.cs`
- **Location**: `UnifiedSnoop/Inspectors/AutoCAD/`
- **Interface**: Implements `ICollector`

### DXF Code Interpretation

The collector uses standard AutoCAD DXF code ranges to interpret data types:
- Follows the AutoCAD DXF Reference Guide conventions
- Handles common application data patterns
- Gracefully handles unknown or custom codes

### Error Handling

- Null or empty ResultBuffers are detected and displayed
- Individual entry parsing errors are caught and reported
- Invalid type conversions show error messages with context

## Use Cases

### 1. Debugging Application Data
View custom data stored by third-party applications to understand their data structures.

### 2. Data Recovery
Inspect XRecords to recover information when application code is not available.

### 3. Application Development
Verify that your application is storing data correctly in XRecords.

### 4. Drawing Analysis
Understand what custom data exists in a drawing and which applications created it.

### 5. Quality Control
Check that application data is consistent across multiple drawings.

## Best Practices

### When Viewing XRecords
1. Check the DXF code to understand data type
2. Look for patterns (e.g., code 1 often stores application name)
3. Note the handle for reference in code
4. Document any unknown data patterns

### For Developers
1. Use standard DXF code ranges for compatibility
2. Store application name in code 1 or 1001
3. Document your XRecord data structure
4. Consider using codes 1000+ for application-specific data

## Limitations

1. **Read-Only**: UnifiedSnoop only displays XRecord data; it does not modify it
2. **Binary Data**: Large binary data may be displayed as raw values
3. **Complex Types**: Some complex object references may show as handles only
4. **Performance**: XRecords with thousands of entries may take time to display

## Related Features

- **Dictionary Browser**: View all dictionaries in the database
- **Handle Lookup**: Use `SNOOPHANDLE` to jump directly to an XRecord by handle
- **Export**: Export XRecord data to CSV for analysis
- **Search**: Use Ctrl+F to search within XRecord properties

## Troubleshooting

### XRecord Not Appearing
- Ensure you're looking in the correct dictionary
- Try using `SNOOPHANDLE` with the known handle
- Check if the object is erased (deleted but not purged)

### Data Not Formatted Correctly
- Unknown DXF codes display as their raw type
- Custom codes may need manual interpretation
- Check the DXF Reference Guide for uncommon codes

### Performance Issues
- Large XRecords (1000+ entries) may be slow to load
- Consider exporting to CSV for analysis
- Use search (Ctrl+F) to find specific entries quickly

## References

- [AutoCAD XRecord Class Documentation](https://help.autodesk.com/view/OARX/2025/ENU/?guid=OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_Xrecord)
- [AutoCAD DXF Reference Guide](https://help.autodesk.com/view/OARX/2025/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3)
- [ResultBuffer Class Documentation](https://help.autodesk.com/view/OARX/2025/ENU/?guid=OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_ResultBuffer)

## Version History

- **v1.0** (Initial Release): Full XRecord viewing support with comprehensive DXF code interpretation

