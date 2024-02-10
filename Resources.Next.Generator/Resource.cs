using System.Collections.Generic;
using System.Linq;

namespace Resources.Next.Generator;

internal record Resource(string Name, Dictionary<string, string> Locales)
{
    public string Name { get; } = Name;
    public Dictionary<string, string> Locales { get; } = Locales;

    public string DefaultLocale => Locales[string.Empty];

    public IEnumerable<KeyValuePair<string, string>> OtherLocales
        => Locales.Where(x => x.Key != string.Empty);
}