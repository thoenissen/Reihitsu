Role: implementer
Commit: 475e353
Verdict: test disputed (two existing RH8201 tests assert a re-indented member, which RH8201-005 rules out; every handed-over test passes)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work follows `00-lead.md` (Q1 = A, decisions 3–18), the specs at `fc567de`, and `03-test-implementer.md`. No test file was changed.

## thoenissen/Reihitsu#1269

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8201InheritdocShouldBeUsedCodeFixProvider.cs` | `CreateInheritdocTrivia` parses `/// <inheritdoc/>` + document line ending instead of building it from `SyntaxFactory` tokens, whose elastic trivia made the code action's post-processing re-indent the comments, directives, and member around it and rewrite their line endings. `ReplaceDocumentation` writes the indentation of the replaced `/**` in front of an ordinary comment or the member that followed `*/` on its line | RH8201-005, SHARED-FIX-006, SHARED-FIX-008 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8202ValueTagMustNotBeUsedCodeFixProvider.cs` | Removes every `<value>` of the reported comment outside `<code>`/`<example>` (outermost only, each with an end tag of its own or empty) in one application, through the existing `GetElementRemovalSpan` | RH8202-001, RH8202-003, SHARED-FIX-001 |
| `src/Reihitsu.Analyzer/Core/DirectDocumentationSyntaxChecker.cs` | New `GetDocumentingTags`: every matching tag, top-level and nested, outside samples (the set `GetFirstDocumentingTag` reports the first of) | RH8202-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8204DoNotUsePlaceholderElementsCodeFixProvider.cs` | Content is the source text between the start tag's end and the end tag's start (syntax tree), not an `XElement` re-serialization; the textual fallback stays for a span that is not the element | RH8204-003, SHARED-FIX-006, SHARED-FIX-008 |

### Disputed tests or scope

Two existing tests (not written by this run) fail and, I believe, assert behavior RH8201-005 rules out:

- `RH8201InheritdocShouldBeUsedAnalyzerTests.VerifyDiagnosticForFieldLikeOverrideEvent` (tagged RH8201-001, RH8201-004)
- `RH8201InheritdocShouldBeUsedAnalyzerTests.VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration` (tagged RH8201-001, -004, -005)

In both inputs the markup closes as `|}        public override event …`, so after the markup is removed the member starts at column 8; the expected output has it at column 4. Before this change the elastic `<inheritdoc/>` trivia let the post-processing re-indent the member. RH8201-005 now says "The member's lines MUST stay as they are", and the fix keeps column 8. Observed: `/// <inheritdoc/>` at 4, `public override event …` at 8. Most likely the inputs meant `|}    public` (as every other test of the class writes it); that correction, or an expected output at column 8, is the test implementer's call.

## thoenissen/Reihitsu#1342

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/DocumentationCommentCodeFixUtilities.cs` | New `LayOutElementAsync` / `CanLayOutElement` (shared by RH8305 and RH8307): the formatter lays out a copy of the reported comment alone (indentation removed, documenting an empty class in a detached compilation unit, via `ReihitsuFormatter.FormatNode`); the element at the same position in the formatted comment replaces only the reported element's span, with the reported line's indentation in front of its further lines. Fix withheld when the element has no end tag of its own, the comment is not `///`, or the comment starts behind code (`DocumentationCommentUtilities.IsWrittenBehindSource`) | RH8305-004/-005, RH8307-004/-005, SHARED-FIX-006, FMT-DOC-008/-009 |
| `…/RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider.cs`, `…/RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider.cs` | No longer format the owning declaration with `FormatNodeInDocumentAsync`; register through `CanLayOutElement` and apply `LayOutElementAsync`. The `FindUnsupportedSyntax(declaration)` check is gone with the declaration formatting | RH8305-004/-005, RH8307-004/-005 |
| `…/RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider.cs` | Sorts by text changes: each canonical element's text moves as written into the slot of the element it replaces; the declaration is no longer formatted. Withheld for a comment written behind code and (#1343) for a reported comment that is not the declaration's first documentation comment | RH8309-004, RH8309-006, RH8309-007 |
| `…/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs` | On the replace path the span ends behind the run's last comment and the white space behind it on its line; the line break and the blank lines after it stay. Summary rewritten | RH8402-006 |

