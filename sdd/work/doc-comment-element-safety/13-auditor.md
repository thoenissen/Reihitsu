Role: auditor
Commit: 9161d75064d937bdd846774065bcca4b187d069e
Verdict: findings — #1329: findings (F4, F5); #1389: findings (F1, F2, F3); #1241: findings (F5); #1344: findings (F3, F5); #1242: pass

Notation: `⏎` is a line break, `U+FEFF` the byte order mark character. "Base" is `f3d514d`, "head" is `9161d75`.

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It did carry one fact: the Release build is green on the head. I took that fact as given.

## thoenissen/Reihitsu#1329

### Scope

Full audit of `FMT-DOC-013`, the `IsLeftAsWritten` guard in `DocumentationCommentFormattingPhase.Execute`, and the RH8301, RH8304, RH8305 and RH8307 silences that share the guard. Head and base are in *Whole run*.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| F4 | low | `sdd/specs/formatter/documentation-comments.md`, FMT-DOC-013 (coverage sentence and third normative sentence) | FC-003 | same requirement | FMT-DOC-004 says a comment that FMT-DOC-001 **or FMT-DOC-002** moves is normalized "unless FMT-DOC-013 leaves its text as written". FMT-DOC-013 names only comments that start their own line and comments that FMT-DOC-001 moves. Example input: `    int A;⏎    /* c */ /// <summary>S⏎    int B;`. Observed output: `/* c */` stays on its line, and `/// <summary>S` gets its own line with its text as written. The code is right, but no requirement states this outcome and no test covers it. | Name FMT-DOC-002 next to FMT-DOC-001 in FMT-DOC-013. Add a full-pipeline test with the input above, tagged FMT-DOC-013 and FMT-DOC-002. |
| F5 | low | see #1241 | — | — | — | — |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment left as written (FMT-DOC-013) | The whole `SingleLineDocumentationCommentTrivia`. `ContainsElementWithoutOwnEndTag` checks every `XmlElementSyntax` at any depth. `Execute` evaluates it after relocation. | RH8301 checks every line of each trivia from `root.DescendantTrivia()`. RH8304 and RH8305 use `ParentTrivia`. RH8307 uses `FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>`. All four call the same `DocumentationCommentUtilities.IsLeftAsWritten`, on the tree before relocation. | Yes. Relocation never changes the classification: a moved comment starts its own line afterwards, and its structure is unchanged. I probed relocated, stranded, nested, multi-trivia and malformed-XML shapes, including `List<int>` and `<br>` in prose. In each, the analyzers were silent exactly where the formatter left the text unchanged, and none of the four rules reported the formatter's output. | agree |
| End tag of its own (`HasOwnEndTag`) | `StartTag.Name` against `EndTag.Name`, compared ordinal. A missing end tag counts as none. | FMT-DOC-013 and SHARED-DOC-017: exact comparison, prefix and local name. | Yes. Observed: `<a:summary>S</b:summary>` is left as written. `<Summary>…</summary>` is left as written. `</summary >` counts as the element's own end tag. | agree (one edge in Hints) |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| End tag of its own | `DocumentationCommentUtilities.HasOwnEndTag`. `rg "EndTag\.IsMissing\|EndTag\.Name"` hits only `src/Reihitsu.Core/DocumentationCommentUtilities.cs:245`. Callers: `ContainsElementWithoutOwnEndTag`, `DocumentationCommentCodeFixUtilities.CanRewriteElement`, RH8309 `CanReorderElements`. | one owner |
| Comment left as written | `DocumentationCommentUtilities.IsLeftAsWritten`. Callers: the phase's `Execute`, RH8301, RH8304, RH8305, RH8307. | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DocumentationCommentWithoutOwnEndTagTests` (18 tests) | The comment's text and the code after it are unchanged; a relocated comment is moved but its text is not changed | Any byte difference, under LF or under CRLF | `FormatterTestsBase.AssertRuleResult`: exact text and line-ending check. When the expected output differs from the input, the helper also runs a second pass. | adequate. Covers the comment and directive gaps (FC-019, FC-020) and the other scopes (FC-027). |
| RH8301, RH8305, RH8307 `…FormatterTests.VerifyFormatterLeavesCommentsKeptAsWrittenUnchanged` | The analyzer is clean before and after formatting, and the formatter leaves the input unchanged | A diagnostic, or a formatter change | `FormatterTestsBase<TAnalyzer>.VerifyFormatter`: two passes, LF and CRLF | adequate |
| RH8301, RH8304, RH8305, RH8307 "does not produce diagnostics" tests | No diagnostic in a comment that is left as written | Any reported diagnostic | `Verify(source)`, which checks the exact markup | adequate |

