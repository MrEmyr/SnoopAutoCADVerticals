# Quick-Deploy.ps1
# Quick deployment script - builds and deploys in one command

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$deployScript = Join-Path $scriptPath "Deploy-ToBundle.ps1"

& $deployScript -BuildFirst -Configuration Release