### Disputed tests or scope

- `/** */` comments: RH8307's analyzer can report an element in a `/** */` comment; `CanLayOutElement` withholds the fix there (RH8307-004 covers `///` only). The old fix formatted the declaration; whether that changed a `/** */` element was not checked. No test covers it.
- The formatter path is reached only through `ReihitsuFormatter.FormatNode` (FMT-PIPE-009). Calling `DocCommentElementNormalizer.BuildReplacement` directly was tried first and is banned by `CodeFixFormatterAccessTests` (its DataRow names the type), so it was reverted. See *Architecture findings*.

## thoenissen/Reihitsu#1343

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.cs` | `TryGetFirstMeaningfulTextSpan` runs the span from the first non-white character through every following text token (references included) up to the line break or the end of the text node | RH8308-001, RH8308-003, RH8308-004 |
| `…/RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider.cs` | `GetDocumentationComment` returns the reported comment only when it is the declaration's first documentation comment (`///` or `/** */`) | RH8309-007 |
| `…/RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider.cs` | Withheld when U+FEFF or U+001A stands in front of a line's `///` (first line: the text in front of the comment on its line; other lines: the exterior trivia). `DocumentationCommentUtilities` (Core) unchanged, per decision 7 | RH8401-005 |

### Disputed tests or scope

_None._

## thoenissen/Reihitsu#1459

