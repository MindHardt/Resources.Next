namespace Resources.Next.Core;

/// <summary>
/// An attribute family to configure Resources.Next source generator.
/// </summary>
public abstract class ResourcesNextConfigurationAttribute : Attribute
{
    /// <summary>
    /// A separator character for parsing csv files.
    /// Defaults to <c>;</c>.
    /// </summary>
    public char Separator { get; set; }

    /// <summary>
    /// A <see cref="ResourcesGenerationKind"/> of the generated resources.
    /// Defaults to <see cref="ResourcesGenerationKind.Auto"/>.
    /// </summary>
    public ResourcesGenerationKind Kind { get; set; }

    /// <summary>
    /// A name of a column with resource names.
    /// Defaults to <c>Name</c>.
    /// </summary>
    public string NameColumn { get; set; } = null!;

    /// <summary>
    /// A prefix symbols for comments.
    /// Lines starting with this substring are ignored
    /// when parsing resources.
    /// Defaults to <c>#</c>.
    /// </summary>
    public string CommentPrefix { get; set; } = null!;

    /// <summary>
    /// A global fallback configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class GlobalAttribute : ResourcesNextConfigurationAttribute;
    
    /// <summary>
    /// Allows overriding <see cref="ResourcesNextConfigurationAttribute.GlobalAttribute"/> values for certain resources.
    /// </summary>
    /// <param name="targets"></param>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class OverrideAttribute(params string[] targets) : ResourcesNextConfigurationAttribute
    {
        public string[] Targets { get; } = targets;
    }
}