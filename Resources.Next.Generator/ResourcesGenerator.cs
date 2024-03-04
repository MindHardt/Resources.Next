using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Resources.Next.Generator.Generators;
using Resources.Next.Generator.Source;
using Resources.Next.Core;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UseRawString

namespace Resources.Next.Generator;

[Generator]
public class ResourcesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{nameof(DictionaryLocalizedResource)}.g.cs", SourceText.From(DictionaryLocalizedResource.Source, Encoding.UTF8));
            ctx.AddSource($"{nameof(FunctionalLocalizedResource)}.g.cs", SourceText.From(FunctionalLocalizedResource.Source, Encoding.UTF8));
        });
        
        var configurations = GetAttributeProvider(context.SyntaxProvider);
        
        var resourceFiles = context.AdditionalTextsProvider
            .Where(IsResourceFile)
            .Collect();
        
        context.RegisterSourceOutput(configurations.Combine(resourceFiles),
            (ctx, t) =>
            {
                var (attributes, texts) = t;

                var resContext = ResourcesGenerationContext.Create(attributes, ctx.CancellationToken);

                var resourceClassNames = new List<string>(texts.Length);
                foreach (var text in texts)
                {
                    var resourceName = Path.GetFileNameWithoutExtension(text.Path);
                    resourceClassNames.Add(resourceName);
                    
                    var currentContext = resContext.GetConfiguration(resourceName);

                    var resource = GenerateResourceClass(text, currentContext, ctx.CancellationToken);
                    
                    ctx.AddSource(resource.FileName, resource.Source);
                }

                var finderClass = ResourceFinder.Generate(resourceClassNames);
                ctx.AddSource($"{nameof(ResourceFinder)}.g.cs", SourceText.From(finderClass, Encoding.UTF8));
            });
    }

    private static IncrementalValueProvider<ImmutableArray<AttributeData>> GetAttributeProvider(
        SyntaxValueProvider syntaxProvider)
    {
        var globalAttributeName = typeof(ResourcesNextConfigurationAttribute.GlobalAttribute).FullName!;
        var globalProvider = syntaxProvider.ForAttributeWithMetadataName(globalAttributeName,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: (ctx, _) => ctx.Attributes)
            .SelectMany((x, _) => x)
            .Collect();
        
        var overrideAttributeName = typeof(ResourcesNextConfigurationAttribute.OverrideAttribute).FullName!;
        var overrideProvider = syntaxProvider.ForAttributeWithMetadataName(overrideAttributeName,
                predicate: static (node, _) => node is CompilationUnitSyntax,
                transform: (ctx, _) => ctx.Attributes)
            .SelectMany((x, _) => x)
            .Collect();
        
        return globalProvider.Combine(overrideProvider)
            .SelectMany((x, _) => x.Left.AddRange(x.Right))
            .Collect();
    }

    private static bool IsResourceFile(AdditionalText file) =>
        Path.GetExtension(file.Path) is ".csv" && 
        Path.GetFileNameWithoutExtension(file.Path).Contains("Resources");

    private static ResourcesSourceFile GenerateResourceClass(AdditionalText file, ResourceConfigurationInternal configuration, CancellationToken ct)
    {
        var effectiveConfiguration = configuration.WithEffectiveGenerationKind();
        
        var fileName = Path.GetFileName(file.Path);

        var className = Path.GetFileNameWithoutExtension(fileName);
        string source;
        try
        {
            var lines = file.GetText(ct)?.Lines;
            if (lines is null)
            {
                throw new IOException($"There was an error reading file {fileName}");
            }
            
            var resources = GetResources(lines, effectiveConfiguration, ct);

            source = ResourceClass.Generate(className, file.Path, configuration, resources, effectiveConfiguration.Kind!.Value);
        }
        catch (Exception e)
        {
            source = ResourceClass.GenerateError(className, file.Path, configuration, e);
        }
        
        return new ResourcesSourceFile($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static IReadOnlyCollection<GeneratedResource> GetResources(
        TextLineCollection fileContent,
        ResourceConfigurationInternal config,
        CancellationToken ct)
    {
        List<GeneratedResource> resources = [];

        var lines = fileContent
            .Select(x => x.ToString())
            .Where(x =>
                x.StartsWith(config.CommentPrefix!) is false &&
                string.IsNullOrWhiteSpace(x) is false)
            .Select(x => x.Split(config.Separator!.Value)
                .Select(y => y.Replace(@"\n", "\n"))
                .ToArray());

        var header = lines.Take(1).First();

        var nameIndex = Array.IndexOf(header, config.NameColumn);

        var cultureIndices = header
            .Select((culture, index) => (culture, index))
            .Where(x => x.culture != config.NameColumn)
            .ToDictionary(x => x.index, x => x.culture);

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var resourceRow in lines.Skip(1))
        {
            ct.ThrowIfCancellationRequested();

            var locales = resourceRow
                .Select((resource, index) => (resource, index))
                .Where(x => cultureIndices.ContainsKey(x.index))
                .ToDictionary(x => cultureIndices[x.index], x => x.resource);

            var localizedResource = new GeneratedResource(resourceRow[nameIndex], locales);
            resources.Add(localizedResource);
        }

        return resources;
    }
}