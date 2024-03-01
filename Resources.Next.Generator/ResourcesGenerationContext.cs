using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Resources.Next.Shared;

namespace Resources.Next.Generator;

internal record ResourcesGenerationContext(
    ResourceConfigurationInternal GlobalConfiguration,
    Dictionary<string, ResourceConfigurationInternal> Overrides)
{
    public ResourceConfigurationInternal GlobalConfiguration { get; } = new(
        GlobalConfiguration.AttributeSyntax,
        GlobalConfiguration.Separator ?? ';',
        GlobalConfiguration.Kind ?? ResourcesGenerationKind.Auto,
        GlobalConfiguration.NameColumn ?? "Name",
        GlobalConfiguration.CommentPrefix ?? "#");
    
    public Dictionary<string, ResourceConfigurationInternal> Overrides { get; } = Overrides;

    public ResourceConfigurationInternal GetConfiguration(string typeName)
    {
        return Overrides.TryGetValue(typeName, out var @override)? 
            @override.WithFallback(GlobalConfiguration) 
            : GlobalConfiguration;
    }

    public static ResourcesGenerationContext Create(ImmutableArray<AttributeData> attributes, CancellationToken ct)
    {
        const string globalName = nameof(ResourcesNextConfigurationAttribute.GlobalAttribute);
        var global = attributes
            .FirstOrDefault(x => x.AttributeClass!.Name == globalName) is { } attribute
            ? ResourceConfigurationInternal.FromAttribute(attribute, ct)
            : (ResourceConfigurationInternal?)null;

        Dictionary<string, ResourceConfigurationInternal> overrides = [];

        const string overrideName = nameof(ResourcesNextConfigurationAttribute.OverrideAttribute);
        foreach (var @override in attributes.Where(x => x.AttributeClass!.Name == overrideName))
        {
            var configuration = ResourceConfigurationInternal.FromAttribute(@override, ct);
            
            var targets = @override.ConstructorArguments
                .SelectMany(x => x.Values)
                .Select(x => x.Value)
                .Cast<string>();

            foreach (var target in targets)
            {
                overrides.Add(target, configuration);
            }
        }
        
        return new ResourcesGenerationContext(global ?? default, overrides);
    }
}

internal readonly record struct ResourceConfigurationInternal(
    AttributeSyntax? AttributeSyntax,
    char? Separator,
    ResourcesGenerationKind? Kind,
    string? NameColumn,
    string? CommentPrefix)
{
    public AttributeSyntax? AttributeSyntax { get; } = AttributeSyntax;
    public char? Separator { get; } = Separator;
    public ResourcesGenerationKind? Kind { get; } = Kind;
    public string? NameColumn { get; } = NameColumn;
    public string? CommentPrefix { get; } = CommentPrefix;

    public ResourceConfigurationInternal WithFallback(ResourceConfigurationInternal fallback) => new(
        AttributeSyntax ?? fallback.AttributeSyntax,
        Separator ?? fallback.Separator,
        Kind ?? fallback.Kind,
        NameColumn ?? fallback.NameColumn,
        CommentPrefix ?? fallback.CommentPrefix);

    public static ResourceConfigurationInternal FromAttribute(AttributeData attributeData, CancellationToken ct)
    {
        var sourceSyntax = attributeData.ApplicationSyntaxReference?.GetSyntax(ct) as AttributeSyntax;
        var separator = attributeData.GetSeparator();
        var kind = attributeData.GetKind();
        var nameColumn = attributeData.GetNameColumn();
        var commentPrefix = attributeData.GetCommentPrefix();

        return new ResourceConfigurationInternal(sourceSyntax, separator, kind, nameColumn, commentPrefix);
    }

    [SuppressMessage("ReSharper", "UseWithExpressionToCopyRecord")]
    public ResourceConfigurationInternal WithEffectiveGenerationKind() => Kind switch
    {
        ResourcesGenerationKind.Auto =>
#if DEBUG
            new ResourceConfigurationInternal(AttributeSyntax, Separator, ResourcesGenerationKind.Dictionary, NameColumn, CommentPrefix),
#else
            new ResourceConfigurationInternal(AttributeSyntax, Separator, ResourcesGenerationKind.Functional, NameColumn, CommentPrefix),
#endif

        _ => new ResourceConfigurationInternal(AttributeSyntax, Separator, Kind, NameColumn, CommentPrefix)
    };
}