# UnifiedSnoop - Deployment Guide

**Version:** 1.0  
**Last Updated:** November 14, 2025

---

## 📦 **Build Outputs**

UnifiedSnoop builds **TWO separate DLLs** for different AutoCAD/Civil 3D versions:

### ✅ **For AutoCAD/Civil 3D 2024**
- **Framework:** .NET Framework 4.8
- **Location:** `bin\x64\Release\net48\UnifiedSnoop.dll`
- **Size:** ~41 KB

### ✅ **For AutoCAD/Civil 3D 2025/2026**
- **Framework:** .NET 8.0
- **Location:** `bin\x64\Release\net8.0-windows\win-x64\UnifiedSnoop.dll`
- **Size:** ~43 KB

---

## 🔨 **Building the Project**

### **Prerequisites**
- .NET 8.0 SDK installed
- AutoCAD 2024 installed (for net48 build references)
- Visual Studio 2022 or later (optional, for IDE support)

### **Build Commands**

**Build both versions (Release):**
```powershell
cd UnifiedSnoop
dotnet build -c Release
```

**Build both versions (Debug):**
```powershell
dotnet build -c Debug
```

**Build specific framework only:**
```powershell
# Build only .NET Framework 4.8 (2024)
dotnet build -c Release -f net48

# Build only .NET 8.0 (2025+)
dotnet build -c Release -f net8.0-windows
```

**Clean build:**
```powershell
dotnet clean
dotnet build -c Release
```

---

## 🚀 **Deployment Instructions**

### **For AutoCAD/Civil 3D 2024**

1. **Locate the DLL:**
   ```
   UnifiedSnoop\bin\x64\Release\net48\UnifiedSnoop.dll
   ```

2. **Copy to a stable location** (recommended):
   ```
   C:\AutoCAD_Plugins\UnifiedSnoop\2024\UnifiedSnoop.dll
   ```

3. **Load in AutoCAD 2024:**
   - Open AutoCAD or Civil 3D 2024
   - Type: `NETLOAD`
   - Browse to the `net48\UnifiedSnoop.dll` file
   - Click "Load"

4. **Test the installation:**
   ```
   SNOOPVERSION
   ```
   Should display: "AutoCAD/Civil 3D 2024 (.NET Framework 4.8)"

5. **Run the inspector:**
   ```
   SNOOP
   ```

### **For AutoCAD/Civil 3D 2025/2026**

1. **Locate the DLL:**
   ```
   UnifiedSnoop\bin\x64\Release\net8.0-windows\win-x64\UnifiedSnoop.dll
   ```

2. **Copy to a stable location** (recommended):
   ```
   C:\AutoCAD_Plugins\UnifiedSnoop\2025\UnifiedSnoop.dll
   ```

3. **Load in AutoCAD 2025/2026:**
   - Open AutoCAD or Civil 3D 2025 or 2026
   - Type: `NETLOAD`
   - Browse to the `net8.0-windows\win-x64\UnifiedSnoop.dll` file
   - Click "Load"

4. **Test the installation:**
   ```
   SNOOPVERSION
   ```
   Should display: "AutoCAD/Civil 3D 2025+ (.NET 8.0)"

5. **Run the inspector:**
   ```
   SNOOP
   ```

---

## ⚠️ **CRITICAL: Version Compatibility**

### **DO NOT MIX VERSIONS!**

| ❌ **WRONG** | ✅ **CORRECT** |
|--------------|----------------|
| Loading `net48` DLL in AutoCAD 2025 | Loading `net48` DLL in AutoCAD 2024 |
| Loading `net8.0-windows` DLL in AutoCAD 2024 | Loading `net8.0-windows` DLL in AutoCAD 2025+ |

**What happens if you use the wrong version?**
- ❌ Runtime errors
- ❌ "Could not load file or assembly" errors
- ❌ Application crashes

**How to identify which DLL you have:**
- After loading, type `SNOOPVERSION` to see which version is loaded
- The DLL will validate on startup and warn if incompatible

---

## 🔄 **Auto-Loading (Optional)**

To automatically load UnifiedSnoop when AutoCAD starts:

### **Method 1: ACAD.LSP (AutoLISP)**

Create or edit `acad.lsp` in your AutoCAD support path:

**For AutoCAD 2024:**
```lisp
(command "NETLOAD" "C:\\AutoCAD_Plugins\\UnifiedSnoop\\2024\\UnifiedSnoop.dll")
```

