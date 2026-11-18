$root = Join-Path $PSScriptRoot "src"

$projects = Get-ChildItem $root -Recurse -Filter *.csproj |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj)\\' -and
        $_.Name -notmatch 'Tests|IntegrationContracts' -and
        (Select-String -Path $_.FullName -Pattern 'Sdk="Microsoft.NET.Sdk.Web"' -Quiet)
    }

foreach ($p in $projects) {
    Write-Host "Starting $($p.BaseName)..."
    Start-Process powershell -ArgumentList @(
        "-NoExit", "-Command",
        "`$Host.UI.RawUI.WindowTitle = '$($p.BaseName)'; dotnet run --project `"$($p.FullName)`""
    )
    Start-Sleep -Seconds 2
}