Role: auditor
Commit: 0c154f10285c400b033a1c202c83ef73c94608f6
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `0c154f10285c400b033a1c202c83ef73c94608f6` is checked out, and `git status` shows a clean tree. `origin/main` and `git ls-remote origin refs/heads/main` both resolve to `9a89c85e2cdd66a0d55cdfd821c5a6fde6d5c5e2`, which is an ancestor of the head. The diff from the base matches what the handovers describe:

- production: `src/Reihitsu.Core/LineEndingUtilities.cs` and `src/Reihitsu.Analyzer/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsAnalyzer.cs`;
- tests: `LineEndingUtilitiesTests.cs` (new), `ReihitsuFormatterLineEndingTests.cs`, `RH5604...AnalyzerTests.cs`;
- handovers under `sdd/work/line-break-token-kinds/`.

`scripts/lane-check.sh bug-fix` returns `confirmed — claimed bug-fix, diff needs bug-fix`. I trusted the cleaner's Release build and the roles' focused test runs, and re-ran no test.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None_ | | | | | | | |

### Against the specs

- **thoenissen/Reihitsu#1886, issue goal "one shared definition":** met. `GetLineBreakTrivia` and `GetLineBreakTokens` hold the kind and the `descendIntoTrivia: true` flag together. `DetectEndOfLine`, `NormalizeLineEndings`, and `RH5604...Analyzer.OnSyntaxTree` (`:57`, `:67`) all walk through them.
- **Issue goal "outputs unchanged":** met by construction. Each old statement was `DescendantTrivia(descendIntoTrivia: true)` filtered on `EndOfLineTrivia`, or `DescendantTokens(descendIntoTrivia: true)` filtered on `XmlTextLiteralNewLineToken`. The new helpers are the same expressions. `NormalizeLineEndings` still replaces tokens first and trivia second on the token-normalized node, and still returns the input node when nothing differs. In the analyzer, the "mixed" rule (`counts.Count <= 1`), the call to `DetectEndOfLine`, and the per-line location are unchanged.
- **Requirements:** RH5604-001..005, FMT-PIPE-007, and FMT-EOL-001..004 are unchanged and still hold. The run's narrowing (decision 2: no merge of the "mixed" and "predominant" rules) is kept.
- **Doc-comment claims checked against Roslyn 5.12.0.** I ran a scratch probe outside the repository, using `CSharpSyntaxTree.ParseText` with `DocumentationMode.Parse`:
  - A line break inside an element tag (`<para⟨CRLF⟩ /// foo="a">`) is structured `EndOfLineTrivia`, in both `///` and `/** */`.
  - A line break inside an attribute value is an `XmlTextLiteralNewLineToken`.

  Both summaries are therefore accurate. That includes "the trivia inside structured trivia such as documentation comments" in `GetLineBreakTrivia`, and "attribute values" in `GetLineBreakTokens`.

### Visible output is decided (check 10)

No existing expected value changed. For each new expected value:

