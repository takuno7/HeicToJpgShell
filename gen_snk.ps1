$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider(1024)
[System.IO.File]::WriteAllBytes((Join-Path $PSScriptRoot "key.snk"), $rsa.ExportCspBlob($true))
Write-Host "key.snk generated successfully."

