# UnifiedSnoop - Implementation Plan & Solution Architecture

**Project:** UnifiedSnoop - Unified AutoCAD & Civil 3D Inspection Tool  
**Date:** November 14, 2025  
**Architecture Diagram:** `UnifiedSnoop_Architecture.drawio`

---

## 🎯 **Project Goals**

1. ✅ **Create NEW project** - Zero modifications to original code
2. ✅ **Use Civil3DSnoop UI** as the main form foundation
3. ✅ **Extend for AutoCAD** general object inspection
4. ✅ **All C#** - No VB.NET code
5. ✅ **Unified experience** - Single tool for both platforms
6. ✅ **Modular architecture** - Easy to extend and maintain

---

## 📐 **Architecture Overview**

### **Layer Architecture (6 Layers)**

```
┌──────────────────────────────────────────────────────┐
│  Layer 1: Platform Layer (AutoCAD + Civil 3D APIs)  │
├──────────────────────────────────────────────────────┤
│  Layer 2: Application Layer (Commands & Entry)      │
├──────────────────────────────────────────────────────┤
│  Layer 3: Core Infrastructure (Collectors & Data)   │
├──────────────────────────────────────────────────────┤
│  Layer 4: Services Layer (Business Logic)           │
│         Layer 5: Inspectors Layer (Type-Specific)    │
├──────────────────────────────────────────────────────┤
│  Layer 6: UI Layer (Civil3DSnoop Style Interface)   │
└──────────────────────────────────────────────────────┘
```

---

## 🏗️ **Detailed Layer Specifications**

### **Layer 1: Platform Layer**

**Purpose:** Interface with AutoCAD and Civil 3D APIs

**Components:**
- AutoCAD .NET API (v25.0.1)
- Civil 3D .NET API (v13.8.280)
- .NET 8.0 Runtime
- Platform Detection Service

**Key Features:**
- Automatic Civil 3D detection
- Conditional API loading
- Runtime assembly checking

---

### **Layer 2: Application Layer**

**Purpose:** Application initialization, commands, and menu integration

#### **2.1 App.cs**
```csharp
[assembly: ExtensionApplication(typeof(UnifiedSnoop.App.SnoopApplication))]

public class SnoopApplication : IExtensionApplication
{
    public void Initialize()
    {
        // Register collectors
        // Add context menu
        // Detect platform (AutoCAD vs Civil 3D)
    }
    
    public void Terminate()
    {
        // Cleanup
    }
}
```

#### **2.2 Commands.cs**
```csharp
[CommandMethod("SnoopDatabase")]
[CommandMethod("SnoopEntities")]
[CommandMethod("SnoopCivil3DDoc")]     // Only if Civil 3D available
[CommandMethod("SnoopAlignments")]     // Only if Civil 3D available
```

#### **2.3 ContextMenu.cs**
- Right-click menu integration
- Dynamic menu based on platform
- AutoCAD options always visible
- Civil 3D submenu when available

---

### **Layer 3: Core Infrastructure**

**Purpose:** Foundation classes and data collection framework

#### **3.1 Collectors (ICollector Interface)**

**Base Interface:**
```csharp
public interface ICollector
{
    string Name { get; }
    bool CanCollect(object obj);
    List<PropertyData> Collect(object obj, Transaction trans);
    Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans);
}
```

**Implementations:**
1. **ReflectionCollector** - Uses .NET Reflection (Civil3DSnoop approach)
2. **PropertyCollector** - Extracts standard properties
3. **MethodCollector** - Discovers parameterless methods

#### **3.2 Data Models**

**PropertyData:**
```csharp
public class PropertyData
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public object RawValue { get; set; }
    public string DeclaringType { get; set; }
    public bool IsCollection { get; set; }
    public bool HasError { get; set; }
}
```

**ObjectNode:**
```csharp
public class ObjectNode
{
    public string Name { get; set; }
    public object Object { get; set; }
    public ObjectId ObjectId { get; set; }
    public List<ObjectNode> Children { get; set; }
    public bool IsCollection { get; set; }
}
```

#### **3.3 Helpers**

**TransactionHelper:**
```csharp
public class TransactionHelper : IDisposable
{
    private Database _db;
    private Transaction _trans;
    
    public void Start() { }
    public void Commit() { }
    public void Abort() { }
    public void Dispose() { }
}
```

**ReflectionHelper:**
```csharp
public static class ReflectionHelper
{
    public static PropertyInfo[] GetPublicProperties(Type type)
    public static bool IsEnumerable(object obj)
    public static string FormatValue(object value)
}
```

---

### **Layer 4: Services Layer**

