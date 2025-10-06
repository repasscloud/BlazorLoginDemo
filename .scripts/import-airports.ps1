#!/usr/bin/env pwsh
# Requires PowerShell 7+

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Low')]
param(
    [switch]$ShowJson   = $false,   # remove Mandatory
    [string]$CsvPath    = $(Join-Path -Path $PSScriptRoot -ChildPath "data/airports.csv"),
    [string]$ApiUrl     = "http://localhost:8090/api/v1/admin/kerneldata/airport-info",
    [string]$BulkApiUrl = "http://localhost:8090/api/v1/admin/kerneldata/airport-info/bulk-upsert",
    [string]$ApiKey     = "abc123",
    [int]$Batch         = 0
)


# === Dry-run plumbing: if -ShowJson is set, intercept the first Invoke-RestMethod call ===
if ($ShowJson) {
    $script:__DryRun = $true
    $script:__DryRunFired = $false

    function Invoke-RestMethod {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory = $true)]
            [string]$Uri,

            [ValidateSet('GET','POST','PUT','PATCH','DELETE','HEAD','OPTIONS','TRACE')]
            [string]$Method = 'GET',

            [object]$Body,
            [hashtable]$Headers,
            [string]$ContentType,

            # Capture any other pass-through parameters without interfering
            [Parameter(ValueFromRemainingArguments = $true)]
            $Remaining
        )

        if ($script:__DryRun -and -not $script:__DryRunFired) {
            Write-Host "=== DRY RUN: No request will be sent ==="
            Write-Host ("Method : {0}" -f $Method)
            Write-Host ("URL    : {0}" -f $Uri)
            Write-Host "Headers:"
            if ($null -ne $Headers) {
                $Headers | ConvertTo-Json -Depth 10 | Write-Output
            } else {
                Write-Output "{}"
            }
            Write-Host "Body:"
            if ($PSBoundParameters.ContainsKey('Body')) {
                if ($Body -is [string]) {
                    # Already a JSON string. Show as-is.
                    Write-Output $Body
                } else {
                    # Likely an object. Show JSON rendering.
                    $Body | ConvertTo-Json -Depth 20 | Write-Output
                }
            } else {
                Write-Output "(no body)"
            }
            Write-Host "=== End preview. Exiting due to -ShowJson ==="
            $script:__DryRunFired = $true
            exit 0
        }

        # If somehow still called in non-dry mode, pass through to the builtin.
        Microsoft.PowerShell.Utility\Invoke-RestMethod @PSBoundParameters
    }
}
# === End dry-run plumbing ===

Set-StrictMode -Version Latest

# ----------------------- Helpers (approved verbs) ----------------------------

function Convert-AirportType {
    [CmdletBinding()]
    param([string]$Value)
    switch (($Value ?? '').ToLower().Trim()) {
        "small_airport"  { return "SmallAirport" }
        "medium_airport" { return "MediumAirport" }
        "large_airport"  { return "LargeAirport" }
        "heliport"       { return "Heliport" }
        "seaplane_base"  { return "SeaplanePort" }
        "balloonport"    { return "BalloonPort" }
        "closed"         { return "Closed" }
        default          { return "Unknown" }
    }
}

function ConvertTo-Boolean {
    [CmdletBinding()]
    param([AllowNull()]$InputObject)
    if ($null -eq $InputObject) { return $false }
    $s = [string]$InputObject
    $s = $s.Trim().ToLowerInvariant()
    return @('y','yes','true','1') -contains $s
}

function ConvertTo-Int {
    [CmdletBinding()]
    param([AllowNull()]$InputObject, [int]$Default = 0)
    if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
    try { return [int]$InputObject } catch { return $Default }
}

function ConvertTo-Double {
    [CmdletBinding()]
    param([AllowNull()]$InputObject, [double]$Default = 0.0)
    if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
    try { return [double]$InputObject } catch { return $Default }
}

function New-AirportPayload {
    [CmdletBinding()]
    param([Parameter(Mandatory=$true)][pscustomobject]$Row)

    $payload = [ordered]@{
        id               = 0
        ident            = ([string]$Row.ident).Trim()
        type             = Convert-AirportType $Row.type
        name             = ([string]$Row.name).Trim()
        latitudeDeg      = (ConvertTo-Double $Row.latitude_deg)
        longitudeDeg     = (ConvertTo-Double $Row.longitude_deg)
        elevationFt      = (ConvertTo-Int    $Row.elevation_ft)
        continent        = ([string]$Row.continent).Trim()     # server converts to enum
        isoCountry       = ([string]$Row.iso_country).Trim()   # server converts to enum
        isoRegion        = ([string]$Row.iso_region).Trim()
        municipality     = ([string]$Row.municipality).Trim()
        scheduledService = (ConvertTo-Boolean $Row.scheduled_service)
        gpsCode          = ([string]$Row.gps_code).Trim()
        iataCode         = ([string]$Row.iata_code).Trim()
        localCode        = ([string]$Row.local_code).Trim()
    }
    return [pscustomobject]$payload
}

