namespace Resources.Next.Generator.Source;

internal static class DictionaryLocalizedResource
{
    internal const string Source =
      $$"""
        {{SourceConstants.FileHeader}}
        
        using System;
        using System.Collections.Frozen;
        using System.Collections.Generic;
        using Resources.Next.Core;
        
        {{SourceConstants.NamespaceDirective}}
        
        /// <summary>
        /// A <see cref="LocalizedResource"/> implementation that stores its localizations in a <see cref="FrozenDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This implementation is more heavy on memory but is easier to debug.
        /// </remarks>
        internal class DictionaryLocalizedResource(IEnumerable<KeyValuePair<string, string>> localizations) : LocalizedResource
        {
            private readonly FrozenDictionary<string, string> _localizedResources = localizations.ToFrozenDictionary();
        
            public override string Default => 
                _localizedResources.GetValueOrDefault(string.Empty)
                ?? throw new InvalidOperationException("No default localization found for this resource");
        
            public override string? GetOrNull(string culture) =>
                _localizedResources.GetValueOrDefault(culture);
        }
        """;
}