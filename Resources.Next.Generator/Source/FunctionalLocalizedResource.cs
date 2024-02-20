namespace Resources.Next.Generator.Source;

internal static class FunctionalLocalizedResource
{
    internal const string Source = 
      $$"""
        {{SourceConstants.FileHeader}}
        
        using System;
        
        {{SourceConstants.NamespaceDirective}}
        
        /// <summary>
        /// A <see cref="LocalizedResource"/> implementation that uses <see cref="Func{T, TResult}"/>
        /// to retrieve localizations.
        /// </summary>
        /// <remarks>
        /// This implementation is expected to be more optimal but harder to debug.
        /// </remarks>
        public class FunctionalLocalizedResource(Func<string, string?> localizationProvider) : LocalizedResource
        {
            public override string Default => 
                localizationProvider(string.Empty) 
                ?? throw new InvalidOperationException("No default localization found for this resource");
        
            public override string? GetOrNull(string culture) =>
                localizationProvider(culture);
        }
        """;
}