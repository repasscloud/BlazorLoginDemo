#!/usr/bin/env pwsh
# 02-import-continents.ps1

param(
  [string]$ApiUrl     = 'http://localhost:8090/api/v1/admin/geographic/continents',
  [string]$CsvRelative = 'csv/region_continent_data.csv',
  [string]$LogPath    = "$PSScriptRoot/import.log"
)

$ErrorActionPreference = 'Stop'

function Write-Log {
  param([string]$Message, [ValidateSet('INFO','WARN','ERROR')]$Level='INFO')
  $ts = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss.fff')
  $line = "[${ts}] [$Level] $Message"
  $line | Tee-Object -FilePath $LogPath -Append
}

# Resolve CSV path:
# - If $CsvRelative is absolute, use it.
# - Otherwise try ../$CsvRelative (keeps your layout where pwsh/ sits next to csv/)
if ([System.IO.Path]::IsPathRooted($CsvRelative)) {
  $CsvPath = $CsvRelative
} else {
  $CsvPath = Join-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -ChildPath $CsvRelative
  if (-not (Test-Path $CsvPath)) {
    # fallback to script folder relative
    $CsvPath = Join-Path -Path $PSScriptRoot -ChildPath $CsvRelative
  }
}

Write-Log "=== BEGIN import continents ==="
Write-Log "API: $ApiUrl"
Write-Log "CSV: $CsvPath"

if (-not (Test-Path $CsvPath)) { throw "CSV not found: $CsvPath" }

# Read CSV as-is. Expect headers: region,regionId,continent,continentIso
$rows = Import-Csv -Path $CsvPath
$ok = 0; $fail = 0; $skipped = 0

foreach ($r in $rows) {
  # Map fields
  $name     = ($r.continent).ToString().Trim()
  $isoCode  = ($r.continentIso).ToString().Trim()
  $regionId = $null
  if ($r.PSObject.Properties.Name -contains 'regionId' -and $null -ne $r.regionId -and $r.regionId.ToString().Trim() -ne '') {
    [int]$regionId = $r.regionId
  }

  # Skip rows with no continent or no regionId
  if ([string]::IsNullOrWhiteSpace($name) -or $null -eq $regionId) {
    $skipped++; Write-Log ("SKIP -> continent='{0}' regionId='{1}'" -f $name, $r.regionId) 'WARN'; continue
  }

  # Build payload. id always 0
  $payload = [ordered]@{
    id       = 0
    name     = $name
    isoCode  = $isoCode
    regionId = $regionId
  }
  $json = $payload | ConvertTo-Json -Compress

  try {
    $null = Invoke-RestMethod -Method Post -Uri $ApiUrl -ContentType 'application/json' -Body $json
    $ok++; Write-Log ("OK   -> name='{0}', iso='{1}', regionId={2}" -f $name, $isoCode, $regionId)
  }
  catch {
    $fail++
    $msg = $_.Exception.Message
    $respBody = $null
    if ($_.Exception.Response -and $_.Exception.Response.GetResponseStream) {
      try {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $respBody = $reader.ReadToEnd()
        $reader.Dispose()
      } catch { }
    }
    if ($respBody) {
      Write-Log ("FAIL -> name='{0}', iso='{1}', regionId={2} | error={3} | body={4}" -f $name, $isoCode, $regionId, $msg, $respBody) 'ERROR'
    } else {
      Write-Log ("FAIL -> name='{0}', iso='{1}', regionId={2} | error={3}" -f $name, $isoCode, $regionId, $msg) 'ERROR'
    }
  }
}

Write-Log ("Summary: ok={0} fail={1} skipped={2}" -f $ok, $fail, $skipped)
Write-Log "=== END import continents ==="

if ($fail -gt 0) { exit 10 } else { exit 0 }
