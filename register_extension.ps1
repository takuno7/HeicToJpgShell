# register_extension.ps1

# Check for administrator privileges and re-launch if needed
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]"Administrator")) {
    Write-Host "Administrator privileges required. Relaunching with UAC prompt..." -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

$ProjectDir = $PSScriptRoot
if (-not $ProjectDir) { $ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path }

$DllPath = Join-Path $ProjectDir "HeicToJpgShell.dll"
$RegAsm = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"

Write-Host "--- Registering HEIC to JPG Shell Extension ---" -ForegroundColor Cyan

if (-not (Test-Path $DllPath)) {
    Write-Host "[ERROR] DLL not found: $DllPath" -ForegroundColor Red
    exit 1
}

# 1. Unregister previous versions
Write-Host "[1/3] Unregistering previous versions..."
& $RegAsm /u "$DllPath" /codebase 2>&1 | Out-Null

# 2. Register new version
Write-Host "[2/3] Registering COM server..."
$result = & $RegAsm "$DllPath" /codebase 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[WARNING] RegAsm returned errors:" -ForegroundColor Yellow
    $result | Write-Host
} else {
    Write-Host "  COM server registered successfully." -ForegroundColor Green
}

# 3. Setup Explorer associations (.heic)
Write-Host "[3/3] Setting up Explorer associations (.heic)..."
$Clsid = "{A3D7395B-883F-4D2A-9817-A9684C511B34}" # From HeicToJpgContextMenu.cs
$HandlerName = "HeicToJpgShell"

$RegPaths = @(
    "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\.heic\ShellEx\ContextMenuHandlers\$HandlerName",
    "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\.HEIC\ShellEx\ContextMenuHandlers\$HandlerName"
)

foreach ($path in $RegPaths) {
    if (-not (Test-Path $path)) {
        New-Item -Path $path -Force | Out-Null
    }
    Set-ItemProperty -Path $path -Name "(Default)" -Value $Clsid
    Write-Host "  Registered: $path" -ForegroundColor Green
}

Write-Host "--- Registration Complete! ---" -ForegroundColor Green
Write-Host "Please restart Explorer or right-click a .heic file to test."
