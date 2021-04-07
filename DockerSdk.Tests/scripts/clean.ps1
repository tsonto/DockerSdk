Set-StrictMode -Version 3
$ErrorActionPreference = "Continue"

Write-Output "Cleaning up docker environment"
Push-Location $PSScriptRoot

# Pull in definitions.
. ./definitions.ps1

# Shut down containers.
foreach ($id in docker container ls --filter="name=ddnt" --quiet)
{
    docker container stop $id --time 1
}

# Remove containers.
foreach ($id in docker container ls --filter="name=ddnt" --quiet --all)
{
    docker container rm $id --force --volumes
}

# Remove images in reverse order of how they were created.
$tags = $imageDefinitions.Name
$tags = [System.Linq.Enumerable]::Reverse([string[]] $tags) 
foreach ($tag in $tags)
{
    $id = docker image ls ddnt:$tag --quiet --no-trunc
    if ($id)
    {
        docker image rm $id --force
    }
}

# Remove all other images that are clearly for these tests.
$ids = [string[]]@()
$ids += @( docker image ls ddnt --quiet )
$ids += @( docker image ls emdot/emdotsdk --quiet )
$ids += @( docker image ls emdot/emdotsdk-private --quiet )
foreach ($id in $ids)
{
    docker image rm --force $id
}

# Remove ID files.
Remove-Item **/*.id

Pop-Location
Write-Output "Cleanup complete"
