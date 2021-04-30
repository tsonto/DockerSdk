Set-StrictMode -Version 3
$ErrorActionPreference = 'Stop'

Write-Output 'Setting up docker environment'
Push-Location $PSScriptRoot

# Pull in definitions.
. ./definitions.ps1

function VerifySetup()
{
    foreach ($name in $containerDefinitions.Name)
    {
        if (!(VerifyContainer $name))
        {
            return $false
        }
    }

    foreach ($name in $networkDefinitions.Name)
    {
        if (!(VerifyNetwork $name))
        {
            return $false
        }
    }

    foreach ($name in $imageDefinitions.Name)
    {
        if (!(VerifyImage $name))
        {
            return $false
        }
    }

    return $true
}

function VerifyContainer($name)
{
    $path = "$name.id"
    if (!(Test-Path $path))
    {
        # Either the tests haven't been set up, or someone has deleted the .id file (such as by `git clean -f`). Clean and rebuild.
        return $false
    }
    $fileId = Get-Content $path | Select-Object -First 1

    $dockerId = docker container ls --all --quiet --no-trunc --filter=name=ddnt-$name
    if (!$dockerId)
    {
        # The setup has run, but Docker isn't in the correct state. Clean and rebuild.
        return $false
    }
    
    if ($fileId -ne $dockerId)
    {
        # The setup has run, but for a different set of IDs (possibly a copy of ./up.ps1 from a different path ran). Clean and rebuild.
        return $false
    }

    return $true
}

function VerifyNetwork($name)
{
    $path = "$name.network.id"
    if (!(Test-Path $path))
    {
        # Either the tests haven't been set up, or someone has deleted the .network.id file (such as by `git clean -f`). Clean and rebuild.
        return $false
    }
    $fileId = Get-Content $path | Select-Object -First 1

    $dockerId = docker network ls --filter=name=ddnt-$name --quiet --no-trunc
    if (!$dockerId)
    {
        # The setup has run, but Docker isn't in the correct state. Clean and rebuild.
        return $false
    }
    
    if ($fileId -ne $dockerId)
    {
        # The setup has run, but for a different set of IDs (possibly a copy of ./up.ps1 from a different path ran). Clean and rebuild.
        return $false
    }

    return $true
}

function VerifyImage($name)
{
    $path = "$name/image.id"
    if (!(Test-Path $path))
    {
        # Either the tests haven't been set up, or someone has deleted the .id file (such as by `git clean -f`). Clean and rebuild.
        return $false
    }
    $fileId = Get-Content $path | Select-Object -First 1

    $dockerId = docker image ls ddnt:$name --quiet --no-trunc
    if (!$dockerId)
    {
        # The setup has run, but Docker isn't in the correct state. Clean and rebuild.
        return $false
    }
    
    if ($fileId -ne $dockerId)
    {
        # The setup has run, but for a different set of IDs (possibly a copy of ./up.ps1 from a different path ran). Clean and rebuild.
        return $false
    }

    return $true
}

# Determine whether to clean and whether to build.
if (VerifySetup)
{
    Write-Output "Already set up"
    Pop-Location
    return
}

# Start from a fresh state.
& ./clean.ps1

# Build the images.
foreach ($entry in $imageDefinitions)
{
    $name = $entry.Name
    $args = $entry['Args'] ?? ''
    Push-Location $name
    Invoke-Expression "docker build . --tag ddnt:$name --quiet $args > image.id"
    Pop-Location
}

# Create the networks.
foreach ($entry in $networkDefinitions)
{
    $name = $entry.Name
    $args = $entry['Args'] ?? ''
    Invoke-Expression "docker network create $args ddnt-$name > $name.network.id"
}

# Start the containers.
foreach ($entry in $containerDefinitions)
{
    $name = $entry.Name
    $image = $entry['Image'] ?? $name
    $args = $entry['Args'] ?? ''
    Invoke-Expression "docker run --detach --name ddnt-$name $args ddnt:$image > $name.id"
}

# Pause for a bit to give the registries time to start up.
Start-Sleep 2  # in seconds

Pop-Location
Write-Output "Setup complete"
