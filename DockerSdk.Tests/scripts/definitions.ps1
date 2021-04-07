Set-StrictMode -Version 3
$ErrorActionPreference = 'Stop'

# Defines the images to build, in the order that they should be built.
# It's intentional that "empty" is not built here--that's used for building the emdot/docker-sdk:empty image on Dockerhub.
$imageDefinitions = @(
    @{
        name = 'parent'
    },
    @{
        name = 'registry-open'
    },
    @{
        name = 'registry-htpasswd-tls'
    },
    @{
        name = 'infinite-loop'
    },
    @{
        name = 'inspect-me-1'
        args = '--label override-1="from-build"'
    },
    @{
        name = 'inspect-me-2'
    },
    @{
        name = 'error-out'
    }
)

# Defines the containers to start. If the image is not specified, it defaults to the name.
# It's intentional that error-out immediately fails and that some other containers immediately run to completion.
$containerDefinitions = @(
    @{
        name = 'registry-open'
        args = '--publish 4000:80'
    },
    @{
        name = 'registry-htpasswd-tls'
        args = '--publish 5001:443'
    },
    @{
        name = 'running'
        image = 'infinite-loop'
    },
    @{
        name = 'exited'
        image = 'inspect-me-1'
    },
    @{
        name = 'failed'
        image = 'error-out'
    }
)
