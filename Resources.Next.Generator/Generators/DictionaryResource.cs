using System.Linq;
// ReSharper disable UseRawString

namespace Resources.Next.Generator.Generators;

internal static class DictionaryResource
{
    internal static string Generate(Resource resource) => $@"
    /// <summary>
    /// A resource with following default value:
    /// <code>{resource.DefaultLocale}</code>
    /// </summary>
    public static LocalizedResource {resource.Name} {{ get; }} = new DictionaryLocalizedResource
    ([
        KeyValuePair.Create(string.Empty, ""{resource.DefaultLocale}""),
{string.Join("\n", resource.OtherLocales.Select(x => $"\t\tKeyValuePair.Create(\"{x.Key}\", \"{x.Value}\"),"))}
    ]);";
}