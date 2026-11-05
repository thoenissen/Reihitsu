Role: auditor
Commit: f11929560b8d407183a33d7316f15864122addf6
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This audits the repair delta of repair round 1 against `79ae25f3`, the lead's repair-round commit on top of the audited head `779ecf7e` of `14-auditor.md`. Head `f1192956` is checked out with a clean tree. Base `origin/main` is `df4f828d`, as my prompt states, and the branch contains it (`git merge-base --is-ancestor`). The repair diff (`git diff 79ae25f3 f1192956`) holds what handovers 15–19 describe:

- the FMT-STRUCT-026 and FMT-PIPE-029 amendments, and the change-doc amendment;
- the formatter, RH7101, and architecture tests;
- `TokenGapUtilities`, `FieldDeclarationSplitTransform.BuildTrailingTrivia`, and `ReihitsuFormatter.FormatSplitFieldInContext`;
- the removal of the private `GetColumn` copy.

The repair stays inside the run's components and their counterparts, so this is a delta audit, not a full one. I relied on the cleaner's Release build and the roles' focused test results, and did not re-run them.

How I checked:

- I ran `scripts/apply-fix.sh RH7101 --max-iterations 1` on the head over 22 fixture shapes, LF and CRLF. All 44 runs ended `fixed`, one action and one iteration each, and LF and CRLF gave identical diffs.
- I formatted the same fixtures with `scripts/format.sh` on the head, and formatted the result a second time: 0 of 22 changed.
- I compared each fix result with the formatter's output.
- The fixtures were in the scratchpad and are deleted.

Shapes:

- A `/* */` comment that spans lines behind a separating comma, combined with:
  - a leading comment on the next declarator (`/* */` and `/** */`);
  - an initializer and a method behind;
  - nesting;
  - an empty comment body;
  - a target behind `{`;
  - a member behind on the target's line;
  - a member in front, with three declarators;
  - a struct, a record, and an interface `const`;
  - a tab-indented member.
- Shared-line gaps in front and behind, holding `/* */` and `/** */` comments in every combination of one-line and spanning lines, `///`, and a spanning `/** */` with a line break behind it.

