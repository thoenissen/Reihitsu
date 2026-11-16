Role: test implementer
Commit: 2e0f6ee (head when I started; the prompt named 47c6c0c "or later", which does not exist in this repository — `git cat-file -t 47c6c0c` fails — so I worked on the branch head)
Verdict: tests written (#1505, #1577 green; #1576 written, blocked on the project reference the implementer adds)

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decisions in `00-lead.md`; I worked from those, the skeptic's handover, and FMT-PIPE-027.

All three issues are coverage or helper work, not bug fixes: every new check is expected green on the unchanged formatter, and it is. The "fails for the right reason" step is done for the new helper and its tests by mutation (below), since the formatter itself has no defect to reproduce here.

Shared helper (decision 4): `PhaseStabilityCheck.AssertNoPhaseChanges(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)` and `AssertNoPhaseChanges(SyntaxTree outputTree)` in `src/Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck.cs`. Neither one needs a session, and both fail with the text the session's check fails with. That text is now built once, in `PhaseStabilityCheck.DescribeFailure`, which `PhaseStabilitySession.Check` also calls. The tree overload takes FMT-PIPE-027's input as the requirement states it: the output's text, the line ending `ReihitsuFormatterHelpers.DetectEndOfLine` finds in it, and the tree's parse options. The class summary now says who runs the check and how (FC-031). `GetCurrentSession`'s message ("runs only inside a test of a class derived from FormatterTestsBase") still holds: it concerns only the session path.

## thoenissen/Reihitsu#1505

Scope per decision 2: `ObservedPipeline.Run` now takes its root and context from `SecondPassInput.Create` (the two duplicated lines and two usings are gone). `SecondPassInput`'s summary no longer lists its users; it says every helper that builds a pass's root and context from a fixture's text takes them from there (FC-031). `FormatterPhaseTestsBase` is unchanged (decision 2).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/Pipeline/ObservedPipelineTests.RunFormatsWithTheLanguageVersionOfTheGivenParseOptions` | — (tool test; no tag) | `ObservedPipeline.GetPhaseOutput`, LF and CRLF | green: `class C\n{\n}` turns into `class C;` in `StructuralTransformPhase` under default options and keeps its braces under C# 11 |
| `Unit/Pipeline/ObservedPipelineTests.RunParsesWithThePreprocessorSymbolsOfTheGivenParseOptions` | — | same | green: a misindented member inside `#if FEATURE` is reindented with the symbol and is unchanged disabled text without it |

Both are green before and after the refactor, as they should be: the change does not alter behavior. Mutation: with `SecondPassInput.Create(input, endOfLine, null)` in `ObservedPipeline.Run`, both fail. The language-version test is the first anywhere that observes the context's language version through `ObservedPipeline`; the only existing options test (`PhaseTreeRoundTripTests.PhaseOutputIsParsedAgainWithThePreprocessorSymbolsOfTheGivenTree`) covers symbols only.

### Not covered, and why

- `FormatterPhaseTestsBase` and its five subclasses: decision 2 leaves them unchanged; nothing to test.

## thoenissen/Reihitsu#1577

Seven classes now call `PhaseStabilityCheck.AssertNoPhaseChanges` at their full-run idempotency assertion (decision 3). Each test summary that gained the check now says so.

| Class / site | Call | Line ending and options used |
|---|---|---|
| `Unit/ReihitsuFormatterLineEndingIndependenceTests.AssertSameOutputAsCopy` | tree overload on the `FormatSyntaxTree` output (new private `FormatOutputTree`) | detected in the output (`\r`, U+0085, U+2028, U+2029 inputs); one check covers `FormatDocumentAsync` too, since the test asserts both outputs are equal |
| `Unit/ReihitsuFormatterLineEndingTests.AssertFormatted` | tree overload | detected |
| `Regression/Indentation/RawStringOtherLineTerminatorAlignmentTests.RawStringWithOtherLineTerminatorIsAligned` | tree overload | detected; 4 terminators × LF/CRLF |
| `Regression/LineBreaks/SingleLineTypeBraceWithLeadingUsingTests.ExpandedTypeRemainsStableOnSecondPass` | text overload | `"\n"`, default options (as its `Format`) |
| `Regression/Structural/DirectivePreservationTests.AssertFormatted` | text overload | `"\n"`, `DEBUG`. The symbol array became a shared `_parseOptions` field, so `Format` and the check use the same options object |
| `Regression/LineBreaks/DocumentationCommentCollapseGuardTests` — the four `…IsIdempotent` tests | text overload | `"\n"`, default |
| `Unit/Pipeline/FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` | text overload | `context.EndOfLine`, default |

`LineBreakRewriterTests` and `UsingDirectiveOrderingRewriterTests` are unchanged and have no new summary sentence (decision 3).

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit/Pipeline/PhaseStabilityAssertionTests.AssertionPassesOnFormattedSourceWithoutASession` | FMT-PIPE-027 | `AssertNoPhaseChanges(text, eol, null)`, LF and CRLF; the class does not derive from `FormatterTestsBase`, so there is no session | green |
| `…AssertionFailsNamingThePhaseThatChangesTheOutput` | FMT-PIPE-027 | same | green: throws with `IndentationPhase changes the final output under LF/CRLF line endings when it runs alone (FMT-PIPE-027).` and the first changed line |
| `…AssertionParsesTheOutputWithTheGivenParseOptions` | FMT-PIPE-027 | same, with `FEATURE` | green: fails with the symbol, passes without it |
| `…AssertionOnAnOutputTreeUsesTheDetectedLineEndingAndTheTreeParseOptions` | FMT-PIPE-027 | tree overload | green: names `LF`/`CRLF` as detected, and honors the tree's `FEATURE` symbol |
| `…AssertionPassesOnTheOutputTreeOfTheFormatter` | FMT-PIPE-027 | tree overload on a `FormatSyntaxTree` output that has `FEATURE` | green |
| the seven call sites above | — (no new tag: the check is incidental to each test's own requirement) | `AssertNoPhaseChanges` | green, including `DirectivePreservationTests`, which the skeptic's trace could not settle |

Mutation: with the tree overload passing `"\n", null` instead of the detected line ending and the tree's options, `AssertionOnAnOutputTree…` and `AssertionPassesOnTheOutputTreeOfTheFormatter` fail. The existing `PhaseStabilityCheckTests.AssertRuleResult…FailsOnAPhaseThatChangesTheOutput` tests still pass, so the session's failure text did not change when it moved into `DescribeFailure`.

### Not covered, and why

- CRLF in the three regression classes (FC-017): the skeptic notes that the issue does not ask for it. The check uses the line ending each class formats with, which is LF only.
- `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` still compares `TrimEnd('\n')` of both passes. That weak assertion predates this run, and decision 3 does not ask me to tighten it; the added check compares full text. Left as is.
- Failure classes: FC-031 (stale summaries) is handled above. FC-033 (the harness normalizes the scenario away) is handled by the detected line ending and passing the fixture's options. No class touching code fixes, analyzers, comments or directives in gaps, or unsupported shapes applies, because this is test-helper code with no formatter change.

## thoenissen/Reihitsu#1576

New method `FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile(string relativePath)` in `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs`, tagged `[Requirement("FMT-PIPE-027")]`, a `[DynamicData(nameof(SourceFiles))]` row per file (choice 2A). For each line ending the scan already uses (`ParseUnderEachLineEnding`, which skips syntax-error variants), it does three things in order:

- skips a tree for which `ReihitsuFormatter.HoldsHidingCarrier` returns true (FMT-PIPE-033);
- formats the rest with `FormatSyntaxTree` and skips a result with `Unsupported != null`;
- runs `PhaseStabilityCheck.AssertNoPhaseChanges(result.Output)` on what is left: the detected line ending and the tree's (default) options.

A failure names the phase and the line ending, and the row names the file. I used the shared assertion rather than calling `FindChangingPhases` directly, so the failure text is the one statement in `DescribeFailure`; it is public, like `FindChangingPhases`. The class summary now mentions the check.

### For the implementer: missing project reference

`src/Reihitsu.SelfHosting.Test/Reihitsu.SelfHosting.Test.csproj` needs:

```xml
<ProjectReference Include="..\Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj" />
```

Without it the project does not build: `FormatterSelfHostingTests.cs(12,26): error CS0234: The type or namespace name 'Test' does not exist in the namespace 'Reihitsu.Formatter'`. I tried to add the reference temporarily to run the scan locally, and the permission system refused the `.csproj` edit, so **I have not run `--project selfhosting`**. Once the reference exists, please confirm:

- `scripts/build.sh` builds, and `RequirementAttribute` resolves to `Reihitsu.SelfHosting.Test.RequirementAttribute` (the enclosing namespace wins over the `using Reihitsu.Formatter.Test.Helpers;`). If it is reported as ambiguous, qualify it.
- `scripts/test.sh --project selfhosting` discovers only the self-hosting tests, not the referenced `Reihitsu.Formatter.Test` tests, and the new method is green on every row. The skeptic's spot traces predict green.
- The added CI time: about one more single-phase run per phase, per file and line ending, over roughly 1,800 files.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` | FMT-PIPE-027 | `PhaseStabilityCheck.AssertNoPhaseChanges(SyntaxTree)` | does not compile until the reference exists; then expected green on every row |

### Not covered, and why

- Generated-code headers: there is no public check. The scan's catalog already excludes generated file names (`RepositoryRoot.IsGeneratedSourceFile`), as the skeptic's choice 3 says.
- A test that the skip branches are taken: no source in the repository is unsupported, and the skeptic did not settle whether any holds a hiding carrier, so a branch test would need a fixture file in the scanned tree. That is not worth adding to a CI-only scan.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseStabilityAssertionTests|FullyQualifiedName~ObservedPipelineTests|FullyQualifiedName~PhaseStabilityCheckTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterLineEnding|FullyQualifiedName~RawStringOtherLineTerminator|FullyQualifiedName~SingleLineTypeBraceWithLeadingUsing|FullyQualifiedName~DirectivePreservationTests|FullyQualifiedName~DocumentationCommentCollapseGuard|FullyQualifiedName~FormattingPipelineTests"
scripts/test.sh --project formatter        # 5250 passed on this head
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~NoPhaseChangesTheFormattedOutputOfSourceFile"   # after the reference is added
```

## Follow-up candidates

- `FormattingPipelineTests.ExecuteAlreadyFormattedCodeNoChanges` compares the two passes after `TrimEnd('\n')`, which is weaker than whole-text idempotency (FC-017). Tests lane, `agent-filed`. It may also be absorbed by the existing consolidation candidate for the private `ApplyRule` copies.
