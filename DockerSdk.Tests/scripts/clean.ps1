Set-StrictMode -Version 3
$ErrorActionPreference = "Continue"

Write-Output "Cleaning up docker environment"
Push-Location $PSScriptRoot

# Pull in definitions.
. ./definitions.ps1

# Shut down containers.
foreach ($id in docker container ls --filter="name=ddnt" --quiet --no-trunc)
{
    docker container stop $id --time 1
}

# Remove containers.
foreach ($id in docker container ls --filter="name=ddnt" --quiet --no-trunc --all)
{
    docker container rm $id --force --volumes
}

# Remove networks.
foreach ($id in docker network ls --filter="name=ddnt" --quiet --no-trunc)
{
    docker network rm $id
}

# Remove volumes.
foreach ($id in docker volume ls --filter="name=ddnt" --quiet)
{
    docker volume rm $id
}

# Remove images in reverse order of how they were created.
$tags = $imageDefinitions.Name
$tags = [System.Linq.Enumerable]::Reverse([string[]] $tags) 
foreach ($tag in $tags)
{
    # Does the image exist?
    $id = docker image ls ddnt:$tag --quiet --no-trunc
    if ($id)
    {
        # Remove all containers for it, force-stopping them if needed. This catches containers that 
        # aren't named as expected, which can happen when troubleshooting.
        foreach ($container in docker container ls --filter=ancestor=ddnt:$tag --quiet --no-trunc --all)
        {
            docker container rm --force $container
        }

        # Remove the image.
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
    # Remove all containers for it, force-stopping them if needed. This catches containers that 
    # aren't named as expected, which can happen when troubleshooting.
    foreach ($container in docker container ls --filter=ancestor=$id --quiet --no-trunc --all)
    {
        docker container rm --force $container
    }

    # Remove image.
    docker image rm --force $id
}

# Remove ID files.
Remove-Item **/*.id

Pop-Location
Write-Output "Cleanup complete"
