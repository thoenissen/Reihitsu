Role: auditor
Commit: ba4e49b
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta `0260784..ba4e49b` against the audit at 0260784 (`12-auditor.md`). Head ba4e49b (`claude/dreamy-curie-x5c135`, checked out, clean). `origin/main` is e3924e22321a0b8a0c0b72ce185c2ed6fc81f9b7 (`git ls-remote` agrees) and is merged into the head. The diff is what `13`–`17` describe. Production: `GenericConstraintLineBreakRewriter` (line-start predicate, shared batch join), `LineBreakTriviaUtilities.JoinGaps` (new), and `ListElementInteriorJoinRewriter` (calls `JoinGaps`). Tests: six new `ConstraintClauseLineBreakTests`. Specs: FMT-BREAK-037 and the change doc. The repair stays on the rewriter, its counterpart, and the requirement, and it changes no requirement ID, so this audit covers the delta. `scripts/lane-check.sh behavior-change`: confirmed.

Evidence: static tracing, and `scripts/format.sh` at the head (two passes, LF and CRLF) over eleven wrapped and unwrapped clauses with multi-line comments in every gap. The same inputs ran on worktrees of 0260784 and e3924e2, both removed afterwards. `scripts/apply-fix.sh RH5110` ran over four clauses whose `where` shares the declaration line (`S`, `P`, `N`, `M`). Fixtures were in the scratchpad.

## Previous findings

- **#1: closed.** Members (a)–(d) now stay as written (examples `M`, `N`, `O`, `P`; tests added). Each was stable on a second pass and identical under LF and CRLF. Member (e) is now unchanged, as decision 11 and its test require. Every decision of the rewriter now uses `SyntaxTriviaUtilities.AreSeparatedByEndOfLine`, which is the predicate behind `SyntaxTokenPositionUtilities.IsFirstOnLine`, which `GenericConstraintContributor` uses. The `IsWrapped` summary and the class remarks now describe the code.
- **#2: closed.** The replacement-map batch join is now `LineBreakTriviaUtilities.JoinGaps`, which both rewriters call. The moved loop is byte-identical to both former copies, including the original `token` passed to `CreateJoinedTrailingTrivia`. `SelectJoinableGaps` keeps the per-token cancellation check and the `CanJoin` order, and `CanJoin` reads the unmodified root, so the lazy iterator changes no result. `ChainLineBreakRewriter` keeps its own copy, which is pre-existing and recorded as follow-up 10.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConstraintClauseLineBreakTests.cs`; guard `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine`, condition `AreSeparatedByEndOfLine(clause.Constraints[separatorIndex].GetLastToken(), separator)` | FC-003, FC-017 | – | same requirement | The repair changed this condition from `HasLineBreakBetween` to the line-start predicate, and that changed visible output. No test pins it, because the only test with a multi-line comment in front of a comma (`O`) is a clause that is not wrapped and never reaches `PlaceOneConstraintPerLine`. Input `where T : class /* a⏎ b */, new(),⏎ IDisposable`. At 0260784 it split partway: `b */, new(),⏎ IDisposable`, so `new()` shared the comma's line. At the head it gives `b */,⏎ new(),⏎ IDisposable`, both at column 18, which is right under FMT-BREAK-037/Q1A. If the condition is reverted alone, every test still passes. The other wrapped-side counterparts of the repair's unwrapped examples are also untested, though the head handles them correctly. `M`'s counterpart is `where T /* a⏎ b */ : class,⏎ new()`, which aligns `new()` under `class` at column 19. `N`'s counterpart is `where T : /* a⏎ b */ class,⏎ new()`, which aligns `new()` at column 17. So FC-003's "both sides" holds only for `P`/`S`. | Add wrapped-side tests for `O` (required: this guard), and for `M` and `N` (alignment under a first constraint on the comment's closing line, FMT-INDENT-032). Take the expected values from the head outputs above, which follow FMT-BREAK-037 and FMT-INDENT-032 as written. No spec change is needed. Optionally add an example `O` → wrapped line to FMT-BREAK-037's `M`/`N`/`O`/`P`/`S` example. |

Members of #3's class (multi-line comment in a gap of a **wrapped** clause), all observed at the head, stable on a second pass, and identical under LF and CRLF:

