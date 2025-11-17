# UnifiedSnoop User Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Commands](#commands)
4. [User Interface](#user-interface)
5. [Features](#features)
6. [Keyboard Shortcuts](#keyboard-shortcuts)
7. [Advanced Usage](#advanced-usage)
8. [Troubleshooting](#troubleshooting)

---

## Introduction

**UnifiedSnoop** is a comprehensive object inspection tool for AutoCAD and Civil 3D. It allows you to explore and examine properties of any object in your drawing database with an intuitive, feature-rich user interface.

### Key Features
- ✅ Inspect any AutoCAD or Civil 3D object
- ✅ Hierarchical tree view of database structure
- ✅ Specialized collectors for common entity types
- ✅ Export to CSV, Excel, and JSON formats
- ✅ Object comparison (side-by-side)
- ✅ Bookmark favorite objects
- ✅ Search and filter properties
- ✅ Context menu integration
- ✅ Keyboard shortcuts for power users

### Supported Versions
- **AutoCAD 2024** (.NET Framework 4.8)
- **AutoCAD 2025+** (.NET 8.0)
- **Civil 3D 2024+** (with specialized Civil 3D collectors)

---

## Getting Started

### Installation
1. Copy the `UnifiedSnoop.bundle` folder to:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\
   ```
2. Start or restart AutoCAD/Civil 3D
3. The plugin will load automatically

### First Launch
Type `SNOOP` at the AutoCAD command line to open the UnifiedSnoop UI.

---

## Commands

UnifiedSnoop provides three commands:

### 1. `SNOOP`
**Primary command** - Opens the UnifiedSnoop user interface.

```
Command: SNOOP
```

**What it does:**
- Opens the main inspection UI
- Shows the database tree structure
- Allows you to browse and inspect any object

### 2. `SNOOPHANDLE`
**Direct object inspection** - Opens UnifiedSnoop directly to a specific object by its handle.

```
Command: SNOOPHANDLE
Enter object handle: 1A7
```

**Use case:** When you know an object's handle and want to inspect it directly.

### 3. `SNOOPVERSION`
**Diagnostics command** (hidden) - Displays version and build information.

```
Command: SNOOPVERSION
```

---

## User Interface

### Main Window Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [Select Object] [Refresh] [Export] [Compare] [Bookmarks]  │
├──────────────────┬──────────────────────────────────────────┤
│                  │  Property    │ Type      │ Value         │
│  Database        │──────────────┼───────────┼───────────────│
│  ├─ Blocks       │  Name        │ String    │ Line          │
│  ├─ Layers       │  Start Point │ Point3d   │ (0,0,0)       │
│  ├─ Linetypes    │  End Point   │ Point3d   │ (10,10,0)     │
│  └─ ...          │  Length      │ Double    │ 14.1421       │
│                  │  Layer       │ String    │ 0             │
│                  │  ...         │ ...       │ ...           │
│                  │              │           │               │
├──────────────────┴──────────────────────────────────────────┤
│  [Search: _______] [Clear]  [Copy Value] [Copy All]         │
├──────────────────────────────────────────────────────────────┤
│  Status: Ready                                               │
└──────────────────────────────────────────────────────────────┘
```

### Components

#### 1. **Toolbar** (Top)
- **Select Object**: Pick an object in the drawing to inspect
- **Refresh**: Reload the current view
- **Export**: Export properties to CSV/Excel/JSON
- **Compare**: Compare two objects side-by-side
- **★ Add**: Add current object to bookmarks
- **Bookmarks**: View and manage bookmarked objects

#### 2. **Tree View** (Left Panel)
Displays the hierarchical structure of the database:
- **Blocks**: All block definitions
- **Layers**: Layer table records
- **Linetypes**: Linetype definitions
- **Entities**: All entities in model/paper space
- **Civil 3D Objects**: Alignments, Surfaces, Corridors, etc.

**Tip:** Click the `[+]` to expand nodes and explore nested objects.

#### 3. **Properties List** (Right Panel)
Shows detailed properties of the selected object:
- **Property**: The property name
- **Type**: The data type (String, Double, Point3d, etc.)
- **Value**: The current value

Properties are organized by **Category** for easy navigation.

#### 4. **Search Bar** (Bottom)
- **Search Box**: Filter properties by name or value
- **Clear Button**: Reset the search
- **Copy Value**: Copy the selected property value
- **Copy All**: Copy all properties to clipboard

#### 5. **Status Bar**
Displays current operation status and helpful messages.

---

## Features

### 1. Object Selection

#### Method A: From Tree
1. Open `SNOOP`
2. Navigate the tree (Database → Blocks → ModelSpace)
3. Expand a block to see entities
4. Click any entity to view its properties

#### Method B: Pick from Drawing
1. Open `SNOOP`
2. Click **[Select Object]**
3. Pick an object in the drawing
4. Properties appear in the right panel

#### Method C: By Handle
1. Type `SNOOPHANDLE`
2. Enter the object handle (e.g., `1A7`)
3. Properties appear immediately

#### Method D: Context Menu
1. Right-click in the drawing
2. Select **UnifiedSnoop**
3. Choose **Snoop Selected Object** or **Show Properties**

### 2. Export Properties

UnifiedSnoop supports three export formats:

#### Export to CSV
```
Property,Type,Value
Name,String,Line
Start Point,Point3d,"(0.0000, 0.0000, 0.0000)"
End Point,Point3d,"(10.0000, 10.0000, 0.0000)"
Length,Double,14.1421
```

**Use case:** Import into Excel, Google Sheets, or databases.

#### Export to Excel (Tab-Delimited)
Same as CSV but tab-separated for direct Excel compatibility.

#### Export to JSON
```json
{
  "ObjectType": "Line",
  "ExportDate": "2025-11-17T14:30:00",
  "Properties": [
    {
      "Name": "Start Point",
      "Type": "Point3d",
      "Value": "(0.0000, 0.0000, 0.0000)",
      "Category": "Geometry"
    }
  ]
}
```

**Use case:** API integration, data processing, automation scripts.

**How to export:**
1. Select an object
2. Click **[Export...]**
3. Choose format (CSV, Excel, or JSON)
4. Save the file

### 3. Object Comparison

Compare two objects side-by-side to see differences:

**Steps:**
1. Select the first object
2. Click **[Compare...]**
3. Select **Pick from Drawing** or **From Database**
4. Select the second object
5. Review differences highlighted in **yellow**

**Options:**
- ☑ **Show Differences Only**: Hide identical properties
- **Export Comparison**: Save comparison results to CSV

**Use case:** Compare similar entities, check style differences, validate object properties.

### 4. Bookmarks

Save frequently-accessed objects for quick reference:

#### Add a Bookmark
1. Select an object
2. Click **[★ Add]**
3. Enter a descriptive name
4. Click **OK**

#### View Bookmarks
1. Click **[Bookmarks]**
2. Select a bookmark from the list
3. Click **[Go To]** to inspect it

#### Manage Bookmarks
- **Delete**: Remove a single bookmark
- **Clear All**: Remove all bookmarks

**Use case:** Track key objects, mark problem entities, save reference points.

### 5. Search and Filter

Quickly find properties:

1. Type in the **Search** box (e.g., "layer")
2. Properties are filtered in real-time
3. Click **[Clear]** to show all properties again

**Search matches:**
- Property names (e.g., "Layer", "Color")
- Property values (e.g., "0", "Red")

### 6. Copy to Clipboard

#### Copy Single Property
1. Select a property in the list
2. Click **[Copy Value]** or press **Ctrl+C**
3. Value is copied to clipboard

#### Copy All Properties
1. Click **[Copy All]** or press **Ctrl+Shift+C**
2. All properties are copied in this format:
   ```
   Property: Value
   Property: Value
   ```

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **F5** | Refresh current view |
| **Ctrl+F** | Focus search box |
| **Ctrl+C** | Copy selected property value |
| **Ctrl+Shift+C** | Copy all properties |
| **Ctrl+L** | Focus tree view (left panel) |
| **Ctrl+P** | Focus properties list (right panel) |
| **Ctrl+B** | Add bookmark |
| **Ctrl+Shift+B** | View bookmarks |
| **Esc** | Clear search (if focused) / Close form |

---

## Advanced Usage

### Specialized Collectors

UnifiedSnoop includes **specialized collectors** for common object types that provide enhanced property displays:

#### AutoCAD Collectors
- **Line**: Start/End points, Length, Angle (degrees & radians), Delta vector
- **Arc**: Center, Radius, Start/End angles, Arc length
- **Circle**: Center, Radius, Diameter, Circumference, Area
- **Polyline**: Vertex count, Length, Closed status, Bulges, Width
- **Text** (DBText & MText): Content, Position, Height, Style, Alignment
- **Dimension**: Measurement, Type, Text position, Specific dimension properties
- **Layer**: Color, Linetype, States (On/Off, Frozen, Locked), Plottable

#### Civil 3D Collectors
- **Alignment**: Stations, Length, Style, Design speed
- **Surface**: Statistics, Boundaries, Border type
- **Corridor**: Baselines, Regions, Feature lines
- **Pipe Network**: Parts, Structures, Network references
- **Profile**: Start/End stations, Min/Max elevation
- **ProfileView**: Station/Elevation ranges
- **Assembly**: Groups, Code set style
- **PointGroup**: Point count, Query builder info

**Note:** If no specialized collector exists, the **ReflectionCollector** automatically uses reflection to extract all public properties.

### Context Menu Integration

UnifiedSnoop integrates into the AutoCAD right-click menu:

**Right-click on an object → UnifiedSnoop →**
- **Snoop Object**: Open UI with selected object
- **Show Properties**: Quick property inspection

### Working with Large Drawings

For drawings with thousands of objects:

1. Use **SNOOPHANDLE** for direct access
2. Use **Search** to filter properties quickly
3. Expand only the tree branches you need
4. Use **Bookmarks** to mark important objects

### Error Logging

UnifiedSnoop automatically logs errors for diagnostics:

**Log Location:**
```
C:\Users\[Username]\AppData\Local\Temp\UnifiedSnoop\Logs\
```

**Log File Format:**
```
UnifiedSnoop_YYYYMMDD.log
```

Each log entry includes:
- Timestamp
- Log level (Error, Warning, Info, Debug)
- Message
- Context (if applicable)
- Exception details (if any)

---

## Troubleshooting

### Problem: Command `SNOOP` not found
**Solution:**
1. Check plugin is installed in: `C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\`
2. Check `PackageContents.xml` exists
3. Restart AutoCAD
4. Type `NETLOAD` and manually load `UnifiedSnoop.dll` from the appropriate version folder

### Problem: UI not showing
**Solution:**
1. Check for errors in AutoCAD command line
2. Try `SNOOPVERSION` to verify plugin is loaded
3. Check log files in `%TEMP%\UnifiedSnoop\Logs\`

### Problem: Properties not displaying
**Solution:**
1. Try clicking **[Refresh]**
2. Select a different object and come back
3. Check if the object is valid (not erased)

### Problem: Export fails
**Solution:**
1. Ensure you have write permissions to the target folder
2. Close any open files with the same name
3. Try a different export format

### Problem: Civil 3D objects not showing
**Solution:**
1. Verify you're running Civil 3D (not plain AutoCAD)
2. Check that Civil 3D API references are properly loaded
3. Specialized Civil 3D collectors require `#if CIVIL3D` compilation

### Problem: Slow performance with large databases
**Solution:**
1. Avoid expanding all tree nodes at once
2. Use **Search** to filter results
3. Use **SNOOPHANDLE** for direct access to specific objects
4. Close the form when not in use

---

## Tips and Best Practices

### 🔍 **Exploration Tips**
- Start with small test drawings to get familiar with the UI
- Use the tree view to understand database structure
- Bookmark objects you frequently inspect

### 📊 **Data Export Tips**
- Use **CSV** for Excel/database import
- Use **JSON** for automation and API integration
- Use **Comparison Export** to track changes

### ⚡ **Performance Tips**
- Don't expand all tree nodes in large drawings
- Use **SNOOPHANDLE** when you know the handle
- Close unused comparison windows

### 🔐 **Safety Notes**
- UnifiedSnoop operates in **read-only mode**
- No objects are modified by inspection
- All transactions are committed safely

---

## Support and Feedback

For issues, questions, or feature requests:
- Check the log files in `%TEMP%\UnifiedSnoop\Logs\`
- Review this user guide
- Contact your system administrator

---

## Version History

### Version 1.0.0 (Current)
- Initial release
- Full AutoCAD and Civil 3D support
- Multi-targeting (.NET 4.8 and .NET 8.0)
- 15+ specialized collectors
- Export to CSV, Excel, JSON
- Object comparison
- Bookmarks
- Context menu integration
- Enhanced error logging

---

**Thank you for using UnifiedSnoop!** 🎉

