using Resources.Next.Shared;

[assembly: ResourcesNextConfigurationAttribute.Global(
    Separator = ',')]
    
[assembly: ResourcesNextConfigurationAttribute.Override("SampleResources",
    Separator = ';')]