### Failure classes dismissed

See *Whole run*.

### Hints

- `HasOwnEndTag` accepts an end tag that is missing its `>`, so it counts as the element's own end tag. Input: `/// <summary>a</summary⏎int A;`. The formatter lays the comment out, and no text or code is lost. Under "documentation XML is read as the compiler reads it", it is unclear whether this comment should be left as written instead. Uncertain, so it is not a finding.

## thoenissen/Reihitsu#1389

### Scope

Full audit of three changes:
- the BOM change in `BlankLineTokenCleanupRewriter.CollapseLeadingBlankLines` and in `CleanupPhase.CleanWhitespaceBeforeEndOfLine`;
- the new `ReihitsuFormatterHelpers` BOM helpers;
- the stranded-comment guard (FMT-DOC-003).

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| F1 | high | `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs`, `CollapseLeadingBlankLines` (the `StartsWithByteOrderMark(trivia[0])` branch), together with `IndentationRewriter.TryHandleLineStartWhitespace` | FC-010, FC-012, FC-013, FC-016, FC-003 | PR-introduced (same requirement as FMT-CLEAN-009) | **The formatter is no longer idempotent (FMT-PIPE-004) when the line after the BOM is indented.** The method puts a BOM-only whitespace trivia in front of the indentation trivia it keeps from the next line, so the token gets two whitespace trivia in a row. The Indentation phase treats only the first whitespace at a line start as indentation: it writes the BOM plus the wanted column. It then keeps the second trivia as ordinary white space, so the old indentation stays on top. On the next run, the parser reads the BOM and the spaces as one trivia and indents again. Observed with `scripts/trace.sh`, LF and CRLF: <br>(a) `U+FEFF⏎    class C{int x;}` → pass 1 `U+FEFF    class C⏎{⏎    int x;⏎}` → pass 2 `U+FEFFclass C⏎…`. The same happens with `U+FEFF⏎⏎    // c⏎class C{int x;}`, `U+FEFF⏎    /// <summary>S</summary>⏎    class C{int x;}`, and `U+FEFF⏎    using System;⏎⏎class C{int x;}`. <br>(b) A `U+FEFF` alone on a blank line in the middle of a file, before `else` or `finally`: `        }⏎U+FEFF⏎        else` → pass 1 `U+FEFF                else` (16 spaces) → pass 2 `U+FEFF        else`. The file reader consumes only a leading BOM, so CLI users reach (b) with an ordinary file, for example two BOM files concatenated. <br>RH5204 reports the pass-1 output in both cases. Base is idempotent on every one of these inputs; it drops the BOM. The new `ByteOrderMarkTests` all use a first token without indentation, so none of them meets this input class. | Emit one whitespace trivia, not two. Two ways: merge the BOM with the indentation that follows it, or, at the start of the text, emit the BOM alone and drop the old indentation, because FMT-INDENT decides that column. The owner picks. Add regression tests through `AssertRuleResult` for the four start-of-text inputs in (a), and, after F2, for the other side of (b). |
| F2 | medium | Same branch. The method summary claims a scope ("…because … it is the encoding signature of the text") that the code does not enforce. | FC-002, FC-001, FC-015, FC-031 | PR-introduced | **The BOM is kept wherever it starts the leading trivia of a token the method visits, not only at the start of the text.** The method visits more than the first token of the text: <br>- a token after a `{` that does not end its line; <br>- every `{`, `}`, `else`, `catch`, `finally` and `while` (`VisitToken`). <br>The branch tests `trivia[0]` by its content only, never by its position. The counterparts restrict it: <br>- Cleanup's twin step checks `startsText: original.FullSpan.Start == 0` and `triviaIndex == 0`; <br>- FMT-CLEAN-009 says "at the start of the text"; <br>- answer 4A approved the change in this off-limits phase only for "a first line that holds only the BOM and white space". <br>Observed, head against base: <br>- `class C⏎U+FEFF⏎{⏎    int A;⏎}` → `class C⏎U+FEFF{` (base `{`); <br>- `    int A;⏎U+FEFF⏎}` → `U+FEFF}` (base `}`); <br>- `        int x = 0;⏎U+FEFF⏎        x++;` and `    {⏎U+FEFF⏎        int x = 0;` still drop it, as base does. <br>So a `U+FEFF` in the middle of the text is now kept or dropped depending on the next token. Shape (b) of F1 also comes through this path. | Restrict the branch to the token at text position 0, as Cleanup does. Add tests for the other side: a `U+FEFF` on a blank line in the middle of the text before `{`, `}` and `else` is removed as on base. Add a Cleanup unit test with a white-space trivia that starts with `U+FEFF` before a line break and is not at the start of the text. |
| F3 | low | Invisible `U+FEFF` literals: `src/Reihitsu.Core/DocumentationCommentUtilities.cs:33` (`'﻿'`); `src/Reihitsu.Formatter.Test/Regression/Cleanup/ByteOrderMarkTests.cs:20`; `src/Reihitsu.Formatter.Test/Unit/Cleanup/CleanupPhaseTests.cs:502, 523, 531`; `src/Reihitsu.Core.Test/DocumentationCommentUtilitiesTests.cs:147`; `src/Reihitsu.Analyzer.Test/Documentation/RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests.cs:600-601` | FC-033 | PR-introduced | Every older site writes the escape `﻿`: `IndentationRewriter`, `RH7304…CodeFixProvider`, `RH5204…Analyzer`, `ReihitsuFormatterHelpers` (new), the removed private copy in the phase, and `TrailingTriviaCleanupTests.cs:251`. The new code writes the character itself, which shows no glyph and no diff when an editor or tool strips it. If that happens, the production constant fails to compile, which is loud. The tests fail silently: they keep passing on BOM-free text and test nothing. Example: `ByteOrderMarkBeforeLineBreakIsKept` with an empty constant still passes, because `⏎class C{int x;}` becomes `class C…`. | Write `'﻿'` and `"﻿"` at every site listed. |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Keep the BOM in BlankLines | `trivia[0]` of every token `CollapseLeadingBlankLines` visits, checked by content only | `CleanupPhase.CleanWhitespaceBeforeEndOfLine` (`startsText` and index 0). FMT-CLEAN-009: "at the start of the text". | No | F2 |
| Shape of the trivia that keeps the BOM | `newTrivia`: the BOM trivia, then the indentation trivia, then the rest | `IndentationRewriter.TryHandleLineStartWhitespace`, which treats only the first white space at a line start as indentation | No | F1 |
| Keep the BOM in Cleanup | The first trivia of the leading trivia of the token at `FullSpan.Start == 0`, when a line break follows it | FMT-CLEAN-009 | Yes | agree |
| Stranded behind code (FMT-DOC-003) | `IsStrandedBehindCode`: from the line start to `FullSpan.Start`, any character that is not white space and not BOM; and the owning token does not open its parent | `RegisterRelocation` (now the same Core predicate). `BlankLineTriviaBoundaryRewriter.IsStrandedInlineDocumentation`, a structural test: the first comment in the leading trivia is a doc comment that does not start its own line. | They agree on the text decision for every probed shape: behind `;`, `)`, `+`, `.`, `/* c */`, and an argument list. They differ only for `/* c */ /// …` at the start of a line before `}`. There BlankLines adds a blank line before `/* c */`, which is outside the comment, so no text is touched. | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| BOM at the start of the text | **Added by this PR:** `ReihitsuFormatterHelpers.StartsWithByteOrderMark` and `CreateByteOrderMarkTrivia`, used by BlankLines and Cleanup; the private `DocumentationCommentUtilities.ByteOrderMark`, used by `IsWrittenBehindSource` and `GetContinuationPrefix`. <br>**Pre-existing:** `IndentationRewriter` (lines 163, 169, 336; off-limits), `DocumentationCommentFormattingPhase.HasOwningLeadingSourceOnSameLine`, `RH7304…CodeFixProvider:139,143`, `RH5204…Analyzer:228`. | The owners disagree on position: Cleanup checks it, BlankLines does not (F2). The other differences are pre-existing. |
| Stranded behind code | Core: `IsWrittenBehindSource`, `DocumentsFollowingCode`, `IsStrandedBehindCode`. The phase's private `IsAfterSourceOnSameLine` was removed. `ReihitsuFormatterHelpers.DocumentsFollowingCode` only delegates to Core. Pre-existing related owners: `HasOwningLeadingSourceOnSameLine` (same character test, different span), `BlankLineTriviaBoundaryRewriter.IsStrandedInlineDocumentation` (off-limits). | acceptable. The diff merges one copy, and the remaining ones decide other questions. |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ByteOrderMarkTests` (9 tests) | The BOM stays once at the start of the text, and the rest is laid out | A missing or repeated BOM, or a layout difference | `AssertRuleResult`: exact text, LF and CRLF, second pass | weak corpus. Every first token is unindented, so F1 escapes, and nothing covers the middle of the text (F2). The tests also depend on an invisible literal (F3). |
| `CleanupPhaseTests.KeepsByteOrderMarkBeforeLineBreakOfTheFirstLine`, `…RemovesWhiteSpaceAfterByteOrderMark…` | Cleanup alone keeps the BOM and removes the white space after it | A missing BOM, or white space left behind | Direct `CleanupPhase.Execute`, exact string | adequate for one side of `startsText`. The other side is missing (F2). |
| `StrandedDocumentationCommentTextTests` (11 tests) | A stranded comment is unchanged byte for byte; a relocated comment is still laid out | Any byte difference | `AssertRuleResult` | adequate. I also probed a tab-indented continuation line and a line that holds only white space after `///`: both are kept. |