**Purpose:** Business logic and orchestration

#### **4.1 ObjectDiscoveryService**

```csharp
public class ObjectDiscoveryService
{
    public ObjectNode BuildDatabaseTree(Database db, Transaction trans)
    public ObjectNode BuildCivil3DTree(CivilDocument civilDoc, Transaction trans)
    public List<ObjectNode> FindEntities(SelectionSet ss, Transaction trans)
}
```

**Responsibilities:**
- Discover objects in database
- Build object hierarchies
- Handle selection sets
- Navigate collections

#### **4.2 PropertyExtractionService**

```csharp
public class PropertyExtractionService
{
    public List<PropertyData> ExtractProperties(object obj, Transaction trans)
    public Dictionary<string, IEnumerable> ExtractCollections(object obj, Transaction trans)
    public string FormatValue(object value)
}
```

**Responsibilities:**
- Extract object properties
- Format values for display
- Handle collections
- Error management

#### **4.3 PlatformDetectionService**

```csharp
public static class PlatformDetectionService
{
    public static bool IsCivil3DAvailable()
    public static bool IsCivil3DDocument(Database db)
    public static List<string> GetAvailableFeatures()
}
```

**Responsibilities:**
- Detect Civil 3D availability
- Check if document is Civil 3D
- Return available features
- Conditional feature loading

---

### **Layer 5: Inspectors Layer**

**Purpose:** Type-specific object inspection

#### **5.1 AutoCAD Inspectors**

Each inspector handles a specific AutoCAD object type:

```csharp
public class DatabaseInspector : IInspector
{
    public List<PropertyData> Inspect(Database db, Transaction trans)
    {
        // Extract database-specific properties
        // Add custom formatted data
        // Return structured property list
    }
}
```

**Inspector Types:**
- DatabaseInspector
- EntityInspector
- BlockTableInspector
- LayerTableInspector
- DimensionStyleInspector
- TextStyleInspector
- LinetypeInspector
- ... more

#### **5.2 Civil 3D Inspectors**

Each inspector handles a specific Civil 3D object type:

```csharp
public class AlignmentInspector : IInspector
{
    public List<PropertyData> Inspect(Alignment alignment, Transaction trans)
    {
        // Extract alignment-specific properties
        // Add geometry information
        // Include related objects (profiles, etc.)
    }
}
```

**Inspector Types:**
- CivilDocumentInspector
- AlignmentInspector
- ProfileInspector
- SurfaceInspector
- CorridorInspector
- AssemblyInspector
- PipeNetworkInspector
- PointGroupInspector
- ... more

---

### **Layer 6: UI Layer (Civil3DSnoop Style)**

**Purpose:** User interface based on Civil3DSnoop pattern

#### **6.1 MainSnoopForm**

**Layout (Same as Civil3DSnoop):**
```
┌──────────────────────────────────────────────────────┐
│  Snoop AutoCAD / Civil 3D Database                   │
├──────────────────┬───────────────────────────────────┤
│                  │  Name          | Type    | Value  │
│  TreeView        ├───────────────────────────────────┤
│  (Objects)       │  Database      | String  | C:\... │
│                  │  BlockTable    | ObjectId| ...    │
│  ○ Database      │  LayerTable    | ObjectId| ...    │
│    ├─ Blocks     │  Entities      | Int     | 1250   │
│    ├─ Layers     │                                   │
│    ├─ Entities   │  ListView (Properties)            │
│  ○ Civil 3D      │                                   │
│    ├─ Alignments │  - Grouped by declaring type      │
│    ├─ Surfaces   │  - Sortable columns               │
│    └─ Corridors  │  - Click collection → expand tree │
│                  │                                   │
│                  │                                   │
├──────────────────┴───────────────────────────────────┤
│  [Select Object]  [Select Another File...]           │
└──────────────────────────────────────────────────────┘
```

**Features:**
- TreeView for object hierarchy
- ListView for properties (Name | Type | Value)
- Select object on-screen
- Open external files
- Export properties
- Search/filter

#### **6.2 Custom Controls**

**ObjectTreeView:**
```csharp
public class ObjectTreeView : TreeView
{
    public void LoadDatabase(Database db, Transaction trans)
    public void LoadCivil3DDocument(CivilDocument doc, Transaction trans)
    public void ExpandCollection(TreeNode node, IEnumerable collection)
}
```

**PropertyListView:**
```csharp
public class PropertyListView : ListView
{
    public void DisplayProperties(List<PropertyData> properties)
    public void GroupByType()
    public void EnableColumnSorting()
}
```

---

## 📋 **Implementation Phases**

### **Phase 1: Project Setup & Foundation** (Week 1)

