#!/usr/bin/env pwsh
# 01-import-regions.ps1

param(
  [string]$ApiUrl = 'http://localhost:8090/api/v1/admin/geographic/regions',
  [string]$CsvRelative = 'csv/region_data.csv',
  [string]$LogPath = "$PSScriptRoot/import.log"
)

$ErrorActionPreference = 'Stop'

function Write-Log {
  param([string]$Message, [ValidateSet('INFO','WARN','ERROR')]$Level='INFO')
  $ts = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss.fff')
  $line = "[${ts}] [$Level] $Message"
  $line | Tee-Object -FilePath $LogPath -Append
}

# Paths
$CsvPath = Join-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -ChildPath $CsvRelative

Write-Log "=== BEGIN import run ==="
Write-Log "API: $ApiUrl"
Write-Log "CSV: $CsvPath"

if (-not (Test-Path $CsvPath)) { throw "CSV not found: $CsvPath" }

# Normalize CSV: ensure header 'region', drop blank lines
$tempCsv = [System.IO.Path]::GetTempFileName()
$raw = Get-Content -Raw -Path $CsvPath
if ($raw -match '^\s*region\s*(\r?\n)') {
  $lines = $raw -split '\r?\n' | Where-Object { $_.Trim() -ne '' }
  $lines | Set-Content -Path $tempCsv -Encoding UTF8
} else {
  # no header -> add one
  @('region') + ($raw -split '\r?\n' | Where-Object { $_.Trim() -ne '' }) |
    Set-Content -Path $tempCsv -Encoding UTF8
}

$rows = Import-Csv -Path $tempCsv
$ok = 0; $fail = 0; $skipped = 0

foreach ($r in $rows) {
  $name = ($r.region).ToString().Trim()
  if ([string]::IsNullOrWhiteSpace($name)) {
    $skipped++; Write-Log "SKIP -> empty region row" 'WARN'; continue
  }

  $bodyObj = [ordered]@{ id = 0; name = $name }
  $json = $bodyObj | ConvertTo-Json -Compress

  try {
    Invoke-RestMethod -Method Post -Uri $ApiUrl -ContentType 'application/json' -Body $json
    $ok++; Write-Log ("OK   -> {0}" -f $name)
  }
  catch {
    $fail++
    $msg = $_.Exception.Message
    $body = $null
    if ($_.Exception.Response -and $_.Exception.Response.GetResponseStream) {
      try {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $body = $reader.ReadToEnd()
        $reader.Dispose()
      } catch { }
    }
    if ($body) {
      Write-Log ("FAIL -> {0} | error={1} | body={2}" -f $name, $msg, $body) 'ERROR'
    } else {
      Write-Log ("FAIL -> {0} | error={1}" -f $name, $msg) 'ERROR'
    }
  }
}

Remove-Item $tempCsv -Force -ErrorAction SilentlyContinue

Write-Log ("Summary: ok={0} fail={1} skipped={2}" -f $ok, $fail, $skipped)
Write-Log "=== END import run ==="

if ($fail -gt 0) { exit 10 } else { exit 0 }
