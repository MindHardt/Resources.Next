using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UseRawString

namespace Resources.Next.Generator;

[Generator]
public class ResourcesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(ctx =>
        {
            ctx.AddSource("IResourceProvider.g.cs", SourceText.From(
                @"// <auto-generated/>
#nullable enable
namespace Resources.Next;

/// <summary>
/// An interface for resource providers. The implementors are expected to be source-generated.
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Looks for a resource with key equal to <paramref name=""key""/>
    /// in this <see cref=""IResourceProvider""/>.
    /// </summary>
    public static abstract LocalizedResource? Find(string key);
}
", Encoding.UTF8));
            
            ctx.AddSource("LocalizedResource.g.cs", SourceText.From(
                @"// <auto-generated/>
#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace Resources.Next;

/// <summary>
/// Wraps a localized resource.
/// </summary>
public class LocalizedResource(IEnumerable<KeyValuePair<string, string>> localizations)
{
    private readonly FrozenDictionary<string, string> _localizations = localizations.ToFrozenDictionary();

    /// <summary>
    /// A default fallback value of this <see cref=""LocalizedResource""/>.
    /// </summary>
    public string Default => _localizations[string.Empty];

    /// <summary>
    /// Gets value of this resource with provided <paramref name=""culture""/>
    /// or throws <see cref=""KeyNotFoundException""/> if none is found.
    /// </summary>
    public string GetRequired(string culture) => _localizations[culture];

    /// <inheritdoc cref=""GetRequired(string)""/>
    public string GetRequired(CultureInfo? culture = null) =>
        GetRequired((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets value of this resource with provided <paramref name=""culture""/>
    /// or <see langword=""null"" /> if none is found.
    /// </summary>
    public string? GetOrNull(string culture) => _localizations.GetValueOrDefault(culture);

    /// <inheritdoc cref=""GetOrNull(string)""/>
    public string? GetOrNull(CultureInfo? culture = null) =>
        GetOrNull((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets value of this resource with provided <paramref name=""culture""/>
    /// or <see cref=""Default""/> fallback value.
    /// </summary>
    public string GetOrDefault(string culture) => _localizations.GetValueOrDefault(culture) ?? Default;

    /// <inheritdoc cref=""GetOrDefault(string)""/>
    public string GetOrDefault(CultureInfo? culture = null) =>
        GetOrDefault((culture ?? CultureInfo.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets <see cref=""Default""/> fallback value for display.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => Default;
}
", Encoding.UTF8));
        });
    }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var file in context.AdditionalFiles)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            var fileName = Path.GetFileName(file.Path);
            var isResourceFile = Path.GetExtension(fileName) is ".csv" && fileName.Contains("Resources");
            if (isResourceFile is false)
            {
                continue;
            }

            var resourceClass = GenerateResourceClass(file, context.CancellationToken);

            context.AddSource(resourceClass.FileName, resourceClass.Source);
        }
    }

    internal static ResourcesSourceFile GenerateResourceClass(AdditionalText file, CancellationToken ct)
    {
        var fileName = Path.GetFileName(file.Path);

        var lines = file.GetText(ct)?.Lines;
        if (lines is null)
        {
            throw new IOException($"There was an error reading file {fileName}");
        }

        var className = Path.GetFileNameWithoutExtension(fileName);
        var resources = GetResources(lines, ct);

        var source = GenerateClass(className, resources, file.Path);
        return new ResourcesSourceFile($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    internal static string GenerateClass(string className, IReadOnlyCollection<Resource> resources, string filePath) => 
        $@"// <auto-generated/>
#nullable enable

using Resources.Next;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Globalization;
using System.Threading;
using System;

namespace Resources.Next.Generated;

/// <summary>
/// An <see cref=""IResourceProvider""/> created from file:
/// <code>{filePath}</code>
/// </summary>
public class {className} : IResourceProvider
{{
    private {className}() {{}}
{GenerateDictionary(resources)}
{string.Join("\n", resources.Select(GenerateResource))}
}}";

    internal static string GenerateDictionary(IEnumerable<Resource> resources) => @$"
    public static LocalizedResource? Find(string key) => key switch
    {{
{string.Concat(resources.Select(x => $"\t\t\"{x.Name}\" => {x.Name},\n"))}
        _ => null
    }};
";

    internal static string GenerateResource(Resource resource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{resource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {resource.Name} {{ get; }} = new
    ([
        KeyValuePair.Create(string.Empty, ""{resource.DefaultLocale}""),
{string.Join("\n", resource.OtherLocales.Select(x => $"\t\tKeyValuePair.Create(\"{x.Key}\", \"{x.Value}\"),"))}
    ]);";

    internal static IReadOnlyCollection<Resource> GetResources(TextLineCollection fileContent, CancellationToken ct)
    {
        const char separator = ';';
        const string nameIdentifier = "Name";
        const string commentPrefix = "#";

        List<Resource> resources = [];

        var lines = fileContent
            .Select(x => x.ToString())
            .Where(x =>
                x.StartsWith(commentPrefix) is false &&
                string.IsNullOrWhiteSpace(x) is false)
            .Select(x => x.Split(separator)
                    .Select(y => y.Replace(@"\n", "\n"))
                    .ToArray());

        var header = lines.Take(1).First();

        var nameIndex = Array.IndexOf(header, nameIdentifier);

        var cultureIndices = header
            .Select((culture, index) => (culture, index))
            .Where(x => x.culture is not nameIdentifier)
            .ToDictionary(x => x.index, x => x.culture);

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var resourceRow in lines.Skip(1))
        {
            ct.ThrowIfCancellationRequested();

            var locales = resourceRow
                .Select((resource, index) => (resource, index))
                .Where(x => cultureIndices.ContainsKey(x.index))
                .ToDictionary(x => cultureIndices[x.index], x => x.resource);

            var localizedResource = new Resource(resourceRow[nameIndex], locales);
            resources.Add(localizedResource);
        }

        return resources;
    }

}