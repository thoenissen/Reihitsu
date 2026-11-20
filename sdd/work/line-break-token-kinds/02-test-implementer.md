Role: test implementer
Commit: 5adc655
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Per `00-lead.md` decisions 1–2, these are characterization tests. They pass on the unchanged tree and have to keep passing after the refactor. No reproduction exists because the issue reports no wrong output.

## Tests

All tests below pass on 5adc655 (unchanged production code). The `Terminator × line ending` rows are DataRows.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5604...AnalyzerTests.VerifyForeignLineBreakInsideADocumentationCDataSectionIsFixedToThePredominantLineEnding` (`\r\n`/LF, `\n`/CRLF, `\r`/LF, U+2028/CRLF) | RH5604-001, RH5604-005 | `Verify(source, fixed, …)` via `VerifyForeignLineBreakIsFixed` | pass |
| `…InsideADocumentationXmlCommentIsFixed…` (same rows) | RH5604-001, RH5604-005 | same | pass |
| `…InsideADocumentationElementTagIsFixed…` (`<para⟨eol⟩ foo="a">`) | RH5604-001, RH5604-005 | same | pass |
| `…InsideADocumentationAttributeValueIsFixed…` | RH5604-001, RH5604-005 | same | pass |
| `…InsideADelimitedDocumentationCDataSectionIsFixed…` (`/** */`) | RH5604-001, RH5604-005 | same | pass |
| `…InsideAnInterpolationHoleIsFixed…` (regular `$"…"`) | RH5604-001, RH5604-005 | same | pass |
| `…InsideARawInterpolationHoleIsFixed…` (`$"""…"""`) | RH5604-001, RH5604-005 | same | pass |
| `VerifyForeignLineBreakInTheTextOfARawInterpolatedStringDoesNotProduceDiagnostics` | RH5604-003 | `Verify(source)` | pass (not reported) |
| `VerifyNoDiagnosticsWhenEveryLineBreakIsTheSameForeignTerminator` (`\r`, U+0085, U+2028, U+2029; tokens, directive ends, `///`) | — (characterization; RH5604-002 names only CRLF/LF) | `Verify(source, onConfigure)` | pass (not reported) |
| `ReihitsuFormatterLineEndingTests.LineBreakInsideDocumentationCDataSectionIsRewritten` (4 rows as above) | FMT-EOL-003 | `AssertFormatted` (byte-exact, second pass, per-phase stability) via `AssertForeignLineBreakFormatted` | pass |
| `…LineBreakInsideDocumentationXmlCommentIsRewritten` | FMT-EOL-003 | same | pass |
| `…LineBreakInsideDocumentationElementTagIsRewritten` | FMT-EOL-003 | same | pass |
| `…LineBreakInsideDocumentationAttributeValueIsRewritten` | FMT-EOL-003 | same | pass |
| `…LineBreakInsideDelimitedDocumentationCDataSectionIsRewritten` | FMT-EOL-003 | same | pass |
| `…LineBreakInsideInterpolationHoleIsRewritten` | FMT-EOL-001 | same | pass |
| `…LineBreakInsideRawInterpolationHoleIsRewritten` | FMT-EOL-001 | same | pass |
| `…LineBreakInTextOfRawInterpolatedStringIsKept` | FMT-EOL-004 | same | pass |
| `…FileWhoseEveryLineBreakIsTheSameOtherTerminatorGetsLineFeed` (`\r`, U+0085, U+2028, U+2029) | FMT-PIPE-007 | `AssertFormatted` | pass (output all `\n`) |
| `LineEndingUtilitiesTests.LineBreakInsideDocumentationCDataSectionIsACarrier` (`\r\n`, `\r`, U+2028) | FMT-PIPE-007, FMT-EOL-003 | `AssertCarrier<XmlCDataSectionSyntax>` | pass |
| `…LineBreakInsideDocumentationXmlCommentIsACarrier` | FMT-PIPE-007, FMT-EOL-003 | `AssertCarrier<XmlCommentSyntax>` | pass |
| `…LineBreakInsideDocumentationElementTagIsACarrier` | FMT-PIPE-007, FMT-EOL-003 | `AssertCarrier<XmlElementStartTagSyntax>` | pass |
| `…LineBreakInsideDocumentationAttributeValueIsACarrier` | FMT-PIPE-007, FMT-EOL-003 | `AssertCarrier<XmlTextAttributeSyntax>` | pass |
| `…LineBreakInsideDelimitedDocumentationCDataSectionIsACarrier` | FMT-PIPE-007, FMT-EOL-003 | `AssertCarrier<XmlCDataSectionSyntax>` | pass |
| `…LineBreakInsideInterpolationHoleIsACarrier` | FMT-PIPE-007, FMT-EOL-001 | `AssertCarrier<InterpolationSyntax>` | pass |
| `…LineBreakInsideRawInterpolationHoleIsACarrier` | FMT-PIPE-007, FMT-EOL-001 | `AssertCarrier<InterpolationSyntax>` | pass |
| `…LineBreakInTextOfRawInterpolatedStringIsNotACarrier` | FMT-PIPE-007, FMT-EOL-004 | direct: `DetectEndOfLine` on the text node falls back to `Environment.NewLine`; `NormalizeLineEndings(root, "\n")` returns `root` itself | pass |
| `…DetectEndOfLineReturnsLineFeedWhenEveryLineBreakIsTheSameOtherTerminator` (`\r`, U+0085, U+2028, U+2029) | FMT-PIPE-007 | direct `DetectEndOfLine(root)` | pass |

