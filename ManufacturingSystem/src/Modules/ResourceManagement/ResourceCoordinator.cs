namespace ManufacturingSystem.Modules.ResourceManagement;

internal sealed class ResourceCoordinator(IEnumerable<Resource> resources) : IResourceCoordinator
{
    private readonly Dictionary<string, Resource> _resources = CreateResourceMap(resources);

    public bool Acquire(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default)
    {
        var resourcesToAcquire = GetResourcesInAcquisitionOrder(resourceIds);

        return AcquireResources(
            resourcesToAcquire,
            cancellationToken);
    }

    private Resource[] GetResourcesInAcquisitionOrder(IEnumerable<string> resourceIds)
    {
        return
        [
            .. resourceIds
            .Distinct(StringComparer.Ordinal)
            .Select(GetResource)
            .OrderBy(static resource => resource.Id, StringComparer.Ordinal)
        ];
    }

    private static bool AcquireResources(
        Resource[] resourcesToAcquire,
        CancellationToken acquisitionCancellationToken)
    {
        List<Resource> acquiredResources = new(resourcesToAcquire.Length);

        try
        {
            foreach (var resource in resourcesToAcquire)
            {
                if (!WaitUntilAcquired(resource, acquisitionCancellationToken))
                {
                    return false;
                }

                acquiredResources.Add(resource);
            }

            return true;
        }
        finally
        {
            if (acquiredResources.Count != resourcesToAcquire.Length)
            {
                Rollback(acquiredResources);
            }
        }
    }

    private static bool WaitUntilAcquired(
        Resource resource,
        CancellationToken cancellationToken)
    {
        //assumed that resources will be freed in seconds
        while (!resource.Acquire(cancellationToken))
        {
            if (resource.State == ResourceState.Error)
            {
                return false;
            }
        }

        return true;
    }

    private static void Rollback(List<Resource> resources)
    {
        for (var index = resources.Count - 1; index >= 0; index--)
        {
            resources[index].Release(CancellationToken.None);
        }
    }

    private Resource GetResource(string resourceId)
    {
        return _resources.TryGetValue(resourceId, out var resource)
            ? resource
            : throw new KeyNotFoundException($"Resource '{resourceId}' is not registered.");
    }

    private static Dictionary<string, Resource> CreateResourceMap(IEnumerable<Resource> resources)
    {
        return resources.ToDictionary(resource => resource.Id, StringComparer.Ordinal);
    }
}