### Changes
| File | What changed | Requirement |
|---|---|---|
| (RH8305/RH8307 changes under #1342) | The comment is laid out without `FormatNodeInDocumentAsync`, so a declaration directly in the compilation unit gets the same layout | RH8305-004, RH8307-004 |
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | New `IsPrimaryConstructorRun(context)`: true when the syntax node action runs with a primary constructor as `ContainingSymbol` (declared by a `TypeDeclarationSyntax`) | RH8305-004, RH8307-004 (one diagnostic per element) |
| `…/RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.cs`, `…/RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.cs` | Return early on that second run | as above |

### Disputed tests or scope

- **Analyzer change outside the named surfaces.** The `public record R(int A);` rows of `RH8305…VerifyFixLaysOutSummaryOfDeclarationDirectlyInTheCompilationUnit` and `RH8307…VerifyFixLaysOutElementOfDeclarationDirectlyInTheCompilationUnit` failed on `Mismatch between number of diagnostics returned, expected "1" actual "2"`, not on the fix: Roslyn runs syntax node actions for a positional record's nodes once for the type and once for its primary constructor, so RH8305 and RH8307 reported the same element twice. (03-test-implementer.md says every row failed on the expected difference; for these two rows it did not.) Confirmed by probing `context.ContainingSymbol`. The guard keeps explicit constructors (their declaring syntax is a `ConstructorDeclarationSyntax`); existing constructor tests still pass. No test pins a class/struct with a primary constructor separately.

## Policy owners

- Element layout (FMT-DOC-008/009): `rg -n "BuildReplacement|BuildExpandedElement|GetContinuationPrefix" src --glob '!*Test*'` — owner `DocCommentElementNormalizer` (formatter, reached through `ReihitsuFormatter.FormatNode`); `DocumentationCommentUtilities.GetContinuationPrefix` (Core) is used by it and by `MoveContentToNextDocumentationLineAsync` (RH8304), unchanged. No copy added.
- Behind-code: `rg -n "IsWrittenBehindSource|IsStrandedBehindCode|IsLeftAsWritten" src --glob '!*Test*'` — `DocumentationCommentUtilities` (Core), reused by RH8305/RH8307/RH8309 fixes.
- Element removal: `rg -n "GetElementRemovalSpan|CanRewriteElement" src --glob '!*Test*'` — `DocumentationCommentCodeFixUtilities`, reused by RH8202.
- Documenting tags: `rg -n "GetFirstDocumentingTag|GetTagsIncludingNested|IsInsideSample" src --glob '!*Test*'` — `DirectDocumentationSyntaxChecker`; `GetDocumentingTags` added there beside them.
- U+FEFF/U+001A: `rg -n "uFEFF|u001A|ByteOrderMark" src --glob '!*Test*'` — `DocumentationCommentUtilities.ByteOrderMark` (private, Core) and the new private check in the RH8401 fix. Two places decide "stray character in front of `///`" (see *Architecture findings*, decision 7 already took it).

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 14 files changed, 409 insertions(+), 206 deletions(-). No new type. New members: `DirectDocumentationSyntaxChecker.GetDocumentingTags`, `DocumentationAnalysisUtilities.IsPrimaryConstructorRun`, `DocumentationCommentCodeFixUtilities.LayOutElementAsync`/`CanLayOutElement` (internal) and private helpers. Production paths beyond the handovers' named surfaces: `DocumentationAnalysisUtilities` and the RH8305/RH8307 analyzers (record duplicate, above).

## Deleted special cases

_N/A._ (The `ReihitsuFormatter.FindUnsupportedSyntax` checks in the RH8305/RH8307/RH8309 fixes are gone because those fixes no longer format the declaration, not because a shape was declared.)

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH8201|FullyQualifiedName~Documentation.RH8202|FullyQualifiedName~Documentation.RH8204"
# Failed: 2, Passed: 143, Total: 145 (the two disputed RH8201 event tests)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH8305|FullyQualifiedName~Documentation.RH8307|FullyQualifiedName~Documentation.RH8309"
# Passed: 137, Total: 137
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH8402"
# Passed: 38, Total: 38
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Documentation.RH8308|FullyQualifiedName~Documentation.RH8309|FullyQualifiedName~Documentation.RH8401"
# Passed: 102, Total: 102
scripts/test.sh --configuration Debug --project analyzer
# Failed: 2, Passed: 8071, Total: 8073 (the same two)
scripts/test.sh --configuration Debug --project architecture
# Passed: 117, Total: 117
```

`scripts/format.sh` over the 14 production files ran before the final runs; `scripts/build.sh --configuration Debug` builds without warnings.

## Whole run

### Architecture findings

- **No `ReihitsuFormatter` entry point lays out one documentation element.** Components: the RH8305/RH8307 fixes and the formatter's documentation phase (`DocCommentElementNormalizer`, owner of FMT-DOC-008/009). Decision shared: the layout of a reported element. RH8305-004/RH8307-004 now require the formatter's element layout and nothing else, but every entry point formats a node and everything in it (and `FormatNodeInDocumentAsync` keeps a root child's leading trivia, FMT-PIPE-010), while `CodeFixFormatterAccessTests` bans `DocCommentElementNormalizer` for code fixes. The fixes therefore detour: format a detached copy of the comment, map the element back by its ordinal among the comment's `XmlElementSyntax` nodes, re-indent its further lines with the reported line's indentation (~50 lines in `DocumentationCommentCodeFixUtilities.LayOutElementAsync`). That re-indent step is the one place that states something the formatter also decides (the continuation prefix in front of `///`), for the case where the formatter would place the comment at another column. Structural options for the lead: (a) keep the detour; (b) declare `DocCommentElementNormalizer.BuildReplacement` callable from code fixes (rename to a `…Utilities` type and change the ban's DataRow — a test change); (c) a narrow `ReihitsuFormatter` entry point for one documentation element. Input that shows it: any `/// {|RH8305:<summary>S</summary>|}` on a declaration directly in the compilation unit.
- **RH8401 analyzer vs `DocumentationCommentUtilities.RewriteDocumentationLine`** (skeptic's finding): unchanged; decision 7 refused the class, implemented as RH8401-005.

### Follow-up candidates

- Other documentation analyzers that register syntax node actions on documentation trivia or XML nodes (RH8308, RH8309, RH8204, …) probably report twice on a positional record or a type with a primary constructor, for the same Roslyn reason as RH8305/RH8307. Not run. The guard could live in `RegisterSyntaxNodeActionWithDocumentationModeCheck` instead of each analyzer.
- RH8309 accessor comment (already in `00-lead.md`).