### Failure classes dismissed

See *Whole run*.

### Hints

- **Pre-existing, Indentation phase (off-limits).** `IndentationRewriter` keeps a `U+FEFF` that starts the white space of any line, not only the first. Base already does this for `U+FEFF        else` in the middle of a file.
- **Pre-existing.** `int M() => 0 /// <summary>end</summary>⏎;` ends with `;` in column 0 after the expression body becomes a block. Base produces the same layout and also loses the comment text.
- **Latent truncation risk.** `Execute` still keeps only `SyntaxFactory.ParseLeadingTrivia(updatedCommentText)[0]` of a rebuild. I probed 13 well-formed shapes (CDATA, XML comment, `<code>`, `<para>`, a processing instruction, entities, a missing separator, end of file) and none loses a character. A check that the rebuild reparses as one trivia would close the risk. The skeptic already listed this as a follow-up candidate.

## thoenissen/Reihitsu#1241

### Scope

Full audit of the following:
- `DocumentationCommentCodeFixUtilities.CanRewriteElement`;
- the new RH8204 guard;
- RH8309's `CanReorderElements`, `GetDocumentationComment` and the async `RegisterCodeFixesAsync`;
- the Fix All behavior of all four fixes.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| F5 | low | Rule pages. **Withheld fix:** `documentation/rules/RH8202.md`, `RH8204.md`, `RH8309.md`. **Exceptions:** `documentation/rules/RH8301.md`, `RH8304.md`, `RH8305.md`, `RH8307.md`. | — | same requirement | Each page names only "never closed, or closed by the end tag of another element". The specs decide one more case: an end tag spelled differently. `documentation/rules/RH8107.md` names it, and tests pin it for the other rules. Examples: RH8202 reports `<Value>The size</value>` with no fix (`RH8202…Tests.cs:878`), RH8204 reports `<Placeholder>Later text</placeholder>` with no fix (`RH8204…Tests.cs:320`), and the layout rules stay silent on a comment such as `/// <summary>S</returns>`, where no other element exists. A reader of these pages cannot tell why no fix is offered, or why nothing is reported. This is text only. | Use the wording of RH8107's page on each listed page: "never closed, closed by the end tag of another element, or by an end tag spelled differently". |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Withhold RH8107, RH8202, RH8204 | The node `FindNode` returns at the diagnostic span: the reported element, whose span is the element's own (`xmlNode.GetLocation()`) | SHARED-DOC-017: "a reported element that has no end tag of its own" | Yes. With `apply-fix.sh`: <br>- `<remarks><returns>Nothing</returns>` and `<remarks><placeholder>x</placeholder>` are offered (the reported element has its own end tag), and nothing beyond the element is touched; <br>- `<returns>a <c>b</returns>` is withheld; <br>- `<placeholder>a <see>b</x></placeholder>` is offered and keeps `a <see>b</x>`. | agree |
| Withhold RH8309 | Every node of `Content` of the comment that `GetDocumentationComment` returns, the same call `ApplyCodeFixAsync` rewrites | SHARED-DOC-017 ("any top-level element") and RH8309-005 (before, between, after) | Yes. A nested element without its own end tag is offered and sorted with its parent; the formatter then leaves the comment as written. | agree |

