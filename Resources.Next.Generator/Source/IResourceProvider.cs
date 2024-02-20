namespace Resources.Next.Generator.Source;

/// <summary>
/// An interface for resource providers. The implementors are expected to be source-generated.
/// </summary>
// ReSharper disable once InconsistentNaming
internal static class IResourceProvider
{
    internal const string Source = 
      $$"""
        {{SourceConstants.FileHeader}}
        
        {{SourceConstants.NamespaceDirective}}

        /// <summary>
        /// An interface for resource providers. The implementors are expected to be source-generated.
        /// </summary>
        public interface IResourceProvider
        {
            /// <summary>
            /// Looks for a resource with key equal to <paramref name="key"/>
            /// in this <see cref="IResourceProvider"/>.
            /// </summary>
            public static abstract LocalizedResource? Find(string key);
        }
        """;
}