Role: auditor
Commit: ee49ee6
Verdict: findings

My prompt named the issue, slug, lane, SHAs, and handovers to read. It did not include an intended fix, a suspected cause, or a hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#1491 (behavior change).

- Head: `ee49ee6e089ccfb80b908b1234fbb5064921bd91`. The checkout is clean and at this commit.
- `origin/main` and `git ls-remote origin refs/heads/main` both give `8c7d5bec9fb134a44d6cfc07a46a072055a70100`, which is an ancestor of the head.
- The run's diff is `52b69fa..ee49ee6`. It matches the handovers:
  - production: `DocumentationCommentFormattingPhase`, `BlankLineTriviaBoundaryRewriter`, `BlankLineEditor`, and the new `BlockStartUtilities` and `TriviaListUtilities.IndexOfFirstComment`;
  - tests: two phase-level files, one full-pipeline file, and additions to the node-level, RH3202, and requirement tests;
  - the two specs, the change doc, and `AGENTS.md`.
- `scripts/lane-check.sh behavior-change` reports `confirmed`.

Method: differential runs. I built `Reihitsu.Cli` from `origin/main` (a worktree in the scratchpad) and from the head. Both CLIs formatted the same fixtures, under LF and CRLF, twice each. I compared main against head after the first pass, and each version's first pass against its second.

