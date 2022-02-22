

$ProjectName = "MockWebApi"
$ProjectFileName = ".\$ProjectName\$ProjectName.csproj"


function GetProjectVersion($ProjectFileName)
{
    Write-Host "file name '$($ProjectFileName)'"

    $ProjectFileXml = ""
    $ProjectFileXml = [Xml] (Get-Content "$ProjectFileName")
    $PackageVersion = ""
    $PackageVersion = ($ProjectFileXml.Project.PropertyGroup[0].Version).trim()

    $PackageVersion
}


Set-Location $PSScriptRoot\..

$PackageVersion = GetProjectVersion $ProjectFileName

"DOCKER_CONTAINER_IMAGE_VERSION=$PackageVersion"
