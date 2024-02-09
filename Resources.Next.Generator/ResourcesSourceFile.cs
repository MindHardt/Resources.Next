using Microsoft.CodeAnalysis.Text;

namespace Resources.Next.Generator;

internal record ResourcesSourceFile(string FileName, SourceText Source)
{
    public string FileName { get; } = FileName;
    public SourceText Source { get; } = Source;
}