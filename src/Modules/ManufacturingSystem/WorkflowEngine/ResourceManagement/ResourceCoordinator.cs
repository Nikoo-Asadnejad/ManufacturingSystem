namespace ManufacturingSystem.WorkflowEngine.ResourceManagement;

internal sealed class ResourceCoordinator(IEnumerable<IResource> resources, ILogger<ResourceCoordinator> logger) : IResourceCoordinator
{
    private readonly Dictionary<string, IResource> _resources = CreateResourceMap(resources);

    public async Task<IResource[]> AcquireAsync(
        IEnumerable<string> resourceIds,
        CancellationToken cancellationToken = default)
    {
        var resourcesToAcquire = GetResourcesInAcquisitionOrder(resourceIds);

        return await AcquireResources(
            resourcesToAcquire,
            cancellationToken);
    }

    public void Release(
        IEnumerable<IResource> resources)
    {
        foreach (var resource in resources.Reverse())
        {
            var registeredResource = GetResource(resource.Id);
            if (registeredResource is not null)
            {
                registeredResource.Release();
            }
        }
    }
    
    /// <summary>
    /// Sort resources in same order to avoid circular wait's and deadlocks.
    /// </summary>
    /// <param name="resourceIds"></param>
    /// <returns></returns>
    private IResource[] GetResourcesInAcquisitionOrder(IEnumerable<string> resourceIds)
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
        IResource[] resourcesToAcquire,
        CancellationToken cancellationToken)
    {
        var acquiredResources = new List<IResource> (resourcesToAcquire.Length);

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
                 Rollback(acquiredResources);
            }
        }
    }

    private void Rollback(
        List<IResource> resources)
    {
        for (var index = resources.Count - 1; index >= 0; index--)
        {
             resources[index].Release();
        }
    }
    
    private IResource? GetResource(string resourceId)
    {
        _resources.TryGetValue(resourceId, out var resource);
        return resource;
    }
    
    private static Dictionary<string, IResource> CreateResourceMap(IEnumerable<IResource> resources)
    {
        return resources.DistinctBy(r=> r.Id).ToDictionary(resource => resource.Id, StringComparer.Ordinal);
    }
}
