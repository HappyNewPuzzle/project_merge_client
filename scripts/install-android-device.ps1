param([string]$AdbPath,[string]$ApkPath = "$PSScriptRoot\..\Builds\Android\ProjectMerge.apk")
$ErrorActionPreference = 'Stop'
if (-not (Test-Path $AdbPath)) { throw 'adb not found.' }
$devices = & $AdbPath devices | Select-String "\tdevice$"
if (@($devices).Count -ne 1) { throw "Exactly one authorized Android device is required; found $(@($devices).Count)." }
if (-not (Test-Path $ApkPath)) { throw 'APK not found.' }
& $AdbPath install -r $ApkPath
if ($LASTEXITCODE -ne 0) { throw 'APK install failed.' }
& $AdbPath shell monkey -p com.happynewpuzzle.projectmerge 1
if ($LASTEXITCODE -ne 0) { throw 'App launch failed.' }