- Corpus 1 has 767 fixtures. It crosses seven delimited-comment shapes with five in-front gaps and twelve behind-it shapes. It adds the no-gap form and 69 contexts, each with five comment shapes: enum, namespace, file-scoped namespace, every using/`extern alias`/assembly-attribute shape, top-level statements, statements, `else`/`catch`, local function, switch label, labeled statement, accessors, parameters, type parameters, base list, attribute list, initializers, arguments, binary operands, member-access dot, ternary, `{` with and without code behind, `}`, every member kind, record, interface, lambda block, switch expression, banner, own-line `/* c */`, `// line`, `#if`/`#if false`/`#region`, and verbatim and raw literals in front.
- Corpus 2 has 128 fixtures, all expression-scope or line-break-phase constructs: ternaries (one line, partly wrapped, nested), binary, `??`, `is`, member access, `?.`, chains, arguments, named arguments, initializers, lambdas, switch arms, cast, index, tuple, interpolation, `default`, `typeof`, query clauses, every statement keyword, assignment, `where` constraint, attribute arguments, default parameters, and expression bodies. Each comes in two gap shapes (direct, and behind `/* c */`).
- Code-fix surface: I ran `scripts/apply-fix.sh` for RH3202 and RH5308, from the head and from the main worktree.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs:203` (registration) and `:128-145` (previous-token relocation), read by `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/TernaryLineBreakRewriter.cs` `NormalizeQuestionTokenPosition` / `NormalizeColonTokenPosition` | FC-013, FC-016, FC-027 | – | PR-introduced | A delimited documentation comment behind the `?` or `:` of a conditional expression now gets different output, and that output breaks FMT-BREAK-018, an unchanged requirement. The change doc says "Visible output changes: None — … Every input keeps today's output"; the issue's Done-when 3 says the same. Input, both through the CLI and through the RH3202 fix: `int M(bool b) => b ? 1 : /** d */ 2;`. On `main` the result is `return b` / `? 1` / `:` / blank / `/** d */ 2;`, and RH5308 reports nothing. On head it is `return b` / `? 1 :` / blank / `/** d */ 2;`: the `:` ends a line, which breaks FMT-BREAK-018 ("put `?` and `:` at the start of their own lines"). RH5308 now reports that output, and its fix makes no progress (`apply-fix.sh RH5308`: `no-progress`, LF and CRLF). The `?` form, `b ? /** d */ 1 : 2`, changes from `b` / `?` / blank / `/** d */ 1` to `b ?` / blank / `/** d */ 1`. Cause: the documentation phase writes the line break into the trailing trivia of the token in front of the comment. The blank-line phase used to write it into the owning token's leading trivia. `TernaryLineBreakRewriter` reads `HasTrailingEndOfLine(operator)`; when it is true it refuses to join across the comment (`WouldJoinAcrossUnjoinableTrivia`) and leaves the operator at the end of its line. The skeptic saw this exact pair differ (02-skeptic.md, "3 differing pairs") and set it aside as "not moved today". On `main` it is moved, because `2` opens its node, so `DocumentsFollowingCode` holds. No test pins any ternary member. Failing members, all under LF and CRLF, on both whole-document formatting and the RH3202 fix, with the comment written directly behind the operator or behind a trailing `/* c */`: (a) behind `:` of a one-line conditional; (b) behind `?` of a one-line conditional; (c) behind `:` of a conditional wrapped as `b` / `? 1 : /** d */ 2`; (d) behind `?` of a conditional wrapped as `b ? /** d */ 1` / `: 2`; (e) behind `:` or `?` of a nested conditional (`b ? (c ? 1 : /** d */ 2) : 3`, `b ? 1 : c ? /** d */ 2 : 3`); each in both an expression body and a `return` statement. Holding members (output identical to `main`): a conditional that already has `:` at line start (`b` / `? 1` / `: /** d */ 2`, also in an assignment statement); every other token in front of the comment: `,` in argument, initializer, enum, parameter, type-parameter, and base lists and in switch arms; `+`, `==`, `&&`, `\|\|`, `??`, `is`; `.` and `?.` (including in wrapped chains); `=`, `+=`, `=>` (expression body, lambda, switch arm); `(`, `[`, `<`; named-argument, base-list, and constraint `:`; `case … when`; `return`, `throw`, `in`, `where`; `;` and `}` before members and statements; and every statement, using, and member shape in corpus 1. All 895 fixtures are idempotent on both versions. | Restore `main`'s output for the ternary members, and add the five failing members as tests (full pipeline, LF and CRLF, plus one RH3202 fix case). One option is to write a moved delimited comment's line break where the blank-line phase wrote it, in the owning token's leading trivia. If so, re-run the differential corpus, since every other member must stay identical. A change in `TernaryLineBreakRewriter` would also change the existing `///` output (see *Hints*), which would be an output question. Otherwise the lead lists the change in the change doc and asks it as an output question. |
| 2 | low | `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs:16` (class summary) and `:75` (`MoveBeforeOwningToken` `<returns>`) | FC-031 | – | PR-introduced | The class summary says the phase "starts a new line in front of every documentation comment, single-line or delimited, written behind the code in front of it". It does not do this for stranded comments, for delimited comments behind `{`, a switch-label colon, or an earlier comment, or for a comment that does not open its node. The method summary at `:101` states this correctly. `MoveBeforeOwningToken` now also takes delimited comments: N1 at node level (`/** a */ /** b */` at the root's start), where `/** b */` and any code stay on the comment's line. Its `<returns>` still says "moved onto a line of its own". | Qualify the class summary the way the method summary is qualified (FMT-DOC-001's conditions). Say "onto a new line" in `MoveBeforeOwningToken`'s `<returns>`. Text only. |

Finding 2 is confined to documentation comments of internal members and cannot change compiled behavior. Finding 1 blocks the verdict.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which delimited documentation comment written behind code starts a new line (`RegisterRelocation` + `IsMovableDelimitedDocumentation`) | Text of the comment's physical line in front of it (`IsWrittenBehindSource`), or `RootPrecedingToken.EndsLine` plus the root's first line; the owning token's whole leading trivia up to the first comment (`IndexOfFirstComment`); the resolved preceding token (`PrecedingTokenFacts.Resolve` → `BlockStartUtilities.IsFirstInBlock`); the owning token's parent (`DocumentsFollowingCode`) | `main`'s blank-line condition: first comment of leading trivia; `!IsFirstInBlock`; `DocumentsFollowingCode`; `!StartsOwnLine(token, commentIndex, facts)` (trivia-based); `!IsPrecededByDirective`; previous facts exist | Same set. The text-based "written behind" and the trivia-based `StartsOwnLine` could only disagree on a line break that is neither `EndOfLineTrivia` nor inside text in front of the comment. Comment text ends with `*/`, a token's text ends with its own characters, a directive ends its line, and Roslyn never makes `/** */` trailing trivia, so no such input exists. The directive qualifier cannot be constructed (a directive ends its line). The differential runs confirm the set: no member is moved by only one version. | holds |
| Whether the blank-line boundary skips a documentation comment (`IsInlineDocumentation`) | First comment of the owning token's leading trivia, its kind, and `StartsOwnLine` at that index against the resolved preceding facts | The documentation phase's move set (row above) | Every doc comment the documentation phase moves starts its line afterwards, so it is not exempt and gets FMT-BLANK-011's blank line. Every doc comment it leaves behind code is exempt. The skip was widened from stranded-only to every inline documentation comment. For `///` this changes nothing in the pipeline: the documentation phase moves every `///` that passes `DocumentsFollowingCode`. `BlankLinePhaseDocumentationCommentTests` pins it on both sides. The `previousTokenFacts.Exists == false` branch is unreachable, because the call sits behind `isFirstInBlock == false`. It was unreachable on `main` too. | holds |
| Where the move puts the line break (previous token's trailing trivia vs. owning token's leading trivia) | `previousTokenRelocationCounts` (trailing trivia of `GetPreviousPresentToken`) | `main`: `EnsureBlankLineBeforeFirstComment` inserted both breaks into the owning token's leading trivia; `TernaryLineBreakRewriter` reads `HasTrailingEndOfLine` / `HasLeadingEndOfLine` of `?` and `:` | Disagree for conditional operators | finding #1 |
| First in block (`BlockStartUtilities.IsFirstInBlock`) | Resolved preceding token: none, `{`, switch-label colon | `BlankLineEditor.IsExemptFromPrecedingBlankLineBeforeRegionDirective` (a different decision, documented: the colon is not exempt there); `BlankLineTokenCleanupRewriter:488` (blank-line removal after `{`, a different decision) | The body was moved unchanged from `BlankLineEditor`; one owner and two callers | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A documentation comment written behind code starts a new line | `DocumentationCommentFormattingPhase.RegisterRelocation` (both forms). `BracePlacer` still carries `{ /** d */ code` with its code (decision #5, recorded in the change doc). `BlankLineTokenCleanupRewriter.StartsWithSingleLineDocumentationCommentAfterLineBreak` is reached only by `FormatNode`, which has no preceding token. `UsingTrailingTriviaBuilder:191` handles a delimited comment behind a directive, which the pipeline no longer reaches (02-skeptic.md). | one owner, plus the documented `BracePlacer` case |
| First comment of a token's leading trivia | `TriviaListUtilities.IndexOfFirstComment` (`rg "IndexOfFirstComment\|FindFirstComment\|FirstCommentIndex"`): the documentation phase and the blank-line rewriter (twice). The private `FindFirstCommentIndex` is removed. | one owner |
| Token is first in a block or switch section | `BlockStartUtilities.IsFirstInBlock`, called by the blank-line rewriter and the documentation phase. RH8303's private `IsFirstInBlock` (`RH8303…Analyzer.cs:112`) is on the analyzer side and predates the run. | one formatter owner |
| Documentation-comment kind | `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` (Core), reused | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `DelimitedDocumentationRelocationPhaseTests` (23) | The documentation phase alone moves exactly today's delimited set, inserts no blank line, and keeps the trailing whitespace and the line endings | A delimited comment left behind code, or moved where it must stay, or an extra line break, or a foreign line ending | Phase alone, LF and CRLF. Compares after `TrimLineStarts`; `AssertUnchanged` compares exactly. | Adequate for placement. Whitespace left in front of a moved comment is invisible here, but the full-pipeline tests catch it. |
| `BlankLinePhaseDocumentationCommentTests` (12) | The blank-line phase alone inserts no line break in front of either form and gives a doc comment that starts its line FMT-BLANK-011's blank line, with the first-in-block exemption | Any change to an inline doc comment, or a missing or extra blank line | Phase alone, LF and CRLF, exact | adequate |
| `DelimitedDocumentationRelocationTests` (29) | Full-pipeline output for today's moved and kept shapes | A different output, a second-pass change, or a phase reversal | `AssertRuleResult` (LF and CRLF, two passes, phase stability) | Adequate for the shapes covered. No ternary member is covered (finding #1). |
| `ReihitsuFormatterNodeBlankLineTests` (+4: N1, N1+N4, N4, N2) | Node-level output matches document level, and FMT-PIPE-010 write-back holds | A different target output | `AssertFormatsTarget` (LF and CRLF, second pass) | adequate |
| RH3202 (+3: N1, N2, N4 through the fix) | The code-fix output is today's output | A different fixed text, or no convergence | `Verify`, LF and CRLF | adequate |
| `DocumentationCommentRequirementTests.DocumentationBehindOpeningBraceMovesOntoItsOwnLine` (tag added) | FMT-BLANK-012's opening-brace example | A different output | `AssertRuleResult` | adequate |

Visible output is decided (check 10). Every new or changed expected value of a formatter or code-fix test is `main`'s output. The tests passed on cea4238, before the implementation (04-test-implementer.md), and my corpora give identical output for every pinned shape. So no expected value puts a visible output in question. No rule page changed. Finding #1's change is in output that no test pins.

## Failure classes dismissed

- FC-004: no comment, directive, or disabled text is lost or reordered in 895 fixtures. Every differing output keeps all trivia.
- FC-005: the directive qualifier cannot be constructed (a directive ends its line). The cases with directives behind the comment (`#if`, `#if false`, `#region`, `#pragma`) are identical to `main`.
- FC-006, FC-022 to FC-026, FC-029, FC-036: only line starts move. No tokens change, and literals, identifiers, and modifiers are untouched. Verbatim and raw literals in front of the comment are identical to `main`.
- FC-007, FC-008: RH3202 converges in 2 iterations on head and on `main`. RH5308's `no-progress` on shapes where the comment is already on its own line is the same on both versions (`apply-fix.sh RH5308`, identical diffs).
- FC-009, FC-018, FC-030: no code fix or rename is changed.
- FC-010, FC-011, FC-012: every fixture is a fixed point after one pass under LF and CRLF on head. The phase-level tests check each phase against itself.
- FC-014: no new rule.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*.
- FC-019, FC-020, FC-021: comments and directives are in the corpus gaps (in front of the comment, behind it, multi-line `/* */` ending on the line, `// line` behind). All are identical to `main`.
- FC-028: N1, N2, and N4 are pinned at node level. `FormatNode` has no preceding token, so nothing moves before or after the change. The `FormatNodeInDocumentWithContextAsync` callers (RH5111, RH3208, fluent chains) pass through the same documentation-phase path as `FormatNodeInDocumentAsync`. The ternary member reaches RH3202 (finding #1).
- FC-032: the Release build and the validator follow. Code-fix results through the formatter were checked directly (finding #1).
- FC-033, FC-034, FC-035: expected values come from `main`'s output, observed before the implementation. The tests use the reported shapes themselves.
- FC-037: no text-only claim is made.
- FC-001, FC-002, FC-003, FC-013, FC-016, FC-027, FC-031: checked and not dismissed. FC-013, FC-016, and FC-027 are finding #1; FC-031 is finding #2. FC-001, FC-002, and FC-003 hold (see *Guard scope*).
- Class matrix "Tab width across formatter phases": not reached. The preparation phase replaces tabs before the documentation phase, and the corpus's tab gaps are identical to `main`.

## Architecture findings

_None._ The run removes the duplicate ownership the issue names (decisions #8 and #12): one owner each for the move, the first-comment fact, and the first-in-block fact. Finding #1 is a trivia-placement dependency between the documentation phase and `TernaryLineBreakRewriter`. Neither component owns, copies, or predicts the other's decision, so it is not one of the structural causes.

## Hints

- Not a finding: the `///` form of finding #1 predates the run. On `main`, `int M(bool b) => b ? 1 : /// d` / `2;` formats to `? 1 :` / blank / `/// d` / `2;`, and RH5308 reports that output with a `no-progress` fix. The same happens for `b ? /// d` / `1 : 2`. The documentation phase writes the `///` line break into the trailing trivia of `:` or `?`. Follow-up candidate: FMT-BREAK-018 against RH5308 for any documentation comment moved off a conditional operator. Its fix would change visible output.
- The scratchpad worktree of `origin/main` that I used for the differential runs has been removed.
