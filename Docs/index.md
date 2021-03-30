DockerSdk is the .NET SKD for [Docker](https://www.docker.com/). It's a fully asynchronous, non-blocking way to interact with
a Docker daemon programmatically. It works with .NET 5.0 or later.

Be aware that, **until version 1.0, the API will be subject to change without notice**. (This is consistent with semver.)

## Installing ##
DockerSdk is published as [a NuGet package](https://www.nuget.org/packages/DockerSdk/). You can install it from within Visual 
Studio / Visual Studio Code, or from the command line:

Package Manager:
```{.ps1}
    Install-Package DockerSdk -Version 0.5.0
```

.NET CLI:
```
    dotnet add package DockerSdk --version 0.5.0
```

Package reference:
```{.xml}
    <PackageReference Include="DockerSdk" Version="0.5.0" />
```

F# interactive:
```{.fs}
    #r "nuget: DockerSdk, 0.5.0"
```

## Getting started ##
The SDK is available in the `DockerSdk` namespace. Connecting to a Docker daemon on the local machine is as easy as:

```{.cs}
    var client = await DockerClient.StartAsync();
```

If you want to use the configuration from the [standard environment variables](https://docs.docker.com/compose/reference/envvars/), use:

```{.cs}
    var options = ClientOptions.FromEnvironment();
    var client = await DockerClient.StartAsync(options);
```

For more control over the connection, see [ClientOptions](@ref DockerSdk.ClientOptions) and the other overloads of
[StartAsync](@ref DockerSdk.DockerClient.StartAsync).

Once you have a Docker client object, you can access its functionality via the `Containers`, `Images`, etc. properties.

## Examples ##

List containers:

```{.cs}
    Container[] containers = await client.Containers.ListAsync();
```

Create an image by pulling from a Docker Registry:

```{.cs}
    Image image = await client.Images.PullAsync("fedora/memcached:alpha");
```

Run a container:

```{.cs}
   Container container = await client.Containers.CreateAndStartAsync("mcr.microsoft.com/powershell"); 
```

Stop a container:

```{.cs}
    await container.StopAsync();
```

## Docker Registries ##
The SDK can work with public and private registries. (A registry is a site that hosts Docker images, such as DockerHub.) If you don't provide
credentials for a registry, the SDK will attempt to connect anonymously. For some registries that's sufficient to pull from public repositories.
To access private repositories, however, you will need to provide credentials before starting the push/pull operation. This is similar to the
Docker CLI's `docker login` command.

For registries that authenticate by username and password, use AddBasicAuth. For example, to authenticate with DockerHub you would use:

```{.cs}
    client.Registries.AddBasicAuth("docker.io", username, password);
```

For registries that authenticate by identity token, use AddIdentityToken. For example, to authenticate with the Azure registry you would use:

```{.cs}
    client.Registries.AddIdentityToken("mcr.microsoft.com", token);
```

## API versions ##
When the SDK connects a client to a Docker daemon, it automatically negotiates the API version to use. Higher API versions allow newer features, 
whereas lower API versions have fewer capabilities. The SDK automatically uses the highest API version that both the SDK and the daemon support. 
If there is no overlap in support, `StartAsync` will throw a [DockerVersionException](@ref DockerSdk.DockerVersionException).

If you attempt to use a feature that the negotiated API doesn't support, the SDK will throw a `NotSupportedException`. To avoid this, check the 
client object's [ApiVersion](@ref DockerSdk.DockerClient.ApiVersion) property to verify that the negotiated version is suitable for your needs.
The documentation for the SDK's methods will tell you which API versions they're supported on. If there is no such comment, you may safely 
assume that it's supported.

Version support:

| SDK versions   | API versions | Docker versions |
|----------------|--------------|-----------------|
| 0.1.0 - 0.5.x  | 1.41 - 1.41  | 20.10 - 20.10   |

## I need even more control! ##
If you find that you need more fine-grained control over the functionality, or if you need better performance than the SDK can give you, you can
skip the SDK layer and instead use [Docker.DotNet](https://github.com/dotnet/Docker.DotNet). This may require more expertese with Docker and the
conventions of the Docker REST API, but it provides a strong foundation for working with the Docker daemon at the lowest-available level.

## License ##
DockerSdk is licensed under the [MIT license](articles/LICENSE.md).

## Code of Conduct
This project subscribes to a [Code of Conduct](articles/CODE-OF-CONDUCT.md).

