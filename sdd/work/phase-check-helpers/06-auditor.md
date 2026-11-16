Role: auditor
Commit: 1b8a1bd
Verdict: #1505: pass, text cleanup; #1577: findings; #1576: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict; I worked from the issues, `00-lead.md`, handovers 01 to 05, FMT-PIPE-027, and `git diff 372daee...1b8a1bd`.

State verified: the checkout is `1b8a1bdd9771191cccec13ff32c78fed5a3aea38`; `git ls-remote origin refs/heads/main` is `372daeee7a757fe7a3026896a594ece4df03fa8a` and is an ancestor of the head; the diff (`scripts/README.md`, four `Helpers/` files, eight test classes, two new test classes, the self-hosting test and its `.csproj`) is what handovers 02 and 03 describe. `scripts/lane-check.sh tooling` → `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

Finding numbers start at `#1` (first audit of the run).

## thoenissen/Reihitsu#1505

### Scope
Full audit, head 1b8a1bd against base 372daee.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Formatter.Test/Helpers/SecondPassInput.cs:10-15` (summary) | FC-031 | – | PR-introduced | The new summary states a universal rule: "Every test helper that builds the root and the context of a formatter pass from a fixture's text takes them from here". It holds under `Helpers/` (`FormatterTestsBase.ApplyRule:338`, `PhaseStabilityCheck.FindChangingPhases:216`, `ObservedPipeline.Run:61`), but not in the test project: the private `Format` helpers of `DocumentationCommentCollapseGuardTests:123`, `SingleLineTypeBraceWithLeadingUsingTests:64`, `DirectivePreservationTests:48` (all touched by this diff), `FormattingPipelineTests:144`, `PhaseTreeRoundTripTests:441`, `PhaseAnnotationTests:223,354`, `RawStringMovedLineStartTests:566`, `ThrowOperandPrecedenceExpressionBodyTests:1093`, and `Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs:178` each build root and context themselves. The old summary listed its actual users and was true. | Scope the sentence to what holds, e.g. "the shared helpers under `Helpers/`". Comment only: it cannot reach compiled behavior. |

### Guard scope
_N/A — the change adds no predicate; `ObservedPipeline.Run` now calls `SecondPassInput.Create`, whose body is byte-identical to the two lines it replaces._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Parse a pass's input with the fixture's options and build its context with `LanguageVersionResolver.Resolve(tree.Options)` | `Helpers/SecondPassInput.Create` (sole owner under `Helpers/`); private copies outside `Helpers/` listed in #2 | ok for the issue's scope (decision 2); the copies outside `Helpers/` are the skeptic's architecture finding, already a follow-up candidate |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ObservedPipelineTests.RunFormatsWithTheLanguageVersionOfTheGivenParseOptions` | `ObservedPipeline` formats with the language version of the given options | With options ignored, C# 11 also yields `class C;` → `AreEqual(input, withCSharp11)` fails (mutation confirmed by 02) | `GetPhaseOutput`, LF and CRLF | adequate |
| `ObservedPipelineTests.RunParsesWithThePreprocessorSymbolsOfTheGivenParseOptions` | `ObservedPipeline` parses with the given symbols | Without the symbol the member stays at 8 spaces → `Contains("    int _value;")` fails | same | adequate |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None_ beyond the skeptic's recorded one (private copies outside `Helpers/`; follow-up candidate in `00-lead.md`).

### Hints
_None._

### Follow-up candidates
_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#1577

