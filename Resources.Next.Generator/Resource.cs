using System.Collections.Generic;
using System.Linq;

namespace Resources.Next.Generator;

internal record Resource(string Name, Dictionary<string, string> Locales)
{
    public string Name { get; } = Name;
    public Dictionary<string, string> Locales { get; } = Locales;

    public string DefaultLocale
    {
        get => Locales[string.Empty];
        set => Locales[string.Empty] = value;
    }

    public IEnumerable<KeyValuePair<string, string>> OtherLocales
        => Locales.Where(x => x.Key != string.Empty);
}