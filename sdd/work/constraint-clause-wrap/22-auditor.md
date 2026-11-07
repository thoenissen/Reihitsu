Role: auditor
Commit: 25b7dc3
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta `ba4e49b..25b7dc3`, measured against the audit at ba4e49b (`18-auditor.md`). The head 25b7dc3 (`claude/dreamy-curie-x5c135`) is checked out and clean. `origin/main` is e3924e22321a0b8a0c0b72ce185c2ed6fc81f9b7 and is merged into the head. The diff matches what `19`–`21` describe:
- three new tests in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConstraintClauseLineBreakTests.cs`;
- a rewrap of the `JoinGap` summary in `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs`;
- handovers and lead notes.

No spec, change doc, ledger, or rule page changed. The repair stays on finding #3's tests, so this audit covers the delta only.

`scripts/lane-check.sh behavior-change`: confirmed. `scripts/verify-text-only.sh --base ba4e49b --head 25b7dc3`: `GenericConstraintLineBreakRewriter.cs` is text-only. Its 1149 tokens are identical and only documentation trivia changed. The test file is the only changed file that carries compiled behavior.

Evidence:
- I checked the expected columns of the three tests by measuring them against the raw-string indentation.
- I ran a mutation check in a scratch worktree of 25b7dc3, which was removed afterwards. I reverted only the second condition of `PlaceOneConstraintPerLine`, back to `TokenGapUtilities.HasLineBreakBetween(clause.Constraints[separatorIndex].GetLastToken(), separator)` as at 0260784. `ConstraintClauseLineBreakTests` then failed exactly one test, `CommaBehindMultiLineCommentClosingLineInWrappedClauseIsFollowedByLineBreak` (47/48).
- Unmutated, the class gives 48/48.

## Previous findings

- **#1: closed** (in `18-auditor.md`). The repair does not touch it.
- **#2: closed** (in `18-auditor.md`). The repair does not touch it.
- **#3: closed.** Every member that #3 listed as untested now has a test:
  - Wrapped `O` (`where T : class /* a⏎ b */, new(),⏎ IDisposable`) has a test, and it pins the guard: reverting the separator condition alone now fails exactly that test. Its expected value is `b */,⏎ new(),⏎ IDisposable`, with both later constraints at column 18, under `class` behind `where T : `. FMT-BREAK-037 and FMT-INDENT-032 demand that output.
  - Wrapped `M` is tested. `new()` is at column 19, under `class`, which stands on the comment's closing line (`b */ : class,` with `b` at 12).
  - Wrapped `N` is tested. `new()` is at column 17, under `class` on `b */ class,`.
  - In every input, the later constraint starts at the block column (12), so the alignment is exercised, not only kept.
  - The other members of #3's table were already recorded as covered, and the repair does not change them.
  - The optional FMT-BREAK-037 example was declined (decision 12). That is a spec-text choice. No requirement is missing, because the output is already required and now pinned.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | none | – |

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption. The only production edit is a documentation-trivia rewrap, proven text-only. The guard table of `18-auditor.md` still applies unchanged. Its one open row ("Comma kept at the start of its line", no test pins it) is now pinned by the wrapped-`O` test, which the mutation check verified._

## Policy ownership
_N/A — the repair moves no policy. The owners recorded in `18-auditor.md` are byte-identical in the production code (text-only proof)._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ColonBehindMultiLineCommentClosingLineInWrappedClauseAlignsUnderFirstConstraint` (wrapped `M`) | FMT-BREAK-037: a comment interior between the type parameter and the colon puts no token on a line of its own. FMT-INDENT-032: later constraints align under a first constraint that stands on the comment's closing line. | Colon or `class` moved to its own line; `new()` not at column 19; instability on a second pass or under CRLF | `AssertRuleResult(input, expected)` (LF, CRLF, second pass, phase stability) | adequate. No single rewriter condition changes this output, so the test guards the contributor's alignment, as `19-test-implementer.md` states. |
| `FirstConstraintBehindMultiLineCommentClosingLineInWrappedClauseAlignsLaterConstraints` (wrapped `N`) | The same, at colon–first constraint | `class` split from `b */`; `new()` not at column 17 | `AssertRuleResult(input, expected)` | adequate |
| `CommaBehindMultiLineCommentClosingLineInWrappedClauseIsFollowedByLineBreak` (wrapped `O`) | FMT-BREAK-037: a comma behind a comment's closing line stays there, and the constraint behind it starts a line. No partial split. FMT-INDENT-032: column 18. | `new()` left on the comma's line (the 0260784 output) | `AssertRuleResult(input, expected)` | adequate. The mutation check shows it fails when the guard named in #3 is reverted alone. |

## Visible output is decided

The three tests expect outputs that FMT-BREAK-037 (the multi-line-comment paragraph, one constraint per line, comma at the end of the line in front) and FMT-INDENT-032 (alignment under the first constraint) already demand as written. Both requirements stand unchanged in this repair. No rule page changed. Nothing is undecided.

## Failure classes dismissed

Every class not named here is dismissed as in `12-auditor.md` and `18-auditor.md`. The repair changes no compiled production code (text-only proof), so none of those reasons moved.
- FC-001/FC-002/FC-003: no guard changed. FC-003's "both sides" gap from #3 is now covered for `M`, `N`, `O`, `P`, and `S`.
- FC-017: see *Assertion adequacy*.
- FC-031: the rewrapped `JoinGap` summary has the same words as before and still describes the code: it joins through `CollapseTokenToSameLine`, which refuses a comment, a directive, or disabled text. The three test summaries describe what their assertions check. The diff adds no `TODO`, no tracker number, and no commented-out code.
- FC-032: `20-cleaner.md` built Release with 0 warnings after the last change. The class passes 48/48 at the head.

## Architecture findings

_None._

## Hints

- `18-auditor.md`'s cosmetic hint about the long second summary line of `JoinGap` is resolved.
- `18-auditor.md`'s other hints are unchanged and pre-existing:
  - the `TokenGapUtilities.HasLineBreakBetween` summary (follow-up 11);
  - the lost space behind `*/` in `System. /* a⏎ b */ IDisposable`;
  - the first line of an own-line multi-line comment in front of a later constraint is reindented while its closing line is not.
