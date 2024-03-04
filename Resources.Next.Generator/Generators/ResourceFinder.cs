using System.Collections.Generic;
using System.Linq;
using Resources.Next.Generator.Source;

namespace Resources.Next.Generator.Generators;

internal static class ResourceFinder
{
    public static string Generate(IEnumerable<string> resourceClasses) => $@"{SourceConstants.FileHeader}

using System;
using Resources.Next.Core;

{SourceConstants.NamespaceDirective}

/// <summary>
/// Finds resource with <paramref name=""name""/> or <see langword=""null""/> if none is found.
/// </summary>
/// <typeparam name=""TResource"">The resource type to look in.</typeparam>
/// <returns></returns>
public static class {nameof(ResourceFinder)}
{{
    public static LocalizedResource? FindIn<TResource>(string name)
        where TResource : IResourceProvider
        => typeof(TResource).Name switch
    {{
{string.Concat(resourceClasses.Select(x => $"\t\t\"{x}\" => {x}.Instance.FindResource(name),\n"))}
        _ => throw new InvalidOperationException()
    }};
}}
";
}