#### **Day 1-2: Project Creation**
- [ ] Create new solution `UnifiedSnoop.sln`
- [ ] Set up project structure (folders)
- [ ] Add NuGet packages:
  - AutoCAD.NET (25.0.1)
  - AutoCAD.NET.Model (25.0.0)
  - Civil3D.NET (13.8.280)
- [ ] Configure .NET 8.0 target framework
- [ ] Set up build configurations

#### **Day 3-4: Core Infrastructure**
- [ ] Create ICollector interface
- [ ] Implement ReflectionCollector (port from VB)
- [ ] Create PropertyData model
- [ ] Create ObjectNode model
- [ ] Implement TransactionHelper

#### **Day 5: Platform Detection**
- [ ] Implement PlatformDetectionService
- [ ] Test Civil 3D detection
- [ ] Test assembly loading

---

### **Phase 2: UI Foundation** (Week 2)

#### **Day 1-3: Main Form (Convert from VB to C#)**
- [ ] Create MainSnoopForm.cs
- [ ] Design UI layout (same as Civil3DSnoop)
  - TreeView (left panel)
  - ListView (right panel)
  - Buttons (bottom)
- [ ] Implement form load logic
- [ ] Implement tree node selection handler
- [ ] Implement property display logic

#### **Day 4-5: Custom Controls**
- [ ] Create ObjectTreeView
- [ ] Create PropertyListView
- [ ] Implement collection expansion
- [ ] Implement property grouping

---

### **Phase 3: Application Layer** (Week 3)

#### **Day 1-2: App Entry Point**
- [ ] Create App.cs with IExtensionApplication
- [ ] Implement Initialize()
- [ ] Implement Terminate()
- [ ] Register collectors
- [ ] Add initialization messages

#### **Day 3-4: Commands**
- [ ] Implement SnoopDatabase command
- [ ] Implement SnoopEntities command
- [ ] Implement SnoopCivil3DDoc command (conditional)
- [ ] Implement SnoopAlignments command (conditional)

#### **Day 5: Context Menu**
- [ ] Create ContextMenu.cs
- [ ] Add right-click menu items
- [ ] Implement dynamic menu (show Civil 3D options if available)
- [ ] Wire up command execution

---

### **Phase 4: Services Layer** (Week 4)

#### **Day 1-2: ObjectDiscoveryService**
- [ ] Implement BuildDatabaseTree()
- [ ] Implement BuildCivil3DTree()
- [ ] Implement FindEntities()
- [ ] Handle selection sets

#### **Day 3-4: PropertyExtractionService**
- [ ] Implement ExtractProperties()
- [ ] Implement ExtractCollections()
- [ ] Implement value formatting
- [ ] Handle special cases (ObjectId, collections, etc.)

#### **Day 5: Integration Testing**
- [ ] Test in plain AutoCAD
- [ ] Test in Civil 3D
- [ ] Verify platform detection
- [ ] Test all commands

---

### **Phase 5: AutoCAD Inspectors** (Week 5)

#### **Day 1: Core Inspectors**
- [ ] DatabaseInspector
- [ ] EntityInspector
- [ ] BlockTableInspector

#### **Day 2-3: Symbol Table Inspectors**
- [ ] LayerTableInspector
- [ ] LinetypeInspector
- [ ] TextStyleInspector
- [ ] DimensionStyleInspector

#### **Day 4-5: Entity Inspectors**
- [ ] LineInspector
- [ ] ArcInspector
- [ ] CircleInspector
- [ ] PolylineInspector
- [ ] TextInspector
- [ ] DimensionInspector

---

### **Phase 6: Civil 3D Inspectors** (Week 6)

#### **Day 1: Foundation**
- [ ] CivilDocumentInspector
- [ ] StylesInspector

#### **Day 2: Alignment Objects**
- [ ] AlignmentInspector
- [ ] ProfileInspector
- [ ] ProfileViewInspector

#### **Day 3: Surface & Corridor**
- [ ] SurfaceInspector (TinSurface, GridSurface)
- [ ] CorridorInspector
- [ ] CorridorSurfaceInspector

#### **Day 4: Pipe Networks**
- [ ] PipeNetworkInspector
- [ ] PipeInspector
- [ ] StructureInspector

#### **Day 5: Other Objects**
- [ ] AssemblyInspector
- [ ] SubassemblyInspector
- [ ] PointGroupInspector

---

### **Phase 7: Advanced Features** (Week 7)

#### **Day 1-2: Enhanced UI Features**
- [ ] Search/filter in TreeView
- [ ] Search/filter in ListView
- [ ] Export to CSV
- [ ] Export to JSON

