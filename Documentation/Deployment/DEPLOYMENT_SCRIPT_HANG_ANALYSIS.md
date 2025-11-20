# Deployment Script Hang Analysis

**Date:** November 20, 2025  
**Issue:** Deployment script appears to hang and requires cancellation  
**Affected Script:** `UnifiedSnoop/Deploy/Deploy-ToBundle.ps1`

---

## Executive Summary

The deployment script hangs at **Step 8: Git Operations** (lines 541-621) due to problems with:
1. Git credential authentication in background PowerShell jobs
2. Unreliable timeout implementation using `Wait-Job`
3. Git Credential Manager attempting to prompt for credentials in a non-interactive context

**Impact:** Deployment appears to freeze after successfully deploying DLLs, preventing script completion and requiring manual cancellation.

---

## Root Cause Analysis

### 1. Git Operations in Background Job (Lines 600-621)

The script attempts to run git operations (`add`, `commit`, `push`) inside a PowerShell background job with a 30-second timeout:

```powershell
$job = Start-Job -ScriptBlock $gitOperations -ArgumentList $version, $deploymentTime, $ProjectRoot
$completed = Wait-Job $job -Timeout 30
```

**Problems:**
- `Start-Job` creates a **new PowerShell process** that doesn't inherit:
  - Git credentials from the parent session
  - SSH agent connections
  - Git Credential Manager state
  - Environment variables (beyond what's explicitly passed)

### 2. Git Push Hangs (Line 573)

```powershell
$env:GIT_TERMINAL_PROMPT=0  # Disable git credential prompts
git push --porcelain 2>&1 | Out-Null
```

**Problems:**
- Setting `GIT_TERMINAL_PROMPT=0` inside the job has **no effect** on Git Credential Manager (GCM)
- Git Credential Manager on Windows may:
  - Attempt to show a GUI credential prompt (which hangs in background job)
  - Wait indefinitely for credentials
  - Timeout after several minutes
- The `--porcelain` flag only affects output format, not authentication
- Using HTTPS remote (`https://github.com/MrEmyr/SnoopAutoCADVerticals.git`) requires authentication

### 3. Credential Helper: `manager`

Current git config:
```
credential.helper=manager
```

Git Credential Manager (`manager`) is designed for interactive use and:
- Expects to show GUI prompts for credentials
- Doesn't work well in background jobs or non-interactive contexts
- Can hang indefinitely when it can't display prompts

### 4. Unreliable Timeout Implementation

```powershell
$completed = Wait-Job $job -Timeout 30

if ($completed) {
    # Process completed
} else {
    # Timeout - try to stop job
    Stop-Job $job -ErrorAction SilentlyContinue
    Remove-Job $job -Force -ErrorAction SilentlyContinue
}
```

**Problems:**
- `Wait-Job -Timeout 30` should work, but can fail if:
  - The job is in a hung state and doesn't respond to signals
  - Git process is waiting for credentials and ignores termination
  - Child processes (git) aren't terminated when parent job is stopped
- `Stop-Job` may not terminate child processes spawned by the job
- On Windows, git.exe may continue running even after PowerShell job is stopped

---

## Evidence

### Current Git Configuration
```
Remote: https://github.com/MrEmyr/SnoopAutoCADVerticals.git
Credential Helper: manager
```

### Deployment Log
Last successful deployments:
- 2025-11-19 16:27:22 - Version 1.0.5
- 2025-11-19 16:56:12 - Version 1.0.6
- 2025-11-19 17:13:21 - Version 1.0.7

All deployments reached the git operations stage.

### Script Behavior
1. ✅ Build succeeds
2. ✅ DLLs deployed successfully
3. ✅ Verification passes
4. ✅ Success message displayed
5. ⚠️ **Hangs at "📤 Updating GitHub repository..."**
6. ❌ User must cancel with Ctrl+C

---

## Why This Wasn't Caught Earlier

1. **Non-deterministic behavior**: The hang only occurs when:
   - Git credentials have expired
   - Network is slow
   - GitHub is unresponsive
   - First-time authentication is needed

2. **Silent failure**: The script displays "Updating GitHub repository (optional, may take a moment)..." but provides no feedback during the 30-second timeout period

3. **Timeout may appear to work initially**: On systems with cached credentials, git push completes quickly and the issue isn't observed

---

## Solutions (In Order of Preference)

### Option 1: Remove Automatic Git Operations (RECOMMENDED)

**Rationale:**
- Deployment scripts should focus on deployment, not source control
- Git operations are developer responsibility
- Mixing deployment and version control creates fragility
- Users may not want automatic commits/pushes

**Implementation:**
```powershell
# Remove lines 541-621 entirely
# Add simple reminder at the end
Write-Host "💡 Remember to commit and push your changes:" -ForegroundColor Cyan
Write-Host "   git add -A" -ForegroundColor Gray
Write-Host "   git commit -m 'Deployment v$version - $deploymentTime'" -ForegroundColor Gray
Write-Host "   git push" -ForegroundColor Gray
```

**Pros:**
- Eliminates hanging entirely
- Simpler, more focused script
- Gives user control over git operations
- Follows Unix philosophy: "Do one thing well"

**Cons:**
- User must manually commit/push

---

### Option 2: Fix Background Job Implementation

**Implementation:**
```powershell
# Use timeout command (Windows) or custom timeout implementation
$gitTimeout = 15  # seconds

Write-Host "📤 Attempting to update GitHub repository..." -ForegroundColor Cyan

# Disable credential prompts
$env:GIT_TERMINAL_PROMPT = "0"
$env:GCM_INTERACTIVE = "never"  # Disable Git Credential Manager GUI

# Check if there are changes
$gitStatus = git status --porcelain 2>&1
if ($gitStatus) {
    git add -A 2>&1 | Out-Null
    
    $commitMessage = "Deployment v$version - $deploymentTime"
    git commit -m $commitMessage 2>&1 | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   → Attempting push (timeout: ${gitTimeout}s)..." -ForegroundColor Cyan
        
        # Use Start-Process with timeout
        $gitPush = Start-Process git -ArgumentList "push" -NoNewWindow -PassThru -RedirectStandardError "git-error.txt"
        $gitPush | Wait-Process -Timeout $gitTimeout -ErrorAction SilentlyContinue
        
        if ($gitPush.HasExited -and $gitPush.ExitCode -eq 0) {
            Write-Host "✅ GitHub repository updated!" -ForegroundColor Green
        } else {
            if (-not $gitPush.HasExited) {
                $gitPush | Stop-Process -Force
            }
            Write-Host "⚠️  Git push timed out - please push manually" -ForegroundColor Yellow
        }
    }
}
```

**Pros:**
- Maintains automatic git functionality
- Better timeout implementation

**Cons:**
- Still fragile due to credential issues
- More complex error handling
- May still fail on credential expiration

---

### Option 3: Make Git Operations Optional with Flag

**Implementation:**
```powershell
param(
    [switch]$BuildFirst = $true,
    [switch]$CleanDeploy = $false,
    [switch]$PushToGit = $false,  # NEW: opt-in git operations
    [string]$Configuration = "Release"
)

# ... rest of script ...

if ($PushToGit) {
    Write-Host "📤 Updating GitHub repository..." -ForegroundColor Cyan
    # ... git operations ...
} else {
    Write-Host "💡 To push changes to GitHub, run:" -ForegroundColor Cyan
    Write-Host "   git add -A && git commit -m 'Deployment v$version' && git push" -ForegroundColor Gray
}
```

**Pros:**
- User control over git operations
- Maintains functionality for those who want it
- Default behavior (no push) doesn't hang

**Cons:**
- Adds script complexity
- Git functionality still fragile when enabled

---

## Recommendation

**Implement Option 1: Remove Automatic Git Operations**

### Justification
1. **Separation of concerns**: Deployment ≠ Source control
2. **Reliability**: Eliminates entire class of hanging issues
3. **User control**: Developers should decide when to commit/push
4. **Simplicity**: Reduces script complexity and maintenance burden
5. **Best practices**: Most deployment tools don't auto-commit

### Additional Improvements
1. Add clear messaging about manual git operations
2. Provide exact commands for user to run
3. Consider creating a separate script for git operations if desired

---

## Implementation Plan

1. ✅ Analyze root cause (COMPLETE)
2. ⬜ Create backup of current script
3. ⬜ Remove git operations (lines 541-621)
4. ⬜ Add clear messaging about manual git workflow
5. ⬜ Test deployment script without git operations
6. ⬜ Update deployment documentation
7. ⬜ Commit changes with fix description

---

## Testing Verification

After implementing the fix, verify:
- ✅ Script completes without hanging
- ✅ Success message displays properly
- ✅ Clear instructions provided for git operations
- ✅ Script execution time under 30 seconds (for normal builds)
- ✅ No background jobs or processes left running

---

## Related Documentation

- `Documentation/Deployment/DEPLOYMENT_GUIDE.md`
- `Documentation/Deployment/DEPLOYMENT_RULES.md`
- `UnifiedSnoop/Deploy/Deploy-ToBundle.ps1`

---

## Lessons Learned

1. **Avoid mixing concerns**: Deployment scripts should deploy, not manage source control
2. **Background jobs inherit nothing**: PowerShell jobs run in isolated processes
3. **Credential managers are interactive**: Git Credential Manager designed for human interaction
4. **Timeouts are tricky**: Process termination on Windows is unreliable
5. **Test unhappy paths**: Scripts work differently when credentials expire or network fails

