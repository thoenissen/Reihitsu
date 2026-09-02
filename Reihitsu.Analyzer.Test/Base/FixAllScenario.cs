using System;

using Microsoft.CodeAnalysis.Testing;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Scenario that applies a code fix to a document containing more than one diagnostic
/// </summary>
/// <param name="Source">The source text to test, which may include markup syntax</param>
/// <param name="FixedSource">The expected fixed source text</param>
/// <param name="Expected">The expected diagnostics, of which there must be at least two</param>
/// <param name="Configure">Additional configuration of the test</param>
public sealed record FixAllScenario(string Source,
                                    string FixedSource,
                                    DiagnosticResult[] Expected,
                                    Action<CodeFixTest<DefaultVerifier>> Configure = null);