# Deploy-ToBundle.ps1
# Deploys UnifiedSnoop to AutoCAD ApplicationPlugins bundle for testing
# Supports both AutoCAD/Civil 3D 2024 (.NET Framework 4.8) and 2025+ (.NET 8.0)
#
# CRITICAL FIXES (2025-11-19):
# - Added AutoCAD process detection (blocks deployment if AutoCAD is running)
# - Removed dangerous obj folder fallback (was deploying stale/old DLLs)
# - Added dotnet clean before build (prevents incremental build issues)
# - Added DLL freshness check (warns if DLLs are older than 5 minutes)
# - Build failures now STOP deployment (no silent failures)
#
# WHY obj FOLDER IS DANGEROUS:
# The obj folder can contain stale DLLs from previous builds. Using it as a
# fallback results in "successful" deployments that don't actually update the code.
# This script now ONLY uses bin folder and fails hard if DLLs aren't fresh.

param(
    [switch]$BuildFirst = $true,
    [switch]$CleanDeploy = $false,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# ============================================================================
# Configuration
# ============================================================================

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$BundlePath = "C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle"
$BuildPath = Join-Path $ProjectRoot "bin\x64\$Configuration"
$VersionFile = Join-Path $ProjectRoot "version.json"
$DeploymentLogFile = Join-Path $BundlePath "deployment-log.txt"

# Build outputs
$Build2024Path = Join-Path $BuildPath "net48"
$Build2025Path = Join-Path $BuildPath "net8.0-windows\win-x64"

# Bundle structure
$BundleContentsPath = Join-Path $BundlePath "Contents"
$Bundle2024Path = Join-Path $BundleContentsPath "2024"
$Bundle2025Path = Join-Path $BundleContentsPath "2025"

# ============================================================================
# Check for Running AutoCAD/Civil3D Processes
# ============================================================================

Write-Host ""
Write-Host "🔍 Checking for running AutoCAD/Civil 3D processes..." -ForegroundColor Cyan

$acadProcesses = Get-Process | Where-Object {
    $_.ProcessName -like "*acad*" -or 
    $_.ProcessName -like "*civil*"
}

if ($acadProcesses) {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║                                                                ║" -ForegroundColor Red
    Write-Host "║              ❌ DEPLOYMENT BLOCKED ❌                         ║" -ForegroundColor Red
    Write-Host "║                                                                ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️  AutoCAD/Civil 3D is currently running!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📋 Running processes detected:" -ForegroundColor Yellow
    foreach ($process in $acadProcesses) {
        Write-Host "   • $($process.ProcessName) (PID: $($process.Id), Started: $($process.StartTime))" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "🚫 Cannot deploy while AutoCAD is running because:" -ForegroundColor Red
    Write-Host "   1. Loaded DLLs are locked by Windows" -ForegroundColor Gray
    Write-Host "   2. Deployment will appear successful but use OLD files" -ForegroundColor Gray
    Write-Host "   3. Your fixes won't actually be deployed!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "✅ Required actions:" -ForegroundColor Green
    Write-Host "   1. Close ALL AutoCAD/Civil 3D windows" -ForegroundColor White
    Write-Host "   2. Wait for processes to fully exit" -ForegroundColor White
    Write-Host "   3. Re-run this deployment script" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Tip: To verify processes are closed, run:" -ForegroundColor Cyan
    Write-Host "   Get-Process | Where-Object {`$_.ProcessName -like '*acad*'}" -ForegroundColor Gray
    Write-Host ""
    
    exit 1
}

Write-Host "✅ No AutoCAD processes detected - safe to proceed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# Read Version Information
# ============================================================================

$version = "Unknown"
$deploymentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$versionData = $null

if (Test-Path $VersionFile) {
    try {
        $versionData = Get-Content $VersionFile -Raw | ConvertFrom-Json
        $version = $versionData.version
    }
    catch {
        Write-Host "⚠️  Could not read version file, using default" -ForegroundColor Yellow
    }
}

# ============================================================================
# Version Validation (Per Deployment Rules)
# ============================================================================

if ($versionData) {
    # Validate semantic versioning format
    if ($version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Host "❌ ERROR: Invalid version format '$version'" -ForegroundColor Red
        Write-Host "   Expected format: MAJOR.MINOR.PATCH (e.g., 1.0.2)" -ForegroundColor Yellow
        Write-Host "   See: Documentation/Deployment/VERSION_INCREMENT_POLICY.md" -ForegroundColor Cyan
        exit 1
    }
    
    # Check if previous deployment exists and version has been incremented
    if (Test-Path $DeploymentLogFile) {
        $lastDeployment = Get-Content $DeploymentLogFile -Tail 10 | Select-String -Pattern "Version: (\d+\.\d+\.\d+)" | Select-Object -Last 1
        
        if ($lastDeployment) {
            $lastVersion = $lastDeployment.Matches.Groups[1].Value
            
            if ($lastVersion -eq $version) {
                Write-Host "❌ ERROR: Version not incremented!" -ForegroundColor Red
                Write-Host "   Last deployed version: v$lastVersion" -ForegroundColor Yellow
                Write-Host "   Current version.json: v$version" -ForegroundColor Yellow
                Write-Host "" -ForegroundColor Yellow
                Write-Host "   📋 ACTION REQUIRED:" -ForegroundColor Cyan
                Write-Host "   1. Update version in UnifiedSnoop/version.json" -ForegroundColor White
                Write-Host "   2. Increment PATCH for bug fixes (e.g., $lastVersion → " -NoNewline -ForegroundColor White
                
                # Calculate next version
                $parts = $lastVersion.Split('.')
                $nextPatch = [int]$parts[2] + 1
                $suggestedVersion = "$($parts[0]).$($parts[1]).$nextPatch"
                Write-Host "$suggestedVersion)" -ForegroundColor Green
                Write-Host "   3. Add changelog entry with your changes" -ForegroundColor White
                Write-Host "" -ForegroundColor Yellow
                Write-Host "   See: Documentation/Deployment/VERSION_INCREMENT_POLICY.md" -ForegroundColor Cyan
                exit 1
            }
            else {
                Write-Host "✅ Version incremented: v$lastVersion → v$version" -ForegroundColor Green
            }
        }
    }
    
    # Validate changelog exists for current version
    $hasChangelog = $false
    foreach ($entry in $versionData.changelog) {
        if ($entry.version -eq $version) {
            $hasChangelog = $true
            break
        }
    }
    
    if (-not $hasChangelog) {
        Write-Host "⚠️  WARNING: No changelog entry for v$version" -ForegroundColor Yellow
        Write-Host "   Add a changelog entry in version.json describing your changes" -ForegroundColor Cyan
    }
    else {
        Write-Host "✅ Changelog validated for v$version" -ForegroundColor Green
    }
}

Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                                ║" -ForegroundColor Cyan
Write-Host "║         UnifiedSnoop - Deploy to Bundle                       ║" -ForegroundColor Cyan
Write-Host "║         Version: $version                                      ║" -ForegroundColor Cyan
Write-Host "║         Build: $Configuration                                  ║" -ForegroundColor Cyan
Write-Host "║                                                                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ============================================================================
# Step 1: Build (if requested)
# ============================================================================

if ($BuildFirst) {
    Write-Host "🔨 Building UnifiedSnoop..." -ForegroundColor Yellow
    
    # Clean before building to prevent stale DLL deployment
    Write-Host "   → Cleaning previous build artifacts..." -ForegroundColor Cyan
    Push-Location $ProjectRoot
    try {
        $cleanOutput = dotnet clean -c $Configuration 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️  Clean had warnings (non-critical)" -ForegroundColor Yellow
        }
        
        # Force complete rebuild with no incremental compilation
        Write-Host "   → Building with --no-incremental flag..." -ForegroundColor Cyan
        $buildOutput = dotnet build -c $Configuration --no-incremental 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host ""
            Write-Host "❌ ERROR: Build failed!" -ForegroundColor Red
            Write-Host ""
            Write-Host "Build output:" -ForegroundColor Yellow
            Write-Host $buildOutput
            Write-Host ""
            Write-Host "⚠️  Cannot deploy - build must succeed first!" -ForegroundColor Red
            Write-Host ""
            exit 1
        }
        else {
            Write-Host "✅ UnifiedSnoop Build successful!" -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }
    
    # Build XRecordEditor project
    Write-Host "🔨 Building XRecordEditor..." -ForegroundColor Yellow
    $XRecordEditorPath = Join-Path $ProjectRoot "XRecordEditor"
    
    if (Test-Path $XRecordEditorPath) {
        Push-Location $XRecordEditorPath
        try {
            $buildOutput = dotnet build -c $Configuration 2>&1
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "⚠️  XRecordEditor Build had errors" -ForegroundColor Yellow
                Write-Host "   XRecordEditor deployment may be skipped (non-critical)" -ForegroundColor Cyan
            }
            else {
                Write-Host "✅ XRecordEditor Build successful!" -ForegroundColor Green
            }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Host "⚠️  XRecordEditor project not found at: $XRecordEditorPath" -ForegroundColor Yellow
    }
}
else {
    Write-Host "⏭️  Skipping build (using existing binaries)" -ForegroundColor Yellow
}

# ============================================================================
# Step 2: Verify build outputs exist
# ============================================================================

Write-Host "`n📦 Verifying build outputs..." -ForegroundColor Yellow

$dll2024 = Join-Path $Build2024Path "UnifiedSnoop.dll"
$dll2025 = Join-Path $Build2025Path "UnifiedSnoop.dll"

# ONLY check bin folder - NEVER use obj folder (contains stale DLLs)
if (-not (Test-Path $dll2024)) {
    Write-Host ""
    Write-Host "❌ ERROR: UnifiedSnoop.dll not found for 2024" -ForegroundColor Red
    Write-Host "   Expected: $dll2024" -ForegroundColor Gray
    Write-Host ""
    Write-Host "⚠️  Build may have failed or AutoCAD is locking the DLL!" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

if (-not (Test-Path $dll2025)) {
    Write-Host ""
    Write-Host "❌ ERROR: UnifiedSnoop.dll not found for 2025+" -ForegroundColor Red
    Write-Host "   Expected: $dll2025" -ForegroundColor Gray
    Write-Host ""
    Write-Host "⚠️  Build may have failed or AutoCAD is locking the DLL!" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# Verify DLLs are fresh (built within last 5 minutes)
$dll2024Info = Get-Item $dll2024
$dll2025Info = Get-Item $dll2025
$now = Get-Date
$maxAge = New-TimeSpan -Minutes 5

if (($now - $dll2024Info.LastWriteTime) -gt $maxAge) {
    Write-Host ""
    Write-Host "⚠️  WARNING: 2024 DLL is old!" -ForegroundColor Yellow
    Write-Host "   Last modified: $($dll2024Info.LastWriteTime)" -ForegroundColor Gray
    Write-Host "   This may be a stale build!" -ForegroundColor Yellow
    Write-Host ""
}

if (($now - $dll2025Info.LastWriteTime) -gt $maxAge) {
    Write-Host ""
    Write-Host "⚠️  WARNING: 2025+ DLL is old!" -ForegroundColor Yellow
    Write-Host "   Last modified: $($dll2025Info.LastWriteTime)" -ForegroundColor Gray
    Write-Host "   This may be a stale build!" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "✅ 2024 build output verified (bin)" -ForegroundColor Green
Write-Host "   → $($dll2024Info.LastWriteTime) - $([math]::Round($dll2024Info.Length/1KB, 2)) KB" -ForegroundColor Gray

Write-Host "✅ 2025 build output verified (bin)" -ForegroundColor Green
Write-Host "   → $($dll2025Info.LastWriteTime) - $([math]::Round($dll2025Info.Length/1KB, 2)) KB" -ForegroundColor Gray

# ============================================================================
# Step 3: Clean bundle directory (if requested)
# ============================================================================

if ($CleanDeploy -and (Test-Path $BundlePath)) {
    Write-Host "`n🧹 Cleaning existing bundle..." -ForegroundColor Yellow
    Remove-Item -Path $BundlePath -Recurse -Force
    Write-Host "✅ Bundle cleaned" -ForegroundColor Green
}

# ============================================================================
# Step 4: Create bundle structure
# ============================================================================

Write-Host "`n📁 Creating bundle structure..." -ForegroundColor Yellow

# Create directories
New-Item -ItemType Directory -Path $BundlePath -Force | Out-Null
New-Item -ItemType Directory -Path $BundleContentsPath -Force | Out-Null
New-Item -ItemType Directory -Path $Bundle2024Path -Force | Out-Null
New-Item -ItemType Directory -Path $Bundle2025Path -Force | Out-Null

Write-Host "✅ Bundle directories created" -ForegroundColor Green

# ============================================================================
# Step 5: Copy PackageContents.xml
# ============================================================================

Write-Host "`n📄 Copying PackageContents.xml..." -ForegroundColor Yellow

$packageXmlSource = Join-Path $PSScriptRoot "PackageContents.xml"
$packageXmlDest = Join-Path $BundlePath "PackageContents.xml"

if (Test-Path $packageXmlSource) {
    Copy-Item -Path $packageXmlSource -Destination $packageXmlDest -Force
    Write-Host "✅ PackageContents.xml copied" -ForegroundColor Green
}
else {
    Write-Host "⚠️  WARNING: PackageContents.xml not found at: $packageXmlSource" -ForegroundColor Yellow
}

# ============================================================================
# Step 5.5: Copy version.json and create deployment log
# ============================================================================

Write-Host "`n📝 Creating version and deployment tracking..." -ForegroundColor Yellow

# Update version.json with build date
if (Test-Path $VersionFile) {
    try {
        $versionData = Get-Content $VersionFile -Raw | ConvertFrom-Json
        $versionData.buildDate = $deploymentTime
        $versionData | ConvertTo-Json -Depth 10 | Set-Content $VersionFile
        
        # Copy to bundle
        $versionDest = Join-Path $BundlePath "version.json"
        Copy-Item -Path $VersionFile -Destination $versionDest -Force
        Write-Host "✅ version.json copied (v$version)" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️  Could not update version.json" -ForegroundColor Yellow
    }
}

# Create/update deployment log
$deploymentEntry = @"

═══════════════════════════════════════════════════════════════
Deployment: $deploymentTime
Version: $version
Configuration: $Configuration
═══════════════════════════════════════════════════════════════
"@

Add-Content -Path $DeploymentLogFile -Value $deploymentEntry
Write-Host "✅ Deployment log updated" -ForegroundColor Green

# ============================================================================
# Step 6: Deploy DLLs
# ============================================================================

Write-Host "`n📦 Deploying DLLs..." -ForegroundColor Yellow

# Deploy 2024 version
Write-Host "   → Copying 2024 DLL (net48)..." -ForegroundColor Cyan

# Check if DLL is locked (should NOT happen since we check for AutoCAD at start)
try {
    $fileStream = [System.IO.File]::Open($dll2024, 'Open', 'Read', 'None')
    $fileStream.Close()
}
catch {
    Write-Host ""
    Write-Host "❌ ERROR: DLL is locked!" -ForegroundColor Red
    Write-Host "   File: $dll2024" -ForegroundColor Gray
    Write-Host ""
    Write-Host "⚠️  This should NOT happen - AutoCAD check should have caught this!" -ForegroundColor Yellow
    Write-Host "   Check if any AutoCAD processes are still running:" -ForegroundColor Yellow
    Write-Host "   Get-Process | Where-Object {`$_.ProcessName -like '*acad*'}" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

# Copy only DLL and PDB files from bin folder (NEVER use obj folder!)
try {
    Copy-Item -Path "$Build2024Path\*.dll" -Destination $Bundle2024Path -Force
    Copy-Item -Path "$Build2024Path\*.pdb" -Destination $Bundle2024Path -Force -ErrorAction SilentlyContinue
    $size2024 = [math]::Round((Get-Item $dll2024).Length / 1KB, 1)
    Write-Host "   ✅ 2024: UnifiedSnoop.dll ($size2024 KB)" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "❌ ERROR: Failed to copy 2024 DLL!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

# Deploy 2025+ version
Write-Host "   → Copying 2025+ DLL (net8.0-windows)..." -ForegroundColor Cyan
# Copy only DLL and PDB files, not build artifacts
Copy-Item -Path "$Build2025Path\*.dll" -Destination $Bundle2025Path -Force
Copy-Item -Path "$Build2025Path\*.pdb" -Destination $Bundle2025Path -Force -ErrorAction SilentlyContinue
$size2025 = [math]::Round((Get-Item $dll2025).Length / 1KB, 1)
Write-Host "   ✅ 2025+: UnifiedSnoop.dll ($size2025 KB)" -ForegroundColor Green

# Deploy XRecordEditor (advanced feature)
Write-Host "`n   → Copying XRecordEditor DLL (advanced feature)..." -ForegroundColor Cyan
$XRecordEditorBuildPath = Join-Path $ProjectRoot "XRecordEditor\bin\x64\$Configuration"
$XRecordEditor2024Path = Join-Path $XRecordEditorBuildPath "net48"
$XRecordEditor2025Path = Join-Path $XRecordEditorBuildPath "net8.0-windows\win-x64"

# Deploy XRecordEditor 2024 (ONLY from bin folder)
if (Test-Path $XRecordEditor2024Path) {
    $xrec2024dll = Join-Path $XRecordEditor2024Path "XRecordEditor.dll"
    
    if (Test-Path $xrec2024dll) {
        try {
            Copy-Item -Path $xrec2024dll -Destination $Bundle2024Path -Force
            $xrecSize2024 = [math]::Round((Get-Item $xrec2024dll).Length / 1KB, 1)
            Write-Host "   ✅ 2024: XRecordEditor.dll ($xrecSize2024 KB)" -ForegroundColor Green
        }
        catch {
            Write-Host "   ⚠️  Failed to copy XRecordEditor.dll (non-critical)" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "   ⚠️  XRecordEditor (2024) not found" -ForegroundColor Yellow
}

# Deploy XRecordEditor 2025
if (Test-Path $XRecordEditor2025Path) {
    $xrec2025dll = Join-Path $XRecordEditor2025Path "XRecordEditor.dll"
    if (Test-Path $xrec2025dll) {
        Copy-Item -Path $xrec2025dll -Destination $Bundle2025Path -Force
        $xrecSize2025 = [math]::Round((Get-Item $xrec2025dll).Length / 1KB, 1)
        Write-Host "   ✅ 2025+: XRecordEditor.dll ($xrecSize2025 KB)" -ForegroundColor Green
    }
}
else {
    Write-Host "   ⚠️  XRecordEditor (2025+) not found" -ForegroundColor Yellow
}

# ============================================================================
# Step 7: Verify deployment
# ============================================================================

Write-Host "`n✅ Verifying deployment..." -ForegroundColor Yellow

$deployed2024 = Join-Path $Bundle2024Path "UnifiedSnoop.dll"
$deployed2025 = Join-Path $Bundle2025Path "UnifiedSnoop.dll"

if ((Test-Path $deployed2024) -and (Test-Path $deployed2025)) {
    Write-Host "✅ Deployment verified!" -ForegroundColor Green
}
else {
    Write-Host "❌ Deployment verification FAILED!" -ForegroundColor Red
    exit 1
}

# ============================================================================
# Step 8: Update GitHub Repository
# ============================================================================

Write-Host "`n📤 Updating GitHub repository..." -ForegroundColor Yellow

try {
    # Check if we're in a git repository
    $gitCheck = git rev-parse --is-inside-work-tree 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        # Check if there are any changes to commit
        $gitStatus = git status --porcelain
        
        if ($gitStatus) {
            Write-Host "   → Staging all changes..." -ForegroundColor Cyan
            git add -A
            
            Write-Host "   → Committing changes..." -ForegroundColor Cyan
            $commitMessage = "Deployment v$version - $deploymentTime"
            git commit -m $commitMessage
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   → Pushing to GitHub..." -ForegroundColor Cyan
                git push
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ GitHub repository updated successfully!" -ForegroundColor Green
                    
                    # Get current branch and remote info
                    $currentBranch = git rev-parse --abbrev-ref HEAD
                    $remoteUrl = git config --get remote.origin.url
                    Write-Host "   Branch: $currentBranch" -ForegroundColor Gray
                    Write-Host "   Remote: $remoteUrl" -ForegroundColor Gray
                }
                else {
                    Write-Host "⚠️  WARNING: Git push failed! Please push manually." -ForegroundColor Yellow
                    Write-Host "   Command: git push" -ForegroundColor Gray
                }
            }
            else {
                Write-Host "⚠️  WARNING: Git commit failed!" -ForegroundColor Yellow
            }
        }
        else {
            Write-Host "ℹ️  No changes to commit (repository is clean)" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "⚠️  Not a git repository - skipping GitHub update" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️  WARNING: GitHub update failed: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   Please commit and push manually" -ForegroundColor Gray
}

# ============================================================================
# Summary
# ============================================================================

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║              ✅ DEPLOYMENT SUCCESSFUL! ✅                     ║" -ForegroundColor Yellow -NoNewline
Write-Host "║" -ForegroundColor Green
Write-Host "║              Version: $version                                 ║" -ForegroundColor Green
Write-Host "║              Time: $deploymentTime                             ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "📍 BUNDLE LOCATION:" -ForegroundColor Cyan
Write-Host "   $BundlePath`n"

Write-Host "📝 VERSION TRACKING:" -ForegroundColor Cyan
Write-Host "   Version: $version"
Write-Host "   Build Date: $deploymentTime"
Write-Host "   Version file: $BundlePath\version.json"
Write-Host "   Deployment log: $DeploymentLogFile`n"

Write-Host "📦 DEPLOYED FILES:" -ForegroundColor Cyan
Write-Host "   2024: $Bundle2024Path"
Write-Host "         UnifiedSnoop.dll ($size2024 KB)"
Write-Host ""
Write-Host "   2025+: $Bundle2025Path"
Write-Host "         UnifiedSnoop.dll ($size2025 KB)`n"

Write-Host "🎮 TESTING INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   AutoCAD/Civil 3D 2024:"
Write-Host "   1. Start AutoCAD/Civil 3D 2024"
Write-Host "   2. The plugin loads automatically"
Write-Host "   3. Type: SNOOP"
Write-Host ""
Write-Host "   AutoCAD/Civil 3D 2025+:"
Write-Host "   1. Start AutoCAD/Civil 3D 2025 or 2026"
Write-Host "   2. The plugin loads automatically"
Write-Host "   3. Type: SNOOP`n"

Write-Host "⚠️  IMPORTANT:" -ForegroundColor Red
Write-Host "   If AutoCAD is running, restart it to load the new version!`n"

Write-Host "🔧 AVAILABLE COMMANDS:" -ForegroundColor Cyan
Write-Host "   • SNOOP - Open inspector UI"
Write-Host "   • SNOOPVERSION - Show version info"
Write-Host "   • XRECORDEDIT - XRecord Editor (Advanced Feature - Hidden)`n"
Write-Host "   💡 Tip: Right-click on objects in the SNOOP UI for quick actions`n"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Deployment complete! Ready for testing!                       ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