#### **Day 3-4: Additional Commands**
- [ ] Snoop by Handle
- [ ] Snoop nested entities
- [ ] Compare two objects

#### **Day 5: Error Handling**
- [ ] Comprehensive try-catch blocks
- [ ] User-friendly error messages
- [ ] Logging mechanism

---

### **Phase 8: Testing & Documentation** (Week 8)

#### **Day 1-3: Comprehensive Testing**
- [ ] Test all AutoCAD object types
- [ ] Test all Civil 3D object types
- [ ] Test in AutoCAD 2024
- [ ] Test in Civil 3D 2024/2025
- [ ] Test with large drawings
- [ ] Test with complex Civil 3D models

#### **Day 4: Documentation**
- [ ] Create README.md
- [ ] Create user guide
- [ ] Document architecture
- [ ] Add code comments

#### **Day 5: Packaging**
- [ ] Create .bundle structure
- [ ] Test deployment
- [ ] Create installer/setup guide
- [ ] Final release prep

---

## 🔑 **Key Design Decisions**

### **1. UI Choice: Civil3DSnoop Style** ✅

**Rationale:**
- Simple, effective layout
- TreeView + ListView pattern works well
- Familiar to users
- Easy to implement in C#

**Benefits:**
- Clean, uncluttered interface
- Hierarchical object navigation
- Property grouping by type
- Collection expansion inline

### **2. Data Collection: Reflection-Based** ✅

**Rationale:**
- Works for any object type
- No need to know structure in advance
- Handles new AutoCAD/Civil 3D versions
- Same approach as original Civil3DSnoop

**Augmented with:**
- Type-specific inspectors for better formatting
- Known property handling
- Banned property list for problematic properties

### **3. Extensibility: Inspector Pattern** ✅

**Rationale:**
- Easy to add new object types
- Separation of concerns
- Type-specific formatting
- Optional enhancement of reflection data

**Structure:**
```csharp
public interface IInspector
{
    bool CanInspect(object obj);
    List<PropertyData> Inspect(object obj, Transaction trans);
}
```

### **4. Platform Detection: Runtime** ✅

**Rationale:**
- Single DLL works in both AutoCAD and Civil 3D
- No separate builds
- Automatic feature enablement
- Graceful degradation

**Implementation:**
- Check for Civil 3D assemblies at runtime
- Conditionally load Civil 3D collectors
- Dynamic menu generation
- Feature availability reporting

---

## 📦 **Project Structure**

```
UnifiedSnoop/
├── UnifiedSnoop.sln
├── UnifiedSnoop.csproj
├── README.md
├── LICENSE
│
├── App/
│   ├── App.cs                        # IExtensionApplication
│   ├── Commands.cs                   # AutoCAD commands
│   └── ContextMenu.cs                # Right-click menu
│
├── Core/
│   ├── Collectors/
│   │   ├── ICollector.cs             # Collector interface
│   │   ├── ReflectionCollector.cs    # Reflection-based
│   │   └── PropertyCollector.cs      # Property extraction
│   ├── Data/
│   │   ├── PropertyData.cs           # Property model
│   │   ├── ObjectNode.cs             # Tree node model
│   │   └── CollectionInfo.cs         # Collection metadata
│   └── Helpers/
│       ├── TransactionHelper.cs      # Transaction management
│       ├── ObjectHelper.cs           # Object utilities
│       └── ReflectionHelper.cs       # Reflection utilities
│
├── Services/
│   ├── ObjectDiscoveryService.cs     # Find/discover objects
│   ├── PropertyExtractionService.cs  # Extract properties
│   └── PlatformDetectionService.cs   # Detect AutoCAD/Civil3D
│
├── Inspectors/
│   ├── IInspector.cs                 # Inspector interface
│   ├── AutoCAD/
│   │   ├── DatabaseInspector.cs
│   │   ├── EntityInspector.cs
│   │   ├── BlockInspector.cs
│   │   └── ... (15+ inspectors)
│   └── Civil3D/
│       ├── CivilDocumentInspector.cs
│       ├── AlignmentInspector.cs
│       ├── SurfaceInspector.cs
│       └── ... (12+ inspectors)
│
├── UI/
│   ├── Forms/
│   │   ├── MainSnoopForm.cs          # Main form
│   │   ├── MainSnoopForm.Designer.cs
│   │   └── MainSnoopForm.resx
│   └── Controls/
│       ├── ObjectTreeView.cs         # Custom TreeView
│       └── PropertyListView.cs       # Custom ListView
│
└── Resources/
    └── Icons/                        # UI icons
```

---

