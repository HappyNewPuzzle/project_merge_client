param([switch]$RequireProductionUrl)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$assets = Join-Path $project 'Assets'

# 관리자 인증, 인증서 우회, 평문 토큰 저장이 제품 코드에 들어오면 배포 전에 즉시 실패시킵니다.
$forbidden = 'X-Admin-Key|class\s+\w*CertificateHandler|PlayerPrefs\.(?:SetString|GetString)|Debug\.Log.*(?:accessToken|refreshToken|guestToken)'
& rg -n $forbidden $assets -g '*.cs' -g '*.java' -g '*.mm'
if ($LASTEXITCODE -eq 0) { throw '제품 코드에서 금지된 보안 패턴을 발견했습니다.' }
if ($LASTEXITCODE -gt 1) { throw '보안 스캔을 실행하지 못했습니다.' }

if ($RequireProductionUrl) {
    $url = $env:MERGEGAME_PRODUCTION_BASE_URL
    if ([string]::IsNullOrWhiteSpace($url) -or -not $url.StartsWith('https://') -or $url.EndsWith('.invalid')) {
        throw '실제 HTTPS MERGEGAME_PRODUCTION_BASE_URL이 필요합니다.'
    }
}
Write-Output 'Production security checks passed.'
