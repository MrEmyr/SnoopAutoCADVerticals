# UnifiedSnoop - UI Specification

**Version:** 1.0.0  
**Last Updated:** 2025-11-19  
**Platform:** Windows Forms (.NET Framework 4.8 / .NET 8.0)

---

## Table of Contents

1. [Overview](#overview)
2. [Application Window](#application-window)
3. [Layout Structure](#layout-structure)
4. [Components](#components)
5. [Visual Design](#visual-design)
6. [Interaction Patterns](#interaction-patterns)
7. [Features](#features)
8. [Error Handling](#error-handling)
9. [Keyboard Shortcuts](#keyboard-shortcuts)

---

## Overview

UnifiedSnoop is a database inspector tool for AutoCAD and Civil 3D that provides a dual-pane interface for exploring database objects and their properties. The UI consists of a hierarchical tree view for navigation and a detailed property list for inspection.

### Design Goals

- **Clarity**: Easy to read and understand object hierarchies and properties
- **Efficiency**: Quick navigation and property inspection
- **Robustness**: Graceful error handling for unavailable or restricted properties
- **Discoverability**: Visual cues for interactive elements

---

## Application Window

### Window Properties

- **Title**: `"UnifiedSnoop - Database Inspector ({HostApplication})"`
  - `{HostApplication}` = "AutoCAD", "Civil 3D", or "Unknown"
- **Default Size**: 1200 × 700 pixels
- **Minimum Size**: 600 × 400 pixels
- **Icon**: Application icon (if available)
- **State**: 
  - Opens in Normal state
  - Position centered on screen
  - Supports maximize, minimize, and resize

### Window Layout Hierarchy

```
MainSnoopForm
├── SplitContainer (Horizontal, 2:3 ratio)
│   ├── Panel1 (Left - TreeView)
│   │   └── TreeView (Database Navigation)
│   └── Panel2 (Right - Property Inspector)
│       ├── SearchPanel (Docked Top, 35px)
│       │   ├── Label ("Search:")
│       │   ├── TextBox (Search input)
│       │   ├── Button ("Clear")
│       │   ├── Button ("Copy Value")
│       │   └── Button ("Copy All")
│       ├── HeaderSeparator (Docked Top, 25px)
│       └── ListViewContainer (Docked Fill)
│           └── ListView (Property Grid)
├── StatusPanel (Docked Bottom, 25px)
│   └── Label (Status text)
└── TopPanel (Docked Top, 35px)
    └── Label (Property count: "Loaded X properties for {ObjectType}")
```

---

## Layout Structure

### Split Container

- **Orientation**: Vertical split (left/right panels)
- **Splitter Width**: 4 pixels
- **Splitter Distance**: 400 pixels (left panel width)
- **Splitter Ratio**: Approximately 1:2 (33% left, 67% right)
- **Fixed Panel**: None (both panels resize proportionally)
- **Collapsible**: No
- **Border Style**: Fixed3D

### Panel Sizing

- **Left Panel (TreeView)**: 
  - Initial width: 400px
  - Minimum width: 200px
  - Fills full height between top and bottom panels

- **Right Panel (Property Inspector)**:
  - Initial width: 800px
  - Minimum width: 400px
  - Contains search bar, separator, and property list

---

## Components

### 1. Tree View (Database Navigator)

**Location**: Left panel of split container

**Properties**:
```csharp
{
    Dock = DockStyle.Fill,
    HideSelection = false,
    ShowLines = true,
    ShowPlusMinus = true,
    ShowRootLines = true,
    BackColor = Color.White,
    BorderStyle = BorderStyle.FixedSingle,
    FullRowSelect = true,
    ItemHeight = 20,
    Scrollable = true
}
```

**Structure**:
```
Database (Root)
├── AutoCAD Collections
│   ├── Block Definitions (BlockTable)
│   │   └── [Block entries...]
│   ├── Layers (LayerTable)
│   │   └── [Layer entries...]
│   ├── Text Styles (TextStyleTable)
│   ├── Dimension Styles (DimStyleTable)
│   ├── Linetypes (LinetypeTable)
│   ├── Named Objects Dictionary
│   │   └── [Dictionary entries...]
│   └── Model Space
│       └── [Entity entries...]
└── Civil 3D Collections (if available)
    ├── Alignments
    ├── Surfaces
    ├── Corridors
    ├── Profiles
    ├── Pipe Networks
    └── [Other Civil 3D objects...]
```

**Node Format**:
- Simple objects: `"{Name} [{Type}]"` or `"{Type}"`
- Collections: `"{CollectionName} ({Count} items)"`
- Errors: Red text with error icon
- Diagnostics: Gray text for information messages

**Interaction**:
- Single-click: Select node
- Double-click: Expand/collapse (if has children)
- On selection: Load properties into right panel

### 2. Search Panel

**Location**: Top of right panel (docked)

**Dimensions**: 
- Height: 35px
- Padding: 5px all sides
- Background: SystemColors.Control

**Components**:

#### Search Label
```csharp
{
    Text = "Search:",
    Width = 60,
    TextAlign = ContentAlignment.MiddleLeft,
    Dock = DockStyle.Left
}
```

#### Search TextBox
```csharp
{
    Width = 200,
    Dock = DockStyle.Left,
    Margin = new Padding(5, 0, 10, 0)
}
```
- Real-time filtering as user types
- Case-insensitive search
- Searches property names, types, and values

#### Clear Button
```csharp
{
    Text = "Clear",
    Width = 70,
    Dock = DockStyle.Left,
    Margin = new Padding(5, 0, 5, 0)
}
```
- Clears search filter
- Returns to full property list

#### Copy Value Button
```csharp
{
    Text = "Copy Value",
    Width = 90,
    Dock = DockStyle.Left,
    Margin = new Padding(5, 0, 5, 0)
}
```
- Copies selected property value to clipboard
- Disabled when no selection

#### Copy All Button
```csharp
{
    Text = "Copy All",
    Width = 80,
    Dock = DockStyle.Left,
    Margin = new Padding(5, 0, 5, 0)
}
```
- Copies all visible properties to clipboard
- Format: Tab-separated values (TSV)

### 3. Header Separator

**Location**: Between search panel and ListView

**Properties**:
```csharp
{
    Height = 25,
    Dock = DockStyle.Top,
    BackColor = SystemColors.ControlLight,
    BorderStyle = BorderStyle.FixedSingle,
    Text = ""  // Empty label for spacing
}
```

**Purpose**: Provides visual space to ensure ListView column headers are visible

### 4. ListView (Property Grid)

**Location**: Right panel, below search panel and separator

**Properties**:
```csharp
{
    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | 
             AnchorStyles.Left | AnchorStyles.Right,
    Location = new Point(5, 30),  // 30px from top
    View = View.Details,
    FullRowSelect = true,
    GridLines = true,
    HideSelection = false,
    MultiSelect = false,
    BackColor = Color.White,
    BorderStyle = BorderStyle.FixedSingle,
    HeaderStyle = ColumnHeaderStyle.Clickable,
    Scrollable = true
}
```

**Sizing**: Dynamically sized on container resize:
- Width: `Container.Width - 10px` (5px margins each side)
- Height: `Container.Height - 35px` (30px top, 5px bottom)

**Columns**:

| Column | Width | Alignment | Purpose |
|--------|-------|-----------|---------|
| Property | 250px | Left | Property name |
| Type | 180px | Left | Property data type |
| Value | Auto-fill (-2) | Left | Property value |

**Column Headers**:
- **Property**: Name of the object property
- **Type**: .NET type name (e.g., "String", "Int32", "ObjectId")
- **Value**: Formatted value or collection indicator

**Row Format**:
- Standard property: `PropertyName | TypeName | FormattedValue`
- Collection property: `PropertyName | TypeName | [Collection: X items]` (blue text)
- Error property: `PropertyName | TypeName | [Error: message]` (red text)
- Not applicable: `PropertyName | TypeName | [Not Applicable]` (gray text)

**Visual Indicators**:

1. **Collection Items** (Blue):
   - Color: `Color.Blue`
   - Text: `"[Collection: X items]"`
   - Font: Standard
   - Cursor: Hand pointer on hover
   - Action: Double-click to expand in tree

2. **Error Items** (Red):
   - Color: `Color.Red`
   - Text: `"[Error: {message}]"`
   - Font: Standard
   - Indicates critical property access failure

3. **Not Applicable Items** (Gray):
   - Color: `Color.Gray`
   - Text: `"[Not Applicable]"`
   - Font: Standard
   - Indicates expected/benign property access failure

### 5. Top Panel (Property Count)

**Location**: Top of main form (docked)

**Properties**:
```csharp
{
    Height = 35,
    Dock = DockStyle.Top,
    BackColor = SystemColors.Control,
    Padding = new Padding(10, 8, 10, 8)
}
```

**Content**: Label showing:
- `"Loaded X properties for {ObjectType}"`
- Example: `"Loaded 325 properties for Database"`
- Color: Blue text
- Font: Bold

### 6. Bottom Panel (Status Bar)

**Location**: Bottom of main form (docked)

**Properties**:
```csharp
{
    Height = 25,
    Dock = DockStyle.Bottom,
    BackColor = SystemColors.Control,
    Padding = new Padding(10, 4, 10, 4),
    BorderStyle = BorderStyle.FixedSingle
}
```

**Content**: Label showing:
- Status messages
- Selected object information
- Operation feedback
- Default: `"Ready"`

---

## Visual Design

### Color Scheme

| Element | Color | RGB | Usage |
|---------|-------|-----|-------|
| Background (Panels) | SystemColors.Control | System | Standard panel background |
| Content Background | White | 255,255,255 | TreeView, ListView |
| Standard Text | Black | 0,0,0 | Normal text |
| Collection Text | Blue | 0,0,255 | Clickable collections |
| Error Text | Red | 255,0,0 | Critical errors |
| Not Applicable Text | Gray | 128,128,128 | Benign failures |
| Status Text | Blue | 0,0,255 | Information messages |
| Border | System | System | Fixed3D style |
| Separator | SystemColors.ControlLight | System | Visual spacing |

### Typography

| Element | Font | Size | Style |
|---------|------|------|-------|
| Standard Text | Segoe UI | 9pt | Regular |
| Property Count | Segoe UI | 9pt | Bold |
| Status Text | Segoe UI | 9pt | Regular |
| TreeView Nodes | Segoe UI | 9pt | Regular |
| ListView Items | Segoe UI | 9pt | Regular |
| Column Headers | Segoe UI | 9pt | Bold |

### Spacing

- **Panel Padding**: 5-10px
- **Control Margins**: 5px between related controls
- **Button Spacing**: 5px horizontal gap
- **Row Height (TreeView)**: 20px
- **Row Height (ListView)**: Auto (based on font)
- **Splitter Width**: 4px
- **Border Width**: 1px (FixedSingle)

### Icons

- **TreeView Icons**: None (text-based hierarchy)
- **Error Indicators**: Red text color
- **Collection Indicators**: Blue text color
- **Cursor Changes**: Hand pointer for collections

---

## Interaction Patterns

### TreeView Interactions

1. **Select Node**:
   - **Action**: Single-click on node
   - **Result**: Load properties in right panel
   - **Visual Feedback**: Node highlights

2. **Expand/Collapse**:
   - **Action**: Click +/- icon OR double-click node
   - **Result**: Show/hide child nodes
   - **Visual Feedback**: Icon changes, children appear/disappear

3. **Lazy Loading**:
   - **Action**: Expand node with placeholder child
   - **Result**: Dynamically load actual children
   - **Visual Feedback**: Placeholder replaced with real nodes

### ListView Interactions

1. **Select Property**:
   - **Action**: Single-click on row
   - **Result**: Row highlights, Copy Value button enables
   - **Visual Feedback**: Full row selection

2. **Expand Collection** ⭐:
   - **Action**: Double-click on blue collection item
   - **Result**: Creates new tree node and expands collection contents
   - **Visual Feedback**: Tree updates, cursor returns to normal
   - **Requirements**: 
     - Item must be a collection (blue text)
     - Must have `[Collection: X items]` format

3. **Hover Collection**:
   - **Action**: Mouse over blue collection item
   - **Result**: Cursor changes to hand pointer
   - **Visual Feedback**: Indicates clickability

4. **Search/Filter**:
   - **Action**: Type in search box
   - **Result**: Real-time filtering of visible properties
   - **Visual Feedback**: ListView updates immediately

### Button Interactions

1. **Clear Search**:
   - **Action**: Click "Clear" button
   - **Result**: Clear search box, show all properties
   - **Visual Feedback**: Search box empties, list restores

2. **Copy Value**:
   - **Action**: Click "Copy Value" button (with selection)
   - **Result**: Copy selected property value to clipboard
   - **Visual Feedback**: Status message confirms copy

3. **Copy All**:
   - **Action**: Click "Copy All" button
   - **Result**: Copy all visible properties as TSV to clipboard
   - **Visual Feedback**: Status message shows count copied

---

## Features

### 1. Hierarchical Database Navigation

**Description**: Tree-based navigation of AutoCAD/Civil 3D database structure

**Components**:
- Root "Database" node
- AutoCAD Collections (always present)
- Civil 3D Collections (when available)

**Behavior**:
- Lazy loading for performance
- Automatic expansion of interesting nodes
- Error nodes for inaccessible collections

### 2. Property Inspection

**Description**: Detailed property listing using reflection

**Features**:
- Shows all public properties
- Displays property name, type, and value
- Handles errors gracefully
- Stores raw values for collection expansion

**Property Types Supported**:
- Primitives (int, double, string, bool, etc.)
- Complex objects (formatted ToString())
- Collections (IEnumerable, arrays, lists)
- ObjectIds (AutoCAD references)
- Enums (formatted names)

### 3. Collection Expansion ⭐

**Description**: Interactive expansion of collection properties

**Workflow**:
1. User views properties of an object
2. Collection properties display as `[Collection: X items]` in blue
3. User hovers over collection → cursor changes to hand
4. User double-clicks collection → system:
   - Creates new child node under current tree node
   - Iterates through collection items
   - Opens ObjectIds using transaction
   - Populates tree with collection contents
   - Selects first item in collection

**Supported Collection Types**:
- `IEnumerable<T>` (generic collections)
- `ObjectIdCollection` (AutoCAD)
- `DBObjectCollection` (AutoCAD)
- Arrays
- Lists

**Item Display Logic**:
- Simple types: Show directly (e.g., `"Item: 42"`)
- Objects with children: Show as expandable node
- ObjectIds: Open and show as object nodes
- Complex objects: Show ToString() representation

### 4. Real-Time Search/Filter

**Description**: Instant property filtering

**Behavior**:
- Case-insensitive search
- Searches across: property name, type, value
- Updates immediately as user types
- Maintains original data (non-destructive)

**Implementation**:
```csharp
if (name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
    type.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
    value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
{
    // Include in filtered results
}
```

### 5. Copy to Clipboard

**Features**:

1. **Copy Single Value**:
   - Copies value column of selected row
   - Strips brackets from special values
   - Plain text format

2. **Copy All Properties**:
   - Tab-separated values (TSV)
   - Format: `Property\tType\tValue\n`
   - Excel/spreadsheet compatible

### 6. Error Classification ⭐

**Description**: Intelligent error handling for property access

**Categories**:

1. **Critical Errors** (Red):
   - Unexpected exceptions
   - Security violations
   - System errors
   - Display: `[Error: {message}]`

2. **Expected/Benign Failures** (Gray):
   - NotApplicableException
   - NotSupportedException (for specific properties)
   - Erase notifications on non-erased objects
   - Empty/Not Found on inapplicable properties
   - Display: `[Not Applicable]`

**Suppressed Error Patterns**:
- `"Erase"` property → NotImplementedException
- `"Is*Enabled"` properties → NotSupportedException
- `"*NotFound"`, `"*Empty"` messages
- Common expected failures

### 7. Multi-Platform Support

**Platforms**:
- AutoCAD 2024 (.NET Framework 4.8)
- AutoCAD 2025+ (.NET 8.0)
- Civil 3D 2024 (.NET Framework 4.8)
- Civil 3D 2025+ (.NET 8.0)

**Detection**:
- Automatic host application detection
- Version-specific assembly loading
- Graceful degradation for missing features

---

## Error Handling

### Property Access Errors

**Strategy**: Try-catch with classification

```csharp
try {
    value = property.GetValue(obj);
    propData.Value = FormatValue(value);
}
catch (Exception ex) {
    if (IsExpectedPropertyError(ex.Message, property.Name)) {
        propData.Value = "[Not Applicable]";
        propData.HasError = false;  // Don't show as error
        propData.ForeColor = Color.Gray;
    }
    else {
        propData.Value = $"[Error: {ex.InnerException?.Message ?? ex.Message}]";
        propData.HasError = true;
        propData.ForeColor = Color.Red;
    }
}
```

### Collection Iteration Errors

**Strategy**: Skip item, continue iteration

```csharp
foreach (var item in collection) {
    try {
        // Process item
    }
    catch (Exception ex) {
        // Log and skip
        CreateErrorNode($"Error: {ex.Message}");
        continue;
    }
}
```

### TreeView Loading Errors

**Strategy**: Show diagnostic nodes

```csharp
if (civilDocument == null) {
    collections.Add(new ObjectNode {
        Name = "Civil 3D document not available",
        Type = "Diagnostic",
        // ... diagnostic info
    });
}
```

---

## Keyboard Shortcuts

| Shortcut | Action | Context |
|----------|--------|---------|
| Ctrl+C | Copy selected value | ListView focused |
| Ctrl+F | Focus search box | Any |
| Escape | Clear search | Search box focused |
| Enter | Expand/collapse node | TreeView focused |
| Enter | Expand collection | ListView collection item |
| Arrow Keys | Navigate | TreeView/ListView |
| Tab | Cycle through controls | Any |
| F5 | Refresh (future) | Any |

---

## Future Enhancements

### Planned Features

1. **Column Sorting**: Click headers to sort by property/type/value
2. **Property Editing**: Edit writable properties (advanced mode)
3. **Export to File**: Save property data to CSV/JSON
4. **Favorites**: Bookmark frequently accessed objects
5. **History**: Navigate back/forward through selections
6. **Search History**: Remember recent searches
7. **Themes**: Light/dark mode support
8. **Custom Filters**: Save and reuse filter patterns
9. **Property Comparison**: Compare two objects side-by-side
10. **Performance Monitoring**: Show load times for operations

### UI Improvements

1. **Tooltips**: Show full text for truncated values
2. **Context Menus**: Right-click actions for quick operations
3. **Resizable Columns**: Drag column dividers
4. **Row Highlighting**: Alternate row colors for readability
5. **Status Icons**: Visual indicators for property types
6. **Progress Bar**: Show progress for long operations
7. **Docking**: Detachable panels
8. **Multiple Windows**: Open multiple inspectors

---

## Technical Notes

### WinForms Layout Challenges

**ListView Header Visibility Issue**:

The ListView headers were initially obscured due to WinForms layout quirks with nested docking.

**Solution**:
1. Use explicit positioning (`Location = new Point(5, 30)`)
2. Use anchoring instead of docking for ListView
3. Add visual separator with fixed height (25px)
4. Dynamic sizing via container `SizeChanged` event
5. Start ListView 30px from container top

**Code Pattern**:
```csharp
// Container with Fill dock
Panel container = new Panel { Dock = DockStyle.Fill };

// ListView with anchoring and explicit position
ListView list = new ListView {
    Anchor = AnchorStyles.Top | Bottom | Left | Right,
    Location = new Point(5, 30),  // Space for headers
    // ...
};

// Dynamic sizing
container.SizeChanged += (s, e) => {
    list.Size = new Size(
        container.ClientSize.Width - 10,
        container.ClientSize.Height - 35
    );
};
```

### Reflection Performance

**Optimization**:
- Cache PropertyInfo objects
- Lazy load expensive properties
- Filter out internal/debugging properties
- Limit recursion depth

### Thread Safety

**Considerations**:
- All UI updates on main thread
- AutoCAD API calls within transactions
- Async loading for large collections (future)

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-19 | Initial specification |
| | | - Dual-pane layout with TreeView and ListView |
| | | - Collection expansion feature |
| | | - Error classification (red/gray) |
| | | - Real-time search/filter |
| | | - Copy to clipboard |
| | | - Header visibility fixes |

---

## Appendix A: Color Reference

```csharp
// Standard Colors
Color.White          // #FFFFFF - Content backgrounds
Color.Black          // #000000 - Standard text
Color.Blue           // #0000FF - Collections, status
Color.Red            // #FF0000 - Critical errors
Color.Gray           // #808080 - Not applicable

// System Colors
SystemColors.Control        // Panel backgrounds
SystemColors.ControlLight   // Separator background
SystemColors.ControlText    // Standard text
SystemColors.Highlight      // Selection background
SystemColors.HighlightText  // Selection text
```

## Appendix B: Layout Measurements

```
Window: 1200 × 700px (default)
├── TopPanel: Full Width × 35px
├── SplitContainer: Full Width × (Height - 60px)
│   ├── Panel1: 400px × Full Height
│   │   └── TreeView: Fill (with 5px padding)
│   └── Panel2: 800px × Full Height
│       ├── SearchPanel: Full Width × 35px
│       ├── HeaderSeparator: Full Width × 25px
│       └── ListViewContainer: Full Width × Remaining Height
│           └── ListView: (Width - 10px) × (Height - 35px)
└── BottomPanel: Full Width × 25px
```

## Appendix C: Event Handlers

| Event | Handler | Purpose |
|-------|---------|---------|
| TreeView.AfterSelect | TreeView_AfterSelect | Load properties for selected node |
| ListView.SelectedIndexChanged | ListView_SelectedIndexChanged | Enable/disable Copy Value button |
| ListView.DoubleClick | ListView_DoubleClick | Expand collection in tree |
| ListView.MouseMove | ListView_MouseMove | Change cursor for collections |
| SearchBox.TextChanged | SearchBox_TextChanged | Filter properties in real-time |
| ClearButton.Click | ClearButton_Click | Clear search filter |
| CopyValueButton.Click | CopyValueButton_Click | Copy selected value |
| CopyAllButton.Click | CopyAllButton_Click | Copy all properties |
| Form.Load | MainSnoopForm_Load | Initialize UI and load database |
| Container.SizeChanged | (inline) | Resize ListView dynamically |

---

**END OF SPECIFICATION**

