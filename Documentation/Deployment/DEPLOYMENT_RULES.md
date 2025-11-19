# UnifiedSnoop Deployment Rules

**Version:** 1.0  
**Last Updated:** November 19, 2025  
**Status:** ENFORCED

---

## 🚨 **CRITICAL RULES - MUST FOLLOW**

### Rule 1: ALWAYS Use Deploy-ToBundle.ps1 (or Quick-Deploy.ps1)

**MANDATORY:** All deployments MUST use the proper deployment script

✅ **Correct (either works):**
```powershell
cd UnifiedSnoop\Deploy
.\Deploy-ToBundle.ps1   # Main script with all safety checks
# OR
.\Quick-Deploy.ps1      # Convenience wrapper that calls Deploy-ToBundle.ps1
```

Both scripts provide the same safety checks. `Quick-Deploy.ps1` is simply a wrapper that calls `Deploy-ToBundle.ps1` with standard parameters.

### Rule 2: Close AutoCAD Before Deployment

**WHY:** Windows locks loaded DLL files. If AutoCAD is running:
- ❌ Deployment appears successful but copies OLD files
- ❌ Your fixes don't actually get deployed
- ❌ Testing shows old bugs still present
- ❌ Wastes time debugging "phantom" issues

**The Deploy-ToBundle.ps1 script will:**
1. Detect running AutoCAD/Civil 3D processes
2. Block deployment with clear error message
3. Show which processes are running (PID, start time)
4. Provide instructions to resolve

**Example Output When Blocked:**
```
╔════════════════════════════════════════════════════════════════╗
║              ❌ DEPLOYMENT BLOCKED ❌                         ║
╚════════════════════════════════════════════════════════════════╝

⚠️  AutoCAD/Civil 3D is currently running!

📋 Running processes detected:
   • acad (PID: 1240, Started: 19/11/2025 15:00:00)

✅ Required actions:
   1. Close ALL AutoCAD/Civil 3D windows
   2. Wait for processes to fully exit
   3. Re-run this deployment script
```

### Rule 3: Version Must Be Incremented

**Every deployment requires a version increment!**

Follow Semantic Versioning:
- **PATCH** (`1.0.1` → `1.0.2`): Bug fixes, minor changes
- **MINOR** (`1.0.5` → `1.1.0`): New features, backward compatible
- **MAJOR** (`1.9.0` → `2.0.0`): Breaking changes

The script validates:
- ✅ Version format is `MAJOR.MINOR.PATCH`
- ✅ Version is greater than last deployment
- ⚠️ Warns if changelog entry is missing

### Rule 4: Changelog Entry Required

**Every version MUST have a changelog entry!**

Update `UnifiedSnoop/version.json`:
```json
{
  "version": "1.0.6",
  "changelog": [
    {
      "version": "1.0.6",  // ← Must match version above
      "date": "2025-11-19",
      "changes": [
        "Fixed bug XYZ",
        "Added feature ABC"
      ]
    }
  ]
}
```

---

## 📋 **Pre-Deployment Checklist**

Run through this checklist before EVERY deployment:

### Before Running Script

- [ ] All AutoCAD/Civil 3D instances closed
- [ ] Version incremented in `version.json`
- [ ] Changelog entry added for new version
- [ ] Code compiles without errors: `dotnet build -c Release`
- [ ] No linter warnings
- [ ] Changes committed to Git (optional but recommended)

### Running Deployment

```powershell
cd UnifiedSnoop\Deploy
.\Deploy-ToBundle.ps1
```

The script will:
1. ✅ Check for running AutoCAD processes
2. ✅ Validate version increment
3. ✅ Validate changelog entry
4. 🔨 Build both .NET 4.8 and .NET 8.0 versions
5. 📦 Deploy to bundle directory
6. 📝 Update deployment log
7. 📤 Commit and push to GitHub

### After Deployment

- [ ] Open AutoCAD/Civil 3D (2024 or 2025+)
- [ ] Run `SNOOPVERSION` - verify version is correct
- [ ] Run `SNOOP` - verify UI opens without errors
- [ ] Test your changes/fixes
- [ ] Check error log has correct version in filename

---

## 🔍 **Troubleshooting**

### "Deployment Blocked - AutoCAD Running"

**Problem:** AutoCAD/Civil 3D process detected

**Solution:**
1. Close all AutoCAD windows
2. Verify closed: `Get-Process | Where-Object {$_.ProcessName -like '*acad*'}`
3. If still showing, wait 10 seconds and check again
4. If stuck, restart your computer

### "Version Not Incremented"

**Problem:** Version in `version.json` matches last deployment

**Solution:**
1. Open `UnifiedSnoop/version.json`
2. Increment version (e.g., `1.0.5` → `1.0.6`)
3. Update component version to match
4. Add changelog entry
5. Re-run deployment

### "Build Failed"

**Problem:** Code doesn't compile

**Solution:**
1. Check build output for errors
2. Fix compilation errors
3. Test manually: `dotnet build -c Release`
4. Once successful, re-run deployment

### "Deployment Successful But Changes Not Working"

**Problem:** This should NOT happen with Deploy-ToBundle.ps1!

**If this occurs:**
1. Check DLL timestamps in bundle:
   ```powershell
   Get-Item "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\Contents\2024\UnifiedSnoop.dll" | Select LastWriteTime
   ```
2. Should match deployment time
3. If old timestamp, AutoCAD was running during deployment
4. Close AutoCAD and redeploy

---

## ⚙️ **Deployment Script Features**

### Deploy-ToBundle.ps1 Features

**Safety Checks:**
- Process detection (AutoCAD/Civil 3D)
- Version validation (format & increment)
- Changelog validation
- Build output verification
- File lock detection

**Build Process:**
- Multi-target build (net48 + net8.0-windows)
- XRecordEditor separate build
- Release configuration
- Comprehensive error handling

**Deployment:**
- Bundle structure creation
- DLL copying with verification
- PackageContents.xml management
- Version file copying
- Deployment log updates

**Git Integration:**
- Automatic staging of changes
- Commit with version & timestamp
- Push to GitHub
- Branch validation

### Quick-Deploy.ps1

✅ **Safe to use** - This is a convenience wrapper that calls `Deploy-ToBundle.ps1` with standard parameters.

**What it does:**
```powershell
# Quick-Deploy.ps1 internally runs:
.\Deploy-ToBundle.ps1 -BuildFirst -Configuration Release
```

All safety checks are performed by Deploy-ToBundle.ps1.

---

## 📊 **Deployment History**

View deployment history:
```powershell
Get-Content "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\deployment-log.txt"
```

Each deployment logs:
- Version number
- Timestamp
- Configuration (Release/Debug)
- Deployed files

---

## 🎯 **Summary**

### Golden Rules

1. **ALWAYS** use `Deploy-ToBundle.ps1` (or `Quick-Deploy.ps1`)
2. **ALWAYS** close AutoCAD before deploying
3. **ALWAYS** increment version number
4. **ALWAYS** add changelog entry
5. **NEVER** manually copy DLLs

### Quick Reference

**Standard Deployment:**
```powershell
# 1. Close AutoCAD
# 2. Update version.json
# 3. Run deployment
cd UnifiedSnoop\Deploy
.\Deploy-ToBundle.ps1
```

**Verify Deployment:**
```powershell
# Start AutoCAD
# Run: SNOOPVERSION
# Run: SNOOP
# Test your changes
```

---

**For more details, see:**
- `Documentation/Deployment/DEPLOYMENT_GUIDE.md`
- `Documentation/Development/DOCUMENTATION_RULES.md`
- `.cursor/commands/Rebuild&Redploy.md`