| Member | Input | Head output | Test |
|---|---|---|---|
| type parameter–colon | `where T /* a⏎ b */ : class,⏎ new()` | `new()` at column 19, under `class` | none |
| colon–first constraint | `where T : /* a⏎ b */ class,⏎ new()` | `new()` at column 17, under `class` | none |
| in front of a comma | `where T : class /* a⏎ b */, new(),⏎ IDisposable` | `b */,⏎ new(),⏎ IDisposable` at column 18. **Changed by the repair** (0260784 split it partway) | none |
| behind a comma | `S` | as example `S` | `ConstraintBehindMultiLineCommentClosingLineStartsLineInWrappedClause` |
| behind two commas | `class, /* a⏎ b */ new(), /* c⏎ d */ IDisposable,⏎ IComparable` | each constraint on its own line behind its comment, column 18 | covered by the `S` path |
| inside a constraint, outside `<>` | `System./* a⏎ b */IDisposable,⏎ new()` | `new()` at column 18 | none needed: same path as the comma member |
| comment and line break behind a comma | `class, /* a⏎ b */⏎ new(), IDisposable` | `new()` and `IDisposable` at column 18 | covered by the existing comment-gap tests |
| RH5110 fix | `S`, `P`, `N`, `M` with `where` on the declaration line | the same as the formatter. LF and CRLF fixed, 4 iterations for 4 diagnostics | covered by the `VerifyFix*` equivalence |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join a gap (type parameter–colon, inside a constraint outside `<>`) | `AreSeparatedByEndOfLine(previous, token)`: previous trailing and token leading trivia, then `WouldJoinAcrossUnjoinableTrivia` | `ListElementInteriorJoinRewriter.CanJoin` and `CollapseTokenToSameLine` (same predicates) | agree; unchanged from 0260784 | holds |
| Clause wrapped: type parameter–colon | `AreSeparatedByEndOfLine(Name.Last, Colon)` | contributor `SetIfFirstOnLine(colon)` → `IsFirstOnLine` → `AreSeparatedByEndOfLine` | agree (same predicate) | holds; `M` tested |
| Clause wrapped: colon–first constraint | `AreSeparatedByEndOfLine(Colon, first)` | contributor `IsFirstOnLine(firstConstraintToken)` | agree | holds; `N` tested |
| Clause wrapped: inside the constraints | each adjacent token pair from the first constraint's first token | parameter list `HasLeadingEndOfLine`/`HasTrailingEndOfLine` | agree for every probed gap | holds; `O`, `P`, (e) tested |
| First constraint joins the colon (`JoinGap`) | `AreSeparatedByEndOfLine`, then `CollapseTokenToSameLine` refuses on a comment | – | this change is unobservable: wherever the old predicate differed, a comment stands in the gap, and the collapse refuses it | holds |
| Comma kept at the start of its line (`PlaceOneConstraintPerLine`, second condition) | `AreSeparatedByEndOfLine(last token of constraint, separator)` | contributor `AlignSeparators` (first on line) | agree | holds, but no test pins it: #3 |
| Later constraint already starts a line (first condition) | `AreSeparatedByEndOfLine(separator, next first)` | parameter list `EnsureSeparatorsHaveEndOfLine` (`HasTrailingEndOfLine(separator)` or `HasLeadingEndOfLine(next)`) | agree | holds; `S`, `P` tested |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Batch join of several gaps (replacement map) | `rg -n "CreateJoinedTrailingTrivia\|RemoveLeadingEndOfLineAndWhitespace\|JoinGaps" src --glob '!*Test*'`: `LineBreakTriviaUtilities.JoinGaps` (used by `GenericConstraintLineBreakRewriter.JoinUnstructuredWraps` and `ListElementInteriorJoinRewriter.JoinInteriorWraps`). `ChainLineBreakRewriter.CloseGap`/`GetPendingToken` is pre-existing on `main` (follow-up 10). Every other hit is a single-gap join. | one owner in the diff; #2 closed |
| "Does a line break separate two tokens of a constraint clause" | `GenericConstraintLineBreakRewriter` and `GenericConstraintContributor`, both through `SyntaxTriviaUtilities.AreSeparatedByEndOfLine` (Core, SHARED-BLANK-008). `rg -n "AreSeparatedByEndOfLine\|HasLineBreakBetween"` finds no `TokenGapUtilities` call left in the rewriter | one predicate; #1 closed |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ColonBehindMultiLineCommentClosingLineDoesNotWrapClause` (`M`) | FMT-BREAK-037: a comment interior does not wrap at type parameter–colon | `new()` split off; fails on 0260784 | `AssertRuleResult(input)` (LF, CRLF, second pass, phase stability) | adequate |
| `FirstConstraintBehindMultiLineCommentClosingLineDoesNotWrapClause` (`N`) | the same at colon–first constraint | the same | `AssertRuleResult(input)` | adequate |
| `CommaBehindMultiLineCommentClosingLineDoesNotWrapClause` (`O`) | the same in front of a comma, in an unwrapped clause | a split | `AssertRuleResult(input)` | adequate for its side; the wrapped side is #3 |
| `ConstraintBehindMultiLineCommentClosingLineDoesNotWrapClause` (`P`) | no partial split behind a comma | `IDisposable` moved; fails on 0260784 | `AssertRuleResult(input)` | adequate |
| `TypeArgumentBehindMultiLineCommentClosingLineDoesNotWrapClause` ((e)) | a comment interior inside `<>` does not wrap the clause | a clause wrapped one per line; fails on 0260784 | `AssertRuleResult(input)` | adequate |
| `ConstraintBehindMultiLineCommentClosingLineStartsLineInWrappedClause` (`S`) | FMT-BREAK-037/FMT-INDENT-032: a line break goes behind the comment, and the constraint column is 18 | `new()` left behind `b */`; a wrong column | `AssertRuleResult(input, expected)` | adequate |
| (missing) wrapped clause with a multi-line comment in front of a comma, behind the colon, or behind the type parameter | the changed separator guard; alignment under a first constraint on a comment's closing line | – | – | missing: #3 |
| `ListElementInteriorJoin*`, the existing `ConstraintClause*` tests | the `JoinGaps` refactor changes no output | any output change | existing `AssertRuleResult` | adequate (`16-cleaner.md`: 127/127; `15-implementer.md`: all suites green) |

