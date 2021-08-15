

$ProjectName = "MockWebApi"
$ProjectFileName = ".\$ProjectName\$ProjectName.csproj"


function GetProjectVersion($ProjectFileName)
{
    $ProjectFileXml = ""
    $ProjectFileXml = [Xml] (Get-Content "$ProjectFileName")
    $PackageVersion = ""
    $PackageVersion = ($ProjectFileXml.Project.PropertyGroup.Version).trim()

    $PackageVersion
}


Push-Location

Set-Location $PSScriptRoot\..


$PackageVersion = GetProjectVersion $ProjectFileName

Write-Host "DOCKER_CONTAINER_IMAGE_VERSION=$PackageVersion"

Pop-Location