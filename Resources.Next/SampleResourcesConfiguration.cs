using Resources.Next;
using Resources.Next.Generated;

[assembly: ResourcesNextConfiguration(
    GenerationKind = ResourcesGenerationKind.Functional,
    Separator = ",",
    For = [typeof(SampleResources)]
)]