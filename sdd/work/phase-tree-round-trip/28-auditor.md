Role: auditor
Commit: fbd395c
Verdict: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Scope

This audit covers the repair delta of round 3 against `640c650`. The head is `fbd395cfcd2177558dc3341b59794b3e82873908`, and the checkout is that head with a clean tree. `origin/main` is `57f0f4537d38372f412108a8adbc0c2cd8051678`. It matches `git ls-remote origin refs/heads/main` and is an ancestor of the head.

`git diff 640c650..fbd395c` is what handovers 24–27 describe:

- `AssertAnnotationsKept` in `RawStringMovedLineStartTests` now annotates the whole tree, and the class drops one unused using;
- `IsHoleOpeningLine` drops its always-true conjunct and gets a new summary;
- the `ApplyEdits` summary is extended;
- handovers 23–27.

No spec, change doc, or catalog entry changed. `scripts/lane-check.sh behavior-change` confirms the lane, and scope did not grow. The evidence of `23-auditor.md` for the move itself still holds: it covers the `c23/` members, the whole-tree probe, the node-level modes, and the comparison against `main`. The only compiled change in this round is the removed conjunct, which cannot change a result (see #11).

**How I checked.** I exported the head to scratchpad `mut28/` and did not touch the checkout. There I ran `RawStringMovedLineStartTests` against two production mutations of `InsertLeadingText`:

- **A** — the falsifying change of #9: the node is rebuilt with `SyntaxFactory.InterpolatedStringExpression(start, contents, end)`. 5 of 29 tests fail, and they are exactly the five insertion-path members that handover 24 lists. Each fails in `IndentationPhase` with `dropped annotations (FMT-PIPE-022)` on the `InterpolatedStringExpression`. Log: `mut28A.log`.
- **B** — a member of #9's class that handover 24 did not mutate: every existing `InterpolatedStringText` element of the literal is rebuilt from its own token. This drops the annotations of the text nodes and keeps the token annotations. 3 of 29 tests fail on `InterpolatedStringText ' a'` and `' b'`: `FirstLine…`, `DoubleDollarFirstLine…`, and `FirstLineStartingWithAMultiLineHole…`. In the other two insertion-path members, the mutation changes nothing:
  - `OnlyLineBeingAHole…` (`{x}`) has no text element;
  - in `NestedLiteral…`, the nested literal's text elements (`{x}`) are not rebuilt.

  Log: `mut28B.log`.

I ran no other test. Handovers 24 and 25 report that the unmutated head passes 29/29, and the formatter project passes 3402/3402.

## Status of earlier findings

- **#1–#8 — closed** (`18-auditor.md`, `23-auditor.md`). The repair touches none of their code paths beyond #11's removed conjunct, which cannot change a result.
- **#9 — closed.** `AssertAnnotationsKept` now annotates `root.DescendantNodesAndSelf()` and `root.DescendantTokens()`. That is every node and token of the code, structured trivia excluded, which is exactly the span of FMT-PIPE-022. It covers the literal node that `InsertLeadingText` rebuilds and its `InterpolatedStringText` nodes. Mutations A and B both turn the suite red. The pass criterion did not change: each annotation must be on exactly one carrier of the original kind. That is at least as strict as the requirement, because no fixture removes or duplicates an element.
- **#10 — closed.** The `ApplyEdits` summary now names the edit at the token's end. I traced its reach:
  - `FindToken(line.Start)` never returns a token that ends at the line start, so an end edit comes only from the redirect to `previousToken` in `CollectEdits`;
  - that redirect is taken only for an `InterpolatedStringTextToken` in front of a hole-opening line;
  - `{` carries no trivia, so `GetIndentationLength` is 0 and `OldLength` is 0.

  So "replaces nothing and appends the moved indentation of the line that starts behind the token, a line that starts with an interpolation hole's opening brace" is exact. The cleaner proved the change text-only (`26-cleaner.md`).
- **#11 — closed.** The conjunct is gone. Roslyn puts an `InterpolationSyntax` only in the `Contents` of an `InterpolatedStringExpressionSyntax`, and the caller's cast `(InterpolatedStringExpressionSyntax)interpolation.Parent` relies on the same fact, so behavior is unchanged. The new summary says the predicate does not check for raw and why that suffices. I traced that claim:
  - `IndentationRewriter` passes `LayoutModel.GetRawStringShift`, which reads `_movingLiterals` and gives 0 for an unregistered line;
  - a line that starts at a non-raw hole's `{` starts where that string's text ends, because only verbatim text can end in a line break in front of a hole;
  - `StartsInsideNestedText` (token at the line start, no leading trivia, previous token is non-raw text that ends at the line start) keeps such a line out of `_movingLiterals`;
  - so `CollectEdits` skips it before `IsHoleOpeningLine` is reached.

  This matches probe `c23/n3` of the previous audit.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| A moved line starts at a hole's `{` (`IsHoleOpeningLine`, conjunct dropped) | `FindToken(line.Start)`: the token is the `OpenBraceToken` of its parent `InterpolationSyntax`, and its `SpanStart` is the line start | The parent of an `InterpolationSyntax` is always an `InterpolatedStringExpressionSyntax`. `StartsInsideNestedText` keeps every non-raw hole line out of the shift map | Agree. The span is unchanged except for the removed tautology, and the summary now states the reach that the shift map provides | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which token holds a moved raw-string line's indentation | `rg -n "IsHoleOpeningLine\|interpolation.Parent is InterpolatedStringExpressionSyntax" src sdd` finds only `RawStringLineShifts` (declaration at line 285, call at line 252). The repair added no copy, and the owners in `23-auditor.md` are unchanged | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RawStringMovedLineStartTests` (29), `AssertAnnotationsKept` | FMT-PIPE-022 over every node and token of the code, structured trivia excluded | An annotation that a phase's output carries on zero elements, on several, or on an element of another kind. Watched: mutation A (literal node rebuilt) fails 5 tests; mutation B (text nodes rebuilt) fails 3, and the other 2 are unaffected because the mutation changes nothing in their trees | Whole-tree annotation through `ReplaceSyntax`, checked per phase through `FormattingPipeline.Execute` | adequate; it closes #9 |
| Same class, `AssertMoved` and its other assertions | FMT-PIPE-021 and FMT-RAW-001 to FMT-RAW-004 | unchanged from `23-auditor.md` | unchanged | adequate (evidence reused; the methods and expected texts are byte-identical) |

## Failure classes dismissed
These are dismissals for the repair delta. The dismissals of `11-auditor.md`, `18-auditor.md`, and `23-auditor.md` still hold for unchanged code, and the catalog (FC-001–FC-037) did not change.

- FC-001, FC-002, FC-022: the one changed guard is covered under Guard scope. Its span is unchanged apart from a tautology.
- FC-005, FC-019, FC-020, FC-021, FC-006, FC-023: no trivia, literal value, or escape handling changed.
- FC-010, FC-011, FC-012: no loop or layout decision changed, so idempotency and termination keep the evidence of the previous audit.
- FC-007–FC-009, FC-013, FC-014, FC-030: no analyzer or code fix changed.
- FC-015: covered under Policy ownership.
- FC-017: covered under Assertion adequacy. The test that was too weak is now as strong as the requirement.
- FC-024–FC-029, FC-036: nesting and node-level modes are not affected, because the compiled delta cannot change a result.
- FC-031: the summaries of `IsHoleOpeningLine`, `ApplyEdits`, `AssertMoved`, and `AssertAnnotationsKept` describe the code as it now stands (traced above). No `TODO`, no commented-out code, and no tracker number.
- FC-032: handover 25 rebuilt and tested the compiled change `c8b4e86`. The cleaner proved `151f2f5` text-only and built Release. The rest are handovers.
- FC-033–FC-035: the expected values come from the spec and are unchanged.
- FC-037: the cleaner's text-only proof is for a comment-only edit, and its verdict is the one cited.

## Architecture findings

_None._

## Hints

- `PhaseAnnotationTests` still annotates only part of each literal, and not the literal nodes, as handover 24 notes. The node rebuild on the insertion path is now covered by `RawStringMovedLineStartTests`, so this is not a finding. Widening that class to the whole tree would make it a general guard for FMT-PIPE-022 in other phases.
- Mutation B does not affect `OnlyLineBeingAHoleAtColumnZeroMovesRight` or `NestedLiteralLineStartingWithAHoleAtColumnZeroMovesRight`, because they have no existing text element that the mutation rebuilds. This is not a gap: the other three insertion-path members cover that rebuild.
- The mutation tree and logs are in `/tmp/claude-0/-home-user-Reihitsu/73a9bd65-0b12-5c4b-9a2a-b0eaaa85c1fe/scratchpad/`: `mut28/`, `mut28A.log`, and `mut28B.log`. `mut28/` currently holds mutation A and is not part of the repository.
- The two follow-up candidates that the decision deferred still stand: the `///` separator wording, and direct `Reihitsu.Core.Test` coverage of `TokenSeparationUtilities`.
