# 🚀 Deployment Rules for UnifiedSnoop

## ⚠️ CRITICAL RULE #1: GitHub Update Required

**Every deployment MUST update the GitHub repository!**

This is now **automated** in the deployment script.

---

## 📋 Deployment Process

### Automated Flow:

1. **Build** the project (Release configuration)
2. **Deploy** to AutoCAD ApplicationPlugins bundle
3. **Update GitHub** (automatic)
   - Stage all changes (`git add -A`)
   - Commit with version/timestamp
   - Push to remote repository

### Command:

```powershell
cd UnifiedSnoop\Deploy
.\Quick-Deploy.ps1
```

This single command handles **everything** including the GitHub update!

---

## 📤 GitHub Integration Details

### What Happens:
- ✅ All code changes are staged
- ✅ Automatic commit with message: `"Deployment v{version} - {timestamp}"`
- ✅ Push to GitHub remote
- ✅ Display branch and remote info

### If GitHub Update Fails:
- ⚠️ Warning displayed
- ⚠️ Instructions provided for manual update
- ✅ Deployment still completes (local bundle updated)

### Manual GitHub Update (Fallback):

```powershell
git add -A
git commit -m "Manual deployment - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
git push
```

---

## 📝 Why This Rule Exists

1. **Version Control**: Every deployed version is tracked in GitHub
2. **Audit Trail**: Complete history of what was deployed and when
3. **Collaboration**: Team members see latest deployed code
4. **Rollback**: Easy to revert to previous deployment if issues arise
5. **Documentation**: Commit messages provide deployment timestamps

---

## ✅ Verification

After deployment, verify:

```powershell
# Check last commit
git log -1

# Verify it's pushed
git status
```

You should see:
- ✅ Recent commit with deployment message
- ✅ "Your branch is up to date with 'origin/main'" (or your branch name)

---

## 🔧 Configuration

The GitHub update is configured in:
- **Script**: `UnifiedSnoop\Deploy\Deploy-ToBundle.ps1`
- **Step**: Step 8: Update GitHub Repository

---

## 🆘 Troubleshooting

### "Git push failed"
**Solution**: Check network connection, GitHub credentials
```powershell
git push  # Manually push
```

### "Not a git repository"
**Solution**: Ensure you're working in the cloned repository
```powershell
git status  # Verify git repo
```

### "Nothing to commit"
**Solution**: This is fine! No changes = no commit needed
- Script displays: "No changes to commit (repository is clean)"

---

## 🎯 Best Practices

1. ✅ **Always use deployment script** - Don't manually copy DLLs
2. ✅ **Review git status** before deployment if unsure
3. ✅ **Pull latest** before making changes: `git pull`
4. ✅ **Use meaningful commit messages** if committing manually
5. ✅ **Check GitHub** after deployment to verify push

---

## 📚 Related Documentation

- [Deployment Guide](UnifiedSnoop/Deploy/DEPLOYMENT_README.md)
- [Deployment Script](UnifiedSnoop/Deploy/Deploy-ToBundle.ps1)
- [Quick Deploy Script](UnifiedSnoop/Deploy/Quick-Deploy.ps1)

---

**Remember**: 🔄 **Deploy = GitHub Update** ✅

This is **automatic** now - no extra steps needed!

