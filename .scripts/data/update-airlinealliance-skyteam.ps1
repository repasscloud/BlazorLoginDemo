# update-airlinealliance-oneworld.ps1
# PUTs JSON literal "1" to each airline's /alliance endpoint

$BaseUri     = 'http://localhost:8090'
$Alliance    = '3'                     # JSON literal
$Headers     = @{ Accept = '*/*' }
$ContentType = 'application/json'

$Airlines = @(
  'ar','am','ux','af','kl','ro','ci','mu','dl','ga','kq','ke','me','un','vs','mf','sk','sv'
)

$results = foreach ($airline in $Airlines) {
  $uri = "$BaseUri/api/v1/admin/kerneldata/airlines/$airline/alliance"
  $status = $null; $reason = $null; $ok = $false; $content = $null

  try {
    $resp   = Invoke-WebRequest -Method Put -Uri $uri -Headers $Headers -ContentType $ContentType -Body $Alliance
    $status = [int]$resp.StatusCode
    $reason = $resp.StatusDescription
    $content = $resp.Content
    $ok = ($status -ge 200 -and $status -lt 300)
  } catch {
    $ex = $_.Exception
    $resp = $ex.Response
    if ($resp) {
      $status = [int]$resp.StatusCode
      $reason = $resp.StatusDescription
      try {
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $content = $sr.ReadToEnd()
        $sr.Dispose()
      } catch { }
    } else {
      $status = -1
      $reason = $ex.Message
    }
  }

  [pscustomobject]@{
    Airline = $airline
    Status  = $status
    Ok      = $ok
    Reason  = $reason
    Content = $content
    Uri     = $uri
  }
}

# quick on-screen view
$results | Sort-Object Airline | Format-Table Airline, Status, Ok, Reason
# also emit the objects
$results
