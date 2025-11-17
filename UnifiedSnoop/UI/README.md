# UI Layer

User interface components based on Civil3DSnoop style.

## Subfolders:

### **Forms/**
Main application forms:
- `MainSnoopForm.cs` - Main form (TreeView + ListView)
- `MainSnoopForm.Designer.cs` - Designer file
- `MainSnoopForm.resx` - Resources

### **Controls/**
Custom controls:
- `ObjectTreeView.cs` - Enhanced TreeView
- `PropertyListView.cs` - Enhanced ListView

## Main Form Layout:

```
┌─────────────────────────────────────────┐
│  Snoop AutoCAD / Civil 3D Database      │
├──────────────┬──────────────────────────┤
│              │  Name    | Type  | Value │
│  TreeView    ├──────────────────────────┤
│  (Objects)   │  ListView (Properties)   │
│              │                          │
│  ○ Database  │  - Grouped by type       │
│  ○ Civil 3D  │  - Sortable columns      │
│              │  - Collection expansion  │
├──────────────┴──────────────────────────┤
│  [Select Object]  [Select File...]      │
└─────────────────────────────────────────┘
```

## Key Features:

### **TreeView:**
- Hierarchical object display
- LazyFull loading
- Collection expansion
- Context menu
- Icons

### **ListView:**
- Property grid style
- Name | Type | Value columns
- Grouped by declaring type
- Sortable
- Collection detection

### **Buttons:**
- Select Object - Pick on screen
- Select File - Open external DWG

## Event Handlers:
- TreeView_AfterSelect - Load properties
- ListView_SelectedIndexChanged - Handle collections
- Button_Click - Commands

## Next Steps:
1. Design MainSnoopForm layout
2. Add TreeView control
3. Add ListView control
4. Wire up event handlers
5. Implement collection expansion
6. Add buttons and commands

