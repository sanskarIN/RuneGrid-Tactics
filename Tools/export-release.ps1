param(
    [ValidateSet("windows", "linux", "android")]
    [string]$Target = "windows",
    [string]$Godot = $env:GODOT4
)

$Project = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Godot)) { $Godot = "godot" }

switch ($Target) {
    "windows" { $Preset = "Windows Desktop"; $Output = "build/windows/RuneGridTactics.exe" }
    "linux"   { $Preset = "Linux/X11"; $Output = "build/linux/RuneGridTactics.x86_64" }
    "android" { $Preset = "Android"; $Output = "build/android/RuneGridTactics.apk" }
}

New-Item -ItemType Directory -Force -Path (Join-Path $Project (Split-Path -Parent $Output)) | Out-Null
& $Godot --headless --path $Project --export-release $Preset $Output
if ($LASTEXITCODE -ne 0) { throw "Godot export failed with exit code $LASTEXITCODE." }
Write-Host "Export complete: $(Join-Path $Project $Output)"
