namespace Resources.Next;

public interface IResourceProvider
{
    /// <summary>
    /// Looks for a resource with key equal to <paramref name="key"/>
    /// in this <see cref="IResourceProvider"/>.
    /// </summary>
    public static abstract LocalizedResource? Find(string key);
}