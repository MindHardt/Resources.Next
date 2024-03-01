using System.Linq;

namespace Resources.Next.Generator.Generators;

internal static class FunctionalResource
{
    internal static string Generate(Resource resource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{resource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {resource.Name} {{ get; }} = new FunctionalLocalizedResource(static culture => culture switch 
    {{
{string.Join("\n", resource.OtherLocales.Select(x => $"\t\t\"{x.Key}\" => \"{x.Value}\","))}
        _ => ""{resource.DefaultLocale}""
    }});";
}