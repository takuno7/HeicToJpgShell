# unregister_extension.ps1

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
$HandlerName = "HeicToJpgShell"

Write-Host "--- Unregistering HEIC to JPG Shell Extension ---" -ForegroundColor Cyan

if (-not (Test-Path $DllPath)) {
    Write-Host "[ERROR] DLL not found: $DllPath" -ForegroundColor Red
    exit 1
}

# 1. Remove registry entries
Write-Host "[1/2] Removing registry entries..."
$RegPaths = @(
    "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\.heic\ShellEx\ContextMenuHandlers\$HandlerName",
    "Registry::HKEY_CLASSES_ROOT\SystemFileAssociations\.HEIC\ShellEx\ContextMenuHandlers\$HandlerName"
)

foreach ($path in $RegPaths) {
    if (Test-Path $path) {
        Remove-Item -Path $path -Force
        Write-Host "  Removed: $path" -ForegroundColor Green
    } else {
        Write-Host "  Not found (skipping): $path" -ForegroundColor Gray
    }
}

# 2. Unregister COM server
Write-Host "[2/2] Unregistering COM server..."
$result = & $RegAsm /u "$DllPath" /codebase 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[WARNING] RegAsm returned errors:" -ForegroundColor Yellow
    $result | Write-Host
} else {
    Write-Host "  COM server unregistered successfully." -ForegroundColor Green
}

Write-Host "--- Unregistration Complete ---" -ForegroundColor Green
Write-Host "Note: Explorer may still have cached icons. Run 'taskkill /f /im explorer.exe; start explorer.exe' if needed."
