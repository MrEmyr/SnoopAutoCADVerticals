# UnifiedSnoop - Deployment Guide

This directory contains scripts and configuration files for deploying UnifiedSnoop to the AutoCAD ApplicationPlugins bundle for testing.

---

## 📁 **Files in This Directory**

| File | Purpose |
|------|---------|
| `PackageContents.xml` | AutoCAD bundle manifest file |
| `Deploy-ToBundle.ps1` | Full deployment script with options |
| `Quick-Deploy.ps1` | One-click build and deploy |

---

## 🚀 **Quick Start**

### **Method 1: One-Click Deploy (Recommended)**

Open PowerShell in the `Deploy` directory and run:

```powershell
.\Quick-Deploy.ps1
```

This will:
- Build the project (Release configuration)
- Deploy both 2024 and 2025+ versions to the bundle
- Show deployment summary

### **Method 2: Deploy Without Building**

If you've already built the project:

```powershell
.\Deploy-ToBundle.ps1 -BuildFirst:$false
```

### **Method 3: Clean Deploy**

To remove existing bundle and redeploy:

```powershell
.\Deploy-ToBundle.ps1 -CleanDeploy
```

---

## 📦 **Bundle Structure**

After deployment, the bundle is located at:
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
├── PackageContents.xml
└── Contents\
    ├── 2024\
    │   └── UnifiedSnoop.dll  (net48 - for AutoCAD/Civil 3D 2024)
    └── 2025\
        └── UnifiedSnoop.dll  (net8.0 - for AutoCAD/Civil 3D 2025+)
```

---

## 🎮 **Testing After Deployment**

### **AutoCAD/Civil 3D 2024:**
1. **Restart** AutoCAD/Civil 3D 2024 (if running)
2. The plugin loads automatically
3. Type `SNOOP` to start
4. Or right-click an object → "Snoop This Object"

### **AutoCAD/Civil 3D 2025/2026:**
1. **Restart** AutoCAD/Civil 3D 2025 or 2026 (if running)
2. The plugin loads automatically
3. Type `SNOOP` to start
4. Or right-click an object → "Snoop This Object"

---

## 🔧 **Available Commands**

After deployment, these commands are available in AutoCAD:

| Command | Description |
|---------|-------------|
| `SNOOP` | Open the main inspector UI |
| `SNOOPENTITY` | Snoop a selected entity |
| `SNOOPSELECTION` | Snoop multiple selected entities |
| `SNOOPVERSION` | Show version and framework info |
| `SNOOPCOLLECTORS` | List registered collectors |

**Plus:** Right-click context menu → "Snoop This Object"

---

## ⚙️ **Advanced Options**

### **Deploy-ToBundle.ps1 Parameters:**

```powershell
.\Deploy-ToBundle.ps1 [parameters]
```

**Parameters:**
- `-BuildFirst` (default: `$true`) - Build project before deploying
- `-CleanDeploy` (default: `$false`) - Remove existing bundle first
- `-Configuration` (default: `"Release"`) - Build configuration (Release/Debug)

**Examples:**

```powershell
# Deploy Debug build
.\Deploy-ToBundle.ps1 -Configuration Debug

# Deploy without building
.\Deploy-ToBundle.ps1 -BuildFirst:$false

# Clean deploy (removes old files first)
.\Deploy-ToBundle.ps1 -CleanDeploy

# Clean deploy with Debug build
.\Deploy-ToBundle.ps1 -CleanDeploy -Configuration Debug
```

---

## 📋 **Deployment Checklist**

Before testing in AutoCAD:

- [ ] Project built successfully (both net48 and net8.0-windows)
- [ ] Deployment script ran without errors
- [ ] Bundle directory exists: `C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle`
- [ ] Both DLL versions present (2024 and 2025 folders)
- [ ] PackageContents.xml copied correctly
- [ ] **GitHub repository updated** (automatic)
- [ ] AutoCAD/Civil 3D restarted (to load new version)

---

## 📤 **GitHub Integration**

### **Automatic GitHub Updates**

**⚠️ IMPORTANT RULE:** Every deployment automatically updates GitHub!

The deployment script (`Deploy-ToBundle.ps1`) now includes automatic GitHub integration:

1. **Stages all changes** (`git add -A`)
2. **Commits with version info** (e.g., "Deployment v1.0.0 - 2024-11-19 10:30:00")
3. **Pushes to GitHub** (`git push`)

### **What Gets Committed:**
- All source code changes
- Binary files (DLLs) in the bundle
- Documentation updates
- Configuration changes

### **Manual GitHub Update (if needed):**

If automatic push fails, manually update:

```powershell
git add -A
git commit -m "Manual deployment update"
git push
```

### **Skipping GitHub Update:**

The script will gracefully skip GitHub updates if:
- Not in a git repository
- No changes to commit
- Git operations fail (with warnings)

---

## 🐛 **Troubleshooting**

### **"Build FAILED" Error**
- Check that you're in the correct directory
- Run `dotnet build` manually to see detailed errors
- Ensure .NET SDK is installed

### **"DLL not found" Error**
- Build the project first: `dotnet build -c Release`
- Check that both targets are built (net48 and net8.0-windows)

### **Plugin Doesn't Load in AutoCAD**
- Restart AutoCAD/Civil 3D completely
- Check Windows Event Viewer for .NET errors
- Verify you're loading the correct version (2024 in 2024, 2025+ in 2025+)

### **"Access Denied" When Deploying**
- Close AutoCAD/Civil 3D (DLLs may be locked)
- Run PowerShell as Administrator
- Check folder permissions on `C:\ProgramData\Autodesk`

### **Wrong Version Loading**
- Check `SNOOPVERSION` command output
- Verify PackageContents.xml has correct SeriesMin/SeriesMax
- Ensure correct DLL is in the right folder (2024 vs 2025)

---

## 🔄 **Workflow: Development → Testing**

**Recommended workflow for development:**

1. **Make code changes** in Visual Studio/VS Code
2. **Run deployment:** `.\Deploy\Quick-Deploy.ps1`
3. **Restart AutoCAD** (if running)
4. **Test** the changes
5. **Repeat** steps 1-4 as needed

**Tip:** You can keep the PowerShell window open and just re-run `.\Quick-Deploy.ps1` after each change!

---

## 📝 **Notes**

- The deployment script **does not** modify your development bin folder
- Both versions (2024 and 2025+) are always deployed together
- AutoCAD automatically loads the correct version based on its version number
- The bundle location is standard for AutoCAD plugins (no NETLOAD needed!)
- Deployment is **separate from development** - your dev environment is unchanged

---

## 🆘 **Getting Help**

If you encounter issues:

1. Check the troubleshooting section above
2. Verify file paths in the scripts match your system
3. Check PowerShell execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
4. Review the deployment script output for specific errors

---

## ✅ **Success Indicators**

You know deployment worked when:
- ✅ Script shows "DEPLOYMENT SUCCESSFUL!"
- ✅ Both DLL sizes are displayed
- ✅ Bundle directory contains Contents/2024 and Contents/2025 folders
- ✅ After restarting AutoCAD, `SNOOP` command is recognized
- ✅ Right-click shows "Snoop This Object" menu item

---

**Happy Testing! 🎉**