### Policy ownership

See #1329: one owner for "end tag of its own". `rg "CanRemoveElement|CanRewriteElement" src` finds one definition and three callers.

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `…VerifyNoCodeFixFor…` (RH8107, RH8202, RH8204, RH8309) | The diagnostic is reported and no code action is registered | An action is registered for any fixable diagnostic, or a diagnostic is missing | `VerifyNoCodeFixIsOffered`: exact markup, plus `RegisterCodeFixesAsync` called for each diagnostic | adequate (FC-009) |
| `…VerifyFixAllLeaves…Unchanged` (RH8107, RH8204, RH8309) | Fix All fixes the fixable diagnostics; the withheld ones stay at their spans | Text differs, or a withheld diagnostic disappears or moves | `Verify` with `MarkupMode.Allow` for `FixedState` and `BatchFixedState` | adequate (FC-008, SHARED-FIX-002) |
| Offered-side tests (nested element, element beside another without its own end tag, empty element) | The fix still applies where it is safe | The fix is withheld | `Verify` | adequate (FC-003) |

### Failure classes dismissed

See *Whole run*.

### Hints

_None._

## thoenissen/Reihitsu#1344

### Scope

Full audit of three changes:
- `DocumentationCommentUtilities.GetContinuationPrefix`, and its two consumers;
- the RH8304 analyzer silence;
- RH8309-005, as implemented by `CanReorderElements`.

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| F3 | low | see #1389. Here: the literals in `DocumentationCommentUtilitiesTests.cs:147` and `RH8304…AnalyzerTests.cs:600-601` | — | — | — | — |
| F5 | low | see #1241 (`RH8304.md`, `RH8309.md`) | — | — | — | — |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Continuation prefix | Spaces and tabs from the start of the line, after one leading `U+FEFF`. It only needs to know whether a `///` exists on the line, not where. | RH8304-004 ("spaces and tabs at the start of the line") and FMT-DOC-011 ("spaces and tabs in front of the comment's first `///`") | Yes. With `apply-fix.sh`, nothing in front of the comment is repeated: <br>- `/* c */`; <br>- a string literal holding `"///"` in front of the comment; <br>- the BOM followed by indentation. <br>The normalizer takes the prefix from the line of the element's start tag. After relocation that line has the same indentation, and FMT-INDENT decides the column anyway. | agree |
| White-space text in RH8309-005 | Each `XmlTextSyntax` at the top level: `string.IsNullOrWhiteSpace(ValueText)` for every token | RH8309-005: text of white space only, read as the compiler reads it. RH8304 uses the same `ValueText` predicate. | Yes for `&#32;`, `&#x9;`, `&#65;`, `&amp;`. U+00A0 is an edge case (Hints). | agree |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Continuation prefix | `DocumentationCommentUtilities.GetContinuationPrefix` only (`rg '"/// "\|DocumentationExterior'`). Consumers: `DocCommentElementNormalizer` (3 calls) and `DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync` (1 call). | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH8304 `VerifyFixDoesNotRepeat…` and `VerifyFixKeepsTabIndentation…` (9 tests) | The new line starts with the indentation only | Code, a comment, or the BOM is repeated | `Verify` (iterative, Fix All, re-analysis), or the raw `ApplyCodeFixAsync` for BOM, tab and CRLF | adequate. The BOM test depends on an invisible literal (F3). |
| `DocumentationCommentUtilitiesTests.GetContinuationPrefix…` (6 tests) | The exact prefix string | Any other string | Direct call | adequate |
| RH8309-005 tests (13 tests, including the character-reference boundary on both sides) | Withheld for text, comments, CDATA sections and processing instructions; offered for white space | An action is offered, or withheld, on the wrong side | `VerifyNoCodeFixIsOffered`, `Verify` | adequate |

