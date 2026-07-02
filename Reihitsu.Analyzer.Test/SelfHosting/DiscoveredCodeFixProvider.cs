using System;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Reflected code-fix provider metadata
/// </summary>
/// <param name="CodeFixProviderType">Code-fix provider type</param>
/// <param name="DiagnosticId">Diagnostic ID fixed by the provider</param>
internal sealed record DiscoveredCodeFixProvider(Type CodeFixProviderType, string DiagnosticId);