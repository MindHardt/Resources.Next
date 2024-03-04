using System.Linq;
// ReSharper disable UseRawString

namespace Resources.Next.Generator.Generators;

internal static class DictionaryResource
{
    internal static string Generate(GeneratedResource generatedResource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{generatedResource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {generatedResource.Name} {{ get; }} = new DictionaryLocalizedResource
    ([
        KeyValuePair.Create(string.Empty, ""{generatedResource.DefaultLocale}""),
{string.Join("\n", generatedResource.OtherLocales.Select(x => $"\t\tKeyValuePair.Create(\"{x.Key}\", \"{x.Value}\"),"))}
    ]);";
}