### Scope
Full audit, head 1b8a1bd against base 372daee.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityAssertionTests.cs:124-142` (`AssertionPassesOnTheOutputTreeOfTheFormatter`) | FC-017 | – | PR-introduced | The summary says the test verifies the check on `FormatSyntaxTree`'s output tree "with the parse options of the input carried to the output", and the tree overload (`PhaseStabilityCheck.AssertNoPhaseChanges(SyntaxTree)`) depends on exactly that: it reads `outputTree.Options`, which is correct only because `ReihitsuFormatter.FormatSyntaxTree:115` uses `WithRootAndOptions(…, syntaxTree.Options)`; no other test pins that. The fixture cannot observe it: `ConditionalMisindentedSource` puts everything inside `#if FEATURE … #endif`, and the output has the member already indented. If the output tree lost `FEATURE`, the whole class would parse as disabled text, which no phase changes, and the detected line ending is unchanged (the directive lines carry it) — the test still passes under LF and CRLF. The test implementer's mutation (`"\n", null`) failed it only through the CRLF line ending, not through the options. | Give the fixture code that is active only without `FEATURE` and misformatted, e.g. `#if FEATURE\nclass C\n{\n        int _value;\n}\n#else\nclass D\n{\n        int _other;\n}\n#endif`: the formatter leaves the `#else` branch as disabled text, the check with the carried options passes, and with dropped options the indentation phase changes `class D` and the check fails. Alternatively remove the claim from the summary and leave the carry-over to a formatter entry-point test. |
| 3 | low | `src/Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck.cs:21-23` (class summary) and `:245` (`GetCurrentSession` message) | FC-031 | – | PR-introduced | The summary now says "Tests outside that base that assert the idempotency of a full formatter run use `AssertNoPhaseChanges`", which is false for `Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs:114-118` (second full pipeline pass, 159 classes) and `Reihitsu.Cli.Test/Integration/FormatCommandHandlerIntegrationTests.ExecuteAsyncFormattingIsIdempotent`. The `GetCurrentSession` failure message "The phase-level stability check runs only inside a test of a class derived from FormatterTestsBase." is now false as stated: the check also runs outside through `AssertNoPhaseChanges`, and a misuse of `Verify` outside the base is not pointed to it. | Scope the summary sentence to the formatter tests that call it; reword the message to "`Verify` runs only inside … ; outside it, use `AssertNoPhaseChanges`". Text only (the message is a compiled string in a test assertion; no product behavior). |

### Guard scope
_N/A — no predicate added; the call sites pass the line ending and options each class formats with (checked per site below)._

