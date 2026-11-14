Role: implementer
Commit: bc39bfa
Verdict: #1345: tests pass; #1346: tests pass; #1347: tests pass; #1348: tests pass; #1270: tests pass; #1092: tests pass (no production change, decision 8); #1153: tests pass; #1617: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every handed-over test was run on `bc39bfa` first: 69 failed in the analyzer filter of `06-test-implementer.md`, exactly the RED rows listed there. No test, assertion, input, or expected output was changed.

## thoenissen/Reihitsu#1345

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | New `GetDocumentationCommentsBehindCode`: the `///` comments that start behind code (`DocumentationCommentUtilities.IsWrittenBehindSource`) and document the following code (`DocumentsFollowingCode`); a comment stranded behind code is not returned | SHARED-DOC-004 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8301…Analyzer.cs` | The separator check moved into `AnalyzeExterior(position)`; the line scan calls it as before, and the first `///` of each comment from `GetDocumentationCommentsBehindCode` is judged the same way (skipped on lines not formatted and lines of comments left as written) | RH8301-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8301…CodeFixProvider.cs` | Rewrites only the text behind the reported `///` up to the line end, so indentation, or the code in front of a behind-code comment, stays | RH8301-004 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8302…Analyzer.cs` | The line scan also starts a comment run on the first line of a behind-code comment | RH8302-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8303…Analyzer.cs` | Line scan: a `///` line whose comment trivia starts behind code is a continuation line and is skipped. New `AnalyzeDocumentationBehindCode`: reports the first `///` of each behind-code comment unless `IsFirstInBlock` or `IsPrecededByCommentOrDirective` exempts it, the existing exemptions | RH8303-001, RH8303-002 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8303…CodeFixProvider.cs` | When spaces or tabs alone stand in front of the reported `///`, unchanged; otherwise the white space between code and `///` is replaced by line break, empty line, and the leading white space of the line that holds the documented declaration's first token, in the detected line ending | RH8303-005, RH8303-006 |

## thoenissen/Reihitsu#1346

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | New `GetRootWithParsedDocumentation(tree)`: the tree's own root, or for mode `None` the same `SourceText` parsed again with mode `Parse` (same spans) | SHARED-DOC-008 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8401…Analyzer.cs` | The node action with the documentation-mode check became one syntax-tree action for every mode, walking the `SingleLineDocumentationCommentTrivia` of `GetRootWithParsedDocumentation`; diagnostics are located on the analyzed tree | RH8401-001, RH8401-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8401…CodeFixProvider.cs` | Finds the comment in `GetRootWithParsedDocumentation`; the text replacement is unchanged | RH8401-006 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8402…Analyzer.cs` | Removed the early return for mode `None` (and the now unused `Microsoft.CodeAnalysis.CSharp` using) | RH8402-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402…CodeFixProvider.cs` | `GetLeadingHeaderSpan` reads the root from `GetRootWithParsedDocumentation`, so a `///` or `/** */` comment is documentation trivia that ends the comment run in every mode | RH8402-006, RH8402-007 |

## thoenissen/Reihitsu#1347

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8307…Analyzer.cs` | An element is judged only when its parent is a `SingleLineDocumentationCommentTrivia`: nested elements and elements of `/** */` comments are not reported | RH8307-001, RH8307-002 |

The code fix (`DocumentationCommentCodeFixUtilities.CanLayOutElement`/`LayOutElementAsync`, shared) is unchanged; its withholding for `/** */` elements is now unreached from RH8307 but the utility is shared, so it stays.

## thoenissen/Reihitsu#1348

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8302…Analyzer.cs` | New `IsFollowedByRegionDirective`: the trivia at the first non-white-space character of the line behind the blank-line run is an active `RegionDirectiveTriviaSyntax` or `EndRegionDirectiveTriviaSyntax`; `ReportFollowingBlankLineRun` (used for `///` and `/** */`) then reports nothing | RH8302-002 |

No formatter change: the `BlankLineRequirementTests` for FMT-BLANK-010 passed before and after (decision 9).

## thoenissen/Reihitsu#1270

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8106…Analyzer.cs` | Reports every empty top-level `<returns>` (`GetDirectTags(...).Where(IsEmpty)`) instead of judging only the first | RH8106-001, RH8106-002 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8306…Analyzer.cs` | `GetSentenceSummary` became `GetSentenceSummaries` (every top-level `<summary>` that reads as a sentence); a property in one part gets one diagnostic per such summary; a partial property is still reported once, through `PartialMemberUtilities.GetReportedPart`, on its first sentence summary | RH8306-001, RH8306-004 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8305…Analyzer.cs` | Judges every top-level `<summary>` of the comment, not only the first; the fix needed no change | RH8305-001 |
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | `CanInheritDocumentation`: an `EventFieldDeclarationSyntax` inherits only when every declared event overrides or implements (was: first variable only) | RH8203-001 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8201InheritdocShouldBeUsedAnalyzer.cs` | Its private copy of the same event-field rule removed; it calls `CanInheritDocumentation` | RH8201-001 (unchanged behavior) |

## thoenissen/Reihitsu#1092

No production change (decision 8). The characterization test `RH8002….VerifyUndocumentedPartWithoutAccessibilityOfPublicPartialClassIsReported` passes.

