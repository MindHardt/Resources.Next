using Microsoft.CodeAnalysis;

namespace Resources.Next.Generator;

public static class Diagnostics
{
    public static Diagnostic DuplicateConfigurationsForType(string typeName, Location location) =>
        Diagnostic.Create(new DiagnosticDescriptor(
            "RN0002",
            "Duplicate configuration overrides",
            
            "Resources.Next configuration overrides should not affect the same resource type. " +
            "Some of the configurations of type {0} will not be applied.",
            
            "Conflict",
            DiagnosticSeverity.Warning, 
            true), location, typeName);
    
    public static Diagnostic DuplicateGlobalConfigurations(Location location) =>
        Diagnostic.Create(new DiagnosticDescriptor(
            "RN0001",
            "Duplicate global configurations",
            "Resources.Next does not allow duplicate global configurations.",
            "Conflict",
            DiagnosticSeverity.Error, true), location);
}