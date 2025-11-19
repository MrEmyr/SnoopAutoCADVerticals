# Next Development Tasks

**Status:** Pending user testing of v1.0.6  
**Date Created:** November 19, 2025  
**Priority:** Medium

---

## 🔄 **Awaiting v1.0.6 Testing**

Before starting new development, user needs to test current release (v1.0.6):

### Testing Checklist
- [ ] Restart AutoCAD/Civil 3D 2024 or 2025+
- [ ] Run `SNOOPVERSION` - verify shows v1.0.6
- [ ] Run `SNOOP` - verify form opens without crash
- [ ] Verify ListView headers are visible ("Property | Type | Value")
- [ ] Verify TreeView shows database collections
- [ ] Verify splitter can be dragged
- [ ] Test object selection and property inspection

---

## 📋 **Planned Tasks for Next Sprint**

### 1. XRecordEditor for AutoCAD 2025+ ⚡ **HIGH PRIORITY**

**Issue:** XRecordEditor currently only deploys for AutoCAD 2024 (net48). The 2025+ version (net8.0-windows) is not being built/deployed.

**Current Status:**
```
✅ 2024: XRecordEditor.dll (37 KB)
⚠️  XRecordEditor (2025+) not found
```

**Requirements:**
- Build XRecordEditor for .NET 8.0-windows target
- Deploy to `bundle\Contents\2025\` folder
- Ensure same functionality as 2024 version
- Test XRECORDEDIT command in AutoCAD 2025+

**Technical Notes:**
- XRecordEditor project needs multi-targeting like UnifiedSnoop
- Update XRecordEditor.csproj to support both net48 and net8.0-windows
- Verify no .NET 8.0 specific breaking changes
- Test CRUD operations in both versions

**Files to Modify:**
- `UnifiedSnoop/XRecordEditor/XRecordEditor.csproj` - Add multi-target framework
- `UnifiedSnoop/Deploy/Deploy-ToBundle.ps1` - Verify deployment for 2025+ works
- Update documentation about XRecordEditor availability

**Estimated Effort:** 2-4 hours

---

### 2. Post-Testing Bug Fixes

#### ✅ COMPLETED: Negative Dimension Protection (Bug Fix)

**Status:** Fixed on November 19, 2025

**Issue:** ListView size calculations could produce negative dimensions if the panel was resized to be very small, causing potential rendering issues or crashes.

**Locations Fixed:**
- `MainSnoopForm.cs` Line 375-381: Resize event handler
- `MainSnoopForm.cs` Line 466-472: OnLoad method

**Fix Applied:**
- Added `Math.Max(0, calculatedValue)` to ensure non-negative dimensions
- Prevents crashes when form is resized to minimum dimensions
- Improves robustness for edge cases

**Code Changes:**
```csharp
// Before (could be negative):
_listView.Size = new Size(availableWidth - 20, availableHeight - searchPanelHeight - 20);

// After (guaranteed non-negative):
int listViewWidth = Math.Max(0, availableWidth - 20);
int listViewHeight = Math.Max(0, availableHeight - searchPanelHeight - 20);
_listView.Size = new Size(listViewWidth, listViewHeight);
```

**Testing:**
- Should be included in next deployment (v1.0.7)
- Test by resizing form to very small dimensions
- Verify no crashes or rendering issues

---

### 3. Additional Bug Fixes (TBD)

**Depends on:** v1.0.6 testing results

Any additional issues discovered during testing will be added here.

---

### 4. UI Enhancements (Future)

**Lower Priority - Only after testing confirms current UI works:**

Potential improvements:
- [ ] Add keyboard shortcuts for common actions
- [ ] Add context menu options
- [ ] Improve search functionality
- [ ] Add property value editing (if feasible)

---

### 5. Documentation Updates (Ongoing)

- [ ] Update USER_GUIDE.md with v1.0.6 features
- [ ] Add troubleshooting section for common issues
- [ ] Create video walkthrough (optional)

---

## 🚦 **Development Rules**

Before starting any task:
1. ✅ All current tests must pass
2. ✅ User has approved proceeding with development
3. ✅ Version incremented in version.json
4. ✅ Changelog entry added
5. ✅ AutoCAD closed before deployment

---

## 📝 **Notes from User**

**November 19, 2025:**
> "xrecord editor should be available for both 2024 & 2025 releases, add this to the plan for next code developement, do not act right now till i have tested current release"

**Action:** Waiting for v1.0.6 testing results before proceeding.

---

**Next Steps:**
1. User tests v1.0.6
2. User provides feedback
3. Address any critical issues
4. Proceed with XRecordEditor 2025+ implementation

---

**Last Updated:** November 19, 2025 17:05:00

