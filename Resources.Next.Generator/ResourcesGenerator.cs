using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Resources.Next.Generator.Source;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UseRawString

namespace Resources.Next.Generator;

[Generator]
public class ResourcesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) => context.RegisterForPostInitialization(ctx =>
    {
        ctx.AddSource($"{nameof(IResourceProvider)}.g.cs", SourceText.From(IResourceProvider.Source, Encoding.UTF8));
        ctx.AddSource($"{nameof(LocalizedResource)}.g.cs", SourceText.From(LocalizedResource.Source, Encoding.UTF8));
        ctx.AddSource($"{nameof(ResourcesGenerationKind)}.g.cs", SourceText.From(ResourcesGenerationKind.Source, Encoding.UTF8));
        ctx.AddSource($"{nameof(DictionaryLocalizedResource)}.g.cs", SourceText.From(DictionaryLocalizedResource.Source, Encoding.UTF8));
        ctx.AddSource($"{nameof(FunctionalLocalizedResource)}.g.cs", SourceText.From(FunctionalLocalizedResource.Source, Encoding.UTF8));
        ctx.AddSource($"{nameof(ResourcesNextConfigurationAttribute)}.g.cs", SourceText.From(ResourcesNextConfigurationAttribute.Source, Encoding.UTF8));
    });

    public void Execute(GeneratorExecutionContext context)
    {
        var attribute = context.Compilation.Assembly
            .GetAttributes()
            .FirstOrDefault(x => x.AttributeClass!.Name == nameof(ResourcesNextConfigurationAttribute));

        var kind = attribute?.NamedArguments
            .FirstOrDefault(x => x.Key == ResourcesNextConfigurationAttribute.KindProperty)
            .Value.Value is int enumValue
            ? (ResourcesGenerationKind.Enum)enumValue
            : ResourcesGenerationKind.Enum.Auto;
        
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

            var resourceClass = GenerateResourceClass(file, kind, context.CancellationToken);

            context.AddSource(resourceClass.FileName, resourceClass.Source);
        }
    }

    internal static ResourcesSourceFile GenerateResourceClass(AdditionalText file, ResourcesGenerationKind.Enum kind, CancellationToken ct)
    {
        if (kind is ResourcesGenerationKind.Enum.Auto)
        {
            #if DEBUG
            kind = ResourcesGenerationKind.Enum.Dictionary;
            #else
            kind = ResourcesGenerationKind.Enum.Functional;
            #endif
        }
        
        var fileName = Path.GetFileName(file.Path);

        var lines = file.GetText(ct)?.Lines;
        if (lines is null)
        {
            throw new IOException($"There was an error reading file {fileName}");
        }

        var className = Path.GetFileNameWithoutExtension(fileName);
        var resources = GetResources(lines, ct);

        var source = GenerateClass(className, resources, file.Path, kind);
        return new ResourcesSourceFile($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    internal static string GenerateClass(
        string className,
        IReadOnlyCollection<Resource> resources,
        string filePath,
        ResourcesGenerationKind.Enum kind) => 
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
{string.Join("\n", resources.Select(x => GenerateResource(x, kind)))}
}}";

    internal static string GenerateDictionary(IEnumerable<Resource> resources) => @$"
    public static LocalizedResource? Find(string key) => key switch
    {{
{string.Concat(resources.Select(x => $"\t\t\"{x.Name}\" => {x.Name},\n"))}
        _ => null
    }};
";

    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    internal static string GenerateResource(Resource resource, ResourcesGenerationKind.Enum kind) => kind switch
    {
        ResourcesGenerationKind.Enum.Dictionary => GenerateDictionaryResource(resource),
        ResourcesGenerationKind.Enum.Functional => GenerateFunctionalResource(resource),
        _ => throw new InvalidOperationException()
    };

    internal static string GenerateDictionaryResource(Resource resource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{resource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {resource.Name} {{ get; }} = new DictionaryLocalizedResource
    ([
        KeyValuePair.Create(string.Empty, ""{resource.DefaultLocale}""),
{string.Join("\n", resource.OtherLocales.Select(x => $"\t\tKeyValuePair.Create(\"{x.Key}\", \"{x.Value}\"),"))}
    ]);";
    
    private static string GenerateFunctionalResource(Resource resource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{resource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {resource.Name} {{ get; }} = new FunctionalLocalizedResource(static culture => culture switch 
    {{
{string.Join("\n", resource.OtherLocales.Select(x => $"\t\t\"{x.Key}\" => \"{x.Value}\","))}
        _ => ""{resource.DefaultLocale}""
    }});";

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