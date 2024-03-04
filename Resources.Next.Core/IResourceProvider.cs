namespace Resources.Next.Core;

/// <summary>
/// An interface for resources generated with Resources.Next.
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Finds resource with specified name.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Found resource of <see langword="null"/> if none is found.</returns>
    public LocalizedResource? FindResource(string key);
}