using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DockerSdk.Containers;
using DockerSdk.Images;
using DockerSdk.Networks;

namespace DockerSdk
{
    internal class LoadContext
    {
        public ResourceCache<IContainer, IContainerInfo, ContainerFullId, ContainerReference> Containers { get; } = new(c => c.Id);
        public ResourceCache<Image, ImageDetails, ImageFullId, ImageReference> Images { get; } = new(i => i.Id);
        public ConcurrentDictionary<string, INetworkEndpoint> NetworkEndpoints { get; } = new();
        public ResourceCache<INetwork, INetworkInfo, NetworkFullId, NetworkReference> Networks { get; } = new(n => n.Id);

        public class ResourceCache<TResource, TResourceInfo, TFullId, TReference>
            where TFullId : notnull, TReference
            where TReference : notnull
            where TResource : class
            where TResourceInfo : TResource
        {
            public ResourceCache(Func<TResource, TFullId> getId)
            {
                this.getId = getId;
            }

            public readonly ConcurrentDictionary<TFullId, TResource> idToRes = new();
            public readonly ConcurrentDictionary<TReference, TFullId> refToId = new();
            private readonly Func<TResource, TFullId> getId;

            public void Alias(TFullId id, params TReference?[] references)
            {
                foreach (var reference in references)
                    if (reference is not null && !reference.Equals(id))
                        refToId.TryAdd(reference, id);
            }

            public void Cache(TResource resource, params TReference?[] references)
            {
                var id = getId(resource);
                idToRes[id] = resource;
                Alias(id, references);
            }

            public bool TryGet(TReference reference, bool fullOnly, [NotNullWhen(returnValue: true)] out TResource? resource)
            {
                if (TryGetId(reference, refToId, out var id) && idToRes.TryGetValue(id, out resource))
                {
                    if (!fullOnly)
                        return true;
                    return resource is TResourceInfo;
                }

                resource = default;
                return false;
            }

            private static bool TryGetId(TReference input, ConcurrentDictionary<TReference, TFullId> dict, [NotNullWhen(returnValue: true)] out TFullId? id)
            {
                if (input is TFullId tid)
                {
                    id = tid;
                    return true;
                }

                if (dict.TryGetValue(input, out id))
                    return true;

                id = default;
                return false;
            }
        }
    }
}