Per-site input check (FC-033): `LineEndingIndependence`, `LineEnding`, `RawStringOtherLineTerminator` use the tree overload, i.e. the detected line ending (`LineEndingUtilities.DetectEndOfLine` returns only `\n` or `\r\n`, so `DescribeFailure`'s LF/CRLF naming stays exact) and the tree's options (default in all three); `DirectivePreservation` passes the same `_parseOptions` object its `Format` parses with; `SingleLineTypeBrace`, `DocumentationCommentCollapseGuard` pass `"\n"`/default as their `Format` does; `FormattingPipelineTests` passes `context.EndOfLine`/default. The `FormattingContext` default language version and `LanguageVersionResolver.Resolve(default options)` are both `CSharp15`, so the context `SecondPassInput.Create` builds equals the context those private `Format` helpers build.

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| FMT-PIPE-027 failure text (`… changes the final output under … line endings when it runs alone (FMT-PIPE-027).`) | `PhaseStabilityCheck.DescribeFailure:163-168` only; `PhaseStabilitySession.Check` and both `AssertNoPhaseChanges` overloads call it; tests `PhaseStabilityCheckTests:405`, `PhaseStabilityAssertionTests:83,131` assert it | one owner — ok (was inline in the session before) |
| Which phases run alone | `PhaseStabilityCheck.CreateProductionPhases` (reflection on `FormattingPipeline.CreatePhases`), shared by session and session-free paths | one owner — ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PhaseStabilityAssertionTests.AssertionPassesOnFormattedSourceWithoutASession` | session-free assertion runs outside the base and passes on formatted text | throws (no session needed) or reports a phase | text overload, LF/CRLF | adequate |
| `…AssertionFailsNamingThePhaseThatChangesTheOutput` | failure names phase, line ending, requirement, first changed line | wrong phase name, ending, or missing difference text | text overload | adequate |
| `…AssertionParsesTheOutputWithTheGivenParseOptions` | text overload uses the given options | with options ignored, the `FEATURE` case passes → `Assert.Throws` fails | text overload | adequate |
| `…AssertionOnAnOutputTreeUsesTheDetectedLineEndingAndTheTreeParseOptions` | tree overload uses detected ending and tree options | constant `"\n"` → message says LF under CRLF; ignored options → no throw | tree overload | adequate |
| `…AssertionPassesOnTheOutputTreeOfTheFormatter` | passes on real formatter output; options carried | options dropped → still passes | tree overload | weak — finding #1 |
| The seven call sites | per-phase stability of each full-run output | any phase changing that output | `AssertNoPhaseChanges` | adequate for the input each class formats (LF-only in the three regression classes is the issue's scope, recorded by 01/02) |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None_ new. The private `Format` copies are the skeptic's finding, decided as a follow-up (decision 5).

### Hints
- `ReihitsuFormatterLineEndingIndependenceTests.AssertSameOutputAsCopy` checks only the `FormatSyntaxTree` tree and says one check covers `FormatDocumentAsync` because the texts are equal. That holds while the `AdhocWorkspace` project's parse options resolve to the same language version and symbols as the default tree options, which they do for these fixtures; a fixture that depends on a language gate would need its own check.

### Follow-up candidates
- `Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` asserts a full pipeline's second-pass stability under LF and CRLF for 159 analyzer test classes without the per-phase check (FMT-PIPE-027), and `Reihitsu.Cli.Test/Integration/FormatCommandHandlerIntegrationTests.ExecuteAsyncFormattingIsIdempotent` does the same through the CLI. Same class as #1577, outside its scope (the issue names `Reihitsu.Formatter.Test` only); reaching `PhaseStabilityCheck` needs a project reference as in #1576. Structural cause: the per-phase check lives in one test project's helpers while full-run idempotency is asserted in three.

## thoenissen/Reihitsu#1576

### Scope
Full audit, head 1b8a1bd against base 372daee.

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | low | `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs:120-138` (`NoPhaseChangesTheFormattedOutputOfSourceFile`) | FC-001 | – | PR-introduced | The skip set is meant to be "an output the formatter did not format" (FMT-PIPE-027, *Output of a run that formatted*). `FormatSyntaxTree` (`ReihitsuFormatter.cs:96-115`) returns its input unchanged for four causes: syntax errors (skipped by `ParseUnderEachLineEnding`), a hiding carrier (skipped), unsupported (skipped), and a generated-code header (FMT-PIPE-006, not skipped). A source whose first token carries `// <auto-generated/>` but whose name is not `.Designer.cs`/`.g.cs`/`.g.i.cs` would be checked although unformatted, and could fail the row while both FMT-PIPE-004 rows pass. No scanned source has such a header today (`rg -il "<auto-?generated"` hits only literals in test and helper files; the scan is green on 1810 rows), so nothing fails now. | Skip by the one cause the four share instead of listing three of them: `ReferenceEquals(result.Output, syntaxTree)` is true exactly for the unchanged-return branches (a formatted result is always a new tree from `WithRootAndOptions`), so it replaces both explicit skips; or name the remaining gap in the summary. |
| 5 | low | same method, `:125-137` | – (code quality) | – | PR-introduced | The two sibling methods (`FormatterIsIdempotentOnSourceFile`, `FormatterProducesNoChangesOnSourceFile`) collect the failures of both line endings and report them together; the new method throws on the first failing line ending, so a file that fails under LF and CRLF reports only LF. | Collect the failures per line ending (e.g. `FindChangingPhases` + `DescribeFailure`, or catch and aggregate) and fail once, as the siblings do; or accept and leave as is. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which scanned outputs the per-phase check runs on | `ParseUnderEachLineEnding` (syntax errors), `ReihitsuFormatter.HoldsHidingCarrier(tree)`, `result.Unsupported != null` | `ReihitsuFormatter.FormatSyntaxTree:96-115` unchanged-return branches: `HasSyntaxErrors`, `IsAutoGeneratedSource`, `HoldsHidingCarrier`, `result.Unsupported` | three of four; `IsAutoGeneratedSource` uncovered (name-based catalog filter is a different predicate) | finding #4 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Per-phase stability check | `Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck` only; self-hosting reaches it through the new `ProjectReference` (no copy, no `InternalsVisibleTo`) | one owner — ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` | no production phase run alone changes the formatted output of any scanned source, LF and CRLF, detected ending and the tree's options | any phase change → `Assert.Fail` naming phase and ending; row names the file | `AssertNoPhaseChanges(SyntaxTree)` | adequate for its invariant (the skip branches are not exercised: no source reaches them; see #4) |

Project wiring checked: `Reihitsu.SelfHosting.Test.csproj` → `Reihitsu.Formatter.Test.csproj`; `RequirementAttribute` resolves to `Reihitsu.SelfHosting.Test.RequirementAttribute` (enclosing namespace before compilation-unit usings; Release build clean per 03/04); CI runs each test project by its own `.csproj` (`SonarCloud.yml:57-58,70`, `scripts/test.sh:87`), so the referenced assembly's tests are not run twice (03 confirmed the self-hosting total excludes them).

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None_ beyond #4.

### Follow-up candidates
_None_ beyond `00-lead.md`.

## Whole run

### Failure classes dismissed
- FC-001/FC-002 — only one predicate in the diff (the self-hosting skip set), traced above (#4).
- FC-003 — no changed boundary condition in production code.
- FC-004, FC-005, FC-019, FC-020, FC-021 — no edit of tokens, gaps, comments, or directives; test and helper code only. Directive-bearing fixtures (`DirectivePreservationTests`, `#if FEATURE`) are checked under the options they were formatted with.
- FC-006, FC-022–FC-027, FC-029, FC-036 — no rewrite; formatter source untouched (`git diff --stat` has no `src/Reihitsu.Formatter/` path).
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043 — no code fix touched.
- FC-010, FC-012 — the change adds checks for exactly these classes; no phase changed.
- FC-011 — no loop added.
- FC-013, FC-014 — no analyzer or rule.
- FC-015/FC-016 — policy ownership tables above; the failure text moved to one owner.
- FC-017 — finding #1; LF-only regression classes are the issue's scope.
- FC-028 — node-level entry points not involved (FMT-PIPE-027 excludes them).
- FC-031 — findings #2, #3; the other changed summaries (`ObservedPipeline.Run`, the seven test summaries, `FormatterSelfHostingTests` class and method summaries, `scripts/README.md`) describe the new code.
- FC-032 — the self-hosting run (03) is on 5652ed0; later commits change no compiled file (`git diff --stat 5652ed0 1b8a1bd -- src scripts` is empty).
- FC-033 — call sites pass the line ending and options their output was formatted with (table under #1577).
- FC-034, FC-035 — no reproduction; expected values are formatter outputs demanded by unchanged requirements (empty-type semicolon body and its language gate, member indentation).
- FC-037 — no text-only claim made.
- FC-038–FC-042, FC-044 — no unsupported shape or precheck change; no guard modelling a computed result.

### Other checks
- Visible output: no test of a visible surface changes an expected value; the new asserted outputs (`class C;` under default options and braces kept under C# 11; a misindented member indented) follow from unchanged formatter requirements. Nothing undecided.
- Cost of special cases: no production code.
- Scope: every changed path is named by the handovers and decisions 2–7; `scripts/README.md` sentence is decision 7.
- Comments: no `TODO`, no commented-out code, no tracker numbers in changed source.

### Follow-up candidates
- See #1577 (analyzer and CLI idempotency assertions without the per-phase check). The existing candidates in `00-lead.md` stand; `scripts/trace.sh` is still mode `100644` at the head.
