#!/usr/bin/env pwsh
# Requires: PowerShell 7+

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Low')]
param(
    [string]$CsvPath = "./data/airports.csv",
    [string]$ApiUrl  = "http://localhost:8090/api/v1/kerneldata/airport-info",
    [string]$ApiKey  = "abc123"
)

Set-StrictMode -Version Latest

# ----------------------- Helpers (approved verbs) ----------------------------

function ConvertTo-Boolean {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false, ValueFromPipeline=$true)]
        [AllowNull()]$InputObject
    )
    process {
        if ($null -eq $InputObject) { return $false }
        $s = [string]$InputObject
        $s = $s.Trim().ToLowerInvariant()
        return @('y','yes','true','1') -contains $s
    }
}

function ConvertTo-Int {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false, ValueFromPipeline=$true)]
        [AllowNull()]$InputObject,
        [int]$Default = 0
    )
    process {
        if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
        try { return [int]$InputObject } catch { return $Default }
    }
}

function ConvertTo-Double {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false, ValueFromPipeline=$true)]
        [AllowNull()]$InputObject,
        [double]$Default = 0.0
    )
    process {
        if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
        try { return [double]$InputObject } catch { return $Default }
    }
}

function New-AirportPayload {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [pscustomobject]$Row
    )
    # NOTE:
    # - id is always 0
    # - continent and isoCountry are sent as strings; server will map to enums
    # - type is passed through as-is (string like "small_airport"). If you later
    #   enforce an enum int, add a mapping here before returning.
    $payload = [ordered]@{
        id               = 0
        ident            = ([string]$Row.ident).Trim()
        type             = ([string]$Row.type).Trim()
        name             = ([string]$Row.name).Trim()
        latitudeDeg      = (ConvertTo-Double $Row.latitude_deg)
        longitudeDeg     = (ConvertTo-Double $Row.longitude_deg)
        elevationFt      = (ConvertTo-Int    $Row.elevation_ft)
        continent        = ([string]$Row.continent).Trim()
        isoCountry       = ([string]$Row.iso_country).Trim()
        isoRegion        = ([string]$Row.iso_region).Trim()
        municipality     = ([string]$Row.municipality).Trim()
        scheduledService = (ConvertTo-Boolean $Row.scheduled_service)
        gpsCode          = ([string]$Row.gps_code).Trim()
        iataCode         = ([string]$Row.iata_code).Trim()
        localCode        = ([string]$Row.local_code).Trim()
    }
    return [pscustomobject]$payload
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
$sent = 0
$failed = 0

foreach ($row in $rows) {
    $payload = New-AirportPayload -Row $row
    $json = $payload | ConvertTo-Json -Depth 3 -Compress

    $target = "{0} (ident='{1}')" -f $ApiUrl, $payload.ident
    if ($PSCmdlet.ShouldProcess($target, 'POST')) {
        try {
            $null = Invoke-RestMethod -Method POST -Uri $ApiUrl `
                -Headers $headers -ContentType 'application/json' -Body $json `
                -ErrorAction Stop
            $sent++
            Write-Host ("Posted {0}: {1}" -f $sent, $payload.ident)
        }
        catch {
            $failed++
            Write-Warning ("Failed ({0}) ident='{1}': {2}" -f $failed, $payload.ident, $_.Exception.Message)
        }
    }
}

Write-Host ("Done. Sent={0} Failed={1}" -f $sent, $failed)
