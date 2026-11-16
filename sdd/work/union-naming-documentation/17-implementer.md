Role: implementer
Commit: see the commit that adds this handover (subject "Refuse unions with parameter elements in any case or a failing expansion [skip ci]")
Verdict: tests pass

Architecture-finding refusal (lead decision 6, `00-lead.md`) for audit finding #2 (`14-auditor.md`), against SHARED-DOC-019 as widened in 1a32759 (`15-spec-author.md`) and the tests of `16-test-implementer.md` (9283c29). Not a repair round. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the 20 handed-over tests showed 14 failures and 6 passes, exactly as `16-test-implementer.md` lists.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | Part (a): private `HasParameterElement` no longer compares tag names itself; it asks the shared lookup `DirectDocumentationSyntaxChecker.GetTagsIncludingNested` for `param` and `paramref` on every documentation comment of the declaration. That lookup matches the local name (prefix ignored) case-insensitively and returns a non-empty result exactly when the tag occurs at any depth (top level first, otherwise nested), so it states "at any depth, any letter case" with the one existing owner. Part (b): the helper, renamed `IsUnionWithParameterElement` → `IsUnjudgedUnion`, also returns `true` for a union when `XmlDocumentationExpander.CanExpandDocumentation` reports that the compiler's expansion of the union symbol's documentation fails. For a partial union the symbol is the joined one, so the failure refuses every part. Summary rewritten (FC-031) | SHARED-DOC-019 (a), (b) |
| `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs` | New `internal static bool CanExpandDocumentation(MemberDeclarationSyntax, SemanticModel, CancellationToken)`: calls `GetDocumentationCommentXml(expandIncludes: true)` on the declared symbol, the same call `ParseExpandedDocumentation` makes, and returns `false` when it throws (`catch (Exception) when (exception is not OperationCanceledException)`; cancellation passes on). The failure is observed, not predicted | SHARED-DOC-019 (b) |
| `SplitElementDocumentationAnalyzerBase.cs`, `RH8108…Analyzer.cs`, `RH8203…Analyzer.cs` | Call the renamed helper; position unchanged (before every expander call) | RH8035-001, RH8036-001, RH8108-001, RH8203-001 |

Order inside `IsUnjudgedUnion`: kind check, then the syntactic check of the analyzed part, then other parts of a `partial` union, then the expansion probe. So the expansion probe runs only for a union that passed both syntactic checks; non-union declarations return at the kind check as before.

## Policy owners
- Unjudged union: `rg -n "IsUnjudgedUnion|IsUnionWithParameterElement" src` → the helper and its three callers; no other name left in `src`. One owner.
- Tag lookup at any depth: `rg -n "GetTagsIncludingNested|GetTagName\(" src/Reihitsu.Analyzer/Core` → `DirectDocumentationSyntaxChecker` owns the case-insensitive comparison; `HasParameterElement` now reuses it and holds no comparison of its own (the ordinal copy behind #2 is gone).
- Compiler documentation expansion: `rg -n "GetDocumentationCommentXml" src/Reihitsu.Analyzer` → two calls, both in `XmlDocumentationExpander` (`ParseExpandedDocumentation` and the new `CanExpandDocumentation`). The expansion stays with its one owner; no rule calls the compiler API directly.

## Production diff size
`git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'` (working tree before commit): 14 files changed, 316 insertions, 3 deletions. Delta of this step: 5 files, 53 insertions, 21 deletions. No new type; one new `internal` method (`CanExpandDocumentation`), one `internal` method renamed.

## Deleted special cases
_N/A._

## Tests run
- `scripts/format.sh` over the five changed files → Formatted 0 of 5.
- Handed-over tests before the change (`16-test-implementer.md` filter) → Failed: 14, Passed: 6.
- Same filter plus the earlier SHARED-DOC-019 and partial-union tests (`…|PartialUnion|ParamElement|ParamrefElement|ParameterElement`) after the change → Passed: 78, Failed: 0.
- Probe of part (b) alone (not committed): with the syntactic check temporarily matching nothing, the same filter showed 0 AD0001; the 17 failures were Parse-mode, nameless `<param>`, and partial-part cases, which only the syntactic refusal covers (mode-independence, SHARED-DOC-019). The file was restored and the diff confirmed.
- `scripts/test.sh --configuration Debug --project analyzer` → Passed: 10237, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` → Passed: 118, Failed: 0.

## Disputed tests or scope
- **Cost of part (b).** For a union that passes the syntactic checks, the documentation is expanded once in the probe and once more by the rule. The compiler computes the expansion per symbol; whether it caches the `expandIncludes: true` result was not measured. Unions only, so the extra work is limited to that declaration kind.
- **General catch.** `CanExpandDocumentation` catches every exception except cancellation, because which exception the compiler throws is the compiler's (today a `NullReferenceException`). It is the first catch-all in `Reihitsu.Analyzer`; no analyzer warning was raised for it. If the lead or cleaner prefers catching only `NullReferenceException`, that would re-introduce a prediction of the compiler, which decision 6 (b) rules out.

## Architecture findings
_None._ Decision 6 removes the prediction #2 showed: the tag comparison now has one owner, and a failed expansion is observed instead of predicted.

## Follow-up candidates
_None._
