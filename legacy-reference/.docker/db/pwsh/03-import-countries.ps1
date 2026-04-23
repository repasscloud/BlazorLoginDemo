# countries-import-logged.ps1
# PowerShell 7+. Robust CSV import + POST with explicit logging to import.log
# Logs every line through Tee-Object so console and file match.
param(
    [string]$ApiUrl = 'http://localhost:8090/api/v1/admin/geographic/countries',
    [string]$CsvRelative = 'csv/region_continent_country_data_fixed.csv'
)

$ErrorActionPreference = 'Stop'

# Paths
$CsvPath = Join-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -ChildPath $CsvRelative
$LogPath = Join-Path -Path $PSScriptRoot -ChildPath 'import.log'

# Logging
function Write-Log {
    param(
        [Parameter(Mandatory=$true)][string]$Message,
        [ValidateSet('INFO','WARN','ERROR','DEBUG')][string]$Level = 'INFO'
    )
    $line = ('{0} [{1}] {2}' -f (Get-Date -Format o), $Level, $Message)
    $line | Tee-Object -FilePath $LogPath -Append | Out-Host
}

# Init log
New-Item -ItemType File -Force -Path $LogPath | Out-Null
Write-Log "=== START import run ==="

# CSV presence
if (-not (Test-Path $CsvPath)) {
    Write-Log "CSV not found: $CsvPath" 'ERROR'
    exit 2
}

# Normalize to UTF-8 LF and replace smart quotes with plain quotes using Unicode escapes
try {
    $raw = Get-Content -Path $CsvPath -Raw
    $normalized = ($raw -replace '\u201C|\u201D', '"' -replace '\u2018|\u2019', "'" -replace "`r`n|`r", "`n")
    $tempCsv = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tempCsv, $normalized, [System.Text.UTF8Encoding]$true)
    Write-Log "CSV normalized -> $tempCsv" 'DEBUG'
} catch {
    Write-Log ("Failed normalizing CSV: {0}" -f $_.Exception.Message) 'ERROR'
    exit 3
}

# Parse CSV
try {
    $rows = Import-Csv -Path $tempCsv -Delimiter ','
    Write-Log ("Loaded {0} rows" -f $rows.Count)
} catch {
    Write-Log ("CSV parse failed: {0}" -f $_.Exception.Message) 'ERROR'
    Remove-Item $tempCsv -Force -ErrorAction SilentlyContinue
    exit 4
}

# Counters
$ok = 0
$fail = 0
$skipped = 0

# Process
foreach ($r in $rows) {
    # Skip blank or missing country
    if ([string]::IsNullOrWhiteSpace($r.country)) {
        $skipped++
        Write-Log "Skipped row with empty .country" 'WARN'
        continue
    }

    # Payload
    try {
        $payload = [ordered]@{
            id          = 0
            name        = $r.country
            isoCode     = ($r.countryIso).ToUpper()
            flag        = $r.flag
            continentId = [int]$r.continentId
        }
    } catch {
        $fail++
        Write-Log ("Row build failed for '{0}': {1}" -f $r.country, $_.Exception.Message) 'ERROR'
        continue
    }

    $json = $payload | ConvertTo-Json -Depth 4

    # Call API
    try {
        $irmParams = @{
            Method      = 'POST'
            Uri         = $ApiUrl
            ContentType = 'application/json'
            Body        = $json
        }
        $resp = Invoke-RestMethod @irmParams
        $ok++
        Write-Log ("OK -> {0} [{1}]  rsp={2}" -f $r.country, $payload.isoCode, ($resp | ConvertTo-Json -Compress)) 'INFO'
    } catch {
        $fail++
        $msg = $_.Exception.Message
        # Capture response body if present
        if ($_.Exception.Response -and $_.Exception.Response.GetResponseStream) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $body = $reader.ReadToEnd()
                $reader.Dispose()
                Write-Log ("FAIL -> {0} [{1}]  error={2}  body={3}" -f $r.country, $payload.isoCode, $msg, $body) 'ERROR'
                continue
            } catch {
                # ignore secondary read errors
            }
        }
        Write-Log ("FAIL -> {0} [{1}]  error={2}" -f $r.country, $payload.isoCode, $msg) 'ERROR'
    }
}

# Cleanup
Remove-Item $tempCsv -Force -ErrorAction SilentlyContinue

# Summary
Write-Log ("Summary: ok={0} fail={1} skipped={2}" -f $ok, $fail, $skipped)
Write-Log "=== END import run ==="

# Exit code: nonzero if any failures
if ($fail -gt 0) { exit 10 } else { exit 0 }
