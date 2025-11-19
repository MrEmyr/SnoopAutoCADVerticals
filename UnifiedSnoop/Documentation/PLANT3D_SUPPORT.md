# Plant 3D Support in UnifiedSnoop

## Overview

UnifiedSnoop now includes comprehensive support for inspecting AutoCAD Plant 3D objects. Plant 3D uses a fundamentally different architecture for storing custom data compared to standard AutoCAD objects.

## Key Differences: Plant 3D vs Standard AutoCAD

### Standard AutoCAD/Civil 3D
- **Data Storage**: XRecords stored directly in DWG file
- **Location**: Named Objects Dictionary or Extension Dictionaries
- **Format**: Typed Values (ResultBuffer) embedded in the drawing
- **Access**: Direct via `AcDbXrecord` class

### Plant 3D
- **Data Storage**: External Project Database via DataLinksManager
- **Location**: Separate SQLite database managed by Plant 3D Project Manager
- **Format**: Property name/value pairs in external database
- **Access**: Via `AcPpDataLinksManager` API

## Features Implemented

### 1. Plant3DPropertyCollector
**Location**: `UnifiedSnoop\Inspectors\AutoCAD\Plant3DPropertyCollector.cs`

Automatically detects and collects properties from Plant 3D objects including:
- Basic object properties (Handle, ObjectId, Type)
- Plant 3D specific properties (Tag, Spec, NominalDiameter, etc.)
- Information about DataLinks storage
- Guidance on accessing external properties

**Detection**: Identifies Plant 3D objects by namespace:
- `Autodesk.ProcessPower.*`
- `Autodesk.Plant3D.*`
- Types containing `PnP3d`, `PnID`, or `AcPp`

### 2. Plant 3D Property Editor
**Location**: `UnifiedSnoop\Plant3DEditor\`

#### Commands
- **`PLANT3DPROPEDIT`**: Opens dedicated editor for selected Plant 3D object
- **`PLANT3DLIST`**: Lists all Plant 3D objects in the current drawing

#### Features
- View all properties of a Plant 3D object
- Copy property names and values to clipboard
- Information about DataLinks properties
- Guidance on how to edit properties

### 3. Plant3DHelper Utility Class
**Location**: `UnifiedSnoop\Plant3DEditor\Plant3DHelper.cs`

Provides utilities for working with Plant 3D objects:
- Detection of Plant 3D availability
- Property value retrieval using reflection
- Common property name constants
- DataLinks information and guidance
- Value formatting helpers

## Usage

### Inspecting Plant 3D Objects with SNOOP Command

1. Launch UnifiedSnoop: `SNOOP`
2. Select a Plant 3D object (pipe, valve, equipment, etc.)
3. The Plant3DPropertyCollector will automatically be used
4. View properties in the main snoop window

Properties displayed include:
- Object type and handle
- Standard Plant 3D properties
- Information about DataLinks storage
- Guidance on accessing external database properties

### Using PLANT3DPROPEDIT Command

```
Command: PLANT3DPROPEDIT
Select a Plant 3D object: [select object]
```

The Plant 3D Property Editor will open showing:
- Object information
- All available properties with values
- DataLinks storage information
- Context menu to copy properties

### Using PLANT3DLIST Command

```
Command: PLANT3DLIST
```

Lists all Plant 3D objects in the current drawing with:
- Object type and handle
- Common properties (Tag, Description, Spec)
- Total count of Plant 3D objects

## DataLinks Properties

### What are DataLinks?

DataLinks is Plant 3D's system for storing object properties in an external project database rather than in the DWG file itself. This allows:
- Centralized property management across multiple drawings
- Database-driven reporting and validation
- Project-level consistency
- Integration with external systems

### Common DataLinks Properties

Properties stored in the external database include:
- **Code**: Equipment/component code
- **Number**: Tag number
- **Service**: Service designation
- **InsulationType**: Insulation specification
- **PaintCode**: Paint/coating specification
- **FluidCode**: Fluid type
- **PressureClass**: Pressure rating
- **LineNumber**: Piping line number
- **Material**: Material specification

### Accessing DataLinks Properties

DataLinks properties require an active Plant 3D project context and cannot be directly accessed like XRecords.

**To View/Edit DataLinks Properties:**
1. **Plant 3D Data Manager**: `PALETTES` command
2. **Property Palettes**: Use Plant 3D-specific property palettes
3. **Project Manager**: Open Plant 3D Project Manager
4. **API Access**: Use `AcPpDataLinksManager` API

**API Example:**
```csharp
using Autodesk.ProcessPower.DataLinks;

