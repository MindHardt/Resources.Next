// ReSharper disable InvalidXmlDocComment
namespace Resources.Next.Core;

public enum ResourcesGenerationKind : byte
{
    /// <summary>
    /// Generated resources depend on preprocessor directives. #IF DEBUG means it uses
    /// <see cref="Dictionary"/>, otherwise <see cref="Functional"/>.
    /// This is useful for both debugging and performance in release mode.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// Generates <see cref="Resources.Next.DictionaryLocalizedResource"/>s.
    /// </summary>
    Dictionary = 1,
    /// <summary>
    /// Generates <see cref="Resources.Next.FunctionalLocalizedResource"/>s.
    /// </summary>
    Functional = 2,
}