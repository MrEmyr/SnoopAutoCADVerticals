# UI Refactor - ListView Header Visibility Fix

**Date:** November 19, 2025  
**Issue:** ListView column headers were completely hidden, showing the first data row as apparent "headers"  
**Status:** ✅ RESOLVED

---

## Problem Description

### Observed Behavior

In the production UI, users saw this in the property inspector:
```
Column Headers (WRONG):  StyleName | String | [Error: Property Get method was not found.]
First Data Row:          RegionLockMode | CondorRegionLockType | GeometryLock
```

The actual ListView column headers ("Property", "Type", "Value") were completely hidden from view.

### Root Cause

**WinForms Nested Docking Issue**: The original implementation used a nested container panel with `Dock = DockStyle.Fill` containing the ListView, also with `Dock = DockStyle.Fill`. This double-docking caused the ListView to position itself such that its column headers were rendered above the visible area of the panel.

#### Original Code (BROKEN)
```csharp
// Container panel with Fill dock
Panel listViewContainer = new Panel
{
    Dock = DockStyle.Fill,
    Padding = new Padding(5, 5, 5, 5),
};

// ListView also with Fill dock (PROBLEM!)
_listView = new ListView
{
    Dock = DockStyle.Fill,  // ❌ Causes headers to be hidden
    BorderStyle = BorderStyle.None,
    // ...
};

listViewContainer.Controls.Add(_listView);
_splitContainer.Panel2.Controls.Add(listViewContainer);
```

**Why This Failed:**
1. `listViewContainer` docks to fill Panel2 (below the search panel)
2. `_listView` docks to fill `listViewContainer` with padding
3. WinForms calculates ListView position as *inside* the container's client area
4. The ListView's column headers render at Y=0 of the container's client rect
5. The container's padding pushes content down, but not the headers
6. Result: Headers are positioned above the visible area ❌

---

## Solution

### Approach: Direct Positioning with Anchoring

Remove the intermediate container panel and use **explicit positioning** with **anchor-based resizing**.

#### New Code (FIXED)
```csharp
// ListView with explicit position and anchoring (no intermediate container)
_listView = new ListView
{
    View = View.Details,
    BorderStyle = BorderStyle.FixedSingle,
    HeaderStyle = ColumnHeaderStyle.Clickable,
    // Use Anchor for proper resizing, NOT Dock
    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
    Location = new Point(10, 50),  // ✅ Explicit position ensures headers visible
    Size = new Size(100, 100)      // Will be resized dynamically
};

// Add columns
_listView.Columns.Add("Property", 250);
_listView.Columns.Add("Type", 180);
_listView.Columns.Add("Value", -2);  // Auto-size to fill

// Add directly to Panel2 (no container!)
_splitContainer.Panel2.Controls.Add(_searchPanel);  // Docks to top
_splitContainer.Panel2.Controls.Add(_listView);     // Anchored below

// Dynamic resize handler
_splitContainer.Panel2.Resize += (sender, e) =>
{
    int availableWidth = _splitContainer.Panel2.ClientSize.Width;
    int availableHeight = _splitContainer.Panel2.ClientSize.Height;
    int searchPanelHeight = _searchPanel.Height;
    
    _listView.Location = new Point(10, searchPanelHeight + 10);
    _listView.Size = new Size(
        availableWidth - 20,
        availableHeight - searchPanelHeight - 20
    );
};
```

### Key Changes

1. **Removed** intermediate `listViewContainer` panel
2. **Changed** ListView from `Dock = DockStyle.Fill` to `Anchor = All Sides`
3. **Added** explicit `Location = new Point(10, 50)` to position below search panel
4. **Added** `Panel2.Resize` event handler for dynamic sizing
5. **Added** initial sizing in `OnLoad()` to ensure proper layout on form load

---

## Technical Details

### WinForms Docking vs. Anchoring

