Role: test implementer
Commit: c4c242c
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This is repair round 3. It covers audit findings #8 and #9 from `28-auditor.md`, under the decision at the end of `00-lead.md` (audit 2: #8 option A; #9 repaired).

Expected values:
- For #8 they are the document-level output that FMT-PIPE-017 requires. The analyzer tests compare the fix's output with `Format(source)` through `VerifyCodeFixMatchesFormatter`; the formatter tests state the output and also assert it with `AssertRuleResult`.
- I checked every input with `scripts/format.sh`: document-level formatting changes only the code the fix or the entry point formats.

"Fails" means the test fails on c4c242c plus these files, for the reason given. "Passes" means it passes there and must keep passing. Columns are 0-based.

## Tests

### Finding #8: the context entry point on a target that starts a later line of its context

The members are the ones the auditor lists.

| Test | Member | Requirement / failure class | Helper | Expected now |
|---|---|---|---|---|
| `Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests.ArgumentTargetOnALaterLineOfItsContextIsWrittenInThePhaseColumnWithTheLinesAlignedToIt` | (b) Public API: `Call(1,` / `                 Other(2,` / `                 3));`. The target is the argument `Other(…)`, the context is the statement. Expected: `Other` at 13, `3` at 19 under `2`. | FMT-PIPE-017, FMT-INDENT-012; FC-028, FC-016 | `FormatNodeInDocumentWithContextAsync` (two passes, LF and CRLF) and `AssertRuleResult` | fails: `Other` stays at 17 while `3` is at 19 |
| `…ReihitsuFormatterNodeColumnTests.OneLineArgumentTargetOnALaterLineOfItsContextIsWrittenInThePhaseColumn` | Milder member of (b): a one-line argument `2` on a later line of `Call(1,`. Expected at 13. | FMT-PIPE-017, FMT-INDENT-012 | same | fails: `2` stays at 17 |
| `Reihitsu.Analyzer.Test.Formatting.RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyDeclaratorOnALaterLineIsFixedAsTheFormatterLaysItOut` | (a) RH5111 on the declarator `b =` / `Other(1,` / `2)` on a later line of `int a = 1,`. The fix's target is the declarator, its context the statement. Document-level output: `b` at 8, `2` at 18 under `1`. | RH5111-004, FMT-PIPE-017; FC-007, FC-013 | `VerifyCodeFixMatchesFormatter` | fails: the fix gives `b` at 14, `2` at 18 |
| `…RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyParameterOnALaterLineIsFixedAsTheFormatterLaysItOut` | (c) RH5111 on the parameter `int b =` / `5` at 15 in `void M(int a,`. Document-level output: `int b = 5)` at 11. | RH5111-004, FMT-PIPE-017 | same | fails: 15 |
| `…RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyEnumMemberOnALaterLineIsFixedAsTheFormatterLaysItOut` | (c) RH5111 on the enum member `B =` / `2` at 6. Document-level output: `B = 2` at 4. | RH5111-004, FMT-PIPE-017 | same | fails: 6 |

The members the auditor reports as holding stay covered by existing passing tests:
- a mid-line target, where the restore changes no column: `ReihitsuFormatterNodeColumnTests.ChainTargetWithContextBehindTheLastLineOfAMultiLineCommentAlignsTheWrappedLink`, and the existing RH3208 tests with a target behind a label;
- `FluentChainCodeFixHelper`'s fallback to the whole context: the existing RH5201 tests.

### Finding #9: the indentation phase's handling of an attached input

**Reachable input found.** It is reachable through the public API.
- A raw string literal formatted as the target on its own reaches `IndentationPhase.Execute` still attached to its document, because no earlier phase rewrites a lone literal token.
- I found it with a temporary probe that I did not commit. The probe ran `FormattingPipeline.Execute` with the phase observer on seven attached targets: a method, two statements, an equals-value clause, a literal, a property, and an accessor list. It recorded whether `IndentationPhase`'s input still had a parent.
  - Only the numeric literal reached the phase attached.
  - Every larger target arrived detached, as the auditor observed. `HorizontalSpacingPhase` returns a new node for them.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `…ReihitsuFormatterNodeColumnTests.RawStringLiteralTargetMovesContentUnderTheOpeningQuotes` (new). `FormatNodeInDocumentAsync` with the `LiteralExpressionSyntax` `"""` / `    a` / `    """` of `        var s = …` as target. Expected: content and closing marker at 16, under the opening quotes. | FMT-PIPE-017, FMT-RAW-001; FC-017 | entry point (two passes, LF and CRLF) | passes |
| `…IndentationPhaseRawStringMoveTests.MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` (existing; summary updated to say it calls the phase directly and to point at the reachable test above) | FMT-RAW-001; FC-017 | phase-level | passes |

**Falsification.** I checked both tests in a temporary worktree of c4c242c, removed afterwards, with production edited only there:

| Production state | New test | Existing test |
|---|---|---|
| `IndentationRewriter.Apply` with `lineOffset` forced to 0 (clone kept) | passes | passes |
| `IndentationPhase.Execute` without the clone (`lineOffset` kept) | passes | passes |
| both removed | **fails** | **fails** |

So the two mechanisms cover the same input, and each alone is enough. After the implementer removes one of them, as decision audit 2 says (#9), both tests fail without the one that remains. The public-API test is the one that ties it to a reachable input.

## Not covered, and why

- **The auditor's hint about a `///` behind the preceding token when the target starts its context**: the auditor did not verify it with a probe, and it is not one of #8's members. Not tested in this round.
- **CRLF for the RH5111 members**: the analyzer tests run one line ending, as all of them do. The entry-point tests of the same mechanism run LF and CRLF.
- **Failure classes**:
  - FC-004, FC-005, FC-019–FC-021: no gap with a comment or a directive is joined differently by these entry points. RH5111-005 withholds the fix for comments and directives in the construct.
  - FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite, rename, or member-kind logic.
  - FC-008, FC-009: no new withheld fix, no Fix All interaction.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: for the auditor.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111"
scripts/test.sh --project architecture
```

Observed on c4c242c plus these files:
- formatter `ReihitsuFormatterNodeColumnTests`: `Failed: 2, Passed: 10, Total: 12`. The two failures are the #8(b) tests.
- formatter `IndentationPhaseRawStringMoveTests`: `Passed: 14`.
- analyzer `RH5111`: `Failed: 3, Passed: 50, Total: 53`. The three failures are the #8(a) and #8(c) tests.
- architecture: `Passed: 89`.
