# Deploy-ToBundle.ps1
# Deploys UnifiedSnoop to AutoCAD ApplicationPlugins bundle for testing
# Supports both AutoCAD/Civil 3D 2024 (.NET Framework 4.8) and 2025+ (.NET 8.0)

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

# Build outputs
$Build2024Path = Join-Path $BuildPath "net48"
$Build2025Path = Join-Path $BuildPath "net8.0-windows\win-x64"

# Bundle structure
$BundleContentsPath = Join-Path $BundlePath "Contents"
$Bundle2024Path = Join-Path $BundleContentsPath "2024"
$Bundle2025Path = Join-Path $BundleContentsPath "2025"

Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                                ║" -ForegroundColor Cyan
Write-Host "║         UnifiedSnoop - Deploy to Bundle                       ║" -ForegroundColor Cyan
Write-Host "║                                                                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# ============================================================================
# Step 1: Build (if requested)
# ============================================================================

if ($BuildFirst) {
    Write-Host "🔨 Building UnifiedSnoop..." -ForegroundColor Yellow
    
    Push-Location $ProjectRoot
    try {
        $buildOutput = dotnet build -c $Configuration 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build FAILED!" -ForegroundColor Red
            Write-Host $buildOutput
            exit 1
        }
        
        Write-Host "✅ Build successful!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "⏭️  Skipping build (using existing binaries)" -ForegroundColor Yellow
}

# ============================================================================
# Step 2: Verify build outputs exist
# ============================================================================

Write-Host "`n📦 Verifying build outputs..." -ForegroundColor Yellow

if (-not (Test-Path $Build2024Path)) {
    Write-Host "❌ ERROR: net48 build not found at: $Build2024Path" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $Build2025Path)) {
    Write-Host "❌ ERROR: net8.0-windows build not found at: $Build2025Path" -ForegroundColor Red
    exit 1
}

$dll2024 = Join-Path $Build2024Path "UnifiedSnoop.dll"
$dll2025 = Join-Path $Build2025Path "UnifiedSnoop.dll"

if (-not (Test-Path $dll2024)) {
    Write-Host "❌ ERROR: UnifiedSnoop.dll not found for 2024" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $dll2025)) {
    Write-Host "❌ ERROR: UnifiedSnoop.dll not found for 2025+" -ForegroundColor Red
    exit 1
}

Write-Host "✅ All build outputs verified" -ForegroundColor Green

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
# Step 6: Deploy DLLs
# ============================================================================

Write-Host "`n📦 Deploying DLLs..." -ForegroundColor Yellow

# Deploy 2024 version
Write-Host "   → Copying 2024 DLL (net48)..." -ForegroundColor Cyan
Copy-Item -Path "$Build2024Path\*" -Destination $Bundle2024Path -Recurse -Force
$size2024 = [math]::Round((Get-Item $dll2024).Length / 1KB, 1)
Write-Host "   ✅ 2024: UnifiedSnoop.dll ($size2024 KB)" -ForegroundColor Green

# Deploy 2025+ version
Write-Host "   → Copying 2025+ DLL (net8.0-windows)..." -ForegroundColor Cyan
Copy-Item -Path "$Build2025Path\*" -Destination $Bundle2025Path -Recurse -Force
$size2025 = [math]::Round((Get-Item $dll2025).Length / 1KB, 1)
Write-Host "   ✅ 2025+: UnifiedSnoop.dll ($size2025 KB)" -ForegroundColor Green

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
# Summary
# ============================================================================

Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║              ✅ DEPLOYMENT SUCCESSFUL! ✅                     ║" -ForegroundColor Yellow -NoNewline
Write-Host "  ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "📍 BUNDLE LOCATION:" -ForegroundColor Cyan
Write-Host "   $BundlePath`n"

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
Write-Host "   • SNOOPENTITY - Snoop selected entity"
Write-Host "   • SNOOPSELECTION - Snoop multiple entities"
Write-Host "   • SNOOPVERSION - Show version info"
Write-Host "   • SNOOPCOLLECTORS - List registered collectors"
Write-Host "   • Right-click → 'Snoop This Object'`n"

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Deployment complete! Ready for testing!                      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