### Failure classes dismissed

See *Whole run*.

### Hints

- RH8309-005 and RH8304 count U+00A0 as white space, because `string.IsNullOrWhiteSpace` does; XML white space does not include it. With a `/// &#160;` line between `<remarks>` and `<summary>`, the fix is offered and moves `<remarks>` below the U+00A0 line. Whether the maintainer's rule "documentation XML is read as the compiler reads it" excludes U+00A0 is not decided.
- **Pre-existing, FMT-DOC-006.** FMT-DOC-006 removes a literal U+00A0 on a `///` line. The input was `/// <remarks>R</remarks>`, then `///` followed by a space and U+00A0, then the summary; the fix output kept only a bare `///`.
- **Pre-existing, outside this run.** `/// <summary>S</summary><remarks>R</remarks>` keeps `</summary><remarks>R</remarks>` on one line after formatting, and RH8304 reports the formatter's output. No formatter phase splits these elements onto separate lines.

## thoenissen/Reihitsu#1242

### Scope

Spec-only issue: the owner move and SHARED-DOC-018. No code change belongs to it.

### Findings

_N/A — no finding._

### Guard scope

_N/A — no predicate changed for this issue._

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Removing an element with the lines it owns (SHARED-DOC-018) | `DocumentationCommentCodeFixUtilities.GetElementRemovalSpan`. Callers: the RH8107 and RH8202 fixes. The type is now listed once, under `shared/element-documentation.md` and in the `sdd/specs/README.md` index. | one owner |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 21 retagged RH8107 and RH8202 tests, plus `VerifySpecificationExampleRemovesReturnsElementWithTheLinesItOwns` | The removal shape of SHARED-DOC-018 | Other lines removed or kept | `Verify` | adequate. No assertion changed. |

