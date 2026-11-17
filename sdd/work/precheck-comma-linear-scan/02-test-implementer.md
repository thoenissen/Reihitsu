Role: test implementer
Commit: 5872769 (tests written on this head; the commit carrying them is the one that adds this file)
Verdict: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All tests are in `src/Reihitsu.Formatter.Test/Regression/Precheck/LongListPrecheckCostTests.cs`. Each one builds a list of 30 000 elements on one line. Each runs under LF and CRLF, and asserts two things: the precheck's answer, and that one precheck call stays within 3 s. The clock covers only the precheck call or the `FormatSyntaxTree` call, never the parse.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LongArrayInitializerWithoutShapeIsCheckedWithinBudget` | none (no requirement states a cost) | `ReihitsuFormatter.FindUnsupportedSyntax` on the compilation unit, `Stopwatch` | red: about 20 s; the answer is `null` and that part holds |
| `LongArrayInitializerWithShapeAtLastCommaIsRefusedWithinBudget` | none | `ReihitsuFormatter.FormatSyntaxTree` (whole document, the CLI path), `AssertLocationIsShape` | red: about 20 s; refused with the FMT-UNSUP-009 reason at `/* c */`, and the output is unchanged; those parts hold |
| `LongCollectionInitializerWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget` | none | `FindUnsupportedSyntax` on the `InitializerExpressionSyntax`, which is what RH5303 passes | red: about 21 s; reason and location hold |
| `LongArgumentListWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget` | none | `FindUnsupportedSyntax` on the `ArgumentListSyntax`, which is the RH5101/RH5102 kind of target | red: about 25 s; reason and location hold |
| `LongEnumWithoutShapeIsCheckedByTheNodeLevelCheckWithinBudget` | none | `FindUnsupportedSyntax` on the `EnumDeclarationSyntax` | red: about 49 s; the answer is `null` and that part holds |

**Tags.** None of these tests carries a `[Requirement]` tag. The bug violates no requirement: no spec states a cost, and the skeptic found the same. The verdict assertions only touch FMT-UNSUP-009, which the existing tests in `Unit/Precheck/UnsupportedListSeparatorCommentTests.cs` already cover and tag.

**How I know they fail for the right reason.** I ran the same file with `ElementCount = 2000` and then reverted that change. All five tests passed, in 0.3 to 0.7 s each, so every verdict, reason, and location assertion holds on today's code. At 30 000, every failure is the budget assertion. For example: `The time of the precheck of the EnumDeclaration under LF line endings must stay within 3 s for a list of 30000 elements; it took 48.84 s.` The times were measured with 4 parallel MSTest workers (the project default), so contention inflates them somewhat. A linear scan takes milliseconds, which leaves room of several orders of magnitude on both sides of the 3 s budget.

**About `[Timeout]` (decision 4).** I checked this with a throw-away probe. A test with `[Timeout(1000, CooperativeCancellation = true)]` that spins for 4 s without observing the token **passed** after 4 s on MSTest 4.5.3. A cooperative timeout therefore cannot catch this defect, because the precheck observes no token. I used an explicit `Stopwatch` budget assertion instead. It fails with the measured time in its message and does not abort the test run. I removed the probe; it is not committed.

**Existing coverage.** The existing precheck tests stay unchanged and pass on this head: `FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck` gives 464 passed. Verdict identity rests on them, as the skeptic lists in facts 1 to 5. No existing test asserts an output this change alters, because nothing visible changes.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| 30 000-element array initializer, no shape, whole root | LF, CRLF | `FindUnsupportedSyntax` (whole document) | reproduced | 20.4 s vs ≤ 3 s; answer `null` as expected |
| Same list, comment in front of the last comma | LF, CRLF | `FormatSyntaxTree` (whole document) | reproduced | 19.9 s vs ≤ 3 s; refused at `/* c */` as expected |
| 30 000-element collection initializer, shape at the last comma | LF, CRLF | node-level (code-fix registration) | reproduced | 21.1 s vs ≤ 3 s |
| 30 000-element argument list, shape at the last comma | LF, CRLF | node-level | reproduced | 25.2 s vs ≤ 3 s |
| 30 000-member enum, no shape | LF, CRLF | node-level | reproduced | 48.8 s vs ≤ 3 s |
| The same five at 2 000 elements | LF, CRLF | as above | holds | 0.3 to 0.7 s per test; every answer as expected |

The CRLF arm runs only when the LF arm passes, because each test loops LF first. Its timing is therefore not observed today. It will be observed once the LF arm is fast.

## Not covered, and why

- **A per-kind sweep over every list owner.** The skeptic showed that the cost depends only on how many children the comma's parent has; no kind switch is involved. `FindTriviaInFrontOfComma` takes every `CommaToken` of `node.DescendantTokens()` (`rg -n "CommaToken" src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`). I cover three parent kinds (initializer, argument list, enum) and both entry points. Verdict identity per kind is already covered by `UnsupportedListSeparatorCommentTests.ShapeIsUnsupportedInEveryListOf*`.
- **The shape at the first comma.** It is linear today, which the skeptic measured, and its verdict is covered by `FirstOfTwoCommentsIsReported`.
- **Failure classes.** The catalog has no cost class. The classes that reach the changed code concern verdict identity: a comment or directive in the gap, a zero-width token in the gap, structured trivia, disabled text, and node-level boundaries. The existing tests the skeptic names under facts 1 to 5 cover them, and they must stay unchanged and green.
- **Idempotency, a code-fix pass, and Fix All.** No output changes. The code fixes only call `FindUnsupportedSyntax`, and the node-level tests exercise that call directly.
- **Comment and directive in the gap.** The existing tests cover both, for example `Pragma directive in front of an argument comma` and `CommaInADefinedBranchIsUnsupportedAndInDisabledTextIsNot`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Precheck.LongListPrecheckCostTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"
```

## Follow-up candidates

- A cooperative `[Timeout]` does not fail a test that ignores the token; it passed after 4 s against a 1 s limit on MSTest 4.5.3. `CommentJoinTests.cs:203` and `:228` rely on it to catch non-termination. That holds only if those code paths observe the test's cancellation token. If they do not, those tests would hang instead of failing, as a loop that never ends would never return. I have not checked whether they observe it.
