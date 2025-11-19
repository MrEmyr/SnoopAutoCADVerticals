# Quick-Deploy.ps1
# Quick deployment script - builds and deploys in one command
# 
# This is a convenience wrapper that calls Deploy-ToBundle.ps1
# All safety checks (AutoCAD detection, version validation, etc.) are performed by Deploy-ToBundle.ps1

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$deployScript = Join-Path $scriptPath "Deploy-ToBundle.ps1"

& $deployScript -BuildFirst -Configuration Release

