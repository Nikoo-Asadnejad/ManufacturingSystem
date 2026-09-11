namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

internal sealed class ResourceCoordinator(IEnumerable<Resource> resources, ILogger<ResourceCoordinator> logger) : IResourceCoordinator
{
    private readonly Dictionary<string, Resource> _resources = CreateResourceMap(resources);

    public async Task<IResource[]> AcquireAsync(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default)
    {
        var resourcesToAcquire = GetResourcesInAcquisitionOrder(resourceIds);

        return await AcquireResources(
            resourcesToAcquire,
            cancellationToken);
    }

    public async Task ReleaseAsync(
        IEnumerable<IResource> resources ,
        CancellationToken cancellationToken = default)
    {
        foreach (var resource in resources.Reverse())
        {
            var registeredResource = GetResource(resource.Id);
            if (registeredResource is not null)
            {
                await registeredResource.Release(cancellationToken);
            }
        }
    }
    
    /// <summary>
    /// Sort resources in same order to avoid circular wait's and deadlocks.
    /// </summary>
    /// <param name="resourceIds"></param>
    /// <returns></returns>
    private Resource[] GetResourcesInAcquisitionOrder(IEnumerable<string> resourceIds)
    {
        return
        [
            .. resourceIds
            .Distinct(StringComparer.Ordinal)
            .Select(x=> GetResource(x))
            .Where(x=> x != null)
            .Select(x => x!)
            .OrderBy(static resource => resource.Id, StringComparer.Ordinal)
        ];
    }

    private async Task<IResource[]> AcquireResources(
        Resource[] resourcesToAcquire,
        CancellationToken cancellationToken)
    {
        var acquiredResources = new List<Resource> (resourcesToAcquire.Length);

        try
        {
            foreach (var resource in resourcesToAcquire)
            {
                if (!await resource.Acquire(cancellationToken))
                {
                    return [];
                }

                acquiredResources.Add(resource);
            }

            return acquiredResources.ToArray();
        }
        catch(Exception e)
        {
            logger.LogError(e ,"Exception occured during resource acquirement.");
            return [];
        }
        finally
        {
            if (acquiredResources.Count != resourcesToAcquire.Length)
            {
                // aquired resources should release in case of cancellation
                await Rollback(acquiredResources, CancellationToken.None);
            }
        }
    }

    private async Task Rollback(
        List<Resource> resources,
        CancellationToken cancellationToken)
    {
        for (var index = resources.Count - 1; index >= 0; index--)
        {
            await resources[index].Release(cancellationToken);
        }
    }
    
    private Resource? GetResource(string resourceId)
    {
        _resources.TryGetValue(resourceId, out var resource);
        return resource;
    }
    
    private static Dictionary<string, Resource> CreateResourceMap(IEnumerable<Resource> resources)
    {
        return resources.DistinctBy(r=> r.Id).ToDictionary(resource => resource.Id, StringComparer.Ordinal);
    }
}