## 🎨 **UI Screenshots (Concept)**

### **AutoCAD Mode:**
```
┌──────────────────────────────────────────────────────┐
│  Snoop AutoCAD Database                              │
├──────────────────┬───────────────────────────────────┤
│  ○ Database      │  Name          | Type    | Value  │
│    ├─ BlockTabl e│  FileName      | String  | C:\... │
│    ├─ LayerTable │  NumberOfSaves | Int     | 15     │
│    ├─ Entities   │  ApproxNumObjs | Int     | 1250   │
│    └─ ...        │                                   │
│                  │                                   │
└──────────────────┴───────────────────────────────────┘
```

### **Civil 3D Mode:**
```
┌──────────────────────────────────────────────────────┐
│  Snoop Civil 3D Database                             │
├──────────────────┬───────────────────────────────────┤
│  ○ Database      │  Name          | Type    | Value  │
│  ○ Civil 3D      │  Name          | String  | Road A │
│    ├─ Alignments │  Length        | Double  | 1250.5 │
│    │  └─ Road A  │  Type          | Enum    | Centerl│
│    ├─ Surfaces   │  StartStation  | Double  | 0.0    │
│    ├─ Corridors  │                                   │
│    └─ ...        │                                   │
└──────────────────┴───────────────────────────────────┘
```

---

## ⚡ **Technology Stack**

| Component | Technology | Version |
|-----------|-----------|---------|
| Language | C# | 12.0 |
| Framework | .NET | 8.0 |
| UI | WinForms | Built-in |
| AutoCAD API | AutoCAD.NET | 25.0.1 |
| Civil 3D API | Civil3D.NET | 13.8.280 |
| Build Tool | MSBuild | 17.0+ |
| IDE | Visual Studio | 2022 |

---

## 🔒 **Design Constraints**

### **Hard Constraints:**
1. ✅ **No modification** to original projects
2. ✅ **C# only** - No VB.NET
3. ✅ **Civil3DSnoop UI** as foundation
4. ✅ **Extended for AutoCAD** objects

### **Soft Constraints:**
1. ⚠️ **Single DLL** - Works in both AutoCAD and Civil 3D
2. ⚠️ **Backward compatible** - Works with AutoCAD 2024+
3. ⚠️ **Performance** - Handle large drawings efficiently
4. ⚠️ **User-friendly** - Clear, intuitive interface

---

## 📊 **Success Criteria**

### **Functional Requirements:**
- [ ] Loads in AutoCAD 2024+
- [ ] Loads in Civil 3D 2024+
- [ ] Displays AutoCAD database objects
- [ ] Displays Civil 3D objects (when available)
- [ ] Allows on-screen entity selection
- [ ] Allows opening external files
- [ ] Shows properties via reflection
- [ ] Expands collections
- [ ] Groups properties by type

### **Non-Functional Requirements:**
- [ ] Response time < 1 second for typical objects
- [ ] Handles drawings with 10,000+ entities
- [ ] No crashes on unknown object types
- [ ] Clean error messages
- [ ] Professional UI appearance

---

## 🚀 **Deployment**

### **Bundle Structure:**
```
UnifiedSnoop.bundle/
├── PackageContents.xml
└── Contents/
    ├── NET80/
    │   ├── UnifiedSnoop.dll
    │   └── UnifiedSnoop.deps.json
    └── Resources/
        └── Icons/
```

### **Installation:**
1. Copy `.bundle` folder to:
   ```
   %APPDATA%\Autodesk\ApplicationPlugins\
   ```
2. Launch AutoCAD or Civil 3D
3. Right-click → "Snoop" menu appears
4. Run commands or use context menu

---

## 📚 **References**

1. **Architecture Diagram:** `UnifiedSnoop_Architecture.drawio`
2. **Original Projects:**
   - Civil3DSnoop-NET8 (VB.NET)
   - MgdDbg-master (C#)
3. **API Documentation:**
   - [AutoCAD .NET API](https://help.autodesk.com/view/OARX/2024/ENU/)
   - [Civil 3D API](https://help.autodesk.com/view/CIV3D/2024/ENU/)

---

## ✅ **Next Steps**

1. **Review this plan** - Ensure alignment with goals
2. **Set up development environment** - VS 2022, AutoCAD 2024, Civil 3D 2024
3. **Begin Phase 1** - Project setup and foundation
4. **Create git repository** - Version control from day one
5. **Start coding!** - Follow the 8-week implementation plan

---

**Total Estimated Time:** 8 weeks (1 developer, full-time)  
**Alternative:** 3-4 months (part-time development)

**Status:** ✅ **READY TO BEGIN**

