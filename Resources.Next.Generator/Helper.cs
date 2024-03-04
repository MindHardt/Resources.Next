using System.Linq;
using Microsoft.CodeAnalysis;
using Resources.Next.Core;

namespace Resources.Next.Generator;

public static class Helper
{
    public static char? GetSeparator(this AttributeData attribute) => 
        attribute.GetValue(nameof(ResourcesNextConfigurationAttribute.Separator)) is char separator
            ? separator
            : null;
    
    public static ResourcesGenerationKind? GetKind(this AttributeData attribute) => 
        attribute.GetValue(nameof(ResourcesNextConfigurationAttribute.Kind)) is byte kindValue
            ? (ResourcesGenerationKind)kindValue
            : null;

    public static string? GetNameColumn(this AttributeData attribute) =>
        attribute.GetValue(nameof(ResourcesNextConfigurationAttribute.NameColumn)) as string;

    public static string? GetCommentPrefix(this AttributeData attribute) => 
        attribute.GetValue(nameof(ResourcesNextConfigurationAttribute.CommentPrefix)) as string;

    private static object? GetValue(this AttributeData attribute, string name) => attribute.NamedArguments
        .FirstOrDefault(x => x.Key == name)
        .Value.Value;
}