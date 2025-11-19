# UnifiedSnoop - Quick Test Guide
**Version 1.0.1 - UI Fixes Applied**

---

## 🚀 Quick Start

### Load the DLL

**AutoCAD/Civil 3D 2024 and earlier:**
```
NETLOAD
→ Browse to: C:\Users\isaace\SnoopCivil3D\UnifiedSnoop\bin\Release\net48\UnifiedSnoop.dll
```

**AutoCAD/Civil 3D 2025+:**
```
NETLOAD
→ Browse to: C:\Users\isaace\SnoopCivil3D\UnifiedSnoop\bin\Release\net8.0-windows\win-x64\UnifiedSnoop.dll
```

### Launch the Tool
```
SNOOP
```

---

## ✅ Visual Verification (5 seconds)

When the window opens, you should immediately see:

```
┌─────────────────────────────────────────────────────────────────┐
│ Top Panel: "Ready" or "Loaded X properties..."                │ ← Blue text
├─────────────────────────────────────────────────────────────────┤
│ [Select Object] [Refresh] [Export] [Compare] [★Add] [Bookmarks]│ ← Buttons
├─────────────────────────────────────────────────────────────────┤
│                │ [Search:] [_____] [Clear] [Copy Value] [Copy] │
│   TreeView     ├──────────────────────────────────────────────┤
│   with nodes   │ Property    │ Type      │ Value               │ ← Headers!
│   ☑ Database   ├──────────────────────────────────────────────┤
│     ☐ AutoCAD  │ (properties listed here)                      │
│     ☐ Civil3D  │                                               │
│   (splitter)   │                                               │
│                │                                               │
├────────────────┴───────────────────────────────────────────────┤
│ Status: "Form loaded: XXXXpx wide, splitter at 400px"          │ ← Status
└─────────────────────────────────────────────────────────────────┘
```

### ✅ PASS Criteria:
- ✅ TreeView visible on LEFT with "Database" node
- ✅ ListView visible on RIGHT
- ✅ Headers "Property", "Type", "Value" are VISIBLE
- ✅ Vertical splitter between panels (movable)
- ✅ Status bar shows splitter position

### ❌ FAIL Criteria (Reload DLL):
- ❌ TreeView missing or pushed to extreme left
- ❌ Headers not visible
- ❌ Entire window is blank
- ❌ Error messages in AutoCAD command line

---

## 🧪 2-Minute Smoke Test

### Test 1: TreeView Navigation (30 seconds)
1. Click "Database" node → Should expand
2. Click "AutoCAD Collections" → Should expand
3. Click any sub-node → Properties appear in ListView
4. **✅ PASS:** Properties display with visible headers

### Test 2: ListView Headers (15 seconds)
1. Look at top of ListView (right panel)
2. **✅ PASS:** You can clearly see "Property | Type | Value" headers
3. **❌ FAIL:** Headers are missing or obscured

### Test 3: Search (30 seconds)
1. Click in Search box
2. Type "name"
3. **✅ PASS:** Property list filters in real-time
4. Click "Clear" → All properties return

### Test 4: Splitter (15 seconds)
1. Grab the vertical splitter between panels
2. Drag left and right
3. **✅ PASS:** Both panels resize smoothly
4. TreeView minimum is 200px, ListView minimum is 400px

### Test 5: Window Resize (30 seconds)
1. Resize the form smaller
2. Resize the form larger
3. **✅ PASS:** Headers remain visible at all sizes
4. **✅ PASS:** No layout breaking or flickering

---

## 🔍 5-Minute Full Test

### Object Selection
```
1. Click [Select Object]
2. Select any object in drawing
3. ✅ Properties display
4. ✅ TreeView shows selected object
```

### Copy Functions
```
1. Select any property in ListView
2. Click [Copy Value]
3. Paste in Notepad → ✅ Value copied
4. Click [Copy All]
5. Paste in Excel → ✅ Tab-delimited format
```

### Export
```
1. Select an object
2. Click [Export]
3. Choose "Export to CSV"
4. ✅ File saved successfully
```

### Bookmarks
```
1. Select an object
2. Click [★ Add] or press Ctrl+B
3. Enter bookmark name
4. ✅ Bookmark saved
5. Click [Bookmarks]
6. ✅ Bookmark appears in list
```

### Keyboard Shortcuts
```
F5 → ✅ Refresh
Ctrl+F → ✅ Focus search box
Ctrl+C → ✅ Copy selected value (when ListView focused)
Ctrl+L → ✅ Focus TreeView
Ctrl+P → ✅ Focus ListView
Esc → ✅ Clear search (or close form if search empty)
```

---

## 🐛 Troubleshooting

### TreeView Not Visible
**Symptom:** Left panel is missing or tiny  
**Solution:** 
1. Look for splitter (thin vertical bar)
2. Drag it to the right
3. Check status bar for "splitter at Xpx"
4. If still broken: Unload and reload DLL

### Headers Not Visible
**Symptom:** Can't see "Property", "Type", "Value" headers  
**Solution:**
1. Ensure you have the LATEST DLL (Nov 19, 2025, 1:21 PM)
2. Check file size: 163.5 KB (net48) or 172 KB (net8.0)
3. Reload DLL with NETLOAD
4. If still not visible: Wrong DLL version loaded

### No Properties Display
**Symptom:** ListView is empty when clicking nodes  
**Solution:**
1. Check AutoCAD command line for errors
2. Try selecting an object from drawing instead
3. Click [Refresh] button
4. Check error log: `%APPDATA%\UnifiedSnoop\error.log`

### Form Won't Open
**Symptom:** SNOOP command does nothing  
**Solution:**
1. Check command line for error messages
2. Ensure correct DLL version for AutoCAD version
3. Try NETLOAD again
4. Check if Civil 3D is running (tool supports both AutoCAD and Civil 3D)

### Wrong Splitter Position
**Symptom:** Splitter starts at wrong position  
**Solution:**
1. Check status bar: "Form loaded: XXXXpx wide, splitter at XXXpx"
2. Manually drag splitter to desired position
3. Position should be ~400px for 1200px window width
4. If calculation failed, status will show warning

---

## 📞 Report Issues

If you encounter issues:

1. **Check File Version:**
   - Right-click DLL → Properties → Details
   - Last Modified: Nov 19, 2025, 1:21 PM or later

2. **Check Build:**
   - net48: 167,424 bytes (163.5 KB)
   - net8.0: 176,128 bytes (172 KB)

3. **Check AutoCAD Version:**
   - AutoCAD 2024 or earlier → Use net48
   - AutoCAD 2025+ → Use net8.0-windows

4. **Capture Screenshot:**
   - Show entire window
   - Show AutoCAD command line
   - Include any error messages

5. **Check Error Log:**
   - Location: `%APPDATA%\UnifiedSnoop\error.log`
   - Copy last few lines

---

## ✅ Success Indicators

You'll know the UI fixes worked if:

1. ✅ **TreeView is visible** on the left side (not hidden)
2. ✅ **ListView headers are visible** ("Property", "Type", "Value")
3. ✅ **Splitter works** and can be moved between panels
4. ✅ **Properties display** when you click tree nodes
5. ✅ **Status bar shows** splitter position info
6. ✅ **No errors** in AutoCAD command line
7. ✅ **Form resizes** smoothly without breaking layout

---

**Test Duration:** 2-5 minutes  
**Expected Result:** All tests pass ✅  
**UI Version:** v1.0.1 (Nov 19, 2025)

