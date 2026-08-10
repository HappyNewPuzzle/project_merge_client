param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [switch]$RequireDeploymentUrls
)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $project 'Logs'
New-Item -ItemType Directory -Force -Path $logs | Out-Null

# Fail fast on dangerous usage rather than matching harmless DTO field names.
$forbidden = 'X-Admin-Key|CertificateHandler|Debug\.Log.*(?:token|Token)|PlayerPrefs\.(?:SetString|GetString)'
& rg -n $forbidden (Join-Path $project 'Assets') -g '*.cs' -g '*.java' -g '*.mm'
if ($LASTEXITCODE -eq 0) { throw 'Forbidden admin key, certificate bypass, or token persistence code found.' }
if ($LASTEXITCODE -gt 1) { throw 'Security scan could not run.' }

if ($RequireDeploymentUrls) {
    & rg -n 'example\.invalid' (Join-Path $project 'Assets\MergeGame\Runtime\Configuration')
    if ($LASTEXITCODE -eq 0) { throw 'Staging or production URL is still a placeholder.' }
}

function Invoke-Unity([string[]]$Arguments, [string]$Name) {
    $process = Start-Process -FilePath $UnityPath -ArgumentList $Arguments -Wait -PassThru -WindowStyle Hidden
    if ($process.ExitCode -ne 0) { throw "$Name failed: Unity exit code $($process.ExitCode)" }
}

Invoke-Unity @('-batchmode','-nographics','-quit','-projectPath',$project,'-logFile',(Join-Path $logs 'CiCompile.log')) 'Compile'
Invoke-Unity @('-batchmode','-nographics','-projectPath',$project,'-runTests','-testPlatform','EditMode','-testResults',(Join-Path $logs 'CiEditMode.xml'),'-logFile',(Join-Path $logs 'CiEditMode.log')) 'EditMode'
Invoke-Unity @('-batchmode','-nographics','-projectPath',$project,'-runTests','-testPlatform','PlayMode','-testResults',(Join-Path $logs 'CiPlayMode.xml'),'-logFile',(Join-Path $logs 'CiPlayMode.log')) 'PlayMode'

foreach ($suite in 'CiEditMode','CiPlayMode') {
    [xml]$result = Get-Content -Raw (Join-Path $logs "$suite.xml")
    $run = $result.'test-run'
    if ($run.failed -ne '0') { throw "$suite tests failed" }
    Write-Output "$suite passed=$($run.passed) skipped=$($run.skipped)"
}
