Role: test implementer
Commit: 68e9ba3 (base of this repair; the commit of this handover follows it)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the finding to repair (audit #2) and asked me to derive the tag myself, which I did from the spec text below.

## Re-derivation

FMT-BREAK-029 (`sdd/specs/formatter/line-breaks.md:1727-1729`): ¶1, the MUST, covers only a line break the phase writes into the **moved token's leading trivia** while it leaves the previous token's trailing trivia in place. ¶2 says that where the phase rewrites the **previous token's trailing trivia**, it *may* remove the whitespace. `EndLineInFrontOf` appends the break to the previous token's trailing trivia, so it is a ¶2 case: the spec permits keeping the whitespace and permits removing it. No requirement demands the value `"   \n"`. Keeping the whitespace is decision 4 (`00-lead.md`). FMT-BREAK-030 holds only incidentally (one line break, no blank line), and the test does not target it. So the test carries **no tag**. Its assertion stays unchanged as a characterization of decision 4.

The same misreading was in a test I wrote in `02-test-implementer.md`: `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace` (cell B), tagged FMT-BREAK-019 and FMT-BREAK-029. On that path the chain's `EnsureLineBreak` also appends to the previous token's trailing trivia (`07-auditor.md`, composition trace), which is again ¶2. The value the test asserts on the whitespace is "no whitespace at the end of `a.B()` in the final output", and FMT-CLEAN-001 demands that ("MUST remove the whitespace trivia between a token and the line break that ends its line"). I replaced the FMT-BREAK-029 tag with FMT-CLEAN-001 and kept FMT-BREAK-019 (the chain's link layout). The auditor did not name this test. It is the same defect class, in my own PR-introduced test, and the change is test metadata only.

FMT-BREAK-029 keeps its own coverage: `LineBreakResidueWhitespaceTests` (3 tests, `src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakResidueWhitespaceTests.cs:39,56,80`) still carries the tag.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfAppendsLineBreakBehindWhitespace` (tag removed; assertion unchanged) | — (characterization of decision 4) | unit | pass |
| `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace` (tag FMT-BREAK-029 replaced by FMT-CLEAN-001; assertion unchanged) | FMT-BREAK-019, FMT-CLEAN-001 | `AssertRuleResult` | pass |

No assertion, input, or expected value changed. The two classes ran green: 34 passed, 0 failed.

## Scenarios (bug fix)
_None._ This is a repair of tags only.

## Not covered, and why
- The tables in `02-test-implementer.md` (lines 23, 47, 55) still list FMT-BREAK-029 for these two tests. That handover is historical; this table supersedes those rows.
- The auditor's hint about a direct unit test of `JoinGaps`'s `edits` parameter is a hint, not a finding, and not on the worklist. Not written.

## Commands
scripts/test.sh --project formatter --filter "FullyQualifiedName~LineBreakTriviaUtilitiesTests|FullyQualifiedName~ChainGapJoinTests"

## Follow-up candidates
_None._
