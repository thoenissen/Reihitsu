Role: auditor
Commit: 5e2f3561bc278474e6e2d76fdd6974713c66ecf3
Verdict: state mismatch

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `0831dfe` (the commit `12-auditor.md` audited). Head `5e2f356` is checked out and the working tree is clean. The repair is what handovers 13–17 describe. Production code changed in one file: the RH5401 code fix gains `CountBlankLinesInAttributeRegion`, and `CanCollapse` is renamed to `FormatsOnOneLineKeepingBlankLines` with its comments reworded. Tests changed in one file: one test is changed and six are added. The rest is RH5401-003/-004, the change doc, and `sdd/work/`. Scope did not grow.

**State mismatch.** My prompt gave `origin/main` as `6d574e1`. The audit was cut off by a usage limit, and when I checked again with `git ls-remote origin refs/heads/main` it read `e3cf4cdc459371c3a532a3580e6b5fc49a7cf609`. That commit is not merged into the branch. It contains two commits: #1870, which joins element accesses and null-forgiving operators written below their element, and #1872, which rewrites every token of a file in one tree walk in the indentation, cleanup, and tab-replacement phases. Together they change 42 files (+5016/−513). Among them are the line-break phase (`ArgumentListLineStartRewriter` is removed and `AttachedPartLineStartRewriter` is added), the precheck (new FMT-UNSUP-014), and the indentation phase. The RH5401 guard decides from the formatter's output, so the evidence below may change once `main` is merged. `git merge-tree` reports a clean merge. Merge `origin/main`, then repeat this repair audit on the merged head.

The tables below are what I measured on `5e2f356`, before the merge. They are not a verdict. Re-check them on the merged head; fixtures and outputs are listed with each item.

