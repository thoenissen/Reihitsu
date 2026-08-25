using Reihitsu.Analyzer.Test.SelfHosting.Enumerations;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Result of verifying a single rule document's violation/correction example
/// </summary>
/// <param name="Example">The verified example</param>
/// <param name="Outcome">The verification outcome</param>
/// <param name="Detail">Human-readable detail describing the outcome, empty when <see cref="Outcome"/> is <see cref="RuleDocumentationExampleOutcome.Passed"/></param>
internal sealed record RuleDocumentationExampleResult(RuleDocumentationExample Example, RuleDocumentationExampleOutcome Outcome, string Detail);