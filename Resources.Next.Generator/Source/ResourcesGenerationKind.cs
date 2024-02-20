// ReSharper disable UseRawString
namespace Resources.Next.Generator.Source;

public static class ResourcesGenerationKind
{
    public const string Source = $@"{SourceConstants.FileHeader}

{SourceConstants.NamespaceDirective}

public enum ResourcesGenerationKind
{{
    /// <summary>
    /// Generated resources depend on preprocessor directives. #IF DEBUG means it uses
    /// <see cref=""Dictionary""/>, otherwise <see cref=""Functional""/>.
    /// This is useful for both debugging and performance in release mode.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// Generates <see cref=""DictionaryLocalizedResource""/>s.
    /// </summary>
    Dictionary = 1,
    /// <summary>
    /// Generates <see cref=""FunctionalLocalizedResource""/>s.
    /// </summary>
    Functional = 2,
}}
";
    
    public enum Enum
    {
        /// <summary>
        /// Generated resources depend on preprocessor directives. #IF DEBUG means it uses
        /// <see cref="Dictionary"/>, otherwise <see cref="Functional"/>.
        /// This is useful for both debugging and performance in release mode.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// Generates <see cref="DictionaryLocalizedResource"/>s.
        /// </summary>
        Dictionary = 1,
        /// <summary>
        /// Generates <see cref="FunctionalLocalizedResource"/>s.
        /// </summary>
        Functional = 2,
    }
}