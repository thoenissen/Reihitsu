using System;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Reflected analyzer metadata
/// </summary>
/// <param name="AnalyzerType">Analyzer type</param>
/// <param name="DiagnosticId">Diagnostic ID</param>
internal sealed record DiscoveredAnalyzer(Type AnalyzerType, string DiagnosticId);