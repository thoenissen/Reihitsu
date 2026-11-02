Role: auditor
Commit: a04ae63
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta e2e6d50..a04ae63 (repair round 3: `00-lead.md` decisions 32–35). Head `claude/confident-rubin-j3pkrc` at a04ae63 (checked out). `git ls-remote origin refs/heads/main` gives aec8dc64b5e98626c0720171689686a9f2427bab, which is an ancestor of the head. `scripts/lane-check.sh behavior-change`: confirmed.

The diff outside `sdd/work/` is:

- 29d27c9 (decision 34): the summaries of `BlankLineSpacingPolicy` and `LineBreakBlockRewriter.EnsureStatementsStartOnSeparateLines`. `scripts/verify-text-only.sh --base 35d7b9f --head 29d27c9`: `TEXT-ONLY PROOF: PASS`. Both C# files are token-identical, with changed trivia: documentation.
- c4225c3 (decision 33): it removes `ExpressionBehindCommentTailWithoutBlankLineRuleStaysOnCommentLine`, `TerminalBreakBehindCommentTailStaysOnCommentLine`, `StatementBehindCommentTailWithoutBlankLineRuleIsLeftAsWritten`, and the now unused helper `AssertUnchanged`, and narrows the summary of `StatementBehindMultiLineCommentTests`.

That matches handovers 20–22. The scope did not grow: no spec, change doc, rule page, or production behavior changed. This is a delta audit. The evidence of `19-auditor.md` for the unchanged code (fixtures, the corpus against `main`, the FMT-PIPE-025 probe) still applies, because no compiled production token changed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Closed in `19-auditor.md`. The repair touches no code it depends on |
| 2 | closed | Closed in `19-auditor.md`. The repair is a comment-only change in that file |
| 3 | closed | Closed in `19-auditor.md` |
| 4 | open, decided by the lead | The code is unchanged, so every member listed in #4 still fails exactly as on `main`. The lead handled #4 under *Architecture findings* (decision 32: nothing in this run, follow-up with the output question). The follow-up candidate in `00-lead.md` now names the own-line members, the first statement of a block or section, sections, labels, and the structural cause (`TokenGapNormalizer.EnsureTokenStartsLine` via `TriviaListUtilities.FindFirstContentIndexOnLine` against `ChainLineBreakRewriter.IsBehindMultiLineCommentTail`). It is not reported again here. See *Hints* |
| 5 | closed | The summary of `EnsureStatementsStartOnSeparateLines` now limits the move to "when the multi-line comment starts on the previous statement's line" and says "A comment that starts its own line is not handled here". Traced against the code: the `else if (requiresBlankLine)` branch calls `EnsureTokenStartsLine`, which leaves a token behind an own-line comment's tail in place (#4). The sentence "Other pairs whose gap already holds a line break are left as written" matches the code: no branch handles a pair without a rule whose gap already holds a line break |
| 6 | closed | The three tests are gone (no assertion that `B();` or a terminal `break` stays behind `*/` remains). I searched the run's whole test diff against `main` for a statement behind `*/`. Every remaining hit is an input of a tagged test whose pair has a blank-line rule, or a row of the observer fixture. No expected value asserts the undecided shapes. The other side of the guard is still exercised without asserting output: `PhaseLineBreakOwnershipTests` fixture `StatementsBehindMultiLineCommentTail` with rows M8 and M9 (4 observer tests), and `BlankLineSpacingPolicyTests` row `a++; /* x\n */ a--;` → `false` |
| hint (summary wording) | closed | `BlankLineSpacingPolicy` now reads "between statements, depending on the kinds of the statements" |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
_N/A: the repair adds and changes no predicate, guard, or exemption. The verify-text-only proof shows the production tokens unchanged, and the guard-scope rows of `19-auditor.md` stand._

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a token behind a multi-line comment's last line starts its line | Unchanged since `19-auditor.md`: `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, `TokenGapNormalizer.EnsureTokenStartsLine`, `ChainLineBreakRewriter.IsBehindMultiLineCommentTail`, `HasLineBreakBetween` | #4, decided by the lead as decision 32 (follow-up). The repair adds no owner |
| Closing-brace and statement blank-line rules | Unchanged: `BlankLineSpacingPolicy` only | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `StatementBehindMultiLineCommentTests` (7 remaining, tagged) | Unchanged from `19-auditor.md` | as before | `AssertRuleResult` | adequate. The narrowed class summary describes exactly these seven: every pair has a blank-line rule |
| `LineBreakPhaseCommentTailStatementTests` (remaining rows) | Unchanged | as before | `AssertLineStartsBelowCommentTail`. It still uses `_lineEndings`, `NormalizeLineEndings`, `DescribeLineEnding`, and `ExecutePhase`, so the removal leaves no dead helper | adequate. The class summary covers only the rule side, which is what remains |
| Removed tests (3) | — | — | — | They carried no `[Requirement]` tag, so no requirement loses coverage (`22-spec-checker.md`, check 7, agrees) |

Visible output (check 10): the repair adds no expected value. It removes the only two expected values that no requirement or answer covered (#6). No rule page changed.

## Failure classes dismissed

Every class in the catalog was dismissed for the same reason: the repair changes no compiled token, as the text-only proof over 29d27c9 shows, and it only removes tests. No class can reach new behavior. For FC-017 and FC-032, the remaining assertions are unchanged and still checked against `main`'s output (`19-auditor.md`). FC-031: the two changed summaries were traced against the code above, and the narrowed test-class summary matches its remaining tests. FC-037: the text-only claim rests on `verify-text-only.sh`, not on a grep.

## Architecture findings
_None new._ The structure behind #4 is unchanged and decided (decision 32).

## Hints

- **Decision 32 and *Architecture findings*.** `squad/README.md`, *Architecture findings*, says that a class repeated in an audit of the run is changed in this run when the structural change stays within the components the run changes, and `TokenGapNormalizer` is in this run's diff. When that change would put visible output in question, it becomes an output question. Decision 32 instead defers the structure and its output question to a follow-up. The reasons given are that the shapes are outside the issue, identical and stable on `main`, and that the issue's Done-when holds. That choice is the lead's to make, and the README says so. It is recorded here only so that the published PR states it plainly, together with the follow-up link.
- `LineBreakBlockRewriter.cs:65`: the last line of the changed summary is 182 characters long, while the lines around it wrap at about 125. No convention enforces a width, and other files have doc lines that long, so this is cosmetic only.