**For AutoCAD 2025+:**
```lisp
(command "NETLOAD" "C:\\AutoCAD_Plugins\\UnifiedSnoop\\2025\\UnifiedSnoop.dll")
```

### **Method 2: Registry Autoload (Advanced)**

Add registry keys for automatic loading:

**AutoCAD 2024:**
```
HKEY_CURRENT_USER\Software\Autodesk\AutoCAD\R24.2\ACAD-xxxx:409\Applications\UnifiedSnoop
Loader: C:\AutoCAD_Plugins\UnifiedSnoop\2024\UnifiedSnoop.dll
```

**AutoCAD 2025:**
```
HKEY_CURRENT_USER\Software\Autodesk\AutoCAD\R25.0\ACAD-xxxx:409\Applications\UnifiedSnoop
Loader: C:\AutoCAD_Plugins\UnifiedSnoop\2025\UnifiedSnoop.dll
```

---

## 🧪 **Testing Checklist**

After deployment, verify the following:

### **Basic Tests:**
- [ ] `NETLOAD` command loads without errors
- [ ] `SNOOPVERSION` shows correct version
- [ ] `SNOOP` opens the main UI
- [ ] TreeView shows database structure
- [ ] Selecting objects displays properties in ListView

### **AutoCAD-Specific Tests:**
- [ ] `SNOOPENTITY` - Pick a line, arc, circle
- [ ] Inspect layers, blocks, linetypes
- [ ] View properties of selected entities

### **Civil 3D-Specific Tests (if available):**
- [ ] Civil 3D collections appear in tree
- [ ] Alignments can be inspected
- [ ] Surfaces show statistics
- [ ] Specialized collectors are registered (check `SNOOPCOLLECTORS`)

---

## 📁 **Recommended Folder Structure**

```
C:\AutoCAD_Plugins\
└── UnifiedSnoop\
    ├── 2024\
    │   ├── UnifiedSnoop.dll          ← For AutoCAD/Civil 3D 2024
    │   └── README.txt                ← Notes about this version
    └── 2025\
        ├── UnifiedSnoop.dll          ← For AutoCAD/Civil 3D 2025+
        └── README.txt                ← Notes about this version
```

---

## 🛠️ **Troubleshooting**

### **"Could not load file or assembly" Error**

**Cause:** Wrong framework version for your AutoCAD version

**Solution:**
1. Check your AutoCAD version
2. Ensure you're loading the correct DLL:
   - 2024 → `net48\UnifiedSnoop.dll`
   - 2025+ → `net8.0-windows\win-x64\UnifiedSnoop.dll`

### **"Version validation failed" Warning**

**Cause:** DLL built for different AutoCAD version

**Solution:**
- Rebuild the project for your specific version
- Or use the pre-built DLL for your version

### **Civil 3D Collections Not Showing**

**Cause:** Civil 3D is not installed or not detected

**Solution:**
1. Verify Civil 3D is installed
2. Check `SNOOPVERSION` - should show "Civil 3D Available: True"
3. If false, Civil 3D DLLs may not be in the expected location

### **UI Not Appearing**

**Cause:** Form initialization error

**Solution:**
1. Check command line for error messages
2. Try `SNOOPENTITY` for command-line inspection
3. Check Debug output in Visual Studio if debugging

---

## 📊 **Performance Notes**

- **TreeView Limits:** Collections are limited to 100 items to prevent UI freezing
- **Lazy Loading:** Child nodes load only when expanded
- **Memory Usage:** Minimal - transactions are properly disposed
- **Startup Time:** < 1 second for DLL initialization

---

## 🔐 **Security Considerations**

- **Read-Only:** UnifiedSnoop only reads data, never modifies drawings
- **Safe OpenMode:** All database access uses `OpenMode.ForRead`
- **No Network Access:** No external connections or data transmission
- **Local Only:** All operations are local to the AutoCAD session

---

## 📞 **Support**

For issues, questions, or feature requests:

1. Check `DEVELOPMENT_RULES.md` for coding standards
2. Review `VERSION_COMPATIBILITY.md` for version details
3. See `PHASE1_COMPLETE.md` for implementation details

---

## 📝 **Version History**

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | Nov 14, 2025 | Initial release - Multi-target build (2024 & 2025+) |

---

**Happy Snooping!** 🔍

