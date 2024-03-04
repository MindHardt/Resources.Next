using System.Linq;

namespace Resources.Next.Generator.Generators;

internal static class FunctionalResource
{
    internal static string Generate(GeneratedResource generatedResource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{generatedResource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {generatedResource.Name} {{ get; }} = new FunctionalLocalizedResource(static culture => culture switch 
    {{
{string.Join("\n", generatedResource.OtherLocales.Select(x => $"\t\t\"{x.Key}\" => \"{x.Value}\","))}
        _ => ""{generatedResource.DefaultLocale}""
    }});";
}