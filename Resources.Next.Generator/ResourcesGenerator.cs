using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
// ReSharper disable PossibleMultipleEnumeration

namespace Resources.Next.Generator;

[Generator]
public class ResourcesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //Nothing
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

        var source = GenerateClass(className, resources);
        return new ResourcesSourceFile($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    internal static string GenerateClass(string className, IReadOnlyCollection<Resource> resources) =>
   $$"""
     #nullable enable
     using Resources.Next;
     using System.Collections.Generic;
     using System.Collections.Frozen;
     using System.Globalization;
     using System.Threading;
     using System;
     
     namespace Resources.Next.Generated;
     
     public class {{className}}
     {
     {{GenerateDictionary(resources)}}
     {{string.Join("\n", resources.Select(GenerateResource))}}
     }
     """;

    internal static string GenerateDictionary(IEnumerable<Resource> resources) =>
   $$"""
     
     /// <summary>
     /// A readonly dictionary for resources lookup.
     /// </summary>
     public static FrozenDictionary<string, LocalizedResource> Dictionary { get; } = FrozenDictionary.ToFrozenDictionary
     ([
     {{string.Join("\n", resources.Select(x => $"\tKeyValuePair.Create(\"{x.Name}\", {x.Name}),"))}}
     ]);
     """.Replace("\n", "\n\t");

    internal static string GenerateResource(Resource resource) =>
   $$"""
     
     /// <summary>
     /// A resource with following default value:
     /// <code>{{resource.DefaultLocale}}</code>
     /// </summary>
     public static LocalizedResource {{resource.Name}} { get; } = new(new Dictionary<string, string>
     {
     {{string.Join("\n", resource.OtherLocales.Select(x => $"\t[\"{x.Key}\"] = \"{x.Value}\","))}}
        [string.Empty] = "{{resource.DefaultLocale}}"
     });
     """.Replace("\n", "\n\t");

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