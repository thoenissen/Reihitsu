Role: implementer
Commit: 8316e8a
Verdict: test disputed

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This is repair round 3. It covers audit findings #8 and #9 (`28-auditor.md`) under decision "Audit 2" (`00-lead.md`, last section).

Results:
- Every test handed over in `29-test-implementer.md` passes.
- Finding #9 is resolved: exactly one mechanism handles an attached input to the indentation phase.
- Two existing RH5111 tests fail, because decision #8 A changes their output. That output now equals document-level formatting. I did not change the tests; see "Disputed tests or scope".

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs`, `FormatNodeInDocumentWithContextAsync` | The target's original leading trivia is now restored only where FMT-PIPE-010 keeps it: when the target is a direct child of the root, or does not start its line. That is the same condition `FormatNodeInDocumentAsync` uses (`keepsLeadingTrivia`). A target that starts its line keeps the leading trivia and line column the phases gave it, so no step after the last phase moves a line. The annotation is removed in both cases. The context's first-token column now counts the context's own leading whitespace tabs as four only when that token is the target's first token and the trivia is not kept. Then the whitespace is rewritten (FMT-PIPE-017, first bullet). In every other case the whitespace stands as it is, and each character counts as one column, as before. | FMT-PIPE-017, FMT-PIPE-010; finding #8 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | Removed the `lineOffset` mapping, its `GetFirstLine` helper, and the `Microsoft.CodeAnalysis.Text` using. `MoveLines` takes `model.GetRawStringShift` directly. The `Apply` summary now states that the model and the rewritten tree share line numbers, because the root is the root of its own tree, which `IndentationPhase` guarantees for an attached input. | finding #9 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/IndentationPhase.cs` | Comment only. The clone of an attached root is now the one mechanism. The comment says that it also numbers the lines from the node's own start, which the layout and the raw-string move share. | finding #9 |

**Why the clone stays and the mapping goes.**
- The clone is reachable through the public API: a literal target reaches the phase attached (`29-test-implementer.md`).
- The clone is also what makes FMT-PIPE-017's first-token placement independent of attachment.
- The mapping served only direct `Compute` + `Apply` callers with an attached node. None exists: `PhaseCombinationTests`, `IndentationPhaseIntegrationTests`, and `IndentationRewriterTests` all pass tree roots.

**Falsification probe**, not committed. I disabled the clone temporarily (`if (root.Parent != null && false)`) on top of the mapping's removal. Both tests then failed:
- `ReihitsuFormatterNodeColumnTests.RawStringLiteralTargetMovesContentUnderTheOpeningQuotes`
- `IndentationPhaseRawStringMoveTests.MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected`

I restored the file byte for byte from a copy and re-ran both tests: `Passed: 26` for their two classes. So the remaining mechanism is load-bearing, and these two tests falsify it.

## Policy owners

- Restore of a node-level target's leading trivia: `rg -n "keepsLeadingTrivia|WithLeadingTrivia\(originalFirstToken" src/Reihitsu.Formatter`. Both entry points now use the one FMT-PIPE-010 condition: `targetNode.Parent == root || StartsOnNewLineIncludingDocumentation(firstToken) == false`. Nothing else restores trivia after the pipeline.
- Line numbering between the layout and the raw-string move: `rg -n "lineOffset|GetFirstLine" src/Reihitsu.Formatter` finds nothing. The only mechanism for an attached input is the clone in `IndentationPhase.Execute`.

## Tests run

Every result below was observed on the tree of 8316e8a; only the restore check below ran after the probe.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests"` → `Passed: 26`. The test implementer saw 2 failures.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5111"` → `Failed: 2, Passed: 51, Total: 53`. All three handed-over #8 tests pass. The two failures are listed below.

| Project | Result |
|---|---|
| formatter | 2984 passed |
| analyzer | `Failed: 2, Passed: 6227, Total: 6229`; the same two tests |
| architecture | 89 passed |
| core | 533 passed |
| cli | 250 passed |
| tooling | 125 passed |
| playground | 17 passed |
| selfhosting | 3607 passed |

`scripts/format.sh --force` changed none of the three files. The logs show no RH, CS, S, or IDE warning for them.

## Disputed tests or scope

Two existing tests assert the restored original trivia that decision #8 A removes:
- `Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix`
- `Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyCommentInFrontOfParameterDoesNotWithholdTheFix`

**What they assert.** The RH5111 fix's target, an enum member or a parameter, starts its line behind a `// second` comment line. The fixed source keeps that comment directly under the previous member or parameter:

```
    First,
    // second
    Second = 2,
```

```
    void Method(int first,
                // second
                int second = 1)
```

**What happens now.** The fix formats the target with the statement or declaration as context. The target starts its line, so FMT-PIPE-010 does not keep its leading trivia, and the entry point writes back the trivia the phases produced. The phases put a blank line above the comment (FMT-BLANK-011). The comment and the target keep their columns.

```
    First,

    // second
    Second = 2,
```

The parameter case gets the same blank line above `// second`.

**Evidence that the new output is the formatter's.** I ran `scripts/format.sh --dry-run` on both fixed sources as fixtures in the scratchpad. In both, document-level formatting adds exactly that blank line above `// second`. It also changes code outside the target: the enum's trailing comma, and the final line break. So the fix's new output equals the formatter's layout of the target (SHARED-FIX-007, FMT-PIPE-017). The tests' expected text is the old restore, which kept the input's leading trivia although FMT-PIPE-010 does not require it for a target that starts its line.

**Why it is not resolvable in production within the decision.** One alternative is to restore the original trivia but take the phase's indentation for its last line. That keeps the blank-line layout, but it writes back every comment line of the trivia at its input column. Those columns are not the indentation phase's, which goes against FMT-PIPE-017 ("every line they write back MUST end in the column the indentation phase gives it"). It also contradicts decision #8 A, "restore only where FMT-PIPE-010 keeps it".

The expected texts of the two tests have to follow the decision, which is the test implementer's work (escalation 4). Alternatively, if the maintainer wants the blank line kept as written, FMT-PIPE-010 needs a vertical exemption. This is repair round 3 of 3, so the lead decides whether this falls under escalation 7.

## Architecture findings

_None new._ After this round, no step after the last phase decides a written-back line's column in either node-level entry point.

## Follow-up candidates

- Still open from earlier rounds:
  - removing `DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` (decision audit 2, #10);
  - RH5201 against change doc row 10;
  - the RH5304 statement and expression-body targets;
  - `FormatNode` on an attached node.
