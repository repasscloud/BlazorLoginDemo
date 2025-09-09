#!/usr/bin/env pwsh
# Requires PowerShell 7+

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$CsvPath,

    [Parameter(Mandatory=$true)]
    [string]$Ident
)

Set-StrictMode -Version Latest

# ----------------------- Helpers ----------------------------

function Convert-AirportType {
    [CmdletBinding()]
    param([string]$Value)
    switch (($Value ?? '').ToLower().Trim()) {
        "small_airport"  { return "SmallAirport" }
        "medium_airport" { return "MediumAirport" }
        "large_airport"  { return "LargeAiport" }   # enum name has this spelling
        "heliport"       { return "Heliport" }
        "seaplane_base"  { return "SeaplanePort" }
        "balloonport"    { return "BalloonPort" }
        "closed"         { return "Closed" }
        default          { return "Unknown" }
    }
}

function ConvertTo-Boolean {
    param([AllowNull()]$InputObject)
    if ($null -eq $InputObject) { return $false }
    $s = [string]$InputObject
    $s = $s.Trim().ToLowerInvariant()
    return @('y','yes','true','1') -contains $s
}

function ConvertTo-Int {
    param([AllowNull()]$InputObject, [int]$Default = 0)
    if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
    try { return [int]$InputObject } catch { return $Default }
}

function ConvertTo-Double {
    param([AllowNull()]$InputObject, [double]$Default = 0.0)
    if ([string]::IsNullOrWhiteSpace([string]$InputObject)) { return $Default }
    try { return [double]$InputObject } catch { return $Default }
}

function New-AirportPayload {
    param([Parameter(Mandatory=$true)][pscustomobject]$Row)

    [ordered]@{
        id               = 0
        ident            = ([string]$Row.ident).Trim()
        type             = Convert-AirportType $Row.type
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
}

# -------------------------- Main --------------------------------------------

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV file not found: $CsvPath"
}

$row = Import-Csv -Path $CsvPath | Where-Object { $_.ident -eq $Ident }

if (-not $row) {
    Write-Warning "No row found with ident='$Ident'"
    exit 1
}

$payload = New-AirportPayload -Row $row
$json    = $payload | ConvertTo-Json -Depth 3 -Compress

Write-Host $json
