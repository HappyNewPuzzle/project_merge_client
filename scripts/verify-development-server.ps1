param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [string]$BaseUrl = 'http://localhost:5158'
)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$health = Invoke-WebRequest -UseBasicParsing -Uri ($BaseUrl.TrimEnd('/') + '/health') -TimeoutSec 10
if ($health.StatusCode -ne 200) { throw 'Development server health check failed.' }
$previous = $env:MERGEGAME_INTEGRATION_BASE_URL
try {
    $env:MERGEGAME_INTEGRATION_BASE_URL = $BaseUrl
    $args = @('-batchmode','-nographics','-projectPath',$project,'-runTests','-testPlatform','PlayMode',
        '-testFilter','MergeGame.Client.Tests.PlayMode.DevelopmentServerIntegrationTests',
        '-testResults',(Join-Path $project 'Logs\ServerIntegration.xml'),'-logFile',(Join-Path $project 'Logs\ServerIntegration.log'))
    $process = Start-Process -FilePath $UnityPath -ArgumentList $args -Wait -PassThru -WindowStyle Hidden
    if ($process.ExitCode -ne 0) { throw "Unity integration test failed: $($process.ExitCode)" }
} finally { $env:MERGEGAME_INTEGRATION_BASE_URL = $previous }

