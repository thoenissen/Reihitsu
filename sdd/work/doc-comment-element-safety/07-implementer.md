Role: implementer
Commit: afdebac
Verdict: #1329: tests pass; #1389: tests pass; #1241: test disputed; #1344: test disputed; #1242: tests pass

Inputs: `00-lead.md`, `03-skeptic.md`, `04-spec-author.md`, `06-spec-author.md`, `05-test-implementer.md` and the specs they name. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before any change, the handed-over tests were red as `05-test-implementer.md` states: formatter 33 failed of 56, Core 5 failed of 44, analyzer 56 failed of 205 (53 plus the 3 CRLF tests).

After the change, every handed-over test passes except three Fix All tests. A harness setting makes those three fail whatever the production code does (see *Disputed tests or scope* under #1241). No test file was changed.

## thoenissen/Reihitsu#1329

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DocumentationCommentUtilities.cs` | New `HasOwnEndTag`: the element is closed by an end tag whose name, prefix and local name, equals the start tag's name, compared ordinal. New `ContainsElementWithoutOwnEndTag`, which checks every element at any depth. New `IsLeftAsWritten`: a single-line comment is left as written when it is stranded behind code or holds an element without an end tag of its own. | FMT-DOC-013, SHARED-DOC-017 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | After relocation, the element rebuild and the `///` separator normalization skip a comment for which `IsLeftAsWritten` is true. Relocation itself is unchanged, so a relocated comment is still moved. The class summary now states the exemption. | FMT-DOC-013, FMT-DOC-004 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8301…Analyzer.cs` | Skips every line of a `///` comment for which `IsLeftAsWritten` is true. | RH8301-002 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8305…Analyzer.cs` | Returns early for a comment for which `IsLeftAsWritten` is true. | RH8305-002 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8307…Analyzer.cs` | New `IsInCommentLeftAsWritten`, which tests the element's enclosing `DocumentationCommentTriviaSyntax`. A `/** */` comment is unaffected, because `IsLeftAsWritten` answers `false` for anything that is not a single-line documentation comment. | RH8307-002 |

The RH8305 and RH8307 code fixes are unchanged. They are no longer reached for these comments, because no diagnostic is reported there.

### Policy owners

- `rg -n "EndTag.IsMissing|EndTag\.Name" src --glob '!*Test*'` found one owner before the change, `DocumentationCommentCodeFixUtilities.CanRemoveElement`. Afterwards there are no hits outside `DocumentationCommentUtilities.HasOwnEndTag`.
- The "end tag of its own" definition lives in `Reihitsu.Core`. The formatter cannot reference `Reihitsu.Analyzer` or `Reihitsu.Analyzer.CodeFixes`, and all three consumers need it: the formatter, the layout analyzers, and the fixes.
- Note for the spec checker: SHARED-DOC-017's definition is now implemented in `DocumentationCommentUtilities`, which is a code owner of the FMT-DOC spec, while `DocumentationCommentCodeFixUtilities` (SHARED-DOC's code owner) calls it. I left the code-owner lists unchanged. Whether that needs a spec note is the spec author's call.

