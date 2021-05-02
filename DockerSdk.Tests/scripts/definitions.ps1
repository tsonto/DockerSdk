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

# Defines the networks to build.
# It's important that the network creation args specify subnets. If we don't, we may hit Docker's limit of how 
# many IPv4 or IPv6 addresses it can supply from the default subnets, which is rather small by default.
$networkDefinitions = @(
    @{
        name = 'general'
        args = '--attachable --ipv6 --subnet 12.34.56.0/24 --subnet 1234:5678::/32 --label ddnt1=alpha --label ddnt2=beta --gateway 12.34.56.1 --ip-range 12.34.56.0/25 --aux-address foo=12.34.56.2 --aux-address bar=12.34.56.3'
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
