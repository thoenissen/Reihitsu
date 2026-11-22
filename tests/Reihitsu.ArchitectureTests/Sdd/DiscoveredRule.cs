using System.Collections.Generic;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// A shipped analyzer rule as its descriptor and code-fix providers describe it
/// </summary>
/// <param name="Category">Descriptor category</param>
/// <param name="Severity">Default severity</param>
/// <param name="IsEnabledByDefault">Whether the rule is enabled by default</param>
/// <param name="CodeFix">Expected <c>Code fix</c> header value of the rule's spec</param>
/// <param name="TypeNames">Names of the analyzer and code-fix provider types</param>
internal sealed record DiscoveredRule(string Category, string Severity, bool IsEnabledByDefault, string CodeFix, IReadOnlyList<string> TypeNames);