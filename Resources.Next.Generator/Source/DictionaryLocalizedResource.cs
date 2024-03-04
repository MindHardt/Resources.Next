namespace Resources.Next.Generator.Source;

internal static class DictionaryLocalizedResource
{
    internal const string Source =
      $$"""
        {{SourceConstants.FileHeader}}
        
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using Resources.Next.Core;
        
        {{SourceConstants.NamespaceDirective}}
        
        /// <summary>
        /// A <see cref="LocalizedResource"/> implementation that stores its localizations in a <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This implementation is more heavy on memory but is easier to debug.
        /// </remarks>
        internal class DictionaryLocalizedResource(IEnumerable<KeyValuePair<string, string>> localizations) : LocalizedResource
        {
            private readonly IReadOnlyDictionary<string, string> _localizedResources = 
                localizations.ToImmutableDictionary();
        
            public override string Default => _localizedResources.TryGetValue(string.Empty, out var resource)
                ? resource
                : throw new InvalidOperationException("No default localization found for this resource");
        
            public override string? GetOrNull(string culture) => _localizedResources.TryGetValue(culture, out var resource)
                ? resource
                : null;
        }
        """;
}