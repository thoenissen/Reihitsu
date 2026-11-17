Role: auditor
Commit: 091e588caf67c03cead0dce18b660be900a8aa6d
Verdict: pass, text cleanup

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `091e588` on `claude/intelligent-hopper-li5bmz`, checked out with a clean tree. `origin/main` is `abe1313690b712a5b12e3e3cde26f2b4e9b7c3f9`, matching `git ls-remote origin refs/heads/main`, and it is an ancestor of the head (merged as `ec74419`). The diff against `abe1313` is three source paths plus `sdd/work/phase-check-shared/`:

- `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`
- `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5604CodeMustNotContainMixedLineEndingsFormatterTests.cs`
- `src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj`

This matches `02-test-implementer.md` and decisions 2 to 6 in `00-lead.md`. `scripts/lane-check.sh tooling`: confirmed (the claimed lane is tooling, and the diff needs tooling).

thoenissen/Reihitsu#1805 is addressed as follows:

- **Analyzer base half.** Done: every `VerifyFormatter` output under both line endings, plus RH5604's two direct `FormatSyntaxTree` idempotency checks.
- **"Failure text defined once."** Holds: every caller goes through `PhaseStabilityCheck.DescribeFailure`, and nothing is copied.
- **CLI half.** Dropped by decision 1. The issue is `agent-filed`, no visible output is involved, and the PR has to state the departure. I agree with the reason. `FormatCommandHandler` writes `FormatSyntaxTree`'s output verbatim, so a check on the CLI's single 17-line fixture re-tests the formatter and cannot find a CLI defect.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`: summary of the private `VerifyFormatter(string, string, string, Action<…>, DiagnosticResult[])` (line ~99) and summary of `VerifyFormatterFixCore` (line ~170) | FC-031 | – | PR-introduced (both methods are changed by the diff) | Both summaries describe something other than the changed code. `VerifyFormatter` says "remains stable on a second pass under LF and CRLF line endings", but it formats under the one `endOfLine` it receives, and it does not mention the per-phase check it now runs, or the skip for refused input. `VerifyFormatterFixCore` says it "asserts that no analyzer diagnostics remain", but it only asserts that the output equals `fixed`. Its caller runs the analyzer. The `<returns>` line was updated; the summary was not. | Reword both summaries to describe the code: one line ending, then the second pass, then the phase-level check unless the input was refused; and "formats and asserts the expected output". Comment-only edit. Prove it with `scripts/verify-text-only.sh`. |

Why `pass, text cleanup`: finding 1 is confined to XML documentation comments of private test methods. It cannot reach compiled behavior, a diagnostic, or rule metadata.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Skip the check for refused input (`isRefused == false`) | `FormattingResult.Unsupported != null` of the base's second `FormattingPipeline.Execute` pass on `formatted`. `ExecutePhases` sets it for a precheck refusal and for a phase refusal (FMT-UNSUP-001, FMT-UNSUP-006) | FMT-PIPE-027's "Output of a run that formatted" bullet excludes input refused as unsupported. `FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` skips any output returned as the input tree (`ReferenceEquals`), which also covers refusal | Agree on refusal. The first pass and the second pass see the same text whenever the first pass refused, because a refused output equals its input, so it does not matter that only the second pass's flag is read. The base still checks syntax-error, generated-code, and hiding-carrier output, which the self-hosting caller skips. This is decision 3: stricter than the requirement and never weaker. No such fixture exists today (02), and syntax errors fail the analyzer `Verify` step first | ok |
| Which line ending the check uses (text overload, forced `endOfLine`) | The forced line ending the base formatted with, and `CSharpParseOptions.Default` (`transformParseOptions` is always `null` at both call sites) | FMT-PIPE-027 uses the line ending detected in the output (FMT-PIPE-007) and the tree's parse options. `SecondPassInput.Create` builds the `FormattingContext` exactly as `VerifyFormatterFixCore` does (`LanguageVersionResolver.Resolve(tree.Options)`) | Equal for every output the base asserts: `Assert.AreEqual(@fixed, formatted)` pins the output to `fixedSource` or to `AlternateLineEndings(fixedSource)`, which uses only the forced ending. RH5604's mixed-input fixture through the base normalizes fully (02). The genuinely mixed outputs (RH5604 `AssertExemptInteriorsSurviveNormalization`) use the tree overload, which detects the ending | ok |
| RH5604 direct calls `AssertNoPhaseChanges(firstTree)` | The `FormatSyntaxTree` output tree, parsed with default options, ending detected | FMT-PIPE-027 as stated | The calls follow `Assert.AreEqual(expected, firstTreeText)`, where `expected` differs from `input`. A refused or unchanged run would fail first, so only formatted output reaches the check | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The phase-level check and its failure text | `PhaseStabilityCheck.FindChangingPhases`/`DescribeFailure` (`src/Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck.cs`), which `PhaseStabilitySession.Check` reuses. Callers: `Reihitsu.Formatter.Test` `FormatterTestsBase` (session `Verify`), `FormatterSelfHostingTests:136`, and now the analyzer base and RH5604. `rg "ExecutePhases\(" src/Reihitsu.Analyzer.Test` finds no private copy | One owner. ok |
| Which outputs the check covers | Spec FMT-PIPE-027. Callers: the analyzer base (skips refused input only), the self-hosting test (skips every input returned unchanged), and the `Reihitsu.Formatter.Test` session (skips nothing; its summary says it is stricter on syntax-error output) | The differences are justified (decision 3; the self-hosting test is the requirement verbatim). Each caller is at least as strict as FMT-PIPE-027 on what it covers, and the analyzer base never checks less than the requirement covers. ok. See *Hints* for the session's handling of refused input |
| How a second-pass input is parsed | `SecondPassInput.Create`, and the base's `VerifyFormatterFixCore` (pre-existing private copy of the same two lines) | Identical construction, verified line by line. The copy predates the run, and the run only reads it. ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Every `VerifyFormatter` test of the 158 classes derived from `FormatterTestsBase<TAnalyzer>` | No production phase run alone changes the base's asserted-idempotent output, under each of the two line endings | A phase whose solo output text differs from `reformatted` | `PhaseStabilityCheck.AssertNoPhaseChanges(string, string, CSharpParseOptions)`, which runs the production phase list read by reflection from `FormattingPipeline.CreatePhases`, preparation sub-phases included, and compares full text | As strong as FMT-PIPE-027 on these outputs. Both endings are checked: every public overload calls the private method twice (FC-017 on LF-only holds). The test implementer showed by reverted mutation that it fails under LF and under CRLF with the owner's failure text. ok |
| `RH5604…VerifyFormatterNormalizesXmlCrLfToPredominantLfAndIsIdempotent`, `…XmlLfToPredominantCrLf…` (via `VerifyFormatterEntryPointsAndIdempotency`) | Same, for `FormatSyntaxTree` output, ending detected | Same | `AssertNoPhaseChanges(SyntaxTree)` | ok |
| `RH5604…VerifyExemptInteriorsSurviveNormalizationInAGenuinelyMixedFile` (via `AssertExemptInteriorsSurviveNormalization`) | Same, on output that keeps mixed endings in exempt interiors, ending detected | Same | `AssertNoPhaseChanges(SyntaxTree)` | ok. This is the only member where the detected ending differs from a forced one (FC-033), and it uses the detecting overload |

## Failure classes dismissed

- FC-001, FC-002, FC-003: the only added predicate is the refusal skip. Its span is traced under *Guard scope*. It has no boundary with two sides in today's corpus (no refused fixture, 02). See *Hints*.
- FC-004, FC-005, FC-019, FC-020, FC-021: no edit of tokens, trivia, or directives; test harness only.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite or semantics change.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix touched.
- FC-028: the check is not run on `FormatNode` output. FMT-PIPE-027 excludes it, and RH5604 keeps that output under its existing idempotency assertion only.
- FC-010, FC-012: these are what the check detects. The analyzer project passes with it (02: 10460 of 10460), so this corpus exposes no pre-existing reversal.
- FC-011: there is no loop. Each phase runs once alone.
- FC-013, FC-014: no analyzer or rule change.
- FC-015, FC-016: traced under *Policy ownership*. See *Follow-up candidates* for the one sibling with the same shape that was not taken in.
- FC-017: traced. Both endings are checked, and so is the second-pass output.
- FC-031: finding 1. The class summary of `FormatterTestsBase<TAnalyzer>` and the summary of `PhaseStabilityCheck` stay accurate.
- FC-032: the cleaner's Release build and the test implementer's runs came before the merge of `origin/main` (`ec74419`). That merge brings in only `abe1313` (a formatter join change, PR #1813). The lead reports the build as green after the merge. The validator's default run on this head is the remaining evidence. The run's own change cannot flip a result: it is test-only.
- FC-033: traced. The forced ending matches the output for every base fixture, and the mixed fixture uses the detecting overload.
- FC-034, FC-035: no reproduction, and no expected value derived from code.
- FC-037: no text-only claim made yet. Finding 1's repair needs `scripts/verify-text-only.sh`.
- FC-038 to FC-043: no declared shape, precheck, or code fix touched. FC-042: the refusal skip in the harness is the spec's exclusion, not a phase guard.
- FC-044: no modelled result. The check runs the production phases.
- Visible output (check 10): no expected value of any visible surface is added or changed. The diff adds assertions only.
- Cost of special cases (check 11): no production code.
- Scope (check 9): no edit outside the run. No `TODO` and no issue or PR numbers in changed source (`rg`).

## Architecture findings

_None._

## Hints

- **The refusal skip is unexercised.** No fixture reaches the `isRefused` skip branch (02's instrumentation), so neither side of it is pinned by a test. This is decision 3, and the code agrees with FMT-PIPE-027. It is not a finding.
- **The wiring is not pinned by a committed test.** Removing the `AssertNoPhaseChanges` call from the base would leave every test green. The evidence it runs is the test implementer's reverted mutation. A committed test needs a seam the decisions exclude (`OverrideForCurrentTest` is internal and bound to the session). This is not required by the issue.
- **The `Reihitsu.Formatter.Test` session checks refused output.** Its `ApplyRule` does not read `Unsupported`, so a fixture routed through `AssertRuleResult` that the formatter refuses would be checked against a phase that has no guard for the declared shape. This is pre-existing and outside the diff. I did not establish whether any such fixture exists.

## Follow-up candidates

- The run's candidate list in `00-lead.md` ("RH5021, RH5029, RH5030, RH5104, RH5111, RH5201, RH5412–RH5416, RH6001") is mostly empty of real members:
  - RH5412 to RH5416 assert one `FormatSyntaxTree` pass against an expected text and assert no idempotency.
  - RH5201's idempotency test goes through `VerifyFormatter` and is now checked.
  - RH5111 uses the formatter only to build an analyzer expectation.
  - The one real member is `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH6001KeywordsMustBeSpacedCorrectlyFormatterTests.cs`, lines 154 to 163. One of its tests, `VerifyFormatterFixesGluedFixedKeyword` (declared at line 114), asserts `Format(firstPass) == firstPass` under LF and CRLF through `FormatSyntaxTree`, without the per-phase check. It has the same one-line shape as decision 4. I did not run it with the check.
