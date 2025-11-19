# UnifiedSnoop

**A comprehensive object inspection tool for AutoCAD and Civil 3D**

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/yourusername/UnifiedSnoop)
[![AutoCAD](https://img.shields.io/badge/AutoCAD-2024%2B-red.svg)](https://www.autodesk.com/products/autocad/)
[![Civil3D](https://img.shields.io/badge/Civil%203D-2024%2B-green.svg)](https://www.autodesk.com/products/civil-3d/)
[![.NET](https://img.shields.io/badge/.NET-4.8%20%7C%208.0-purple.svg)](https://dotnet.microsoft.com/)

---

## 📋 Overview

UnifiedSnoop is a powerful object inspection tool designed for AutoCAD and Civil 3D users and developers. It provides a feature-rich user interface for exploring and examining properties of any object in your drawing database.

### 🎯 Key Features

- ✅ **Universal Object Inspection** - Inspect any AutoCAD or Civil 3D object
- ✅ **Specialized Collectors** - Enhanced views for Lines, Arcs, Circles, Polylines, Text, Dimensions, Layers, XRecords, Alignments, Surfaces, Corridors, and more
- ✅ **Multiple Export Formats** - CSV, Excel (tab-delimited), and JSON
- ✅ **Object Comparison** - Side-by-side comparison with difference highlighting
- ✅ **Bookmarks** - Save and quickly access frequently-inspected objects
- ✅ **Search & Filter** - Find properties by name or value instantly
- ✅ **Context Menu Integration** - Right-click access in AutoCAD
- ✅ **Keyboard Shortcuts** - Power-user productivity features
- ✅ **Enhanced Error Logging** - Comprehensive diagnostics and troubleshooting
- ✅ **Multi-Version Support** - Works with AutoCAD 2024 (.NET 4.8) and 2025+ (.NET 8.0)

---

## 🚀 Quick Start

### Installation

1. Download the `UnifiedSnoop.bundle` folder
2. Copy to: `C:\ProgramData\Autodesk\ApplicationPlugins\`
3. Restart AutoCAD/Civil 3D
4. Type `SNOOP` at the command line

### Commands

| Command | Description |
|---------|-------------|
| `SNOOP` | Open the UnifiedSnoop UI |
| `SNOOPHANDLE` | Inspect a specific object by its handle |
| `SNOOPVERSION` | Display version and build information |

### Basic Usage

```
Command: SNOOP
```

1. Browse the database tree (left panel)
2. Select any object to view its properties (right panel)
3. Use **[Select Object]** to pick from the drawing
4. Use **[Export...]** to save properties
5. Use **[Compare...]** to compare two objects

---

## 📦 What's Included

### Core Architecture

```
UnifiedSnoop/
├── Core/
│   ├── Collectors/       # ICollector interface & registry
│   ├── Data/            # PropertyData, ObjectNode models
│   └── Helpers/         # Transaction, Reflection, Version helpers
├── Services/
│   ├── DatabaseService  # High-level database operations
│   ├── ExportService    # CSV/Excel/JSON export
│   ├── BookmarkService  # Bookmark management
│   └── ErrorLogService  # Enhanced logging & diagnostics
├── Inspectors/
│   ├── AutoCAD/         # Line, Arc, Circle, Polyline, Text, Dimension, Layer collectors
│   └── Civil3D/         # Alignment, Surface, Corridor, PipeNetwork, Profile, ProfileView, Assembly, PointGroup collectors
├── UI/
│   ├── MainSnoopForm    # Primary inspection interface
│   ├── ComparisonForm   # Object comparison dialog
│   └── BookmarksForm    # Bookmark management dialog
├── App/
│   ├── App.cs           # Application entry point
│   ├── SnoopCommands.cs # AutoCAD commands
│   └── ContextMenuHandler # Right-click integration
└── Deploy/
    └── PackageContents.xml # AutoCAD bundle configuration
```

### Specialized Collectors

#### AutoCAD Collectors (8)
1. **LineCollector** - Start/End points, Length, Angle, Delta vector
2. **ArcCollector** - Center, Radius, Angles (degrees & radians), Arc length
3. **CircleCollector** - Center, Radius, Diameter, Circumference, Area
4. **PolylineCollector** - Vertices, Length, Closed, Bulges, Width (supports Polyline, Polyline2d, Polyline3d)
5. **TextCollector** - Content, Position, Height, Style (supports DBText & MText)
6. **DimensionCollector** - Measurement, Type-specific properties (Aligned, Rotated, Radial, Diametric)
7. **LayerTableCollector** - Color, Linetype, States (On/Off, Frozen, Locked), Plottable, Transparency
8. **XRecordCollector** - Custom application data with DXF code interpretation (ResultBuffer entries)

#### Civil 3D Collectors (9)
1. **Civil3DAlignmentCollector** - Stations, Length, Style, Design speed
2. **Civil3DSurfaceCollector** - Statistics, Boundaries, Analysis, Style
3. **Civil3DCorridorCollector** - Baselines, Regions, Feature lines
4. **Civil3DPipeNetworkCollector** - Parts, Structures, Network references
5. **Civil3DDocumentCollector** - Civil 3D drawing-level objects
6. **Civil3DProfileCollector** - Start/End stations, Min/Max elevation, Profile type
7. **Civil3DProfileViewCollector** - Station/Elevation ranges, Location
8. **Civil3DAssemblyCollector** - Groups, Code set style
9. **Civil3DPointGroupCollector** - Point count, Query builder

#### Universal Collector
- **ReflectionCollector** - Automatically handles ANY object using reflection (fallback for objects without specialized collectors)

---

## 💡 Features in Detail

### 1. Object Inspection

**Multiple Selection Methods:**
- **Tree Navigation**: Browse database hierarchy (Blocks, Layers, Entities, etc.)
- **Pick from Drawing**: Click **[Select Object]** and pick visually
- **By Handle**: Use `SNOOPHANDLE` command for direct access
- **Context Menu**: Right-click → UnifiedSnoop → Snoop Object

### 2. Export Capabilities

**Three Export Formats:**

#### CSV Export
```csv
Property,Type,Value
Name,String,Line
Start Point,Point3d,"(0.0000, 0.0000, 0.0000)"
End Point,Point3d,"(10.0000, 10.0000, 0.0000)"
Length,Double,14.1421
```

#### Excel Export (Tab-Delimited)
Compatible with Microsoft Excel and LibreOffice Calc.

#### JSON Export
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

**Use Cases:**
- Data analysis and reporting
- API integration
- Automated testing
- Documentation generation

### 3. Object Comparison

Compare any two objects side-by-side:
- ✅ Visual difference highlighting (yellow)
- ✅ "Show Differences Only" filter
- ✅ Export comparison results to CSV
- ✅ Works with any object types

**Example Use Cases:**
- Compare styles
- Verify object consistency
- Identify changes between versions

### 4. Bookmarks

Save frequently-accessed objects:
- ✅ Custom names for easy identification
- ✅ Persistent across sessions
- ✅ Quick "Go To" navigation
- ✅ Bulk delete and clear options

### 5. Search & Filter

Real-time property filtering:
- Search by property name (e.g., "Layer", "Color")
- Search by property value (e.g., "0", "Red")
- Instant results as you type
- Clear button to reset

### 6. Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `F5` | Refresh |
| `Ctrl+F` | Focus search |
| `Ctrl+C` | Copy value |
| `Ctrl+Shift+C` | Copy all |
| `Ctrl+L` | Focus tree |
| `Ctrl+P` | Focus properties |
| `Ctrl+B` | Add bookmark |
| `Ctrl+Shift+B` | View bookmarks |
| `Esc` | Clear search / Close |

### 7. XRecord Support

Comprehensive viewing of custom application data:
- ✅ **Automatic Detection** - XRecords appear in dictionary trees
- ✅ **DXF Code Interpretation** - All standard DXF code ranges supported
- ✅ **Type Formatting** - Strings, Integers, Doubles, Points, Handles, Booleans
- ✅ **Extended Data** - XData entries (codes 1000+) with proper formatting
- ✅ **Entry Indexing** - Each ResultBuffer entry numbered for reference

**Common Locations:**
- Named Object Dictionary (NOD)
- Extension Dictionaries
- Custom application dictionaries

**Use Cases:**
- Debug custom application data
- Recover information from third-party apps
- Verify data storage during development
- Analyze drawing custom data

📖 See [XRECORD_SUPPORT.md](Documentation/XRECORD_SUPPORT.md) for detailed documentation.

---

## 🛠️ Development

### Build Requirements

- Visual Studio 2022 or later
- .NET Framework 4.8 SDK (for AutoCAD 2024)
- .NET 8.0 SDK (for AutoCAD 2025+)
- AutoCAD 2024 or Civil 3D 2024 (for .NET 4.8 development)
- AutoCAD 2025+ or Civil 3D 2025+ (for .NET 8.0 development)

### Project Structure

**Multi-Targeting:**
The project uses multi-targeting to support both .NET Framework 4.8 and .NET 8.0 from a single codebase:

```xml
<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
```

**Conditional Compilation:**
```csharp
#if NET8_0_OR_GREATER
    // .NET 8.0 specific code
#else
    // .NET Framework 4.8 specific code
#endif
```

### Building

#### Option 1: Visual Studio
```
1. Open UnifiedSnoop.sln
2. Select configuration (Debug or Release)
3. Build → Build Solution (Ctrl+Shift+B)
```

#### Option 2: Command Line
```powershell
# Build all targets
dotnet build UnifiedSnoop/UnifiedSnoop.csproj -c Release

# Build specific target
dotnet build UnifiedSnoop/UnifiedSnoop.csproj -c Release -f net48
dotnet build UnifiedSnoop/UnifiedSnoop.csproj -c Release -f net8.0-windows
```

### Deployment

```powershell
# Deploy to bundle location
.\UnifiedSnoop\Deploy\Deploy-ToBundle.ps1
```

**Output Structure:**
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
├── PackageContents.xml
├── Contents\
│   ├── 2024\              # .NET 4.8 DLLs
│   │   └── UnifiedSnoop.dll
│   └── 2025\              # .NET 8.0 DLLs
│       └── UnifiedSnoop.dll
```

---

## 📚 Documentation

- **[User Guide](Documentation/USER_GUIDE.md)** - Complete usage instructions
- **[Deployment Guide](Documentation/DEPLOYMENT_GUIDE.md)** - Installation and deployment
- **[Development Rules](DEVELOPMENT_RULES.md)** - Coding standards and workflow
- **[Architecture Overview](Documentation/ARCHITECTURE.md)** - System design (coming soon)

---

## 🧪 Testing

### Manual Testing Checklist

- [ ] Test `SNOOP` command
- [ ] Test `SNOOPHANDLE` command
- [ ] Test object selection (tree, pick, handle)
- [ ] Test all export formats (CSV, Excel, JSON)
- [ ] Test object comparison
- [ ] Test bookmarks (add, view, delete)
- [ ] Test search functionality
- [ ] Test all keyboard shortcuts
- [ ] Test context menu integration
- [ ] Verify with both AutoCAD 2024 and 2025+
- [ ] Verify with Civil 3D objects (if available)

### Known Limitations

1. **Civil 3D Objects**: Require Civil 3D installation and `#if CIVIL3D` compilation flag
2. **Large Databases**: May be slow when expanding all tree nodes at once (use search instead)
3. **Read-Only**: No object modification capabilities (by design)

---

## 🐛 Troubleshooting

### Common Issues

**Problem: Command not found**
- Verify plugin is installed in `C:\ProgramData\Autodesk\ApplicationPlugins\`
- Restart AutoCAD
- Try manual `NETLOAD`

**Problem: UI not showing**
- Check AutoCAD command line for errors
- Verify correct DLL version (2024 vs 2025+)
- Check error logs in `%TEMP%\UnifiedSnoop\Logs\`

**Problem: Civil 3D objects not working**
- Ensure Civil 3D is installed (not just AutoCAD)
- Verify Civil 3D API references are loaded

### Error Logs

Location: `C:\Users\[Username]\AppData\Local\Temp\UnifiedSnoop\Logs\`

Format: `UnifiedSnoop_YYYYMMDD.log`

---

## 📚 Documentation

**All project documentation has been consolidated in the `Documentation/` folder**

### Quick Links

#### For Users
- **[User Guide](../Documentation/User/USER_GUIDE.md)** - Complete usage guide
- **[XRecord Editor](../Documentation/User/XRECORD_EDITOR.md)** - XRecord editing feature
- **[Deployment Guide](../Documentation/Deployment/DEPLOYMENT_GUIDE.md)** - Installation and setup

#### For Developers
- **[Development Rules](../Documentation/Development/DEVELOPMENT_RULES.md)** - Coding standards and guidelines
- **[Architecture Diagram](../Documentation/Technical/UnifiedSnoop_Architecture.drawio)** - System architecture
- **[Implementation Report](../Documentation/Technical/IMPLEMENTATION_REPORT.md)** - Technical details
- **[Version Compatibility](../Documentation/Technical/VERSION_COMPATIBILITY.md)** - Multi-version support

#### For UI/UX
- **[UI Specification](../Documentation/UI/UI_SPECIFICATION.md)** - Complete UI specification
- **[UI Layout Diagram](../Documentation/UI/UI_Layout.drawio)** - Visual layout reference

### Documentation Structure

```
Documentation/
├── UI/                    - User interface docs
├── User/                  - End-user guides  
├── Technical/             - Technical specs and architecture
├── Deployment/            - Build and deployment guides
├── Development/           - Developer guides and milestones
└── README.md             - Documentation index (start here!)
```

**📖 [View Complete Documentation Index](../Documentation/README.md)**

---

## 📝 Version History

### Version 1.0.0 (Current)
- ✅ Initial release
- ✅ Full AutoCAD and Civil 3D support
- ✅ 16 specialized collectors (7 AutoCAD + 9 Civil 3D)
- ✅ Export to CSV, Excel, JSON
- ✅ Object comparison with difference highlighting
- ✅ Bookmark management
- ✅ Search and filter
- ✅ Context menu integration
- ✅ Keyboard shortcuts
- ✅ Enhanced error logging
- ✅ Multi-targeting (.NET 4.8 and .NET 8.0)
- ✅ `SNOOPHANDLE` command
- ✅ Comprehensive documentation

---

## 🤝 Contributing

Contributions are welcome! Areas for enhancement:
- Additional specialized collectors for more entity types
- Performance optimizations for large databases
- Additional export formats
- UI improvements and themes
- Automated testing framework

---

## 📄 License

[Your license here]

---

## 🙏 Acknowledgments

Built on the foundation of:
- **MgdDbg** (Managed Database Debugger) - Autodesk sample
- **AutoCAD .NET API** - Autodesk
- **Civil 3D .NET API** - Autodesk

---

## 📧 Contact

For issues, questions, or feature requests, please contact [your contact info].

---

**Made with ❤️ for the AutoCAD and Civil 3D community**
