namespace Resources.Next.Generator.Source;

internal class ResourcesNextConfigurationAttribute
{
    internal const string KindProperty = "GenerationKind";
    internal const string Source =
      $$"""
        {{SourceConstants.FileHeader}}
        
        using System;
        
        {{SourceConstants.NamespaceDirective}}
         
        /// <summary>
        /// An attribute used to configure {{SourceConstants.Namespace}}.
        /// </summary>
        [AttributeUsage(AttributeTargets.Assembly)]
        public class ResourcesNextConfigurationAttribute : Attribute
        {
            /// <summary>
            /// A resources generation strategy used to generate resources in this assembly.
            /// Defaults to <see cref="ResourcesGenerationKind.Auto"/>.
            /// </summary>
            /// <seealso cref="ResourcesGenerationKind"/>
            public ResourcesGenerationKind {{KindProperty}} { get; set; } = ResourcesGenerationKind.Auto;
            
            /// <summary>
            /// <code>NOT IMPLEMENTED</code>
            /// A csv separator used for resources in this assembly. 
            /// If emitted then default value <c>;</c> is used.
            /// </summary>
            public string? Separator { get; set; }
            
            /// <summary>
            /// <code>NOT IMPLEMENTED</code>
            /// Specifies resource classes that are affected by this <see cref="ResourcesNextConfigurationAttribute"/>.
            /// If this property is ommited then this attribute is considered "global" and is used for all resources
            /// That don't have more specific configurations.
            /// </summary>
            /// <remarks>Having conflicting resource-specific configurations will cause an error.</remarks>
            public Type[]? For { get; set; }
        }
        """;
}