`AssertCarrier` does two things. It calls `DetectEndOfLine` on the smallest node that holds the one line break, and asserts the bucket that line break counts as (`\r\n` → `\r\n`, a single character → `\n`). It does this only where the bucket differs from the `Environment.NewLine` fallback, so on Linux the `\r\n` row is the conclusive one. It then asserts that `NormalizeLineEndings(root, "\n")` gives the source with exactly that line break rewritten to `\n` and nothing else changed.

Existing tests: none were changed. No output changes in this run, so no existing expectation is affected. The existing RH5604 tests (`VerifyForeignTerminator*`, the literal, comment, and disabled-text exemptions, `VerifyCrLfWinsTie*`, the Fix All scenario), `ReihitsuFormatterLineEndingTests`, `LineEndingNormalizationPhaseTests`, and `ReihitsuFormatterHelpersTests.DetectEndOfLine*` stay as they are and still pin the between-token, directive-end, plain `///` text, fallback, and tie cases.

## Scenarios (bug fix)

No reproduction (decision 1). Characterization arms that were run:

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| One foreign break in CDATA, XML comment, element tag, attribute value (`///`), CDATA (`/** */`), regular hole, raw hole | LF file with `\r\n` / `\r`; CRLF file with `\n` / U+2028 | analyzer + code fix + re-analysis | reported on its line, fixed in one pass | as specified (RH5604-001/005) |
| Same seven positions | same four arms | formatter `FormatSyntaxTree`, 2 passes, per-phase stability | rewritten, nothing else changes | as specified (FMT-EOL-001/003) |
| Same seven positions | `\r\n`, `\r`, U+2028 | Core `DetectEndOfLine` / `NormalizeLineEndings` | counted and rewritten | as specified (FMT-PIPE-007, FMT-EOL) |
| Foreign break in raw interpolated literal text | four arms | analyzer, formatter, Core | not reported, kept, not counted | as specified (RH5604-003, FMT-EOL-004) |
| Every break the same `\r`/U+0085/U+2028/U+2029 | — | analyzer | not reported | characterization (matches the skeptic's observation) |
| Same | — | formatter, Core detection | rewritten to / detected as `\n` | as specified (FMT-PIPE-007) |

Fixture note: in the formatter tests, the hole fixtures are written in the formatter's canonical layout (`{1⟨eol⟩ + 2}`, with the operator leading the continuation line, and raw content at column 16). That way the line ending is the only thing the formatter changes. The first draft used `{1 +⟨eol⟩ 2}`, which the line-break phase re-laid, and that layout change is unrelated to this run.

## Not covered, and why

- **FC-003 (both sides of a condition).** The refactor adds no condition. The carrier side and the non-carrier side are both covered: the seven carrier positions, plus raw interpolated text. Verbatim, raw, and interpolated verbatim text, `/* */`, and disabled text are already covered on the non-carrier side by `VerifyLineEndingsInside*`, `LineBreaksInsideLiterals*`, and `LineEndingNormalizationPhaseTests.ExecuteReturnsItsInputWhenNoLineBreakDiffersFromTheDetectedLineEnding`.
- **The analyzer's "mixed" rule versus the detection's "predominant" rule (skeptic, breaking input 3).** The not-mixed side is pinned by `VerifyNoDiagnosticsWhenEveryLineBreakIsTheSameForeignTerminator`. The mixed side, `\n` plus one lone `\r`, is pinned by the existing `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` (`\r`, LF row).
- **Fallback to `Environment.NewLine` with no carrier (skeptic, input 4).** Already pinned by `ReihitsuFormatterHelpersTests.DetectEndOfLineFallsBackToEnvironmentNewLineWhenNoEndOfLineTriviaFound`. The Core non-carrier test also relies on it.
- **FC-008 (Fix All).** The existing `GetFixAllScenario` already combines an XML and an ordinary carrier in one document. The fix rewrites the whole file through one call, so a new carrier position adds no new Fix All path.
- **FC-019, FC-020, FC-021 (comment or directive in a gap).** The change touches no gap and joins no line. Directive ends as carriers are already pinned (`VerifyForeignTerminatorAtTheEndOfADirective…`, FMT-EOL-002 tests), and the all-foreign fixtures include `#pragma` lines.
- **FC-040 (documentation mode).** Not tested. The three consumers read the same tree, so the refactor cannot make them disagree on mode. This is the skeptic's open follow-up candidate and is out of scope here.
- **FC-033.** Handled. The tests use in-memory strings, so no harness normalizes a lone `\r`. Every carrier is run with `\r` and U+2028 as well as CRLF/LF.
- **FC-010, FC-017.** The formatter tests check a second pass, per-phase stability, and byte-exact output under both LF and CRLF detected endings.
- **FC-004–FC-007, FC-009, FC-011–FC-014, FC-018, FC-022–FC-032, FC-034–FC-039, FC-041–FC-044.** These do not apply. The change rewrites no syntax shape, adds no rule, rename, guard, or precheck shape, and changes no output.
- **Unicode terminators U+0085 and U+2029 at carrier positions.** Only `\r` and U+2028 are used there. The carrier decision is by syntax kind, not by terminator text, and U+0085/U+2029 are already covered between tokens (`VerifyForeignTerminatorBetweenTokens…`, `OtherLineTerminatorsBetweenTokensAreRewritten`) and in the all-foreign tests.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterLineEndingTests"
scripts/test.sh --project core --filter "FullyQualifiedName~LineEndingUtilitiesTests"
scripts/test.sh --project architecture
```

Results on 5adc655: analyzer 79/79, formatter 48/48, core 28/28, architecture 198/198.

## Follow-up candidates

_None_ beyond the skeptic's FC-040 candidate (RH5604 under `DocumentationMode.None`), which is already in `00-lead.md`.
