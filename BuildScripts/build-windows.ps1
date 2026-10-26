param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Version,

    [Parameter(Position = 1)]
    [ValidateSet("win-x64", "win-arm64")]
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"
$Version = $Version -replace '^[vV]', ''
if ($Version -notmatch '^\d+\.\d+\.\d+(\.\d+)?(-[0-9A-Za-z.-]+)?(\+[0-9A-Za-z.-]+)?$') {
    throw "Version must be a 3- or 4-part version, for example 1.0.0.42."
}

$VpkVersion = "1.2.161"
$PackId = "SC2CCME"
$AppName = "SC2CCME"
$RepositoryRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $RepositoryRoot "ModManager.StarCraft.Application/ModManager.StarCraft.Application.csproj"
$ArtifactDirectory = Join-Path $PSScriptRoot "artifacts/$RuntimeIdentifier"
$PublishDirectory = Join-Path $ArtifactDirectory "publish"
$ReleaseDirectory = Join-Path $ArtifactDirectory "releases"
$VpkToolDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "sc2ccme-vpk-$([guid]::NewGuid().ToString('N'))"

Remove-Item $ArtifactDirectory -Recurse -Force -ErrorAction SilentlyContinue
New-Item $PublishDirectory -ItemType Directory -Force | Out-Null
New-Item $ReleaseDirectory -ItemType Directory -Force | Out-Null

try {
    dotnet publish $ProjectPath `
        --configuration Release `
        --runtime $RuntimeIdentifier `
        --self-contained true `
        --output $PublishDirectory `
        "-p:Version=$Version"
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

    dotnet tool install vpk --tool-path $VpkToolDirectory --version $VpkVersion
    if ($LASTEXITCODE -ne 0) { throw "Installing VPK $VpkVersion failed with exit code $LASTEXITCODE." }

    & (Join-Path $VpkToolDirectory "vpk.exe") pack `
        --packId $PackId `
        --packVersion $Version `
        --packTitle $AppName `
        --packDir $PublishDirectory `
        --outputDir $ReleaseDirectory `
        --mainExe "$AppName.exe"
    if ($LASTEXITCODE -ne 0) { throw "VPK packaging failed with exit code $LASTEXITCODE." }
}
finally {
    Remove-Item $VpkToolDirectory -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Velopack packages created in $ReleaseDirectory"