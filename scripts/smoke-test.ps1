[CmdletBinding()]
param(
	[string]$BaseUrl = $env:SMOKE_TEST_URL,
	[string]$Email = $env:SMOKE_TEST_EMAIL,
	[string]$Password = $env:SMOKE_TEST_PASSWORD,
	[string]$ZipCode = $env:SMOKE_TEST_ZIP,
	[string]$PlantName = $(if ([string]::IsNullOrWhiteSpace($env:SMOKE_TEST_PLANT)) { 'Toyon' } else { $env:SMOKE_TEST_PLANT })
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($BaseUrl) -or [string]::IsNullOrWhiteSpace($Email) -or
	[string]::IsNullOrWhiteSpace($Password) -or [string]::IsNullOrWhiteSpace($ZipCode) -or [string]::IsNullOrWhiteSpace($PlantName)) {
	throw 'Smoke-test configuration is incomplete. Set protected SMOKE_TEST_URL, SMOKE_TEST_EMAIL, SMOKE_TEST_PASSWORD, and SMOKE_TEST_ZIP values.'
}

$BaseUrl = $BaseUrl.TrimEnd('/')

function New-RunScopedEmail([string]$baseEmail, [string]$suffix) {
	if ($baseEmail -notmatch '^(?<local>[^@]+)@(?<domain>[^@]+)$') {
		throw 'SMOKE_TEST_EMAIL must be a valid email address.'
	}

	$runId = ([DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString() + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
	return "$($Matches.local)+$runId-$suffix@$($Matches.domain)"
}

function Get-AntiForgeryToken([string]$Html) {
	$match = [regex]::Match($Html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
	if (-not $match.Success) { throw 'Smoke test could not find an anti-forgery token.' }
	return $match.Groups[1].Value
}

function Register-User(
	[Microsoft.PowerShell.Commands.WebRequestSession]$Session,
	[string]$UserEmail,
	[string]$UserPassword,
	[string]$UserZipCode) {
	$registerPage = Invoke-WebRequest -Uri "$BaseUrl/Account/Register" -WebSession $Session -UseBasicParsing
	$registerToken = Get-AntiForgeryToken $registerPage.Content
	$register = Invoke-WebRequest -Uri "$BaseUrl/Account/Register" -Method Post -WebSession $Session -UseBasicParsing -Body @{
		Email = $UserEmail
		Password = $UserPassword
		ConfirmPassword = $UserPassword
		DefaultZipCode = $UserZipCode
		'__RequestVerificationToken' = $registerToken
	}
	if ($register.StatusCode -notin @(200, 302)) {
		throw "Registration smoke test failed for $UserEmail."
	}
}

function Login-User(
	[Microsoft.PowerShell.Commands.WebRequestSession]$Session,
	[string]$UserEmail,
	[string]$UserPassword) {
	$loginPage = Invoke-WebRequest -Uri "$BaseUrl/Account/Login" -WebSession $Session -UseBasicParsing
	$loginToken = Get-AntiForgeryToken $loginPage.Content
	$login = Invoke-WebRequest -Uri "$BaseUrl/Account/Login" -Method Post -WebSession $Session -UseBasicParsing -Body @{
		Email = $UserEmail
		Password = $UserPassword
		'__RequestVerificationToken' = $loginToken
	}
	if ($login.StatusCode -notin @(200, 302)) {
		throw "Login smoke test failed for $UserEmail."
	}
}

function Logout-User([Microsoft.PowerShell.Commands.WebRequestSession]$Session) {
	$logoutPage = Invoke-WebRequest -Uri "$BaseUrl/Account/Logout" -WebSession $Session -UseBasicParsing
	$logoutToken = Get-AntiForgeryToken $logoutPage.Content
	$logout = Invoke-WebRequest -Uri "$BaseUrl/Account/Logout" -Method Post -WebSession $Session -UseBasicParsing -Body @{
		'__RequestVerificationToken' = $logoutToken
	}
	if ($logout.StatusCode -notin @(200, 302)) { throw 'Logout smoke test failed.' }
}

function Save-PlantRecommendation(
	[Microsoft.PowerShell.Commands.WebRequestSession]$Session,
	[string]$UserZipCode,
	[string]$UserPlantName) {
	$indexPage = Invoke-WebRequest -Uri "$BaseUrl/" -WebSession $Session -UseBasicParsing
	$indexToken = Get-AntiForgeryToken $indexPage.Content
	$result = Invoke-WebRequest -Uri "$BaseUrl/" -Method Post -WebSession $Session -UseBasicParsing -Body @{
		ZipCode = $UserZipCode
		PlantName = $UserPlantName
		'__RequestVerificationToken' = $indexToken
	}
	if ($result.StatusCode -notin @(200, 302)) {
		throw "Plant recommendation/save smoke test failed for plant '$UserPlantName'."
	}
}

function Get-MyPlantsPage([Microsoft.PowerShell.Commands.WebRequestSession]$Session) {
	$myPlants = Invoke-WebRequest -Uri "$BaseUrl/MyPlants" -WebSession $Session -UseBasicParsing -MaximumRedirection 0
	if ($myPlants.StatusCode -ne 200 -or $myPlants.Content -notmatch '<h1>My plants</h1>') {
		throw 'Authenticated protected-page smoke test failed.'
	}
	return $myPlants
}

function Get-SavedPlantId([string]$Html) {
	$idMatch = [regex]::Match($Html, '/MyPlants/Details(?:\?id=|/)(\d+)')
	if (-not $idMatch.Success) {
		$idMatch = [regex]::Match($Html, 'asp-route-id="(\d+)"')
	}
	if (-not $idMatch.Success) {
		throw 'Smoke test could not determine a saved plant id from MyPlants.'
	}
	return [int]$idMatch.Groups[1].Value
}

function Toggle-Favorite(
	[Microsoft.PowerShell.Commands.WebRequestSession]$Session,
	[int]$SavedPlantId) {
	$myPlants = Get-MyPlantsPage $Session
	$toggleToken = Get-AntiForgeryToken $myPlants.Content
	$toggle = Invoke-WebRequest -Uri "$BaseUrl/MyPlants?handler=ToggleFavorite" -Method Post -WebSession $Session -UseBasicParsing -Body @{
		id = $SavedPlantId
		'__RequestVerificationToken' = $toggleToken
	}
	if ($toggle.StatusCode -notin @(200, 302)) {
		throw 'Favorite toggle smoke test failed.'
	}
}

function Assert-UserHasSavedPlant(
	[Microsoft.PowerShell.Commands.WebRequestSession]$Session,
	[int]$SavedPlantId) {
	$myPlants = Get-MyPlantsPage $Session
	if ($myPlants.Content -notmatch "/MyPlants/Details(?:\?id=|/)$SavedPlantId(\D|$)") {
		throw "Saved plant id $SavedPlantId was not found in MyPlants."
	}

	if ($myPlants.Content -notmatch 'Remove .* from favorites') {
		throw 'Favorite persistence check failed: expected favorite action label not found.'
	}
}

$sessionA = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$sessionB = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$health = Invoke-WebRequest -Uri "$BaseUrl/health" -WebSession $sessionA -UseBasicParsing
if ($health.StatusCode -ne 200 -or $health.Content -notmatch 'Healthy') {
	throw 'Application readiness or database health check failed.'
}

$emailA = New-RunScopedEmail $Email 'a'
$emailB = New-RunScopedEmail $Email 'b'

Register-User -Session $sessionA -UserEmail $emailA -UserPassword $Password -UserZipCode $ZipCode
Register-User -Session $sessionB -UserEmail $emailB -UserPassword $Password -UserZipCode $ZipCode

# Save and favorite a plant under user A.
Save-PlantRecommendation -Session $sessionA -UserZipCode $ZipCode -UserPlantName $PlantName
$myPlantsA = Get-MyPlantsPage $sessionA
$savedPlantId = Get-SavedPlantId $myPlantsA.Content
Toggle-Favorite -Session $sessionA -SavedPlantId $savedPlantId

# Verify logout/login and persistence for user A.
Logout-User -Session $sessionA
$unauthenticated = Invoke-WebRequest -Uri "$BaseUrl/MyPlants" -WebSession $sessionA -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($unauthenticated.StatusCode -notin @(302, 401, 403)) { throw 'Protected-page denial after logout failed.' }

Login-User -Session $sessionA -UserEmail $emailA -UserPassword $Password
Assert-UserHasSavedPlant -Session $sessionA -SavedPlantId $savedPlantId

# Cross-user read/mutation checks from user B against user A plant id.
$crossRead = Invoke-WebRequest -Uri "$BaseUrl/MyPlants/Details?id=$savedPlantId" -WebSession $sessionB -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($crossRead.StatusCode -notin @(404, 403, 302)) {
	throw "Cross-user read isolation check failed for saved plant id $savedPlantId."
}

Toggle-Favorite -Session $sessionB -SavedPlantId $savedPlantId
Assert-UserHasSavedPlant -Session $sessionA -SavedPlantId $savedPlantId

Write-Output 'Smoke test passed: readiness, run-scoped dual-user registration, authenticated access, save/favorite persistence across relogin, protected-page denial after logout, and cross-user read/mutation isolation.'