| Test | What it expects | What decides it |
|---|---|---|
| RH5604 carrier tests (7 positions × 4 terminator/ending rows) | Reported on the line and fixed in one pass | RH5604-001, RH5604-004, RH5604-005 |
| `VerifyForeignLineBreakInTheTextOfARawInterpolatedStringDoesNotProduceDiagnostics` | Not reported | RH5604-003 |
| Formatter carrier tests | Only the line ending changes; the expected text equals the input text | FMT-EOL-001, FMT-EOL-003. The fixtures are in canonical layout, so no layout output is asserted beyond what the formatter keeps today |
| `LineBreakInTextOfRawInterpolatedStringIsKept` | Kept | FMT-EOL-004 |
| `FileWhoseEveryLineBreakIsTheSameOtherTerminatorGetsLineFeed` | Output uses `\n` | FMT-PIPE-007 ("a line break of a single character MUST count as `\n`") together with FMT-EOL-001 |
| `VerifyNoDiagnosticsWhenEveryLineBreakIsTheSameForeignTerminator` | Not reported | See the first hint |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which trivia carry a line break (`GetLineBreakTrivia`) | The whole node, structured trivia included (directive ends, doc-comment tags) | The former three private copies: `DetectEndOfLine`, `NormalizeLineEndings`, the RH5604 analyzer | Identical kind and traversal; the copies are now deleted | holds |
| Which tokens carry a line break (`GetLineBreakTokens`) | The whole node's tokens, tokens inside structured trivia included (doc-comment text, CDATA, XML comment, attribute value, in `///` and `/** */`) | Same three former copies | Identical | holds |
| Text-line check for fixes that copy a span (`HoldsOtherLineBreak`, SHARED-FIX-008) | The span's text, with every terminator counted, also inside literals and comments | Carrier set above | Deliberately different. SHARED-FIX-008's *Why* states that the check counts literal and comment line breaks. The diff does not touch it | holds (separate decision) |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Line-break carrier set | Production: `LineEndingUtilities.GetLineBreakTrivia` / `GetLineBreakTokens` only. `rg XmlTextLiteralNewLineToken` (production) finds `LineEndingUtilities.cs:47` and `RH8308...Analyzer.cs:90`; RH8308 reads doc-comment line structure, which is a different decision. `rg "descendIntoTrivia: true"` combined with EndOfLine finds no other production statement. Test side: `Formatter.Test/Helpers/ObservedPipeline.FindForeignLineBreaks` (`:163-178`) restates the set | One production owner. The test-side copy is an independent oracle, which keeps it able to falsify the production set (see the second hint) |
| Predominant-ending bucket rule (FMT-PIPE-007) | `LineEndingUtilities.DetectEndOfLine` only. The analyzer calls it at `:80`. `rg '"\r\n" : "\n"'` finds only test helpers | one owner |
| "Mixed" rule (distinct texts) | `RH5604...Analyzer.OnSyntaxTree:73` only | one owner; kept separate by decision 2 |
| Line-ending rewrite | `LineEndingUtilities.NormalizeLineEndings`, called by `LineEndingNormalizationPhase.cs:26` and `RH5604...CodeFixProvider.cs:42` | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `LineEndingUtilitiesTests.*IsACarrier` (7 tests × `\r\n`, `\r`, U+2028) | The one line break is counted by detection and rewritten by normalization, and nothing else changes | Detection on the holder returns the fallback, or the normalized full text differs byte for byte | `AssertCarrier` asserts the placement, the detection (where the fallback differs from the count), and byte-exact normalization of the whole root | adequate. On Linux the detection assertion only decides in the `\r\n` row, but the byte-exact normalization assertion decides every row, and both consumers now read the same enumeration |
| `LineBreakInTextOfRawInterpolatedStringIsNotACarrier` | A line break in literal text is neither counted nor rewritten | `NormalizeLineEndings` returns a different node, or the detection does not fall back | `Assert.AreSame(root, …)` decides all rows | adequate |
| `DetectEndOfLineReturnsLineFeedWhenEveryLineBreakIsTheSameOtherTerminator` | A file of one single-character terminator is detected as `\n` | Returns `\r\n` | Direct assertion | adequate for what it states. On Linux, `\n` coincides with the `Environment.NewLine` fallback, so this test alone cannot tell "counted" from "not counted". The existing `ReihitsuFormatterHelpersTests.DetectEndOfLineCountsALoneCarriageReturnAsLineFeed` pins the counting |
| `ReihitsuFormatterLineEndingTests` (8 new carrier and non-carrier tests × 4 rows, plus the all-foreign test) | Byte-exact output under the LF and the CRLF detected ending, a stable second pass, and no phase changes | Any byte differs, or the second pass or a phase changes the output | `AssertFormatted` plus `PhaseStabilityCheck.AssertNoPhaseChanges`. `AssertForeignLineBreakFormatted` replaces `\n` before the placeholder, so a `\n` terminator is not converted twice | adequate (FC-010, FC-017) |
| `RH5604...AnalyzerTests` (7 carrier tests × 4 rows) | Reported on the whole line, and fixed in one pass to the predominant ending | A missing or extra diagnostic, a wrong span, a fixed text that differs, or a fix that does not converge | `Verify(source, fixed, …, Diagnostics(...))` on the batch code-fix base, which checks the diagnostics and the fixed state. `SkipSuppressionCheck` follows the existing RH5604 tests | adequate |
| `VerifyForeignLineBreakInTheTextOfARawInterpolatedStringDoesNotProduceDiagnostics`, `VerifyNoDiagnosticsWhenEveryLineBreakIsTheSameForeignTerminator` | Not reported | Any diagnostic | `Verify(source)` | adequate. The all-foreign test would also pass if the carrier set were empty, but the carrier tests above rule that out |

## Failure classes dismissed