### Tests run

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~DocumentationCommentWithoutOwnEndTagTests|FullyQualifiedName~StrandedDocumentationCommentTextTests|FullyQualifiedName~ByteOrderMarkTests|FullyQualifiedName~CleanupPhaseTests"` → `Passed! - Failed: 0, Passed: 56`
- The analyzer filter in *Whole run* covers RH8301, RH8305 and RH8307. All of their tests pass.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1389

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DocumentationCommentUtilities.cs` | New `IsWrittenBehindSource`, moved unchanged from the phase's private `IsAfterSourceOnSameLine`: the comment starts behind characters that are neither white space nor `U+FEFF`. New `DocumentsFollowingCode`, moved from `ReihitsuFormatterHelpers`. New `IsStrandedBehindCode`, which combines the two. | FMT-DOC-003 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `DocumentsFollowingCode` now delegates to the Core method, so the BlankLines caller (`BlankLineTriviaBoundaryRewriter`) keeps its behavior and stays unedited. New `StartsWithByteOrderMark` and `CreateByteOrderMarkTrivia` give the two BOM-keeping steps one owner. | FMT-DOC-003, FMT-CLEAN-009 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | `RegisterRelocation` uses `IsWrittenBehindSource` and `IsStrandedBehindCode`. The private copy `IsAfterSourceOnSameLine` is removed. The stranded comment is skipped by the `IsLeftAsWritten` guard (see #1329). | FMT-DOC-003 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | `CollapseLeadingBlankLines`: when the removed run starts with white space that begins with `U+FEFF`, a `U+FEFF`-only trivia is put in front of what remains. This is the one narrow BlankLines change of answer 4A, and the method summary says so. | FMT-CLEAN-009, FMT-BLANK-007 |
| `src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs` | `CleanWhitespaceBeforeEndOfLine(list, startsText)`: in the leading trivia of the token at text position 0, the first whitespace trivia before a line break is replaced by `U+FEFF` alone when it starts with the BOM, and is kept as it is when it already is the BOM alone. The stale comment in `CleanToken` ("BOM is preserved automatically…") is removed (FC-031). | FMT-CLEAN-009, FMT-CLEAN-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8301/RH8305/RH8307…Analyzer.cs` | Stranded comments are silent through `IsLeftAsWritten` (see #1329). | RH8301-002, RH8305-002, RH8307-002 |

The FMT-DOC-011 BOM case (rebuilt lines repeating `U+FEFF`) is fixed through `GetContinuationPrefix`; see #1344.

### Policy owners

- `rg -n "FEFF" src --glob '!*Test*'` found these owners:
  - **Copy removed:** `DocumentationCommentFormattingPhase.IsAfterSourceOnSameLine`, now `DocumentationCommentUtilities.IsWrittenBehindSource`.
  - **Remaining private copy:** `DocumentationCommentFormattingPhase.HasOwningLeadingSourceOnSameLine`. It applies the same "not white space and not BOM" character test to a different span. It is unchanged.
  - **Off-limits for this run:** `IndentationRewriter` (two sites).
  - **Other policies, unchanged:** `RH5204…Analyzer` (indentation measurement) and `RH7304…CodeFixProvider` (region indentation).
- `rg -n "GetFirstToken\(\) == " src --glob '!*Test*'` found:
  - `ReihitsuFormatterHelpers.DocumentsFollowingCode`, which now delegates to Core;
  - `CommentPositionUtilities.IsOwnedByNestedConstruct`, `ListElementInteriorJoinRewriter`, `RH3002`, and `RH5036`, which are other policies.

### Tests run

- The formatter command under #1329 (56 passed) includes `StrandedDocumentationCommentTextTests`, `ByteOrderMarkTests` and `CleanupPhaseTests`.

### Disputed tests or scope

_None._

### Follow-up candidates

- `HasOwningLeadingSourceOnSameLine` in `DocumentationCommentFormattingPhase` keeps its own "source character" test, which excludes white space and `U+FEFF`. A shared character predicate in `DocumentationCommentUtilities` could serve both callers.

## thoenissen/Reihitsu#1241

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs` | `CanRemoveElement` renamed to `CanRewriteElement`, because RH8204 unwraps rather than removes. It now uses `DocumentationCommentUtilities.HasOwnEndTag` instead of `EndTag.IsMissing`. The remarks describe the end tag of another name. | SHARED-DOC-017, RH8107-007, RH8202-004 |
| `RH8107…CodeFixProvider.cs`, `RH8202…CodeFixProvider.cs` | Call the renamed guard. | RH8107-007, RH8202-004 |
| `RH8204DoNotUsePlaceholderElementsCodeFixProvider.cs` | `RegisterCodeFixesAsync` skips a diagnostic for which `CanRewriteElement` is false. | SHARED-DOC-017, RH8204-003 |
| `RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider.cs` | New `CanReorderElements`: the fix is withheld when a top-level `XmlElementSyntax` has no end tag of its own, or when anything other than an element, an empty element, or white-space-only text stands at the top level. White-space-only text is judged by `ValueText`, as RH8304 does. New `GetDocumentationComment`, shared by `ApplyCodeFixAsync` and `RegisterCodeFixesAsync`. `RegisterCodeFixesAsync` is now async, reads the root, and registers only when `CanReorderElements` holds. | SHARED-DOC-017, RH8309-004, RH8309-005 |

### Policy owners

- `rg -n "CanRemoveElement|CanRewriteElement" src` finds one owner, `DocumentationCommentCodeFixUtilities.CanRewriteElement`. Its callers are RH8107, RH8202 and RH8204.

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH8107|FullyQualifiedName~RH8202|FullyQualifiedName~RH8204|FullyQualifiedName~RH8301|FullyQualifiedName~RH8304|FullyQualifiedName~RH8305|FullyQualifiedName~RH8307|FullyQualifiedName~RH8309"` → `Failed! - Failed: 3, Passed: 202, Total: 205`. The three failures are the disputed tests below.

### Disputed tests or scope

Three Fix All tests cannot pass with any implementation, because of how the verifier handles markup in the fixed state:

- `RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests.VerifyFixAllLeavesReturnsElementWithoutEndTagOfItsOwnUnchanged` (SHARED-DOC-017)
- `RH8204DoNotUsePlaceholderElementsAnalyzerTests.VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged` (SHARED-DOC-017)
- `RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests.VerifyFixAllLeavesCommentsWithWithheldFixUnchanged` (RH8309-005, SHARED-DOC-017). This one belongs to #1344 as well.

**What they assert.** Each `fixedSource` marks the withheld diagnostic as remaining, as `{|RH####:…|}`, and calls `Verify(source, fixedSource)` with no configuration.

**Observed after the change.** All three fail at the same step. The verifier's output was:
- RH8107 and RH8204: `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"`.
- RH8309: the same message, with `expected "0" actual "2"`.

The remaining diagnostics are the withheld ones at the marked spans, and the fixed text matches.

**Why.** By default, Microsoft.CodeAnalysis.Testing ignores markup diagnostics in `FixedState` and `BatchFixedState` for an analyzer the fix covers (`MarkupMode.IgnoreFixable`). So the marked remaining diagnostic is never expected. Other tests in the repository that expect a diagnostic to remain set `test.FixedState.MarkupHandling = MarkupMode.Allow`, for example:
- `RH4107ProtectedFieldCasingAnalyzerTests.cs:265-266`
- `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs:1310`
- `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.cs:355`

**Evidence.**
1. I temporarily replaced the three `await Verify(source, fixedSource);` lines with `Verify(source, fixedSource, test => { test.FixedState.MarkupHandling = MarkupMode.Allow; test.BatchFixedState.MarkupHandling = MarkupMode.Allow; })`.
2. I ran `--filter "FullyQualifiedName~VerifyFixAllLeaves"`. Result: `Passed! - Failed: 0, Passed: 3`.
3. I reverted the test files with `git checkout`, so they are unchanged in this commit.

**What the spec says.** SHARED-FIX-002 and SHARED-DOC-017: a withheld diagnostic is still reported and stays after Fix All. The production behavior matches. Only the harness configuration of the three tests disagrees.

**Routing.** To the test implementer: add `MarkupMode.Allow` for `FixedState` (and `BatchFixedState`) in these three tests.

### Follow-up candidates

- The comment in `RH8309…CodeFixProvider.ReorderElements` still calls `<inheritdoc>`, `<seealso>` and `<include>` unknown elements, although all three are in `CanonicalElementOrder`. That method's body is unchanged, so the comment was left alone. The skeptic listed this candidate too.

## thoenissen/Reihitsu#1344

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/DocumentationCommentUtilities.cs` | `GetContinuationPrefix` returns the spaces and tabs at the start of the line, after a leading `U+FEFF` if there is one, followed by `/// `. Code, a delimited comment, or the BOM in front of `///` is no longer copied. The summary is rewritten (FC-031). This is the approved shared-helper change (answer 7A). Both consumers get it: `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync` (RH8304) and `DocCommentElementNormalizer`, whose three call sites are unedited. | RH8304-004, FMT-DOC-011 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8304…Analyzer.cs` | Returns early for a comment for which `IsLeftAsWritten` is true. | RH8304-002 |
| `RH8309…CodeFixProvider.cs` | Withholds the fix for top-level text that is not white space, an XML comment, a CDATA section, or a processing instruction (see #1241). | RH8309-005 |

### Policy owners

- `rg -n "GetContinuationPrefix\(" src --glob '!*Test*'` found two consumers, `DocCommentElementNormalizer` (3 calls) and `DocumentationCommentCodeFixUtilities` (1 call). Neither was edited, and both go through the one helper.
- `DocCommentElementNormalizer.StripDocumentationPrefix` uses the prefix to strip continuation lines. With the narrower prefix, the continuation lines of a BOM-prefixed comment now match it directly instead of falling back to the trimmed branch, and the content is the same either way.

### Tests run

- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~DocumentationCommentUtilitiesTests"` → `Passed! - Failed: 0, Passed: 44`
- In the analyzer filter under #1241, every RH8304 test passes, and every RH8309-005 test passes except the disputed Fix All test.

### Disputed tests or scope

`RH8309…AnalyzerTests.VerifyFixAllLeavesCommentsWithWithheldFixUnchanged`, as described under #1241.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1242

### Changes

_None._ The owner move is in `sdd/` only. The re-tagged SHARED-DOC-018 tests pass with the renamed guard.

### Policy owners

_N/A._

### Tests run

- Covered by the analyzer filter under #1241. All RH8107 and RH8202 tests pass except the RH8107 disputed Fix All test.

### Disputed tests or scope

_None._

### Follow-up candidates

_None._

## Whole run

### Scope

- **Production paths changed (14):**
  - the paths the handovers name: the doc-comment phase, `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines`, `CleanupPhase`, `GetContinuationPrefix`, `DocumentationCommentCodeFixUtilities`, the RH8107, RH8202, RH8204 and RH8309 fixes, and the RH8301, RH8304, RH8305 and RH8307 analyzers;
  - one addition, `ReihitsuFormatterHelpers`, for the BOM helpers and the delegation of `DocumentsFollowingCode`.
- **Not touched:** `DocCommentElementNormalizer`, the RH8305 and RH8307 fixes, the Indentation phase, `FormattingTextAnalysisUtilities`, and every other off-limits file. In the BlankLines phase, only `CollapseLeadingBlankLines` changed (answer 4A).
- **For the lead (escalation 5).** Besides the approved `GetContinuationPrefix` change, `DocumentationCommentUtilities` (Reihitsu.Core, `IsPackable=false`) gained six public static methods: `HasOwnEndTag`, `ContainsElementWithoutOwnEndTag`, `IsWrittenBehindSource`, `DocumentsFollowingCode`, `IsStrandedBehindCode` and `IsLeftAsWritten`.
  - They are additions. No existing member changed behavior apart from `GetContinuationPrefix`, and `ReihitsuFormatterHelpers.DocumentsFollowingCode` keeps its result by delegating.
  - They are public only because Core's types are shared across assemblies that way. The package does not ship them as an API.
  - I chose this over private copies in the formatter, the analyzer and the fixes, which FC-015 forbids. Please confirm that this addition is inside the approved scope.

### Commands and results

- `scripts/build.sh --configuration Debug` → 0 warnings, 0 errors.
- `scripts/format.sh <14 changed files>` → `Formatted 0 of 14 file(s).`
- Full test projects, Debug:
  - formatter: `Passed! - Failed: 0, Passed: 2724`
  - core: `Passed! - Failed: 0, Passed: 533`
  - analyzer: `Failed! - Failed: 3, Passed: 6109, Total: 6112`. The 3 are the disputed Fix All tests.
  - architecture: `Passed! - Failed: 0, Passed: 84`
  - cli: `Passed! - Failed: 0, Passed: 250`