// Get DataLinksManager
AcPpDataLinksManager dlm = /* get from project */;

// Get properties
AcPpStringArray propNames = new AcPpStringArray();
propNames.Add("Tag");
propNames.Add("Service");

AcPpStringArray propValues;
dlm.GetProperties(objectId, propNames, out propValues);

// Set properties
propNames.Clear();
propValues.Clear();
propNames.Add("Tag");
propValues.Add("V-101");
dlm.SetProperties(objectId, propNames, propValues);
```

## Installation Requirements

### Required DLLs (Optional)
Plant 3D support is optional. If Plant 3D DLLs are not available:
- Plant3DPropertyCollector will not be registered
- Other functionality continues to work normally
- Standard AutoCAD/Civil 3D snooping is unaffected

### Plant 3D DLL References
The project includes optional references to:
- `PnP3dObjectsMgd.dll`
- `PnPCommonDbxMgd.dll`
- `PnPDataLinks.dll`

**Default Path**: `C:\Autodesk\AutoCAD_Plant_3D_2024_SDK_English_Win_64bit_dlm\inc-x64\`

You may need to adjust paths in `UnifiedSnoop.csproj` based on your Plant 3D installation.

## Technical Implementation

### Detection Strategy
Plant 3D objects are detected using namespace and type name matching rather than hard type references. This allows the collector to work even when Plant 3D DLLs are not directly referenced during compilation.

### Reflection-Based Approach
Properties are accessed using reflection to avoid hard dependencies. This means:
- ✅ Works with different Plant 3D versions
- ✅ Gracefully handles missing DLLs
- ✅ No compilation errors if Plant 3D is not installed
- ⚠️ Slightly slower than direct property access
- ⚠️ No compile-time type checking

### Integration with UnifiedSnoop
Plant3DPropertyCollector is registered in `App.cs` during initialization:
```csharp
// Plant 3D Object Collectors (optional)
try
{
    registry.RegisterCollector(new Inspectors.AutoCAD.Plant3DPropertyCollector());
}
catch (System.Exception)
{
    // Plant 3D DLLs not available - OK, just skip
}
```

## Comparison with XRecord Support

| Feature | XRecords | Plant 3D DataLinks |
|---------|----------|-------------------|
| Storage Location | Inside DWG file | External database |
| Editability | Full CRUD via UnifiedSnoop | View only (use Plant 3D tools) |
| Data Format | Typed Values (ResultBuffer) | Property name/value pairs |
| Access Method | Direct `AcDbXrecord` | `AcPpDataLinksManager` API |
| Project Dependency | None | Requires Plant 3D project |
| Multi-drawing | Each DWG independent | Shared across project |

## Limitations

1. **Read-Only Access**: UnifiedSnoop provides read-only access to Plant 3D properties. Use Plant 3D tools for editing.

2. **Project Context Required**: DataLinks properties require an active Plant 3D project. Without it, only object properties are visible.

3. **SDK Dependency**: Full functionality requires Plant 3D SDK DLLs to be available at runtime.

4. **Reflection Performance**: Property access is slower than direct access due to reflection usage.

## Future Enhancements

Potential improvements for future versions:
- [ ] Direct DataLinksManager integration for read/write access
- [ ] Property editing capabilities
- [ ] Spec database browsing
- [ ] Content manager integration
- [ ] P&ID object support
- [ ] Isometric drawing object support

## References

- **Plant 3D SDK Documentation**: See `C:\Autodesk\AutoCAD_Plant_3D_2024_SDK_English_Win_64bit_dlm\docs\plantsdk_dev.chm`
- **Sample Code**: See SDK samples in `samples\Piping` and `samples\PnID` directories
- **DataLinksManager**: Key API for Plant 3D property access
- **PnPScanValves.cpp**: Example of DataLinks property manipulation

## Support

For issues or questions about Plant 3D support:
1. Check that Plant 3D is properly installed
2. Verify SDK DLL paths in `UnifiedSnoop.csproj`
3. Review Plant 3D documentation in SDK
4. Check error log: `%TEMP%\UnifiedSnoop_Errors.log`

---

**Note**: This documentation is based on AutoCAD Plant 3D 2024. Other versions may have slight differences in API or behavior.

