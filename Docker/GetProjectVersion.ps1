

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


Set-Location $PSScriptRoot\..

$PackageVersion = GetProjectVersion $ProjectFileName

"DOCKER_CONTAINER_IMAGE_VERSION=$PackageVersion"
