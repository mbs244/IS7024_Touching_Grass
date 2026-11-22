[CmdletBinding()]
param(
	[string]$BaseUrl = $env:SMOKE_TEST_URL,
	[string]$Email = $env:SMOKE_TEST_EMAIL,
	[string]$Password = $env:SMOKE_TEST_PASSWORD,
	[string]$ZipCode = $env:SMOKE_TEST_ZIP
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($BaseUrl) -or [string]::IsNullOrWhiteSpace($Email) -or
	[string]::IsNullOrWhiteSpace($Password) -or [string]::IsNullOrWhiteSpace($ZipCode)) {
	throw 'Smoke-test configuration is incomplete. Set protected SMOKE_TEST_URL, SMOKE_TEST_EMAIL, SMOKE_TEST_PASSWORD, and SMOKE_TEST_ZIP values.'
}

$BaseUrl = $BaseUrl.TrimEnd('/')
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Get-AntiForgeryToken([string]$Html) {
	$match = [regex]::Match($Html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
	if (-not $match.Success) { throw 'Smoke test could not find an anti-forgery token.' }
	return $match.Groups[1].Value
}

$health = Invoke-WebRequest -Uri "$BaseUrl/health" -WebSession $session -UseBasicParsing
if ($health.StatusCode -ne 200 -or $health.Content -notmatch 'Healthy') {
	throw 'Application readiness or database health check failed.'
}

$registerPage = Invoke-WebRequest -Uri "$BaseUrl/Account/Register" -WebSession $session -UseBasicParsing
$registerToken = Get-AntiForgeryToken $registerPage.Content
$register = Invoke-WebRequest -Uri "$BaseUrl/Account/Register" -Method Post -WebSession $session -UseBasicParsing -Body @{
	Email = $Email
	Password = $Password
	ConfirmPassword = $Password
	DefaultZipCode = $ZipCode
	'__RequestVerificationToken' = $registerToken
}
if ($register.StatusCode -notin @(200, 302)) { throw 'Registration smoke test failed.' }

$myPlants = Invoke-WebRequest -Uri "$BaseUrl/MyPlants" -WebSession $session -UseBasicParsing -MaximumRedirection 0
if ($myPlants.StatusCode -ne 200 -or $myPlants.Content -notmatch '<h1>My plants</h1>') { throw 'Authenticated protected-page smoke test failed.' }

$logoutPage = Invoke-WebRequest -Uri "$BaseUrl/Account/Logout" -WebSession $session -UseBasicParsing
$logoutToken = Get-AntiForgeryToken $logoutPage.Content
$logout = Invoke-WebRequest -Uri "$BaseUrl/Account/Logout" -Method Post -WebSession $session -UseBasicParsing -Body @{
	'__RequestVerificationToken' = $logoutToken
}
if ($logout.StatusCode -notin @(200, 302)) { throw 'Logout smoke test failed.' }

$unauthenticated = Invoke-WebRequest -Uri "$BaseUrl/MyPlants" -WebSession $session -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($unauthenticated.StatusCode -notin @(302, 401, 403)) { throw 'Protected-page denial after logout failed.' }

Write-Output 'Smoke test passed: readiness, registration, authenticated access, logout, and protected-page denial.'
