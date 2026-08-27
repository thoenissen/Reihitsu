using System;
using System.Collections.Generic;

namespace Reihitsu.Analyzer.Test.SelfHosting.Utilities;

/// <summary>
/// Rule documents whose <c>### Violation</c> / <c>### Correction</c> examples do not (yet) satisfy every
/// criterion asserted by <see cref="RuleDocumentationExampleTests"/>, together with the reason each is opted
/// out. This list must only shrink: <see cref="RuleDocumentationExampleTests.OptedOutDocumentsStillFailVerification"/>
/// fails when a listed document would actually pass, so a fixed document has to be removed here in the same
/// change that fixes it
/// </summary>
internal static class RuleDocumentationExampleOptOuts
{
    #region Fields

    /// <summary>
    /// Every opted-out rule document, keyed by diagnostic ID, with the reason it is opted out
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> Reasons = new Dictionary<string, string>(StringComparer.Ordinal)
                                                                           {
                                                                               // Example is not a standalone C# compilation unit at all, or cannot carry the information the rule reports on
                                                                               ["RH0001"] = "Example is reihitsu.json, not C#; the verifier can only compile and analyze C# source, so a JSON configuration example can never be run through it.",
                                                                               ["RH0002"] = "Example is a .csproj file, not C#; the verifier can only compile and analyze C# source, so an MSBuild project-file example can never be run through it.",
                                                                               ["RH2109"] = "Example is a Razor component (markup plus a @code block), not a standalone C# compilation unit; the verifier can only parse and compile plain C#.",
                                                                               ["RH5604"] = "Example illustrates a per-line CRLF/LF mix, but the discovery step reads the document with File.ReadAllLines and rejoins it with a single '\\n', which discards each line's original terminator before the example ever reaches the verifier; no markdown content can survive that round trip.",

                                                                               // Context-dependent: needs project configuration, a disabled-by-default rule, or a real file name
                                                                               // that a standalone compilation unit cannot supply
                                                                               ["RH4001"] = "Example's violation depends on the source file's name ('Foo.cs' in a comment) and the code fix renames the file itself; the verifier's in-memory document is always named 'Test.cs' and only compares resulting text, so neither the real mismatch nor the rename is observable here.",
                                                                               ["RH4009"] = "Example needs a reihitsu.json configuration file listing the allowed namespace, which is outside a standalone compilation unit.",
                                                                               ["RH7305A"] = "The analyzer is disabled by default, so it reports nothing without explicit opt-in configuration.",
                                                                               ["RH8402"] = "Example needs a reihitsu.json configuration file describing the copyright header, which is outside a standalone compilation unit.",

                                                                               // Fix declines its own example, throws, or does not converge within the iteration budget
                                                                               ["RH5302"] = "The code fix never resolves the second (or later) trailing logical operator in a multi-operator condition: each iteration inserts more whitespace before the operator on its own line instead of moving it to the start of the next line, so the diagnostic count never drops and the fix grows the line indefinitely. Reproduced by stepping the fix iteration by iteration. Tracked as a genuine code-fix defect in #725.",

                                                                               // Code-fix output differs from the documented correction: an alternative valid form, or a genuine
                                                                               // divergence tracked separately from this issue
                                                                               ["RH5107"] = "The code fix re-indents the continuation line to a column short of (or unrelated to) the aligned column the rule documents; reproduced with several parameter-list shapes. Tracked as a genuine code-fix defect in #724.",
                                                                               ["RH5501"] = "The code fix inserts a blank line between the attribute list and the moved declaration; reproduced with several declarations following the attribute, so the fix always adds this line even though the rule's own description only asks for a line break. Tracked as a genuine code-fix defect in #726.",
                                                                               ["RH5503"] = "The code fix inserts a blank line between the attribute list and the moved declaration; reproduced with several declarations following the attribute, so the fix always adds this line even though the rule's own description only asks for a line break. Tracked as a genuine code-fix defect in #726.",
                                                                               ["RH7103"] = "The code fix drops a blank line that already separated the two members when it reorders them; reproduced independent of member content. Tracked as a genuine code-fix defect in #727."
                                                                           };

    #endregion // Fields
}