## Visible output is decided

`M`, `N`, `O`, `P`, and (e) expect the input unchanged, which is also `main`'s output (the change doc's *Unchanged* section, decisions 8 and 11). `S` (entry 16) is the one new visible output. Q1A demands it: `new()` has to start a line, and the comment, which trails the comma, has to stay on its line by the unchanged FMT-BREAK-001, as for switch sections in FMT-BREAK-033. That leaves one layout, and decision 10 records it. #3's suggested tests assert outputs that FMT-BREAK-037 and FMT-INDENT-032 already demand as written. No rule page changed.

## Failure classes dismissed

Every class not named here is dismissed as in `12-auditor.md`. The repair leaves those reasons as they were: same rewriter entry points, same phase order, no new trivia kinds written.
- FC-001/FC-002: see *Guard scope*. Every predicate the repair changed now matches its counterpart's span and boundary.
- FC-004/FC-005/FC-020: no probe moved or lost a comment. `EndsLine` counts directives, disabled text, and `///` exactly as `HasLineBreakBetween` did. Only a multi-line comment's interior is counted differently.
- FC-010/FC-011/FC-033: all probes were stable on a second pass and identical under LF and CRLF. The appended line break uses `FormattingContext.EndOfLine`.
- FC-007/FC-008/FC-009: `apply-fix.sh RH5110` converged under LF and CRLF, with the iterations equal to the diagnostics.
- FC-015: see *Policy ownership*.
- FC-017: see *Assertion adequacy*; #3.
- FC-019/FC-021: the multi-line-comment axis is now enumerated for both unwrapped and wrapped clauses (#3 table).
- FC-031: the summaries of `JoinUnstructuredWraps`, `IsWrapped`, `JoinGap`, `PlaceOneConstraintPerLine`, `JoinGaps`, `SelectJoinableGaps`, `JoinInteriorWraps`, and the class remarks match the code. There is no `TODO`, no tracker number, and no commented-out code.
- FC-032: `15-implementer.md` ran every non-self-hosting suite after the last production change, and `16-cleaner.md` built Release with 0 warnings.

## Architecture findings

_None._

## Hints

- Pre-existing, outside the diff: the summary of `TokenGapUtilities.HasLineBreakBetween` calls it "the one statement of whether a gap holds a line break", and says that "a token behind such a comment's `*/` is not broken onto a new line by the line-break phase". On `main`, switch sections (FMT-BREAK-033) and parameter lists already decide on `AreSeparatedByEndOfLine`, and with this run constraint clauses (FMT-BREAK-037 `S`) do too. Two predicates decide the same kind of fact in the line-break phase, and the summary overstates its reach. Structural cause: one fact, two owners. A follow-up candidate, not this run's.
- Pre-existing on e3924e2, extending `12-auditor.md`'s spacing hint: `System. /* a⏎ b */ IDisposable` loses the space behind `*/`, in a constraint and in a parameter type alike. Unrelated to this run.
- Uncertain, not traced to an owner: a multi-line comment on its own line in front of a later constraint (`class,⏎ /* a⏎ b */ new(),⏎ IDisposable`) has its first line moved to the constraint column (18), while its closing line `b */ new(),` keeps its column. This is the general handling of multi-line comment interiors, and the rewriter's output for this input is the same at 0260784.
- Cosmetic: the second summary line of `GenericConstraintLineBreakRewriter.JoinGap` runs to about 180 characters, while its neighbors wrap at about 120.
