Param(
    [string]$Rid = "win-x64",     # Change to win-arm64 if needed later
    [string]$Config = "Release"
)

# === Build settings ===
$OutputDir  = "$PSScriptRoot/bin/Output/$Rid"
$BinaryName = "Cinturon360.Tui.exe"
$BinaryPath = Join-Path $OutputDir $BinaryName

Write-Host "Cleaning output directory..."
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

Write-Host "Publishing .NET app ($Rid, $Config)..."
dotnet publish `
  -c $Config `
  -r $Rid `
  -o $OutputDir `
  -p:PublishAot=false `
  -p:SelfContained=true `
  -p:PublishSingleFile=true `
  -p:PublishTrimmed=false

# === Sanity check ===
if (-not (Test-Path $BinaryPath)) {
    Write-Error "ERROR: Binary not found at: $BinaryPath"
    exit 1
}

Write-Host "Publish complete."
Write-Host "Binary location:"
Write-Host "  $BinaryPath"
