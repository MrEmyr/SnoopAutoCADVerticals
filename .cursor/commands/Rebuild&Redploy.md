# Rebuild and Redeploy UnifiedSnoop

## ⚠️ MANDATORY: Use Deploy-ToBundle.ps1

**ALWAYS use `Deploy-ToBundle.ps1` - DO NOT use Quick-Deploy.ps1**

## Quick Command
```powershell
cd UnifiedSnoop\Deploy
.\Deploy-ToBundle.ps1
```

### Why This Script is Safe
Both `Quick-Deploy.ps1` and `Deploy-ToBundle.ps1` provide:
- ✅ Checks for running AutoCAD/Civil 3D (prevents locked file deployment)
- ✅ Validates version increment
- ✅ Ensures proper changelog entries
- ✅ Comprehensive build verification

Note: `Quick-Deploy.ps1` is a wrapper that calls `Deploy-ToBundle.ps1` with standard parameters.

## 📋 Pre-Deployment Checklist

### ✅ REQUIRED: Increment Version

**EVERY deployment MUST increment the version number!**

1. **Open:** `UnifiedSnoop/version.json`

2. **Increment version** (follow Semantic Versioning):
   - **Bug fixes** → Increment PATCH: `1.0.1` → `1.0.2`
   - **New features** → Increment MINOR: `1.0.5` → `1.1.0`
   - **Breaking changes** → Increment MAJOR: `1.9.0` → `2.0.0`

3. **Add changelog entry**:
```json
{
  "version": "1.0.2",  // ← INCREMENT THIS
  "buildDate": "2025-11-19",  // ← Will auto-update
  "components": {
    "UnifiedSnoop": "1.0.2",  // ← INCREMENT THIS
    "XRecordEditor": "1.0.0"
  },
  "changelog": [
    {
      "version": "1.0.2",  // ← ADD NEW ENTRY
      "date": "2025-11-19",
      "changes": [
        "CRITICAL FIX: Description of fix...",
        "Improved XYZ functionality..."
      ]
    },
    // ... previous versions
  ]
}
```

### ✅ Build Status
- [ ] Code compiles without errors
- [ ] No linter warnings
- [ ] All tests pass (if applicable)

### ⚠️ Before Deployment
- [ ] Close ALL AutoCAD/Civil 3D instances
- [ ] Version incremented in `version.json`
- [ ] Changelog entry added
- [ ] Code committed to git

## 🚀 Deployment Process

The script will:
1. ✅ Validate version increment
2. ✅ Validate semantic versioning format
3. ✅ Check for changelog entry
4. 🔨 Build both .NET 4.8 and .NET 8.0 versions
5. 📦 Deploy to ApplicationPlugins bundle
6. 📝 Update deployment log
7. 📤 Commit and push to GitHub

## 📍 Deployment Locations

**Bundle:** `C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\`
- `Contents/2024/` - For AutoCAD/Civil 3D 2024 (net48)
- `Contents/2025/` - For AutoCAD/Civil 3D 2025+ (net8.0)

**Version Tracking:**
- `version.json` - Current version info
- `deployment-log.txt` - Deployment history

## 🧪 Testing After Deployment

1. **Start AutoCAD/Civil 3D** (2024 or 2025+)
2. **Run:** `SNOOPVERSION` to verify version
3. **Run:** `SNOOP` to open inspector
4. **Verify** fixes/features work as expected

## 📚 Documentation

- **Version Policy:** `Documentation/Deployment/VERSION_INCREMENT_POLICY.md`
- **Deployment Guide:** `Documentation/Deployment/DEPLOYMENT_GUIDE.md`
- **Development Rules:** `Documentation/Development/DEVELOPMENT_RULES.md`

## 🆘 Troubleshooting

### "Version not incremented!" error
→ Update version in `version.json` before deploying

### "DLL locked by AutoCAD" warning
→ Close AutoCAD/Civil 3D and retry

### Build errors
→ Run `dotnet clean` then redeploy

## 📚 Documentation

- **Deployment Rules (MANDATORY):** `Documentation/Deployment/DEPLOYMENT_RULES.md`
- **Deployment Guide:** `Documentation/Deployment/DEPLOYMENT_GUIDE.md`
- **Development Rules:** `Documentation/Development/DOCUMENTATION_RULES.md`

## 🔢 Version History Quick Reference

| Version | Date | Changes |
|---------|------|---------|
| 1.0.5 | 2025-11-19 | Deployment: All fixes properly deployed with AutoCAD closed |
| 1.0.4 | 2025-11-19 | Version-numbered error logs |
| 1.0.2 | 2025-11-19 | Fixed SplitterDistance crash, Fixed ListView headers |
| 1.0.1 | 2025-11-19 | Documentation reorganization |
| 1.0.0 | 2025-01-17 | Initial release |