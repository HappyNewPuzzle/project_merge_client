param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [string[]]$Artifacts = @('Builds/Windows/ProjectMerge.exe','Builds/Android/ProjectMerge.apk')
)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$dirty = git -C $project status --porcelain
if ($dirty) { throw 'Release requires a clean Git worktree.' }
& (Join-Path $PSScriptRoot 'verify-client.ps1') -UnityPath $UnityPath -RequireDeploymentUrls
$lines = foreach ($relative in $Artifacts) {
    $path = Join-Path $project $relative
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing artifact: $relative" }
    $hash = Get-FileHash -Algorithm SHA256 -LiteralPath $path
    "$($hash.Hash.ToLowerInvariant())  $relative"
}
$output = Join-Path $project 'Logs\ArtifactChecksums.sha256'
$lines | Set-Content -Encoding ASCII $output
Write-Output "Release readiness passed. Checksums: $output"