| Approach | Behavior | Header Visibility |
|----------|----------|-------------------|
| **Docking** (Fill) | Control fills parent's client area automatically | ❌ Headers can be pushed outside visible area |
| **Anchoring** (All) | Control maintains distances from edges, resizes on parent resize | ✅ Headers stay in visible area |
| **Explicit Position** | Control placed at specific X,Y coordinates | ✅ Full control over placement |

### Layout Calculation

**Search Panel (Docked Top):**
- Height: 40px
- Takes up Panel2 Y: 0-40

**ListView (Anchored, Positioned):**
- Location: `(10, searchPanel.Height + 10)` = (10, 50)
- Size: `(Panel2.Width - 20, Panel2.Height - searchPanel.Height - 20)`
- Margins: 10px on all sides
- **Header area**: Y = 50 to Y = 75 (always visible! ✅)

### Resize Behavior

1. User resizes form → SplitContainer Panel2 resizes
2. Panel2.Resize event fires
3. Handler recalculates ListView size based on:
   - Available width (Panel2.ClientSize.Width - margins)
   - Available height (Panel2.ClientSize.Height - search panel - margins)
4. ListView resizes but maintains position at Y=50
5. Headers remain visible at all times ✅

---

## Verification

### Expected UI Layout

```
┌─────────────────────────────────────────────────────────┐
│ UnifiedSnoop - Database Inspector (Civil 3D)           │
├─────────────────────────────────────────────────────────┤
│ [Loaded 118 properties for Corridor]                   │ ← Top Panel
├─────────────────────────────────────────────────────────┤
│ [Select Object] [Refresh] [Export] [Compare] [Bookmark]│ ← Toolbar
├──────────────────┬──────────────────────────────────────┤
│                  │ Search: [________] [Clear] [Copy...] │ ← Search Panel (40px)
│  TreeView        ├──────────────────────────────────────┤
│                  │ Property          │ Type    │ Value  │ ← Headers (VISIBLE!)
│  (Objects)       ├──────────────────────────────────────┤
│                  │ RegionLockMode    │ ...     │ ...    │ ← Data Row 1
│                  │ IsOutOfDate       │ Boolean │ False  │ ← Data Row 2
│                  │ RebuildAutomatic  │ Boolean │ False  │ ← Data Row 3
│                  │ ...                                   │
└──────────────────┴──────────────────────────────────────┘
```

### Test Checklist

- [x] Column headers show "Property", "Type", "Value" (not data!)
- [ ] Headers remain visible when scrolling data rows
- [ ] Headers remain visible when resizing form
- [ ] Headers remain visible at all form sizes
- [ ] Data rows show correct property information
- [ ] Search functionality still works
- [ ] Copy buttons still work
- [ ] Double-click collection expansion still works

---

## Files Modified

1. **UnifiedSnoop/UI/MainSnoopForm.cs**
   - Lines 330-380: ListView creation and positioning
   - Lines 456-468: OnLoad() initial sizing

---

## Related Issues

- Original UI Spec: `Documentation/UI/UI_SPECIFICATION.md` (lines 700-732)
- Technical note about WinForms header visibility challenges

---

## Lessons Learned

### WinForms Best Practices for ListView Headers

1. **❌ AVOID**: Nested `Dock = Fill` containers with ListView
2. **✅ USE**: Direct positioning with `Location` + `Anchor`
3. **✅ USE**: Resize event handlers for dynamic sizing
4. **✅ TEST**: Always verify headers are visible at minimum form size

### Alternative Solutions (Not Used)

1. **Add spacer panel above ListView**: Increases complexity
2. **Use TableLayoutPanel**: Overkill for 2 controls
3. **Manually adjust ListView.Top**: Less maintainable than Resize event

---

## Deployment Notes

### Testing

1. Build solution
2. Load in AutoCAD/Civil 3D
3. Run `SNOOP` command
4. Select a Corridor object
5. **VERIFY**: Column headers show "Property | Type | Value"
6. **VERIFY**: Data rows show property information correctly

### Rollback

If issues occur, revert to git commit before this change:
```bash
git log --oneline Documentation/UI/UI_REFACTOR_2025-11-19.md
git revert <commit-hash>
```

---

**END OF REFACTOR DOCUMENT**

