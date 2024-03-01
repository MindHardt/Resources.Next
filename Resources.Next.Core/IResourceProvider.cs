using System;
using System.ComponentModel;

namespace Resources.Next.Core;

/// <summary>
/// An interface for resource providers. The implementors are expected to be source-generated.
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Looks for a resource with key equal to <paramref name="key"/>
    /// in this <see cref="IResourceProvider"/>.
    /// </summary>
    public static abstract LocalizedResource? Find(string key);
}