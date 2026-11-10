Role: auditor
Commit: 1d852e3
Verdict: findings (#1269: one low finding; #1342: one medium finding; #1343: pass; #1459: pass)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: checkout is `4555727`, which differs from `1d852e3` only in `sdd/work/doc-fix-scope/00-lead.md`. `origin/main` (`git ls-remote`) is `8ac5705a03d0344a46ad991b4ab26d246a1dadd3`, which is the merge base, so it is merged. The diff `8ac5705..1d852e3` (41 files outside `sdd/work/`) matches the changes listed in handovers 02 to 08. `scripts/lane-check.sh bug-fix`: confirmed.

Evidence: static tracing, plus probe tests in throwaway worktrees of `1d852e3` and `8ac5705` under the scratchpad. They were removed afterwards, and no repository file other than this handover was changed. Probe results are quoted where a finding or a dismissal depends on them. The build and the focused test results of 07/08 are trusted.

## thoenissen/Reihitsu#1269

### Scope

Full audit; head `1d852e3`, base `8ac5705`. Files: RH8201, RH8202, and RH8204 code fixes, `DirectDocumentationSyntaxChecker.GetDocumentingTags`, the specs for RH8201-005, RH8202-001/003, and RH8204-003, and their tests.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8204DoNotUsePlaceholderElementsCodeFixProvider.cs`, `ExtractInnerContent` `default:` branch and `StripPlaceholderTags` | – (dead branch) | – | PR-introduced | The analyzer reports `xmlNode.GetLocation()` of an `XmlElementSyntax` or `XmlEmptyElementSyntax`. For that exact span, `FindNode(…, getInnermostNodeForTie: true)` always returns the element itself, because no child of either node has the element's span. Before this change, the textual fallback ran when `XElement.Parse` failed. Now it can no longer run, and the implementer's description "the textual fallback stays for a span that is not the element" names a case the analyzer never produces. | Delete the fallback and `StripPlaceholderTags`, or keep it and say in its comment that it is defensive only. Owner's call. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8202: the `<value>` elements one application removes | `GetDocumentingTags(comment, "value")`: every `XmlElement`/`XmlEmptyElement` descendant, name matched with OrdinalIgnoreCase, not inside `<code>`/`<example>`. Narrowed by `CanRewriteElement`, outermost only, plus the reported span | Analyzer `GetFirstDocumentingTag` = `GetTagsIncludingNested` (same name match, same `IsInsideSample`) | The removal set is a superset of every element the analyzer can report next, so no second application is needed. A `<value>` without its own end tag is excluded by `CanRewriteElement` and is outside RH8202-001's coverage | holds |
| RH8201: where `<inheritdoc/>` and a member behind `*/` start | The trivia directly in front of the replaced comment (`previousTrivia`), used only when it is `WhitespaceTrivia` | RH8201-005: "the column in which the comment's first `///` or `/**` starts" | Agree. For column 0 the trivia in front is an end-of-line, and nothing is written | holds |
| RH8204: the content kept | The source text from `StartTag.Span.End` to `EndTag.Span.Start` | RH8204-003: start and end tags removed, content kept character for character | Agree | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which tags document the member (RH8202) | `DirectDocumentationSyntaxChecker.GetTagsIncludingNested` (nested fallback) and the new `GetDocumentingTags`, both in `src/Reihitsu.Analyzer/Core/DirectDocumentationSyntaxChecker.cs`. Both use the same name predicate and the same `IsInsideSample` | One type, identical predicate, parity holds (the RH8202 nested and top-level tests exercise both) |
| Element removal span | `DocumentationCommentCodeFixUtilities.GetElementRemovalSpan`/`CanRewriteElement`, reused | one owner |
| `<inheritdoc/>` text | `RH8201…CodeFixProvider.CreateInheritdocTrivia` only | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8201 `VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedComment`, plus the `…WithCarriageReturnLineFeed` variants and `VerifyFixKeeps…` | Every trivia except the comment keeps its line, column, and line ending | Any re-indented directive or member, or one LF in a CRLF document | `Verify`: exact text, LF and CRLF | adequate |
| RH8201 `VerifyMultiLineDocumentationCommentReplacementUsesDocumentEndOfLine` | `/// <inheritdoc/>` ends with the document's line ending | `Environment.NewLine` in an LF document | `ApplyCodeFixAsync`, substring | adequate for its claim; on Linux it cannot fail, which 03 states |
| RH8202 several-elements tests | All `<value>` removed in one application | A second iteration, or a leftover `<value>` | `Verify` (iteration count enforced) | adequate |
| RH8204 content tests (`VerifyPlaceholderContentIsKept`, CRLF raw-text test) | Content kept byte for byte | A re-serialized reference, quote, `>`, white space, or LF | `Verify` and raw `ApplyCodeFixAsync` | adequate |

### Failure classes dismissed

- FC-001, FC-002, FC-003: see *Guard scope*. Both sides are tested for each boundary (column 0 and indented; one `<value>` and several; CDATA and comment content kept).
- FC-004, FC-005, FC-019, FC-020, FC-021: the RH8201 fix now keeps every other trivia as written. `#if`/`#else`/excluded code/`#endif`, `#pragma`, `#region`, `#nullable`, `/* */`, and an ordinary comment are tested. RH8202 and RH8204 edit inside one documentation comment, where no directive can stand.
- FC-006, FC-022: the RH8204 content is copied verbatim, so the literal content cannot change. `ParseLeadingTrivia` runs with default parse options (documentation mode `Parse`) and yields structured `SingleLineDocumentationCommentTrivia`, like the factory version it replaces.
- FC-007, FC-008: RH8202 now converges in one application, and its Fix All works on one diagnostic per declaration. The RH8201 and RH8204 Fix All scenarios are unchanged.
- FC-009: no action is now offered that changes nothing.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter path in these three fixes.
- FC-015, FC-016: see *Policy ownership*.
- FC-017, FC-033, FC-034: CRLF variants exist; see *Assertion adequacy*.
- FC-018, FC-023 to FC-027, FC-029, FC-036: no rename, no expression rewrite, no member-kind dependence.
- FC-030: none of these fixes calls the formatter.
- FC-031: the summaries and remarks of `CreateInheritdocTrivia`, `ReplaceDocumentation`, `ApplyCodeFixAsync` (RH8202), `ExtractInnerContent`, and `GetDocumentingTags` describe the new code.
- FC-032, FC-035, FC-037: the expected values come from the spec examples; no text-only claim is made.

### Architecture findings

_None._

### Hints

_None._

## thoenissen/Reihitsu#1342

### Scope

Full audit; head `1d852e3`, base `8ac5705`. Files: the RH8305, RH8307, RH8309, and RH8402 code fixes, `DocumentationCommentCodeFixUtilities.LayOutElementAsync`/`CanLayOutElement`, `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync`, `DocumentationCommentFormattingPhase.LayOutElement`/`BuildOutermostReplacements`, FMT-PIPE-032, FMT-PIPE-009, FMT-UNSUP-001/002, SHARED-FIX-010, RH8305-004/005, RH8307-004/005/006, RH8309-004/006, and RH8402-005/006.

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs`, `GetLeadingHeaderSpan` (replace path) together with `EnsureHeaderIsSeparatedFromCode` | FC-001, FC-003 | – | PR-introduced | The replaced span now stops at the end of the old run's last comment on its line, so the line break that ends that line stays. `EnsureHeaderIsSeparatedFromCode` returns the header unchanged when it already ends with a line break. Both line breaks then remain. A `copyrightText` with a trailing `\n` is valid: RH0001's `IsSupportedCopyrightHeader` trims it, and `ResolveHeader` keeps it. **Counterexample (probe, ran):** configuration `SpecificationCopyrightText + "\n"`, file `// Copyright (c) Old Corp.⏎namespace N;⏎`. On `8ac5705` the fix gives `// <copyright …>⏎// Copyright (c) Example.⏎// </copyright>⏎namespace N;⏎` (probe passes). On `1d852e3` the same probe fails, and the output has a blank line in front of `namespace N;` that the file did not have. **Class:** header ends with a line break (yes or no) × what follows the run (code on the next line; one or more blank lines; a blank line then a directive or a doc comment; code behind the comment on its line). **Failing members:** header ending with a line break and code on the next line (adds a blank line where `main` was right); header ending with a line break and N blank lines behind the run (output has N+1, one more than the file, while `main` had 0). **Members that hold:** every member with a header that does not end with a line break (the RH8402-006 tests); a header ending with a line break and code behind `*/` on its line (`EnsureHeaderIsSeparatedFromCode` adds nothing); the insert and no-comment paths. RH8402-006's coverage excludes a `copyrightText` that ends with a line break, so no requirement is broken. The change still turns `main`'s correct output for the no-blank-line member into a new blank line, and nothing decides that output. | A cheap check: on the replace path, when the header already ends with a line break and the kept text starts with one, end the span after that one line break, or drop the header's trailing break. Add a test for the trailing-break configuration. Alternatively, accept the carve-out and record why. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8305/RH8307: fix withheld for a comment written behind code | `CanLayOutElement` → `IsWrittenBehindSource(comment.ParentTrivia)`: any character other than white space and U+FEFF between line start and the comment | Analyzers: `IsLeftAsWritten` (reports a comment behind source that documents the following code). RH83xx-005: "first `///` follows a token" | The fix withholds for a superset of RH83xx-005: also for a `/* */` comment or U+001A in front of `///` on the line. RH83xx-005's other side promises the fix only for "nothing but white space in front", so the extra cases are not promised. The shape is rare | holds |
| RH8307: fix withheld for a `/** */` element | `CanLayOutElement`: comment kind `SingleLineDocumentationCommentTrivia` | RH8307-006 | Agree, at the top level and nested (both tested) | holds |
| RH8305/RH8307: element has its own end tag; comment has no element without one | `TryGetElementToLayOut` → `HasOwnEndTag(element)` | Analyzers skip a comment with `ContainsElementWithoutOwnEndTag` (`IsLeftAsWritten`). The phase does the same (FMT-DOC-013). FMT-PIPE-032 does not decide such input | The analyzers never report an element of such a comment, so the entry point never receives one | holds |
| What the entry point lays out | `LayOutElement`: `BuildOutermostReplacements(element.DescendantNodesAndSelf())`, re-found by start position each round, bounded by `MaximumNormalizationRounds` | The phase's `NormalizeOutermostElements` (same `BuildOutermostReplacements`). RH8305 analyzer (top-level `summary`, ordinal, under 3 lines) ⊂ `RequiresNormalization` (summary, OrdinalIgnoreCase). RH8307 `ViolatesRule` = `NeedsLineAlignmentNormalization` (identical logic) | Every reported element is a normalization candidate, so no offered action is a no-op (FC-009). Nested reported elements: probe `{|#0:<remarks>A {|#1:<para>B⏎/// C</para>|}⏎/// D</remarks>|}` passed with `NumberOfFixAllIterations = 1` and `NumberOfIncrementalIterations = 1` | holds |
| RH8309: which comment the fix sorts | `GetDocumentationComment`: the reported comment's `ParentTrivia` == the first `DocumentationCommentTriviaSyntax` (`///` or `/** */`) of the declaration's leading trivia | RH8309-004 coverage (first comment) and RH8309-007 | Agree. Side effect: a comment on an accessor, or after an attribute list, is no longer the declaration's first comment, so its fix is withheld (see *Hints*) | holds |
| RH8309: fix withheld behind code | `IsWrittenBehindSource(comment.ParentTrivia)` | RH8309-006 | Same as the RH8305/RH8307 row | holds |
| RH8402: end of the replaced span | Through the run's last comment and the white space behind it on its line | RH8402-006; separation in `EnsureHeaderIsSeparatedFromCode` | Disagree for a header that ends with a line break | finding #2 |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Element layout (FMT-DOC-008 to 011) | `rg -n "BuildReplacement\|BuildOutermostReplacements\|LayOutElement\b" src --glob '!*Test*'`: `DocCommentElementNormalizer.BuildReplacement`, called only from `DocumentationCommentFormattingPhase.BuildOutermostReplacements`, which the phase and `LayOutElement` share. No documentation code fix calls `FormatNode`/`FormatNodeInDocumentAsync` any more (`rg` over `Rules/Documentation` and `CodeFixes/Core` finds only `FluentChainCodeFixHelper`) | one owner |
| Continuation prefix / column of added lines | `DocumentationCommentUtilities.GetContinuationPrefix` (Core), used by `BuildReplacement` and by RH8304's `MoveContentToNextDocumentationLineAsync` (unchanged) | one owner |
| Comment written behind code | `DocumentationCommentUtilities.IsWrittenBehindSource`/`IsStrandedBehindCode`/`IsLeftAsWritten` (Core), used by the analyzers, `CanLayOutElement`, and the RH8309 fix | one owner |
| Whether an element is "misaligned" (RH8307 vs. the phase) | `RH8307…Analyzer.ViolatesRule`/`TryGetFirstMeaningfulContentPosition` and `DocCommentElementNormalizer.NeedsLineAlignmentNormalization`/`TryGetFirstMeaningfulContentPosition`: two copies with identical logic, both on `main` | pre-existing; parity holds today (see *Hints*) |
| Old-header span (RH8402) | `GetLeadingHeaderSpan` only | one owner (finding #2 is about its counterpart, not a second owner) |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8305/RH8307/RH8309 `VerifyFixKeeps…`, `VerifySpecificationExampleKeepsTheRestOf…` (+ CRLF) | Only the reported element changes | Any change to the body, a nested member, another comment, a blank line, or `{ }` | `Verify`: exact text | adequate |
| RH83xx `VerifyNoCodeFixFor…WrittenBehindCode` (+ CRLF) | No action is registered | An action is offered | `VerifyNoCodeFixIsOffered` (checks every reported diagnostic) | adequate |
| RH8307 `…DelimitedDocumentationComment` (top level and nested) and the `///` other side | Withheld only for `/** */` | An action for `/** */`, or none for `///` | `VerifyNoCodeFixIsOffered`, `Verify` | adequate |
| RH83xx `…LaysOut…OfMemberThatHoldsACommentTheFormatterRefuses` (rewritten) | The fix is offered and the refused comment in the body stays | Withheld, or the body changes | `Verify` | adequate |
| `ReihitsuFormatterDocumentationElementTests` (8 tests) | Exact text under LF and CRLF; a second call leaves the text unchanged | A different column, line ending, or text outside the element; a second pass changes it | `AssertLaysOut` (exact text, both line endings, second pass) | adequate |
| RH8402-006 tests (+ CRLF) | Blank lines behind the old run stay | A blank line removed | `VerifySpecificationFileFix` | adequate for the covered configuration; no test for a header ending with a line break (finding #2) |

### Failure classes dismissed

- FC-004, FC-019, FC-020, FC-021: the RH8305/RH8307 entry point edits inside one element, where no comment or directive can stand. The RH8309 fix moves whole element texts and leaves the separators in place (RH8309-005 withholds for top-level text or XML comments). The RH8402 span ends at the last comment, so nothing behind it moves.
- FC-005: the RH8309-007 directive separator is tested.
- FC-006, FC-022 to FC-026: text-only moves inside documentation. The RH8309 text changes use original spans, so identical elements cannot be re-resolved wrongly. FC-025 is covered by the expression-bodied rows that are kept.
- FC-007, FC-008, FC-009: one application each (the 03/06 tests count iterations). Nested reported RH8307 elements converge in one Fix All iteration (probe). Several RH8309 diagnostics of one comment produce identical change sets. The RH8402 case is finding #2.
- FC-010, FC-011, FC-012: `LayOutElement` is bounded by `MaximumNormalizationRounds`, stops when no replacement is left or the element is not found again, and is idempotent (second-pass assertions). The phase refactor into `BuildOutermostReplacements` keeps the candidate filter, outermost selection, and descending order unchanged; 07 ran the full formatter suite.
- FC-013, FC-014: no analyzer or formatter judgment changes.
- FC-015, FC-016: see *Policy ownership*. No documentation fix keeps a formatter path.
- FC-017, FC-033, FC-034, FC-035: see *Assertion adequacy*. CRLF variants exist and the expected values are the spec examples.
- FC-018, FC-036: no rename.
- FC-027: nested elements (probe) and nested members (tests).
- FC-028: the entry point has no detached variant; FMT-PIPE-032 excludes FMT-PIPE-023.
- FC-030: the defect this issue fixes; every member in 01's matrix is tested.
- FC-031: the summaries of `LayOutElement`, `BuildOutermostReplacements`, `NormalizeOutermostElements`, the phase class, the entry point, `LayOutElementAsync`/`CanLayOutElement`, the RH8309 `GetDocumentationComment`/`CreateReorderChanges`, and RH8402 `GetLeadingHeaderSpan` describe the new code.
- FC-032: the 08 build and tests ran on `c6b0a5c`; `1d852e3` adds only a handover.
- FC-037: no text-only claim.
- FC-038 to FC-042: no shape is declared. The `FindUnsupportedSyntax` calls left the RH8305/RH8307/RH8309 fixes because those fixes no longer format a node, as SHARED-FIX-010's new sentence states.
- FC-043: the new entry point refuses nothing (FMT-UNSUP-001/002 amended), so the fixes ask nothing. `ElementOfADocumentWithAnUnsupportedShapeIsLaidOut` pins it.

### Architecture findings

_None._ Decision 22's structural change removed the copied prefix decision; the element layout has one owner.

### Hints

- The RH8309 accessor follow-up candidate in `00-lead.md` is stale. Probe on `8ac5705`: `/// <remarks>R</remarks>⏎/// {|RH8309:<summary>G</summary>|}⏎get => 0;` registers an action (the no-progress fix). Probe on `1d852e3`: no action. The new `ParentTrivia` comparison withholds it, because the accessor's comment is not the property's first documentation comment. RH8309-001 excludes accessor comments, and no test pins the new withholding. The lead may drop the candidate, or restate it as "reported on an accessor although not covered".
- RH8307's `ViolatesRule` and the phase's `NeedsLineAlignmentNormalization` are two copies of one predicate (pre-existing). The fix now relies on their parity directly (FC-009 if they drift). Follow-up candidate: one shared helper.

## thoenissen/Reihitsu#1343

### Scope

Full audit; head `1d852e3`, base `8ac5705`. Files: the RH8308 analyzer span, the RH8309 later-comment withholding (RH8309-001/007), the RH8401 fix withholding (RH8401-005), and `documentation/rules/RH8401.md`.

### Findings

_None._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH8308: the reported span | From the first non-white-space character of the first text node behind a direct element on its line, through every following text token (references included) up to an `XmlTextLiteralNewLineToken` or the end of the text node | RH8308-003 ("to the end of the line, or the start of the next XML node"). The fix removes the reported span | Agree. Both sides are tested: a single `&amp;` (one token) and several references; text in front of a following element | holds |
| RH8401: fix withheld | The first line from line start to the comment, plus every `DocumentationCommentExteriorTrivia` (indentation and `///`) | RH8401-005 ("a line … has U+FEFF or U+001A in front of its `///`") | Agree. The first-line check also refuses a shape the conversion could handle, because the first line's prefix lies outside the trivia, but RH8401-005 names every line and its *Why* makes the character the author's decision | holds |
| RH8309: later comment withheld | See #1342 | RH8309-007 | holds | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Characters that may stand in front of `///` | Lexer (white space includes U+FEFF and U+001A); `DocumentationCommentUtilities.RewriteDocumentationLine` (`char.IsWhiteSpace`); `DocumentationCommentUtilities.ByteOrderMark` (private, Core); `RH8401…CodeFixProvider.ContainsStrayCharacter` (private, new) | Two owners on purpose: decision 7 (refuse the class first) is recorded and justifies it. No finding |
| RH8308 span | `TryGetFirstMeaningfulTextSpan` only | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8308 `…ThroughReferences`, `…InOneApplication` rows, CRLF, `…InFrontOfElement…` | Span through the end of the text; one application | Span ending at a reference; a second iteration | `Verify` (span markup, iteration count) | adequate |
| RH8309 `VerifyNoCodeFixForLaterDocumentationComment…` (blank line, `//`, `/** */`, directive, CRLF) and `…SortsFirstCommentAndKeepsLaterComment` (+ CRLF) | Withheld for a later comment; the first comment sorted, later ones kept | An action for a later comment; a joined or changed later comment | `VerifyNoCodeFixIsOffered`, `Verify` | adequate |
| RH8401 `VerifyNoCodeFixFor…` (U+FEFF line 2, U+001A line 2, U+FEFF line 3 of 4, CRLF) | No action; every line still reported | An action, or a missing diagnostic | `VerifyNoCodeFixIsOffered` | adequate |

### Failure classes dismissed

- FC-001, FC-002, FC-003: see *Guard scope*.
- FC-004, FC-019 to FC-021: RH8308 removes only text inside one comment. RH8309 and RH8401 now withhold for the shapes in question.
- FC-007, FC-008: RH8308 converges in one application (tested). RH8401's Fix All on a stray-character comment is now withheld (the analyzer part of the tests holds).
- FC-009: the RH8309 no-progress cases from 01 are now withheld and tested.
- FC-010 to FC-014, FC-028, FC-038 to FC-043: no formatter path.
- FC-015: see *Policy ownership* (decision 7).
- FC-022: references inside text are kept as text tokens; only the span changed.
- FC-031: the summaries of `TryGetFirstMeaningfulTextSpan`, `HasStrayCharacterInFrontOfExterior`, `ContainsStrayCharacter`, and `GetDocumentationComment` match the code.
- Rule page `RH8401.md`: the corner-case section is removed, as the rule-page convention in `AGENTS.md` (*Key conventions*) requires. The added sentence ("for every line of the comment at once") is RH8401-004, unchanged. The footer is intact and the formatter is not mentioned. A withheld fix is covered by `general-notes.md`. Check 10 holds (decision 8).

### Architecture findings

_None._ The RH8401 analyzer and Core-helper disagreement was decided under decision 7.

### Hints

_None._

## thoenissen/Reihitsu#1459

### Scope

Full audit; head `1d852e3`, base `8ac5705`. Files: the RH8305/RH8307 fixes on declarations directly in the compilation unit (through FMT-PIPE-032), `DocumentationAnalysisUtilities.IsPrimaryConstructorRun`, the guard in the RH8305 and RH8307 analyzers, and RH8305-003/RH8307-003.

### Findings

_None._

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Skip the second run of a primary-constructor type | `context.ContainingSymbol` is a constructor whose declaring syntax is a `TypeDeclarationSyntax` | RH8305-003/RH8307-003 (one diagnostic per element) | Agree. Probes (ran): `public class C(int a) { /// {|#0:<summary>A</summary>|} public int A => a; }` gives one diagnostic, so a member's comment inside a primary-constructor type is not suppressed. `/// <summary>R</summary> … public record R(int A) { /// <summary>M</summary> public void M() {} }` gives two diagnostics, as expected. An explicit constructor is still reported (tested) | holds |
| File-level declaration layout | FMT-PIPE-032 lays out in place; FMT-PIPE-010's keep rule is not involved | RH8305-004/RH8307-004 ("wherever the declaration stands") | Agree. The 10/7 compilation-unit rows and Fix All are tested | holds |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Primary-constructor double run | `DocumentationAnalysisUtilities.IsPrimaryConstructorRun`, used by the RH8305 and RH8307 analyzers only | One owner. Other documentation analyzers are the follow-up candidate already in `00-lead.md` |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8305/RH8307 `…DirectlyInTheCompilationUnit` (rows, CRLF, Fix All) | The summary or element is laid out and the declaration is unchanged at file level | Unchanged summary (no-progress) or a changed body | `Verify` (exact text, iterations) | adequate |
| `…PrimaryConstructor…ReportedOnce` (6 rows each) and `…ExplicitConstructor…` | One diagnostic per element; an explicit constructor is still reported | Two diagnostics, or none | `Verify` (diagnostic count) | adequate. 06 saw them fail with the guard forced off |

### Failure classes dismissed

- FC-007, FC-009: the no-progress cases from 01 now converge in one application (tested).
- FC-016: RH8307 is the sibling rule and is covered. Other analyzers' double reports are a recorded follow-up candidate.
- FC-024: `record`, `record struct`, and positional records are covered by the rows. `partial`, `file`, and static interface members do not affect the comment layout.
- The remaining classes: as under #1342; the change is the same entry point.

### Architecture findings

_None._

### Hints

_None._

## Whole run

- Lane: `scripts/lane-check.sh bug-fix`, confirmed.
- Scope: every production path is named by a handover or a decision (20 and 22 widened it to the RH8305/RH8307 analyzers, `DocumentationAnalysisUtilities`, and the formatter entry point). No other edits.
- Visible output (check 10): every changed expected value is demanded by an unchanged requirement, or covered by a maintainer answer recorded in `00-lead.md`:
  - RH8201 (SHARED-FIX-006/008, decision 4);
  - RH8202 (SHARED-FIX-001);
  - RH8204 (RH8204-003 and the #1201 Q1 answer, decision 12);
  - RH8305/RH8307/RH8309 layout (SHARED-FIX-006);
  - behind-code withholding (Q1 = A);
  - rare-class withholding (RH8307-006, RH8309-007, RH8401-005; *Code-fix coverage*);
  - RH8308 span (RH8308-003);
  - RH8402 blank lines (SHARED-FIX-006);
  - single report (RH8305-003/RH8307-003);
  - rule page `RH8401.md` (decision 8).
  
  No `undecided output`. Finding #2 is an output change that no test asserts.
- Cost of special cases (check 11): no production code serves only a rare class where a refusal would do. The RH8201 `commentIndentation` and the RH8202 overlap skip are cheap checks. No shape is declared, so FC-042 does not apply.
- No `TODO` and no tracker number in the changed source (`git diff … -- src | rg "TODO|#[0-9]{3,}"`: none).
