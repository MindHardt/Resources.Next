using System.Globalization;
using System.Collections.Generic;
using System.Threading;

namespace Resources.Next.Core;

/// <summary>
/// Wraps a localized resource.
/// </summary>
public abstract class LocalizedResource
{
    /// <summary>
    /// A default fallback value of this <see cref="LocalizedResource"/>.
    /// </summary>
    public abstract string Default { get; }

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or <see langword="null" /> if none is found.
    /// </summary>
    public abstract string? GetOrNull(string culture);
    /// <inheritdoc cref="GetOrNull(string)"/>
    public string? GetOrNull(CultureInfo? culture = null) =>
        GetOrNull((culture ?? Thread.CurrentThread.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or throws <see cref="KeyNotFoundException"/> if none is found.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If no localization is found.</exception>
    public string GetRequired(string culture) => 
        GetOrNull(culture) ?? throw new KeyNotFoundException($"Localization for culture {culture} not found for this resource.");
    /// <inheritdoc cref="GetRequired(string)"/>
    public string GetRequired(CultureInfo? culture = null) =>
        GetRequired((culture ?? Thread.CurrentThread.CurrentCulture).TwoLetterISOLanguageName);
    

    /// <summary>
    /// Gets value of this resource with provided <paramref name="culture"/>
    /// or <see cref="Default"/> fallback value.
    /// </summary>
    public string GetOrDefault(string culture) => 
        GetOrNull(culture) ?? Default;
    /// <inheritdoc cref="GetOrDefault(string)"/>
    public string GetOrDefault(CultureInfo? culture = null) =>
        GetOrDefault((culture ?? Thread.CurrentThread.CurrentCulture).TwoLetterISOLanguageName);

    /// <summary>
    /// Gets <see cref="Default"/> fallback value for display.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => Default;
}