param(
	[Parameter(Mandatory = $true)]
	[string]$BaseUrl,

	[int]$MaxWaitSeconds = 300,

	[string]$OutputPath = "specs/005-azure-sql-managed-identity/evidence/startup-readiness.json"
)

$start = Get-Date
$deadline = $start.AddSeconds($MaxWaitSeconds)
$healthUrl = "$($BaseUrl.TrimEnd('/'))/health"
$ready = $false
$attempts = 0

while ((Get-Date) -lt $deadline) {
	$attempts++
	try {
		$response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 15
		if ($response.StatusCode -eq 200) {
			$ready = $true
			break
		}
	}
	catch {
		Start-Sleep -Seconds 5
	}
}

$end = Get-Date
$elapsed = [Math]::Round(($end - $start).TotalSeconds, 2)
$pass = $ready -and $elapsed -le $MaxWaitSeconds

$dir = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($dir)) {
	New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

$result = [PSCustomObject]@{
	baseUrl = $BaseUrl
	healthUrl = $healthUrl
	startedUtc = $start.ToUniversalTime().ToString("o")
	finishedUtc = $end.ToUniversalTime().ToString("o")
	elapsedSeconds = $elapsed
	maxWaitSeconds = $MaxWaitSeconds
	attempts = $attempts
	readinessReached = $ready
	pass = $pass
}

$result | ConvertTo-Json -Depth 5 | Set-Content -Path $OutputPath -Encoding UTF8

if (-not $pass) {
	throw "Startup readiness failed. Healthy response was not observed within $MaxWaitSeconds seconds."
}
