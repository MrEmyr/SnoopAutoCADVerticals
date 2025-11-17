# UnifiedSnoop Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Deployment Methods](#deployment-methods)
4. [Manual Installation](#manual-installation)
5. [Automated Deployment](#automated-deployment)
6. [Verification](#verification)
7. [Troubleshooting](#troubleshooting)
8. [Uninstallation](#uninstallation)

---

## Overview

This guide covers deployment of UnifiedSnoop to AutoCAD and Civil 3D workstations. UnifiedSnoop uses the `.bundle` structure for automatic loading in AutoCAD 2024+ and Civil 3D 2024+.

### Deployment Targets

UnifiedSnoop requires **separate builds** for different AutoCAD versions due to API differences:

| AutoCAD Version | .NET Version | DLL Location |
|----------------|--------------|--------------|
| AutoCAD 2024 / Civil 3D 2024 | .NET Framework 4.8 | `Contents/2024/` |
| AutoCAD 2025+ / Civil 3D 2025+ | .NET 8.0 | `Contents/2025/` |

---

## Prerequisites

### For End Users

- **AutoCAD 2024+** or **Civil 3D 2024+**
- **Administrator privileges** (for installation to ProgramData)
- **No additional runtimes required** (.NET is included with AutoCAD)

### For Developers/Deployers

- Built DLLs from the UnifiedSnoop project
- PowerShell 5.0 or later (for automated deployment)
- Write access to `C:\ProgramData\Autodesk\ApplicationPlugins\`

---

## Deployment Methods

### Method 1: Manual Installation (Recommended for Single Machines)
- Copy files manually
- Simple and straightforward
- Best for testing and individual workstations

### Method 2: Automated Deployment (Recommended for Multiple Machines)
- Use PowerShell script
- Ideal for IT departments
- Supports deployment to multiple machines via network shares or SCCM

### Method 3: Network Deployment
- Deploy to network location
- Use AutoCAD support file search paths
- Centralized updates

---

## Manual Installation

### Step 1: Prepare the Bundle

Build the project or obtain the pre-built bundle with the following structure:

```
UnifiedSnoop.bundle/
├── PackageContents.xml
└── Contents/
    ├── 2024/
    │   └── UnifiedSnoop.dll
    └── 2025/
        └── UnifiedSnoop.dll
```

### Step 2: Copy to ApplicationPlugins

1. Open File Explorer
2. Navigate to:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\
   ```
   *(Note: ProgramData is a hidden folder. Type the path directly or enable "Show hidden files")*

3. Copy the entire `UnifiedSnoop.bundle` folder to this location

4. Verify the final path is:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
   ```

### Step 3: Restart AutoCAD

1. Close all AutoCAD/Civil 3D instances
2. Launch AutoCAD or Civil 3D
3. The plugin will load automatically

### Step 4: Verify Installation

Type at the command line:
```
Command: SNOOP
```

If the UI opens, installation is successful! ✅

---

## Automated Deployment

### Using the Deployment Script

UnifiedSnoop includes a PowerShell deployment script for automated installation.

#### Prerequisites
- PowerShell 5.0+
- Build completed (DLLs exist in `bin` or `obj` folders)

#### Script Location
```
UnifiedSnoop\Deploy\Deploy-ToBundle.ps1
```

#### Usage

**Option 1: Deploy Current Build**
```powershell
cd UnifiedSnoop\Deploy
.\Deploy-ToBundle.ps1
```

**Option 2: Deploy from Custom Path**
Edit `Deploy-ToBundle.ps1` to specify custom source/destination paths:
```powershell
$sourcePath = "C:\Builds\UnifiedSnoop\bin\Release"
$destPath = "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle"
```

#### What the Script Does

1. Creates the bundle directory structure
2. Copies `PackageContents.xml`
3. Copies `UnifiedSnoop.dll` from `net48` build to `Contents/2024/`
4. Copies `UnifiedSnoop.dll` from `net8.0-windows` build to `Contents/2025/`
5. Handles locked files (when AutoCAD is running) by copying from `obj` folder
6. Provides detailed console output

#### Example Output

```
🚀 Deploying UnifiedSnoop to Bundle Location
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📁 Destination: C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle

✅ Bundle directory created
✅ PackageContents.xml deployed
✅ Net48 DLL deployed (2024)
✅ Net8.0 DLL deployed (2025)

✅ DEPLOYMENT COMPLETE
```

---

## Network Deployment

For enterprise environments with centralized deployment:

### Step 1: Create Network Share

```
\\NetworkServer\AutoCADPlugins\UnifiedSnoop.bundle\
```

### Step 2: Configure AutoCAD Support Paths

**Method A: Via AutoCAD Options**
1. In AutoCAD, type `OPTIONS`
2. Go to **Files** tab
3. Expand **Support File Search Path**
4. Click **Add** → **Browse**
5. Add: `\\NetworkServer\AutoCADPlugins`

**Method B: Via Environment Variable**
Set system environment variable:
```
ACAD_PLUGINSPATH=\\NetworkServer\AutoCADPlugins
```

**Method C: Via acad.lsp**
Add to startup file:
```lisp
(setenv "ACAD_PLUGINSPATH" "\\\\NetworkServer\\AutoCADPlugins")
```

### Step 3: Test on Client Machine

1. Launch AutoCAD
2. Type `SNOOP`
3. Verify the plugin loads from the network location

### Benefits of Network Deployment

✅ Single deployment for all users  
✅ Easy updates (update once, applies to all)  
✅ No admin rights required on client machines  
❌ Requires reliable network connection  
❌ May be slower to load  

---

## Group Policy / SCCM Deployment

For large organizations using Group Policy or SCCM:

### Create Deployment Package

1. **Package Contents:**
   - `UnifiedSnoop.bundle` folder (complete)
   - Installation script

2. **Installation Script (install-unifiedsnoop.ps1):**
   ```powershell
   # Copy bundle to ApplicationPlugins
   $source = "\\DeploymentServer\Packages\UnifiedSnoop.bundle"
   $dest = "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle"
   
   # Remove old version if exists
   if (Test-Path $dest) {
       Remove-Item $dest -Recurse -Force
   }
   
   # Copy new version
   Copy-Item $source $dest -Recurse -Force
   
   Write-Host "UnifiedSnoop installed successfully"
   ```

3. **Deploy via SCCM:**
   - Create Application or Package
   - Set installation command: `powershell.exe -ExecutionPolicy Bypass -File install-unifiedsnoop.ps1`
   - Deploy to target collection
   - Schedule for overnight deployment

4. **Deploy via Group Policy:**
   - Create GPO
   - Computer Configuration → Policies → Windows Settings → Scripts → Startup
   - Add `install-unifiedsnoop.ps1`
   - Link GPO to target OU
   - Machines will install on next reboot

---

## Verification

### Post-Installation Checks

#### 1. Check File Structure
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
├── PackageContents.xml ✅
└── Contents\
    ├── 2024\
    │   └── UnifiedSnoop.dll ✅
    └── 2025\
        └── UnifiedSnoop.dll ✅
```

#### 2. Check AutoCAD Command Line

Launch AutoCAD and check for messages:
```
Command line: Loading UnifiedSnoop...
```

If you see errors, check the [Troubleshooting](#troubleshooting) section.

#### 3. Test Commands

```
Command: SNOOP
Command: SNOOPHANDLE
Command: SNOOPVERSION
```

All three should work without errors.

#### 4. Check Context Menu

1. Draw a line
2. Right-click on the line
3. Verify **UnifiedSnoop** appears in the context menu

#### 5. Test Basic Functionality

1. Type `SNOOP`
2. Click **[Select Object]**
3. Pick a line in the drawing
4. Verify properties appear
5. Try exporting to CSV

If all steps pass, deployment is successful! ✅

---

## Troubleshooting

### Problem: Plugin not loading

**Symptoms:** `SNOOP` command not found

**Solutions:**
1. Verify bundle location:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
   ```

2. Check `PackageContents.xml` exists and is valid

3. Check for DLL blocking:
   - Right-click `UnifiedSnoop.dll` → Properties
   - If you see "This file came from another computer...", click **Unblock**
   - Do this for both 2024 and 2025 DLLs

4. Check AutoCAD version matches DLL version:
   - AutoCAD 2024 → Uses `Contents/2024/UnifiedSnoop.dll`
   - AutoCAD 2025+ → Uses `Contents/2025/UnifiedSnoop.dll`

5. Try manual NETLOAD:
   ```
   Command: NETLOAD
   Browse to: C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\Contents\2024\UnifiedSnoop.dll
   ```
   Check for error messages.

### Problem: Wrong DLL version loaded

**Symptoms:** `Could not load file or assembly` errors

**Solutions:**
1. Ensure correct DLL in correct folder:
   - `.NET 4.8` build → `Contents/2024/`
   - `.NET 8.0` build → `Contents/2025/`

2. Delete any extra DLLs in the root folder

3. Rebuild with correct target framework

### Problem: Civil 3D objects not working

**Symptoms:** Civil 3D objects show generic properties only

**Solutions:**
1. Verify you're running **Civil 3D**, not plain AutoCAD
2. Check Civil 3D API DLLs are in the same folder as `UnifiedSnoop.dll`:
   - `AeccDbMgd.dll`
   - `AeccUiMgd.dll`
3. Verify build was compiled with `#if CIVIL3D` flag

### Problem: Deployment script fails

**Symptoms:** PowerShell script errors

**Solutions:**
1. Run PowerShell as Administrator
2. Set execution policy:
   ```powershell
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```
3. Check paths in script are correct
4. If DLLs are locked (AutoCAD running), script will attempt to copy from `obj` folder

### Problem: Permission denied

**Symptoms:** Cannot copy to `C:\ProgramData\`

**Solutions:**
1. Run as Administrator
2. Check antivirus isn't blocking
3. Deploy to user profile instead:
   ```
   C:\Users\[Username]\AppData\Roaming\Autodesk\ApplicationPlugins\
   ```
   *(Note: This installs for current user only)*

### Problem: Plugin loads but crashes

**Symptoms:** AutoCAD crashes when running `SNOOP`

**Solutions:**
1. Check error logs:
   ```
   C:\Users\[Username]\AppData\Local\Temp\UnifiedSnoop\Logs\
   ```
2. Verify .NET version matches AutoCAD version
3. Check for missing dependencies
4. Try `SNOOPVERSION` to verify basic loading works
5. Report issue with log files

---

## Uninstallation

### Remove UnifiedSnoop

#### Method 1: Manual Removal
1. Close all AutoCAD/Civil 3D instances
2. Delete folder:
   ```
   C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
   ```
3. Restart AutoCAD

#### Method 2: Script Removal
```powershell
# Remove UnifiedSnoop
$bundlePath = "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle"

if (Test-Path $bundlePath) {
    Remove-Item $bundlePath -Recurse -Force
    Write-Host "UnifiedSnoop uninstalled successfully"
} else {
    Write-Host "UnifiedSnoop not found"
}
```

### Clean Up Logs (Optional)
```
C:\Users\[Username]\AppData\Local\Temp\UnifiedSnoop\
```

### Remove Bookmarks (Optional)
Bookmarks are stored in:
```
C:\Users\[Username]\AppData\Local\UnifiedSnoop\bookmarks.json
```

---

## Advanced Deployment Scenarios

### Scenario 1: Multiple AutoCAD Versions on Same Machine

**Problem:** User has both AutoCAD 2024 and 2025 installed.

**Solution:** The bundle structure handles this automatically:
- AutoCAD 2024 loads from `Contents/2024/`
- AutoCAD 2025 loads from `Contents/2025/`
- No conflicts, both work simultaneously ✅

### Scenario 2: Side-by-Side Installation (Dev + Production)

**Setup:**
- Production deployment: `C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\`
- Development deployment: `C:\Development\UnifiedSnoop.bundle\`

**Usage:**
- Production version loads automatically
- For development version, use manual `NETLOAD`

### Scenario 3: Roaming Profile Deployment

**For users with roaming profiles:**

Deploy to:
```
C:\Users\[Username]\AppData\Roaming\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
```

**Benefit:** Plugin follows user to different machines  
**Drawback:** Longer login times (profile copies bundle)

---

## Deployment Checklist

Before deploying to production:

- [ ] Test on development machine
- [ ] Test with AutoCAD 2024
- [ ] Test with AutoCAD 2025+
- [ ] Test with Civil 3D (if applicable)
- [ ] Verify all commands work (`SNOOP`, `SNOOPHANDLE`, `SNOOPVERSION`)
- [ ] Test export functionality (CSV, Excel, JSON)
- [ ] Test context menu integration
- [ ] Verify no errors in AutoCAD command line
- [ ] Check log files for errors
- [ ] Document any environment-specific configuration
- [ ] Create backup of existing plugins (if applicable)
- [ ] Test uninstallation process
- [ ] Prepare rollback plan

---

## Support

For deployment issues:
1. Check error logs: `%TEMP%\UnifiedSnoop\Logs\`
2. Run `SNOOPVERSION` to verify basic loading
3. Review this guide's [Troubleshooting](#troubleshooting) section
4. Contact your system administrator

---

## Appendix: File Locations Reference

### Bundle Location (All Users)
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
```

### Bundle Location (Single User)
```
C:\Users\[Username]\AppData\Roaming\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
```

### Log Files
```
C:\Users\[Username]\AppData\Local\Temp\UnifiedSnoop\Logs\
```

### Bookmarks
```
C:\Users\[Username]\AppData\Local\UnifiedSnoop\bookmarks.json
```

### AutoCAD Support Paths
```
Tools → Options → Files → Support File Search Path
```

---

**Deployment Complete!** 🎉

For usage instructions, see the [User Guide](USER_GUIDE.md).

