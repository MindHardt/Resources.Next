using Resources.Next.Core;

[assembly: ResourcesNextConfigurationAttribute.Global(
    Separator = ',',
    NameColumn = "Identifier",
    Kind = ResourcesGenerationKind.Functional)]
[assembly: ResourcesNextConfigurationAttribute.Override(
    targets: "BarResources",
    Separator = ';',
    NameColumn = "Name",
    Kind = ResourcesGenerationKind.Dictionary)]