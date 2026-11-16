Role: test implementer
Commit: 93a0988
Verdict: tests written (repair round 1, audit findings #1–#5)

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 9 in `00-lead.md`, which settles #4 and #5. I re-derived each suggestion before applying it (notes below). Every change is test code or a test-code comment; no production file is touched.

## thoenissen/Reihitsu#1505

Finding #2 (FC-031): `src/Reihitsu.Formatter.Test/Helpers/SecondPassInput.cs` summary. I did not narrow the sentence to "the shared helpers under `Helpers/`", which the audit suggested, because that claim is false too: `FormatterPhaseTestsBase.ApplyPhase` (`Helpers/FormatterPhaseTestsBase.cs:33`) builds its root and context from a fixture's text without this class (decision 2). The summary now names the users that `rg SecondPassInput` finds: the passes of `FormatterTestsBase`, the passes `ObservedPipeline` observes, the phases the stability check runs alone, and the fixed-point assertion of the check's own tests (`PhaseStabilityCheckTests:260`).

### Tests
_None changed_ (comment only).

## thoenissen/Reihitsu#1577

Finding #1 (FC-017): `Unit/Pipeline/PhaseStabilityAssertionTests.AssertionPassesOnTheOutputTreeOfTheFormatter` now uses the new fixture `BranchesMisindentedSource` (`#if FEATURE` class `C` / `#else` class `D`, a misindented member in each branch), formatted with `FEATURE`. The test asserts:
- the formatter indents `C`'s member and leaves `D`'s member at 8 spaces (disabled text);
- the tree overload passes on the output tree (with the carried options);
- the same output text, parsed without `FEATURE`, fails the check with `phase returned "    int _other;"`. So the test shows within itself that the fixture tells carried options from dropped ones.

Finding #3 (FC-031), `Helpers/PhaseStabilityCheck.cs`:
- The class summary no longer says who calls the session-free assertion. That claim was false for the analyzer and CLI idempotency tests. It now says what the method does: `AssertNoPhaseChanges` and `DescribeChangingPhases` run the check on a full run's final output without the session, with the same text.
- `GetCurrentSession`'s message now reads: "The session of the phase-level stability check exists only inside a test of a class derived from FormatterTestsBase; outside it, use PhaseStabilityCheck.AssertNoPhaseChanges." No test asserts it (`rg "runs only inside a test"` found only the source).

Helper change for #5 (below): I added a public `DescribeChangingPhases(SyntaxTree)`. It returns the failure texts that `AssertNoPhaseChanges(SyntaxTree)` fails with, so a caller can collect them. Both `AssertNoPhaseChanges` overloads now go through a private `DescribeChangingPhases(string, string, CSharpParseOptions)` and `FailOnChangingPhases`. The failure text still has one owner, `DescribeFailure`.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/Pipeline/PhaseStabilityAssertionTests.AssertionPassesOnTheOutputTreeOfTheFormatter` | FMT-PIPE-027 | `AssertNoPhaseChanges(SyntaxTree)` on `FormatSyntaxTree` output, LF and CRLF | green |

Mutation: the tree overload was changed to pass `null` instead of `outputTree.Options`. Under it, `AssertionPassesOnTheOutputTreeOfTheFormatter` fails, because `class D` becomes active, and `AssertionOnAnOutputTreeUsesTheDetectedLineEndingAndTheTreeParseOptions` fails too. The audit says the old version failed this mutation only through the line ending. I reverted the mutation.

## thoenissen/Reihitsu#1576

Finding #4 (FC-001): `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` now skips an output only when `ReferenceEquals(output, syntaxTree)` holds. It no longer lists `HoldsHidingCarrier` and `Unsupported` separately. Before adopting the skip, I checked that it does not empty the scan:
- `ReihitsuFormatter.FormatSyntaxTree` returns the input instance for a syntax error, a generated header, a hiding carrier, or an unsupported refusal. For an input it formats, it returns `WithRootAndOptions`, and Roslyn returns `this` from that when the root is unchanged.
- A scratch probe over 300 files under `src/`, all already formatted, found 0 same-instance outputs and 300 with identical text. A generated-header input came back as the same instance.
- To keep the skip from silently covering every already-formatted file if the pipeline ever returned the input root, I added `Unit/ReihitsuFormatterEntryPointTests.FormatSyntaxTreeReturnsANewTreeForAnInputItFormatsWithoutChangingIt`. The documentation of `FormatSyntaxTree` states the contract it pins: "a new SyntaxTree with formatting applied". The unchanged-return side was already pinned with `Assert.AreSame` (`ReihitsuFormatterEntryPointTests` FMT-PIPE-005/006 tests, `ReihitsuFormatterTests:128-248`).

Finding #5: the method now collects the failures of both line endings with `PhaseStabilityCheck.DescribeChangingPhases` and fails once, naming the file, like its sibling methods. Each failure text names its line ending.

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SelfHosting.Test/Formatting/FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` | FMT-PIPE-027 | `DescribeChangingPhases(SyntaxTree)` | green, 1810 rows |
| `Formatter.Test/Unit/ReihitsuFormatterEntryPointTests.FormatSyntaxTreeReturnsANewTreeForAnInputItFormatsWithoutChangingIt` | — (no tag: it pins the documented return contract of `FormatSyntaxTree`, which no requirement states; FC-001 guard for the skip) | `FormatSyntaxTree`, LF and CRLF | green |

## Not covered, and why
- I did not watch the new entry-point test fail: it passes on the unchanged formatter by design. Making it fail would take a production change that returns the input root, and that is out of my lane.
- I did not run a mutation for #5's aggregation. The code reads directly, and it runs the same per-line-ending loop as `FormatterIsIdempotentOnSourceFile`.
- The hint about `AssertSameOutputAsCopy` covering `FormatDocumentAsync` through text equality was not a finding, so I left it.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseStabilityAssertionTests|FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~ReihitsuFormatterEntryPointTests|FullyQualifiedName~ObservedPipelineTests|FullyQualifiedName~ReihitsuFormatterLineEnding|FullyQualifiedName~RawStringOtherLineTerminatorAlignmentTests|FullyQualifiedName~SingleLineTypeBraceWithLeadingUsingTests|FullyQualifiedName~DirectivePreservationTests|FullyQualifiedName~DocumentationCommentCollapseGuardTests|FullyQualifiedName~FormattingPipelineTests"
# 134 passed
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~NoPhaseChangesTheFormattedOutputOfSourceFile"
# 1810 passed, 2m27s
```
`scripts/format.sh` over the five changed source files: 0 of 5 reformatted.

## Follow-up candidates
_None_ beyond `00-lead.md`.