function Get-Chunk {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][System.Collections.IList]$Items,
        [Parameter(Mandatory=$true)][int]$Start,
        [Parameter(Mandatory=$true)][int]$Count
    )
    $last = [Math]::Min($Start + $Count - 1, $Items.Count - 1)
    if ($last -lt $Start) { return @() }
    if ($Start -eq $last) { return @($Items[$Start]) }
    return $Items[$Start..$last]
}

# -------------------------- Main --------------------------------------------

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV file not found: $CsvPath"
}

$headers = @{
    'accept'    = 'text/plain'
    'X-Api-Key' = $ApiKey
}
$rows = Import-Csv -Path $CsvPath
if (-not $rows -or $rows.Count -eq 0) {
    Write-Warning "No rows in CSV."
    return
}


# === DRY RUN: deterministic and side-effect free ===
if ($ShowJson) {
    $method = 'POST'
    if ($Batch -gt 0) {
        $targetUrl = $BulkApiUrl
        # Build the first batch payload exactly as normal code would
        $take = [Math]::Min($Batch, [int]$rows.Count)
        $batchRows = $rows | Select-Object -First $take
        $batchPayload = foreach ($row in $batchRows) { New-AirportPayload -Row $row }
        $body = $batchPayload | ConvertTo-Json -Depth 20 -Compress
    } else {
        $targetUrl = $ApiUrl
        $payload = New-AirportPayload -Row $rows[0]
        $body = $payload | ConvertTo-Json -Depth 20 -Compress
    }

    Write-Host "=== DRY RUN: No request will be sent ==="
    Write-Host ("Method : {0}" -f $method)
    Write-Host ("URL    : {0}" -f $targetUrl)
    Write-Host "Headers:"
    $headers | ConvertTo-Json -Depth 10 | Write-Output
    Write-Host "Body:"
    Write-Output $body
    Write-Host "=== End preview. Exiting due to -ShowJson ==="
    exit 0
}
# === END DRY RUN ===
$sent   = 0
$failed = 0

if ($Batch -gt 0) {
    # ------------------- Bulk mode -------------------
    $total = $rows.Count
    $index = 0
    while ($index -lt $total) {
        $chunkRows = Get-Chunk -Items $rows -Start $index -Count $Batch
        $payloads  = foreach ($r in $chunkRows) { New-AirportPayload -Row $r }

        $jsonArray = $payloads | ConvertTo-Json -Depth 5 -Compress

        $target = "{0} (count={1}, range={2}-{3})" -f $BulkApiUrl, $payloads.Count, $index, ($index + $payloads.Count - 1)
        if ($PSCmdlet.ShouldProcess($target, 'POST bulk')) {
            try {
                $null = Invoke-RestMethod -Method POST -Uri $BulkApiUrl `
                    -Headers $headers -ContentType 'application/json' -Body $jsonArray -ErrorAction Stop

                $sent += $payloads.Count
                Write-Host ("Posted batch: {0} items (total sent {1}/{2})" -f $payloads.Count, $sent, $total)
            }
            catch {
                $failed += $payloads.Count
                Write-Warning ("Bulk failed for range {0}-{1}: {2}" -f $index, ($index + $payloads.Count - 1), $_.Exception.Message)
            }
        }

        $index += $Batch
    }
}
else {
    # ------------------- Single-row mode -------------------
    foreach ($row in $rows) {
        $payload = New-AirportPayload -Row $row
        $json    = $payload | ConvertTo-Json -Depth 3 -Compress

        $target = "{0} (ident='{1}')" -f $ApiUrl, $payload.ident
        if ($PSCmdlet.ShouldProcess($target, 'POST')) {
            try {
                $null = Invoke-RestMethod -Method POST -Uri $ApiUrl `
                    -Headers $headers -ContentType 'application/json' -Body $json -ErrorAction Stop
                $sent++
                Write-Host ("Posted {0}: {1}" -f $sent, $payload.ident)
            }
            catch {
                $failed++
                Write-Warning ("Failed ident='{0}': {1}" -f $payload.ident, $_.Exception.Message)
            }
        }
    }
}

Write-Host ("Done. Sent={0} Failed={1}" -f $sent, $failed)