- FC-001, FC-002, FC-003: the diff adds no guard and changes no condition. Both sides of the carrier boundary are covered: seven carrier positions, raw interpolated text, and the existing literal, `/* */`, and disabled-text tests.
- FC-004, FC-005, FC-019, FC-020, FC-021: no gap is edited and no line is joined. The rewrite of line-break characters is unchanged.
- FC-006, FC-022 to FC-029, FC-036: no syntax shape is rewritten. Raw and interpolated raw text is pinned as a non-carrier.
- FC-007, FC-008, FC-009, FC-018, FC-030: the RH5604 fix is unchanged and still rewrites the whole file through one call. Convergence is checked by the new code-fix tests. The existing Fix All scenario mixes doc-comment and ordinary carriers.
- FC-010, FC-011, FC-012: no phase changed. The formatter tests check a second pass and per-phase stability.
- FC-013, FC-014: no rule added. The relation between the analyzer and the formatter is unchanged.
- FC-015, FC-016: the issue's own class. Resolved: one production owner (see *Policy ownership*), and no sibling statement remains.
- FC-017: see *Assertion adequacy*.
- FC-031: every changed summary describes the new code: the class, both new methods, `DetectEndOfLine` (bucket rule, tie, fallback), `NormalizeLineEndings`, `OnSyntaxTree`, and the inline comment in `DetectEndOfLine`. The diff has no `TODO`, no commented-out code, and no issue number.
- FC-032: no project result is reused across the compiled change. The implementer's full-project runs are on 92678eb, which is the production head.
- FC-033: the new tests use in-memory strings and lone `\r` and U+2028 rows, so no harness normalizes the terminator away.
- FC-034, FC-035: there is no reproduction (decision 1). The expected values come from specs and were checked to pass before the production change (5adc655).
- FC-037: the diff is not claimed to be text-only.
- FC-038, FC-039, FC-041, FC-042, FC-043: no unsupported shape is declared, and the precheck is untouched.
- FC-040: the three consumers read one tree, so the refactor cannot make them disagree on documentation mode. The pre-existing mode dependence is under *Follow-up candidates*.
- FC-044: nothing is modelled. The carrier set reads Roslyn's own kinds.
- Tab-width matrix: the decision is not reached (no column changes).

## Architecture findings

_None._ The issue's own finding (one rule stated in three places) is resolved by this diff. The production carrier set now has one statement.

## Hints

- **`VerifyNoDiagnosticsWhenEveryLineBreakIsTheSameForeignTerminator` locks an output that no requirement states in so many words.** The test pins "not reported" for a file whose line breaks are all one `\r`, U+0085, U+2028, or U+2029. RH5604-002 names only all-`\r\n` and all-`\n` files. I count this output as decided, for two reasons:
  - RH5604-001 is the only requirement under which RH5604 reports, and its precondition ("When the line breaks of a file are not all the same") is false for this input.
  - RH5604-002's *Why* ("consistency within the file, not a particular line ending") and the rule page ("A file must end all of its lines the same way") both describe this outcome.

  The output is also unchanged from `main`. The spec's introduction can be read the other way: "reports every line whose line ending differs from the file's predominant line ending", where the predominant ending is `\n` here. If the lead reads that as a second reading, check 10 makes this an `undecided output`, and the clarification that decision 3 skipped would close it. The clarification would add "or all one other terminator" to RH5604-002.
- **The test oracle `ObservedPipeline.FindForeignLineBreaks` is a test-side copy of the carrier set.** It is deliberately independent, because an oracle that called `GetLineBreakTrivia`/`GetLineBreakTokens` could not falsify them. If Roslyn ever adds a carrier kind, the oracle and production drift together only by hand. This is not a finding.
- **A skeptic claim was not implemented, on purpose.** Under *Refuse option*, the skeptic said the change "removes the analyzer's second walk of the tree". The implementer kept the `DetectEndOfLine` call, which follows the skeptic's own internal choice 2 (one owner for the bucket rule). The second walk runs only when the file is mixed. This is not a defect.

## Follow-up candidates

- **RH5604 under `DocumentationMode.None` (FC-040), already in `00-lead.md`; new evidence.** This run does not change it.
  - **Probe (Roslyn 5.12.0, scratch, outside the repository).** Under `DocumentationMode.None`, a CRLF inside a `/** */` documentation comment (tag interior) is not a carrier: it is part of the multi-line comment trivia, and no `EndOfLineTrivia` or `XmlTextLiteralNewLineToken` is produced for it. Under `None`, a `///` line's line breaks stay `EndOfLineTrivia` carriers.
  - **Consequence.** RH5604 registers a plain `RegisterSyntaxTreeAction`, so if a build runs it under `None`, it does not report a foreign line break inside a `/** */` comment. The formatter and the CLI parse with the default `Parse` mode and rewrite that line break.
  - **Not checked.** I did not establish which documentation mode a repository build without documentation generation passes to analyzers. The repository's `RegisterSyntaxTreeActionWithDocumentationModeCheck` and `DocumentationAnalysisUtilities` exist for that case, which suggests it is reachable.
  - **Open question.** Whether this is a defect or acceptable under P11 is for a separate premise check.