## thoenissen/Reihitsu#1153

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8032…Analyzer.cs` | A parameter whose identifier has an empty span (no name, as in `extension(long)`) is not reported | RH8032-001 |

## thoenissen/Reihitsu#1617

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | `OverridesOrImplementsMember` judges a partial method, property, or event as a whole: it implements an interface member when the symbol or its other part (`PartialDefinitionPart ?? PartialImplementationPart`) does; new private `ImplementsInterfaceMember` | RH8201-001, RH8203-001, SHARED-DIAG-004 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203…Analyzer.cs` | New `IsPartOfPartialMember` (`partial` modifier, not a type, `PartialMemberUtilities.TryGetParts` succeeds); for such a part the expanded documentation of the symbol, which can be the other part's comment, is not read, so only the part's own `<inheritdoc>` counts | RH8203-001 |

RH8201's analyzer and fix needed nothing beyond the shared inheritance verdict: each part was already analyzed and fixed on its own comment.

## Whole run

### Policy owners
- Behind-code documentation comment of a declaration: `rg -n "IsWrittenBehindSource|DocumentsFollowingCode\(" src --glob '!*Test*'` — `Reihitsu.Core/DocumentationCommentUtilities.cs` (owner of both predicates), `DocumentationCommentFormattingPhase.cs`, `DocumentationCommentCodeFixUtilities.cs`, `RH8309…CodeFixProvider.cs`, and the new `DocumentationAnalysisUtilities.GetDocumentationCommentsBehindCode`, which only combines the two Core predicates and is used by RH8301, RH8302, RH8303.
- Documentation mode `None`: `rg -n "WithDocumentationMode|DocumentationMode == DocumentationMode.None" src --glob '!*Test*'` — `DocumentationModeAnalysisContextExtensions` (skip in mode None for rules about documentation comments), RH0002, `SplitElementDocumentationAnalyzerBase`, the documentation rules registering through the extension, and the new `GetRootWithParsedDocumentation` (only parse-again owner; RH8401 and RH8402 are its only users).
- Inheritance of documentation: `rg -n "OverridesOrImplementsMember|CanInheritDocumentation\(" src --glob '!*Test*'` — `DocumentationAnalysisUtilities` (owner), RH8201, RH8203. RH8201's private event-field copy was folded into the owner.
- Region directive behind blank lines: `rg -n "EndRegionDirectiveTrivia" src/Reihitsu.Analyzer --glob '!*Test*'` — `RegionDirectiveBlankLineAnalyzerBase` (RH5031/RH5032, decides the blank line itself), RH7301, RH7306, `RegionDescriptionAnalyzerBase`; RH8302 only asks whether the following line is an active region directive, it decides no blank line of its own.

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` is empty before this commit (no production change on the branch yet); the working-tree diff this handover commits: `17 files changed, 334 insertions(+), 98 deletions(-)`. No new type; new internal members: `DocumentationAnalysisUtilities.GetRootWithParsedDocumentation`, `GetDocumentationCommentsBehindCode`, private `ImplementsInterfaceMember`; private methods in RH8203, RH8301, RH8302, RH8303.

### Deleted special cases
_N/A._ — no shape is declared unsupported (Q3 B).

### Tests run
Debug build: `Build succeeded`, 0 warnings, 0 errors.

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH8002|FullyQualifiedName~RH8032|FullyQualifiedName~RH8106|FullyQualifiedName~RH8201|FullyQualifiedName~RH8203|FullyQualifiedName~RH830|FullyQualifiedName~RH840"
# Passed!  - Failed:     0, Passed:   637, Skipped:     0, Total:   637
scripts/test.sh --configuration Debug --project analyzer
# Passed!  - Failed:     0, Passed:  8929, Skipped:     0, Total:  8929
scripts/test.sh --configuration Debug --project formatter
# Passed!  - Failed:     0, Passed:  4854, Skipped:     0, Total:  4854
scripts/test.sh --configuration Debug --project core
# Passed!  - Failed:     0, Passed:   651, Skipped:     0, Total:   651
scripts/test.sh --configuration Debug --project architecture
# Passed!  - Failed:     0, Passed:   117, Skipped:     0, Total:   117
```

After the last change (active-directive check in RH8302), `--filter "FullyQualifiedName~RH830|FullyQualifiedName~RH53"`: 630 passed, 0 failed. `scripts/format.sh` over the 17 changed files formatted 0 of them. CLI and tooling projects were not run (no change reaches them).

### Disputed tests or scope
- `GetRootWithParsedDocumentation` parses a mode-`None` tree a second time in RH8401's analyzer and in the RH8401 and RH8402 fixes. It is the literal reading of RH8401-006/RH8402-007 ("same result as mode `Parse`") and avoids a text-level grouping of `//`-comment lines and an XML check, which would be parser-like rules over text. The cost is one extra parse per file, only in projects whose documentation mode is `None`; the lead decides whether that is acceptable.
- RH8401 changed from a syntax-node action to a syntax-tree action (needed to judge mode `None` from the re-parsed root); behavior in mode `Parse`/`Diagnose` is unchanged by its tests.

### Architecture findings
_None._

### Follow-up candidates
- Other readers of a partial member's expanded documentation through `XmlDocumentationExpander` (RH8032, RH8033, RH8101, RH8105, RH8108, and others) may read the other part's comment the same way RH8203 did; already listed by 04-skeptic in `00-lead.md`.
- `DocumentationCommentCodeFixUtilities.CanLayOutElement` still withholds the layout for `/** */` elements; RH8307 no longer reports them, so for RH8307 that branch is unreached. Whether other callers still need it was not checked (small; candidate for the cleaner or a follow-up).
