# build.ps1
$ProjectDir = $PSScriptRoot
if (-not $ProjectDir) {
    $ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
}

# Search for MSBuild path (using vswhere)
$MsBuildExe = "MSBuild.exe" # Default
$ProgramFilesX86 = [System.Environment]::GetEnvironmentVariable('ProgramFiles(x86)')
if (Test-Path ("$ProgramFilesX86\Microsoft Visual Studio\Installer\vswhere.exe")) {
    $installPath = & ("$ProgramFilesX86\Microsoft Visual Studio\Installer\vswhere.exe") -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($installPath) {
        $MsBuildExe = Join-Path $installPath "MSBuild\Current\Bin\MSBuild.exe"
    }
}

if (-not (Test-Path $MsBuildExe)) {
    # Fallback to standard paths
    $paths = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($p in $paths) { if (Test-Path $p) { $MsBuildExe = $p; break } }
}

Write-Host "--- Build Script [Clean Unsigned + Restore] ---"

# 1. Clean folders
Write-Host "[1/3] Cleaning previous build..."
if (Test-Path (Join-Path $ProjectDir "bin")) { Remove-Item -Recurse -Force (Join-Path $ProjectDir "bin") }
if (Test-Path (Join-Path $ProjectDir "obj")) { Remove-Item -Recurse -Force (Join-Path $ProjectDir "obj") }

# 2. Restore packages
Write-Host "[2/3] Restoring packages..."
& $MsBuildExe (Join-Path $ProjectDir "HeicToJpgShell.csproj") -t:restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Restore failed!" -ForegroundColor Red
    return
}

# 3. Build project
Write-Host "[3/3] Building project in Release mode..."
& $MsBuildExe (Join-Path $ProjectDir "HeicToJpgShell.csproj") /p:Configuration=Release /t:Rebuild
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed!" -ForegroundColor Red
    return
}

Write-Host "--- Build Successful! ---"

# 4. Assemble Release Packages (Formerly assemble_release.ps1)
Write-Host "--- Assembling Release Packages ---"
$BinDir = Join-Path $ProjectDir "bin\Release\net48"
$FacadesDir = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Facades"

if (-not (Test-Path $FacadesDir)) {
    Write-Host "[!] Facades directory not found ($FacadesDir). Skipping facade copy (NuGet may have handled it)." -ForegroundColor Yellow
}
else {
    Write-Host "[*] Copying missing .NET Standard facades..."
    $facades = @("netstandard.dll", "System.Runtime.dll", "System.Collections.dll", "System.IO.dll", "System.Threading.Tasks.dll")
    foreach ($f in $facades) {
        $src = Join-Path $FacadesDir $f
        $dest = Join-Path $BinDir $f
        if (Test-Path $src) {
            if (-not (Test-Path $dest)) {
                Copy-Item $src $dest -Force
                Write-Host "  > Added $f"
            }
            else {
                Write-Host "  > $f already exists."
            }
        }
        else {
            Write-Host "  > [WARNING] Facade $f not found in $FacadesDir" -ForegroundColor Yellow
        }
    }
}

# 5. Check for key dependencies
Write-Host "[*] Verifying critical dependencies..."
$deps = @("Openize.Heic.Decoder.dll", "Openize.IsoBmff.dll", "MetadataExtractor.dll", "XmpCore.dll", "SharpShell.dll")
foreach ($d in $deps) {
    if (-not (Test-Path (Join-Path $BinDir $d))) {
        Write-Host "[ERROR] Missing critical dependency: $d" -ForegroundColor Red
    }
}

Write-Host "--- Assembly Complete ---"
Write-Host "Ready to test or register from $BinDir"

