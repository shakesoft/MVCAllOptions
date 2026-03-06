param ($version='latest')

$currentFolder = $PSScriptRoot
$slnFolder = Join-Path $currentFolder "../../"

Write-Host "********* BUILDING DbMigrator *********" -ForegroundColor Green
$dbMigratorFolder = Join-Path $slnFolder "src/MVCAllOptions.DbMigrator"
Set-Location $dbMigratorFolder
dotnet publish -c Release
docker build -f Dockerfile.local -t mvcalloptions-db-migrator:$version .




Write-Host "********* BUILDING Web Application *********" -ForegroundColor Green
$webFolder = Join-Path $slnFolder "src/MVCAllOptions.Web"
Set-Location $webFolder
dotnet publish -c Release
docker build -f Dockerfile.local -t mvcalloptions-web:$version .


Write-Host "********* BUILDING Public Web Application *********" -ForegroundColor Green
$webPublicFolder = Join-Path $slnFolder "src/MVCAllOptions.Web.Public"
Set-Location $webPublicFolder
dotnet publish -c Release
docker build -f Dockerfile.local -t mvcalloptions-web-public:$version .




### ALL COMPLETED
Write-Host "COMPLETED" -ForegroundColor Green
Set-Location $currentFolder
exit $LASTEXITCODE