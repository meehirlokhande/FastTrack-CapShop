# Run from CapShop folder: .\scripts\Generate-CoverageReport.ps1
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$sln = Join-Path $root "backend\CapShop.slnx"
$settings = Join-Path $root "tests\coverlet.runsettings"
$out = Join-Path $root "TestResults"

if (Test-Path $out) { Remove-Item $out -Recurse -Force }
dotnet test $sln -c Release --collect:"XPlat Code Coverage" --results-directory $out --settings $settings

dotnet tool restore
$cobertura = Get-ChildItem -Path $out -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1 -ExpandProperty FullName
if (-not $cobertura) { throw "No coverage.cobertura.xml found under $out" }

$reportDir = Join-Path $root "coverage-report"
if (Test-Path $reportDir) { Remove-Item $reportDir -Recurse -Force }
dotnet reportgenerator -reports:"$cobertura" -targetdir:"$reportDir" -reporttypes:Html

Write-Host "Open: $reportDir\index.html"
