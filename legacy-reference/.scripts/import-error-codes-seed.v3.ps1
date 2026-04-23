#!/usr/bin/env pwsh
# Requires: PowerShell 7+
# Purpose : Import error codes from CSV and POST to admin API.

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Low')]
param(
    [Parameter(Mandatory=$false)]
    [string]$CsvPath = $(Join-Path -Path $PSScriptRoot -ChildPath "data/error-codes-seed.csv"),

    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "http://localhost:8090/api/v1/admin/errorcodes",

    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "abc123",

    [Parameter(Mandatory=$false)]
    [string]$LogPath = $(Join-Path -Path $PSScriptRoot -ChildPath "logs/import-error-codes-seed.log")
)

# ----------------------------- Setup -----------------------------------------

$ErrorActionPreference = 'Stop'

# Ensure log directory exists
$logDir = Split-Path -Path $LogPath -Parent
if (-not [string]::IsNullOrWhiteSpace($logDir) -and -not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

function Write-Log {
    param(
        [Parameter(Mandatory=$true)][string]$Message,
        [ValidateSet('INFO','WARN','ERROR','DEBUG')][string]$Level = 'INFO'
    )
    $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fffK")
    $line = "[{0}] [{1}] {2}" -f $ts, $Level, $Message
    Write-Host $line
    if ($LogPath) { Add-Content -Path $LogPath -Value $line }
}

if (-not (Test-Path -LiteralPath $CsvPath)) {
    Write-Log "CSV not found: $CsvPath" 'ERROR'
    exit 1
}

# ---------------------------- Helpers ----------------------------------------

function New-ErrorCodePayload {
    <#
      .SYNOPSIS
        Convert a CSV row into request payload for ErrorCodeUnified create.
      .PARAMETER Row
        CSV row with: ErrorCode, Title, Message, Resolution, IsClientFacing, IsInternalFacing
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory=$true)][pscustomobject]$Row)

    # Normalize booleans via safe conversion
    $isClient = [System.Convert]::ToBoolean($Row.IsClientFacing)
    $isInternal = [System.Convert]::ToBoolean($Row.IsInternalFacing)

    $payload = [ordered]@{
        ErrorCode         = [string]$Row.ErrorCode
        Title             = [string]$Row.Title
        Message           = [string]$Row.Message
        Resolution        = ($Row.Resolution ?? $null)
        ContactSupportLink= [string]([string]::IsNullOrWhiteSpace($Row.ContactSupportLink) ? '' : $Row.ContactSupportLink)
        IsClientFacing    = $isClient
        IsInternalFacing  = $isInternal
    }

    # Return PSCustomObject for pipeline friendliness
    return [pscustomobject]$payload
}

# Validate CSV headers
$required = @('ErrorCode','Title','Message','Resolution','IsClientFacing','IsInternalFacing')
# Optional columns present in some exports
$optional = @('ContactSupportLink')
$firstLine = (Get-Content -Path $CsvPath -TotalCount 1 -Encoding UTF8)
foreach ($col in $required) {
    if ($firstLine -notmatch "(^|,)\s*${col}\s*(,|$)") {
        Write-Log "Missing required CSV column: $col" 'ERROR'
        exit 1
    }
}

# Prepare headers
$headers = @{'accept'='application/json'}
if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
    $headers['X-Api-Key'] = $ApiKey
}

# --------------------------- Import & POST -----------------------------------

$rows = Import-Csv -Path $CsvPath -Encoding UTF8
if (-not $rows -or $rows.Count -eq 0) {
    Write-Log "CSV contains no rows: $CsvPath" 'ERROR'
    exit 1
}

$sent = 0
$failed = 0
$skipped = 0

Write-Log ("Starting import. Rows={0} ApiUrl={1} CsvPath={2} WhatIf={3}" -f $rows.Count, $ApiUrl, $CsvPath, $PSCmdlet.WhatIfPreference)

foreach ($row in $rows) {
    # Build payload object
    $payload = New-ErrorCodePayload -Row $row
    if ([string]::IsNullOrWhiteSpace($payload.ErrorCode) -or [string]::IsNullOrWhiteSpace($payload.Title)) {
        Write-Log ("Skipping row with missing ErrorCode/Title: {0}" -f ($row | ConvertTo-Json -Compress -Depth 3)) 'WARN'
        $skipped++
        continue
    }

    $ident = $payload.ErrorCode
    $json = $payload | ConvertTo-Json -Depth 5 -Compress

    if ($PSCmdlet.ShouldProcess($ident, "POST $ApiUrl")) {
        try {
            $resp = Invoke-WebRequest -Method POST -Uri $ApiUrl -Headers $headers -ContentType 'application/json' -Body $json -ErrorAction Stop
            $sent++
            Write-Log ("POST OK ident='{0}' Status={1}" -f $ident, $resp.StatusCode) 'INFO'
        }
        catch {
            $failed++
            $detail = $_.ErrorDetails?.Message
            if (-not $detail -and $_.Exception.Response -and $_.Exception.Response.GetResponseStream) {
                try {
                    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                    $detail = $reader.ReadToEnd()
                } catch {}
            }
            Write-Log ("POST FAILED ident='{0}' :: {1}" -f $ident, ($detail ?? $_.Exception.Message)) 'ERROR'
        }
    } else {
        $skipped++
        Write-Log ("WHATIF skip ident='{0}' :: {1}" -f $ident, $json) 'DEBUG'
    }
}

Write-Log ("Done. Sent={0} Failed={1} Skipped={2}" -f $sent, $failed, $skipped) 'INFO'
exit ([int]([bool]$failed))