Probes (LF and CRLF give identical status and diff everywhere; no not-converged, no-progress, invalid, or drift):
- `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit18/fx/` (`a01`–`a17`), output in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit18/fx.out`
- `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit18/fx2/` (`b01`–`b07`), output in `/tmp/claude-0/-home-user-Reihitsu/5c2095c3-beb1-5980-beb5-54178f424a54/scratchpad/audit18/fx2.out`

## Findings

Measured on `5e2f356`, before the merge.

Previous findings:
- #1: closed for every member it listed. `p03`/`N` (`a01`), `s02`/`O` (`a02`), `P` (`a03`), `R` (`a04`), the third list behind a split list (`a11`), and modifiers (`a14`) all give `no-fix-offered`. Two blank lines behind the comma (`s03`) lower the total by construction. A new member of the same class fails: see #4.
- #2: closed. The method is renamed. Its summary, its return doc, and the call-site comment now name both conditions, and the one-line check keeps its own accurate comment.
- #3: closed. The change doc's RH5401 blocks now hold `L` and `Q`, and its decision text names FMT-STRUCT-028.

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs`, `FormatsOnOneLineKeepingBlankLines` (the per-gap check and `CountBlankLinesInAttributeRegion` together); `sdd/specs/analyzer/RH5401.md` RH5401-004 ("Each condition catches what the other does not: … example `S`") | FC-001, FC-016 | #1 | PR-introduced | The two conditions compare counts: the gaps behind each list as written, and the total over the attribute region. Neither sees a blank line that the formatter *moves* inside an attribute list. This happens when the formatter drops one inside a list (by splitting it, or by collapsing two into one) and inserts one in front of a comment on its own line inside another attribute's argument list. The per-gap counts and the total both stay the same, so the fix is offered. It removes the blank line the user wrote and adds one they did not write, which is what maintainer answers 3, 5, and 6 rule out. The spec sentence quoted claims the two conditions together cover the case. Failing members (all `fixed`, LF and CRLF): **`a09`** `[Obsolete,⏎⏎ CLSCompliant(false)]⏎[X(1,⏎ // c⏎ 2)]⏎int E⏎    => 1;` → `[Obsolete]⏎[CLSCompliant(false)]⏎[X(1,⏎⏎ // c⏎ 2)]⏎int E => 1;`; **`a10`**: the same with `X(3)` in the second list, so the comment's list has several attributes; **`b01`** with no split list at all: `[X(1,⏎⏎⏎ 2)]⏎[X(3,⏎ // c⏎ 4)]` → `[X(1,⏎⏎ 2)]⏎[X(3,⏎⏎ // c⏎ 4)]`. Holding members: `S` (an insertion in a gap behind a list, caught per gap); `b02`, insertion only, and `b06`, collapse only (total changes, withheld); `b03`, drop and insertion in one list (withheld); `b07`, a block comment on the code's line (no insertion, withheld); `a07`, a comment then a blank line in one gap behind a list (withheld); `a08`, `a12`, `a16`, `a17` (blank lines kept, offered, output matches the input's blank lines). Rare shape: it needs a comment inside an attribute's argument list, which RH5038 reports. No test covers it. | Refuse this class (`AGENTS.md`, *Code-fix coverage*). For example, withhold the fix when a comment on its own line stands between the brackets of one of the property's attribute lists; the owner re-derives the check. The spec author drops or narrows the "each condition catches what the other does not" sentence and names the refusal. The test implementer adds a withheld test for `a09` and one for `b01`. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH5401: total blank lines over the attribute region (new) | Every token gap from the first list's `OpenBracketToken` through the gap into `AttributeLists.Last().GetLastToken().GetNextToken()`, summed with `TokenGapAnalysis.Between(...).BlankLineCount`. The leading trivia of the first `[` is excluded, both as written and as formatted | RH5401-004 ("from the `[` of its first attribute list through its first token behind its attribute lists") | Same boundaries; the loop ends at the gap into the first own token; a property always has a type, so that token exists | holds |
| RH5401: per-gap check combined with the total (changed gate) | Both conditions are required; a property without attribute lists skips both | RH5401-004, maintainer answer 6B | They catch drop-only, insert-only, and the `S` offset. They miss an offset where both the drop and the insertion lie inside attribute lists | finding #4 |
| RH5401: guard input vs applied formatting (FC-028) | Unchanged from 12 | — | Unchanged | holds (reused) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Blank-line count of a token gap | `TokenGapAnalysis.BlankLineCount` (`src/Reihitsu.Core/TokenGapAnalysis.cs`). The new method only sums the owner's count per gap; there is no private counter | one owner |
| "First token behind the attribute lists" | The same expression in the fix (three uses) and in `OrderingDeclarationUtilities` | not a duplicated policy (reused from 12) |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyFixIsWithheldFor…TheFormatterSplits` (4 tests: `N`, `O`, `P`, `R`) and the combined `VerifyFixIsWithheldWhenFormattingWouldDropABlankLineInsideAnAttributeListItSplits` | Diagnostic stays, no action registered | An action registered | `VerifyNoCodeFixIsOffered`, LF and CRLF | adequate |
| `VerifyFixIsWithheldWhenTheFormatterDropsABlankLineInsideASplitListAndInsertsOneInFrontOfAComment` (`S`) | The per-gap condition withholds when the total is unchanged | An action registered | `VerifyNoCodeFixIsOffered` | adequate |
| `VerifyFixKeepsABlankLineInsideAnAttributeListTheFormatterDoesNotSplit` (`Q`), and the changed `VerifyFixKeepsABlankLineBehindAnAttributeListThatTheFormatterKeeps` | Exact output; the blank line inside an unsplit list stays | Any different byte, or a withheld fix | `VerifyCodeFixWithEndOfLine` / `Verify` | adequate (FC-003: offered side of the new condition) |
| (missing) drop and insertion that offset inside attribute lists | Withheld once #4 is fixed | Fix offered | — | missing, part of finding #4 |

## Failure classes dismissed

Reused from `12-auditor.md`, because the repair changed only the RH5401 gate. The reasons there still hold for FC-002, FC-004–FC-015 and FC-017–FC-044, apart from FC-001/FC-016 (finding #4) and these notes:
- FC-007, FC-009: every offered probe converges in one iteration; every withheld probe still reports the diagnostic.
- FC-031: finding #2 is closed; the new method's summary describes its loop exactly.
- FC-042: no shape declared; the new guard serves no declared shape.

## Architecture findings

_None._ The guard reads the formatter's result and does not restate the blank-line rule. Finding #4 is a scope gap in a count comparison, not a second owner of the policy.

## Hints

- Lone `\r` line endings: unchanged from 12, still unverified.
- `a06`, a blank line in front of the `]` of a list the formatter splits, still gets the fix with the misplaced `]` (`[CLSCompliant(false)⏎⏎ ]`). Lead decision 10 records this as dropped; the repair did not change it.
- `a17`, a blank line inside the arguments of an attribute in a list the formatter splits, at `[X(1,⏎⏎ 2), X(3)]`: this blank line is kept, not dropped, so the fix is offered and correct. It differs from spec example `R` (`[Obsolete(⏎⏎ "a"), …]`, dropped). The spec lists `R` as an example only and does not claim that every such blank line is dropped, so this is not a finding. It is recorded so that nobody turns `R` into a general rule.

## Follow-up candidates

_None_ beyond `00-lead.md`.
