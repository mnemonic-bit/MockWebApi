

$DockerRepoUrl = "www.dockerhub.com"
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

Copy-Item -Path $PSScriptRoot\Dockerfile -Destination $PSScriptRoot\..

Set-Location $PSScriptRoot\..


$PackageVersion = GetProjectVersion $ProjectFileName
$DockerImageTag = "$($DockerRepoUrl)/$($ProjectName.ToLower()):$($PackageVersion)"

Write-Host "Building Docker image for project file name: '$ProjectFileName'"
Write-Host "The Docker image tag is: '$DockerImageTag'"

# Call Docker to build the container.
docker build --no-cache -t $DockerImageTag .


Pop-Location