### Failure classes dismissed

All classes: no code changed for this issue.

### Hints

- RH8107-007 and RH8202-004 still repeat SHARED-DOC-017's withhold for their own rules. That was already so on base, and the spec checker accepted it. `sdd/README.md` says a shared rule is referenced, not restated.

## Whole run

### Scope

- **Checkout.** The head is `9161d75064d937bdd846774065bcca4b187d069e`, on branch `claude/dreamy-albattani-e7phpl`. The working tree was clean.
- **Base.** `origin/main` is `f3d514d0b1cbd8b3e540f2b2c7243dcd21760929`, the SHA in my prompt, and it is an ancestor of the head (`git merge-base --is-ancestor`).
- **This is a full audit, not a repair delta.**
- **The diff is what the handovers describe:**
  - 14 production paths, the list in `07-implementer.md`;
  - the test files of `05-test-implementer.md`;
  - the spec files and rule pages of `04-spec-author.md`, `06-spec-author.md` and `10-spec-author.md`.
- **After `2d810b9`,** only test files and `sdd/` changed.
- **Lane.** `scripts/lane-check.sh bug-fix` reported `LANE CHECK: confirmed — claimed bug-fix, diff needs bug-fix`.
- **Off-limits files.** Of the files the maintainer put off-limits, only `BlankLineTokenCleanupRewriter.cs` changed, and only in `CollapseLeadingBlankLines`, the approved exception. F2 shows that this change reaches further than the approval.

### How the facts were established