Status of the findings of `14-auditor.md`:

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `BuildTrailingTrivia` writes the line break when the comments it carries hold one (`TokenGapUtilities.HasLineBreak`). Members `a`, `b`, `c`, `h`, the two-comment shapes, and the member-in-front case now give `main`'s document-level output and the formatter's layout through the fix (tests in 16; my fixtures e1–e9, f2, f3) |
| 2 | closed for the members it listed | The behind case `q` and the in-front case `p` now hold (tests in 16; my fixture f1 for a method in front). But members that #2 listed as holding, a documentation comment in the gap, now fail: see #6 |
| 3 | closed | The banned set is a dependency closure from the phases, the rewriters, and the pipeline. I checked by `rg` every formatter class not named `…Helpers`, `…Utilities`, or `…Extensions` and not in `Data`. None is referenced only by exempt classes, so the name exemption hides no class that a phase runs |
| 4 | closed | `ReihitsuFormatter` calls `SyntaxTokenPositionUtilities.GetColumn`, which has the same body. The copy is gone |
| 5 | closed | The `GetFixAllScenario` comment describes the sequential Fix All provider |
| Architecture finding of 14 | superseded by the architecture finding below | All three components now use `TokenGapUtilities` for "the gap holds a line break". The decision still has a second owner, see below |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 6 | high | `src/Reihitsu.Formatter/ReihitsuFormatter.cs:468-474` (`sharesLineWithPrecedingMember`, `sharesLineWithFollowingMember`). Counterparts: `DocumentationCommentFormattingPhase` (relocation of an off-position doc comment), `MemberLineStartRewriter` | FC-001, FC-016, FC-013, FC-021 | #2 | PR-introduced (by the repair) | **The decision.** The repair decides "shares the line" with the line-break phase's predicate, applied to the gap in the input. In document-level formatting, the documentation-comment phase runs before the line-break phase and starts a doc comment written behind code on a line of its own. Its member goes with it. When the gap holds a doc comment and a line break inside a comment, the entry point now says "does not share", while document-level formatting starts the first field's line, or the member's line, on a new line. **What breaks.** The amended FMT-PIPE-029 bullet requires the entry point to move "the first field or the other member where document-level formatting moves it". The first field's line is "the target's leading trivia, such as a documentation comment, and then its first token" (FMT-PIPE-029, first bullet), and SHARED-COMMENT-012 puts the target's first token, behind the closing `*/`, on the line in front. The repair also breaks answer 1a–1f A (the fix result follows the formatter's layout). **Example (d1).** Input `class C⏎{⏎    private int _p; /** a⏎       b */ private int _x, _y;⏎}`. The fix gives `private int _p; /** a` / `       b */ private int _x;` / `    private int _y;`. The formatter gives `private int _p;` / blank / `    /** a` / `       b */ private int _x;` / `    private int _y;`, so formatting the fix result moves the doc comment again. At `779ecf7e` the fix moved it: `AreSeparatedByEndOfLine` saw no end-of-line trivia, so the fix's output equalled the formatter's minus the blank line, which 14 measured as a holding member. The repair turned these into failures. **Failing members** (fix output, LF and CRLF; formatter output measured on the head): (d1) in front, a `/** */` that spans lines alone; (d2) behind, the same: `_x, _y; /** a⏎ b */ private int _q;` keeps `_q` behind `_y;`; (d4) in front, `/* a */ /** b⏎ c */`; (g1) behind, the same; (d5) in front, `/* a⏎ b */ /** c */`; (d6) behind, the same; (g2) in front, `/* a⏎ b */ /** c⏎ d */`. Its behind form uses the same predicate and was traced, not run. **Holding members:** a one-line `/** */` alone, in front (d3) and behind (d7); a `/* */` that spans lines alone, in front and behind (`p`, `q`, `pq`, tests); a `/* */` that spans lines plus a one-line `/* */` (f4); a method in front behind a spanning comment (f1). Also a gap with an end of line behind the doc comment: `///` in front (d8) and behind (d9), and a spanning `/** */` followed by a line break (d10). There, the target's first token, or the other member, already starts its line, so FMT-PIPE-018 and FMT-PIPE-010 keep the trivia, unchanged by the repair. No test covers a doc comment in a gap that holds a line break inside a comment | Decide "shares the line" from the phases' own result on the context copy, not by restating one phase's rule on the input. For a target whose first token does not start its line in the document, the question is whether the phases started the first field's line, or the member behind, on a new line. One owner then covers both the doc-comment phase and the line-break phase (see *Architecture findings*). Add RH7101 fix tests and entry-point tests for d1, d2, d5, and d6 that assert the formatter's layout without the blank line, as FMT-PIPE-029 states |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the split writes a line break behind the comments that trailed a comma (`BuildTrailingTrivia`) | The trivia the split builds: one space and each comment of the comma's trailing trivia. True when the last comment is `//`, when a comment leads the next declarator, or when any line terminator stands inside the built list (`TokenGapAnalysis.OfTriviaRange`, every C# line terminator) | `MemberLineStartRewriter` (`HasLineBreakBetween`, the same analysis over the whole gap). The next field's leading trivia is empty unless it holds a comment, and that case already writes the break, so the built list is the whole gap | Same statement, same span. Both sides tested: one-line `/* */` (no break) and spanning `/* */` (break); `// c` behind a spanning comment writes one break | holds |
| Whether another member shares the target's first or last line | `TokenGapUtilities.HasLineBreakBetween` on the input gap | Document-level formatting: `DocumentationCommentFormattingPhase` relocates a doc comment written behind code, then `MemberLineStartRewriter` runs `HasLineBreakBetween` on the gap as that phase left it | Disagree when the gap holds a doc comment together with a line break inside a comment | finding #6 |
| Whether the fix is offered | Unchanged from 14 | Unchanged | Unchanged | holds (14) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The gap holds a line break | `TokenGapUtilities.HasLineBreakBetween` / `HasLineBreak` → `TokenGapAnalysis` (Core). Callers: `MemberLineStartRewriter:96`, `ReihitsuFormatter.cs:472/474`, `FieldDeclarationSplitTransform.BuildTrailingTrivia`. `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` remains at `ReihitsuFormatter.cs:186` (pre-existing, `FormatNodeInDocumentAsync`, hint of 14) | one statement for the three components of decision 21 |
| Whether a member starts a new line in document-level output | The doc-comment phase (relocation) and `MemberLineStartRewriter` in the pipeline; `FormatSplitFieldInContext` restates only the second, on the input | two owners: finding #6 |
| A field behind a comment that spans lines starts its own line | `FieldDeclarationSplitTransform.BuildTrailingTrivia`, stated by FMT-STRUCT-026. The line-break phase leaves such a gap alone, so it writes no second break | one owner; the spec states the split's share |
| Token column | `SyntaxTokenPositionUtilities.GetColumn` (Core, now used by `ReihitsuFormatter`), `LayoutComputer.GetColumn` (pre-existing), `PatternAnchor.GetColumn` (different signature) | the run's copy is gone |
| Which formatter types a code fix must not use | `CodeFixFormatterAccessTests.GetTypesCodeFixesMustNotUse`, a closure with a name and namespace exemption; FMT-PIPE-009 text; decisions 14 and 23 | one enforcement; the exemption matches FMT-PIPE-009's helper carve-out |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `StructuralTransformSeparatorTests` 6 new rows (FMT-STRUCT-026) | Exact phase output, and the text parses back to the tree | A missing or doubled line break behind a comment that spans lines, or a break behind a one-line comment | `AssertSplitFields`: whole text, `AssertSameTree`, LF and CRLF | adequate; both sides of the boundary, and exactly one break with `// c` |
| `FieldDeclarationSplitTests` 7 new rows (FMT-STRUCT-026) | Document-level output equals `main` | A different final layout | `AssertRuleResult`: LF, CRLF, second pass, phase stability | adequate; expected values measured on `main` (16, FC-035) |
| `ReihitsuFormatterFieldSplitTests` 5 new rows (FMT-PIPE-029, FMT-PIPE-010) | Exact document text after the context entry point | A member moved off, or left on, a shared line | `AssertSplitsTarget`: whole text, two passes, LF and CRLF | adequate for the members given; no doc-comment-gap member with a line break inside a comment (#6) |
| RH7101 5 new tests (RH7101-004) | Exact fix output, one offered action, and the fix result is the formatter's stable layout | Any other output, or a formatter change to the result | `Verify`, `AssertSingleFixUnderBothLineEndings`, `AssertFormattingChangesNothing` | adequate; same gap as above (#6) |
| `CodeFixFormatterAccessTests` (FMT-PIPE-009) | No code-fix type depends on a phase, the pipeline, a rewriter, or a class a banned class uses | A code fix that references, for example, `IndentationRewriter` | ArchUnitNET `NotDependOnAny` over the closure; reflection rows for both sides (12 new banned rows, 3 new allowed rows) | adequate; the static rewriters and phase helpers that 14 named are now banned and pinned |

## Failure classes dismissed

- **FC-002**: FMT-PIPE-029's qualifier "on the target's first/last line" is now defined by document-level formatting. The code restates only one of the two phases that decide it, and that is #6, not a missing condition elsewhere.
- **FC-003**: The new comment condition in the split is tested on both sides.
- **FC-004, FC-019, FC-020, FC-021**: The split carries the same comments as before and adds only an end of line, and the fixtures dropped no comment. Directives still withhold the split (FMT-STRUCT-023), and the repair does not change that guard. A `/* */` ending mid-line is the subject of #1, now holding, and of #6.
- **FC-005**: No directive guard changed.
- **FC-006, FC-022–FC-027, FC-029, FC-036**: No new rewrite semantics, literals, identifiers, nested-shape logic, key/value layout, or rename.
- **FC-007, FC-008, FC-009**: The 44 fix runs needed one iteration each. Fix All and the offer guard are unchanged by the repair.
- **FC-010, FC-011, FC-012**: A second formatter pass over all outputs changed nothing. No loop was added.
- **FC-013**: Covered by #6. Every other fix output equals the formatter's layout, apart from members the fix does not touch by design (FMT-PIPE-010/018: d8–d10, and `_q` behind `e */` in e9).
- **FC-014, FC-018, FC-025, FC-028, FC-030**: Not touched by the repair.
- **FC-015**: The `GetColumn` copy is removed. No new copy was found by `rg`.
- **FC-017**: See *Assertion adequacy*.
- **FC-031**: The changed summaries of `TokenGapUtilities.HasLineBreakBetween`, `HasLineBreak`, `BuildTrailingTrivia`, and `GetTypesCodeFixesMustNotUse`, and the new inline comment in `FormatSplitFieldInContext`, describe the new code. The inline comment repeats the claim of the FMT-PIPE-029 bullet, which #6 shows does not hold for a doc-comment gap.
- **FC-032**: Both CLIs used in this audit were built from the head tree by the scripts.
- **FC-033, FC-034, FC-035**: Expected values come from the spec examples, from `main` (measured in 15 and 16), or from answer 1a–1f A. No harness normalizes the comment shapes away; LF and CRLF are both run.
- **FC-037**: Not a text-only change.
- **Matrix *Tab width across formatter phases***: Its decision does not reach the repair. A tab in front of the member holds (f5).

## Architecture findings

- **Components.**
  - `ReihitsuFormatter.FormatSplitFieldInContext` decides whether another member shares the target's first or last line.
  - In document-level formatting, two phases decide whether the first field's line, or the other member's line, starts a new line. `DocumentationCommentFormattingPhase` moves a doc comment written behind code onto its own line, and its member with it. `MemberLineStartRewriter` then breaks a gap that holds no line break.
- **Decision they share.** Whether the first field, or the member behind the target, starts a new line. The amended FMT-PIPE-029 states this as the decision document-level formatting makes.
- **Structural cause.** The entry point predicts the pipeline's decision from the input instead of reading it from the phases' result. Round 0 predicted it with `AreSeparatedByEndOfLine`, which missed a line break inside a comment (#2). The repair predicts it with the line-break phase's rule, which misses the doc-comment phase's relocation (#6). Each round fixed one member and broke or missed the next, so this repeats the class of #2.
- **Input that shows it.** d1, `private int _p; /** a⏎       b */ private int _x, _y;`. Document-level formatting starts the doc comment and `private int _x` on a new line; the fix leaves them behind `_p;`.
- **Scope.** The entry point is in the run, and both phases are its counterparts. The context copy that the phases format is already at hand in `FormatSplitFieldInContext` (`formattedContext`).

## Hints

- `18-cleaner.md` lacks the `Role:` and `Commit:` header lines that `squad/README.md`, *Handovers are files*, requires. This is process only; the file is deleted before publishing.
- The 14 hint about `FormatNodeInDocumentAsync`'s `keepsLeadingTrivia` (`AreSeparatedByEndOfLine`, `ReihitsuFormatter.cs:186`) is unchanged and still unverified for a caller-reached input.