- **Static tracing.** Every changed method, and the counterparts named above.
- **A probe app.** A scratchpad file-based app with two builds, one against the head and one against a `git archive` of base. It ran `ReihitsuFormatter.FormatSyntaxTree` twice under LF and CRLF on about 75 fixtures. For each fixture it compared the code tokens and the characters that are not white space, and it ran RH8301, RH8304, RH8305, RH8307 and RH8309 on the input and on the formatter's output.
- **The repository's tools.** `scripts/trace.sh` confirmed F1 (a) and (b). `scripts/apply-fix.sh` covered RH8107, RH8204, RH8304, RH8305 and RH8309, and I ran base's `apply-fix.sh` for comparison.
- **No repository test was run.** I trusted the cleaner's build and the roles' focused results.
- **Clean-up.** Every fixture and probe was deleted afterwards. This handover is the only file I changed.

### Failure classes dismissed

- **FC-004.** The changed edits only skip a rewrite or narrow the prefix, and the RH8309 withhold covers text that the fix would move. The character check kept every character in all probes.
- **FC-005.** No directive guard changed. A directive after a comment left as written is tested. The BOM followed by `#if` (white space, line break, then `#if DEBUG`) gives `U+FEFF#if DEBUG` and is idempotent.
- **FC-006, FC-022, FC-023, FC-024, FC-025.** No code, literal or identifier is rewritten. A string literal holding `///` in front of a comment does not reach the prefix (probed).
- **FC-007.** Every fix sweep converged in one iteration where a fix was offered.
- **FC-008, FC-009.** Covered by the Fix All and no-fix tests above. RH8305 and RH8307 are not reported where their fixes would do nothing.
- **FC-011.** No loop changed. The new guards `continue` without reporting a change.
- **FC-013.** Not dismissed: it is part of F1. For the comment-text silences, no new case of a rule reporting the formatter's output was found; the RH8304 case in the #1344 hints is pre-existing.
- **FC-014.** No new rule.
- **FC-017.** The helpers check exact text, two passes, and LF and CRLF. The weak corpus is recorded under F1 and F2.
- **FC-018, FC-036.** `CanRemoveElement` became `CanRewriteElement`. The method is internal, and rg finds no other reference.
- **FC-019, FC-020, FC-021.** Comment, directive and `/* c */` gaps are tested. For stranded comments the change only removes edits.
- **FC-026.** Two `///` on one line (probed).
- **FC-027.** Method bodies, a comment before `}`, and the argument, operator and member-access scopes are tested.
- **FC-028.** The formatter phase, inside or outside a document, uses the same guards.
- **FC-029, FC-037.** Not applicable.
- **FC-030.** The RH8309 fix formats the whole member. That is pre-existing, and the run already lists it as a follow-up candidate.
- **FC-031.** I read every changed summary and inline comment. The only mismatch is in F2.
- **FC-032.** The last compiled change is `2d810b9`. Only test files changed since then, and the validator runs the full suite.
- **FC-034.** Each issue's own shape is a test as written.
- **FC-035.** The tests were red before the implementation (`05-test-implementer.md`).

### Hints

- **The scope question is still open.** In `07-implementer.md` the implementer asked about escalation 5: it added six new public static methods to `Reihitsu.Core`'s `DocumentationCommentUtilities` and asked the lead to confirm they are within the approved scope. `09-spec-checker.md` repeated the question. `00-lead.md` records no answer.
- **No direct unit tests for the new Core methods.** The six methods have no tests in `Reihitsu.Core.Test`, and their `ArgumentNullException` paths are never run. Their behavior is covered through the formatter and analyzer tests.
- **Pre-existing, unrelated: an RH8305 fix that makes no progress.** The RH8305 fix makes no progress on the `<summary>` of a type declared first in a file without a namespace, for example `/// <summary>S</summary>⏎class C⏎{⏎    int A;⏎}`. `apply-fix.sh` reports `no-progress` on base and on head. The fix works for the same type inside a namespace, and for members.
- **Pre-existing: RH8304 has no parity test.** RH8304 has no `…FormatterTests` class with a `VerifyFormatter` parity test, unlike RH8301, RH8305 and RH8307.
