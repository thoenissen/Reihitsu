Role: test implementer
Commit: 158c0ba
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

No visible behavior is wrong (01-skeptic: FC-015, structural only), so no test can fail first. Every new test pins today's output of the RH5604 code fix or of `LineEndingNormalizationPhase` and passes on `158c0ba`; it must stay green, unchanged, after the implementer moves the rewrite to one owner. No existing test was changed: the diff of `RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs` is additions only (one `#region Constants` with the placeholder `¶`, two usings, five test methods), and the formatter file is new.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` (8 rows: `\r`, U+0085, U+2028, U+2029 × LF, CRLF) | — (characterization; RH5604-005 is stated for `\r\n`/`\n` files only); FC-010, FC-033 | `Verify(source, fixedSource, …)` | pass |
| `…VerifyForeignTerminatorAtTheEndOfADirectiveIsFixedToThePredominantLineEnding` (4 rows: `\r`, U+2028 × LF, CRLF) | — (characterization); carrier: directive-ending trivia | `Verify` | pass |
| `…VerifyForeignTerminatorInsideADocumentationCommentIsFixedToThePredominantLineEnding` (4 rows: `\r`, U+2028 × LF, CRLF) | — (characterization); carrier: `///` newline token | `Verify` | pass |
| `…VerifyCodeFixConvertsTheLineEndingsOfAFileTheFormatterRefusesAsUnsupported` (LF, CRLF) | RH5604-005; FC-043 (owner must apply no formatter guard) | `Verify` + `ReihitsuFormatter.FindUnsupportedSyntax` asserting the fixture is refused | pass |
| `…VerifyCodeFixConvertsTheLineEndingsOfAFileWithASyntaxError` | RH5604-005; formatter syntax-error guard | `Verify` with `{|CS1002:|}` markup in both states | pass |
| `Reihitsu.Formatter.Test.Unit.Pipeline.LineEndingNormalizationPhaseTests.ExecuteReturnsItsInputWhenNoLineBreakDiffersFromTheDetectedLineEnding` (LF with CRLF kept; CRLF with LF kept; LF with U+2028 kept) | — (the phase summary's identity promise; no requirement states it); FC-022 | `LineEndingNormalizationPhase.Execute` direct, `Assert.AreSame` | pass |
| `…LineEndingNormalizationPhaseTests.ExecuteRewritesAForeignLineBreakThatOnlyADocumentationCommentHolds` (LF, CRLF) | FMT-EOL-003 | `Execute` direct | pass |

Fixtures are byte-exact: templates written with `\n` and a `¶` placeholder, turned into the test's line ending with `ConvertEndOfLine` (or `Replace("\n", …)`), and the placeholder then replaced with the foreign terminator, so no harness normalizes the terminator away (FC-033). The Unicode terminators are `\uXXXX` escapes in the source, checked: the files hold no raw U+0085, U+2028, or U+2029.

Every new RH5604 fix test sets `SkipSuppressionCheck` with the comment the existing tests carry: without it, the harness's injected `#pragma` lines add a line ending to the fixture and the suppression pass sees an extra RH5604 (observed on the first run, `Context: Verifying exclusions in '#pragma warning disable' code`). This is the same harness effect the existing tests in this class skip for.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Existing RH5604 set (analyzer, fix, Fix All, formatter parity) | LF, CRLF | analyzer + fix + formatter | 28 → 47 passed (19 new rows) | as expected; unchanged |
| Foreign `\r`/U+0085/U+2028/U+2029 between tokens | LF, CRLF | code fix | pass | terminator becomes the predominant line ending; `Y` on its own line |
| Foreign `\r`/U+2028 ending a directive | LF, CRLF | code fix | pass | as expected |
| Foreign `\r`/U+2028 inside `///` | LF, CRLF | code fix | pass | as expected |
| Mixed `\r\n`/`\n` in a file holding a comment inside a variable declaration (formatter refuses it) | LF, CRLF | code fix | pass | fix offered and applied; comment kept |
| Same file | — | formatter (`FindUnsupportedSyntax`) | refused | as the skeptic observed (Settled 2) |
| Mixed line endings in a file with CS1002 | CRLF | code fix | pass | fix offered and applied (settles the skeptic's open syntax-error case) |
| Uniform input with other breaks in literal, comment, disabled text | LF, CRLF, U+2028 kept | phase | same reference | as the phase summary promises |
| Only a `///` newline differs | LF, CRLF | phase | rewritten | as FMT-EOL-003 |

## Not covered, and why

- **Phase side of the terminator × carrier matrix**: already covered by `PhaseLineEndingOwnershipTests.ForeignLineBreaksOfTheInputAreRewrittenBeforeEveryLaterPhase` (`\r`, U+0085, U+2028, U+2029, other) and `LineBreaksInsideLiteralsCommentsAndDisabledTextKeepTheirCharacters`, plus `ReihitsuFormatterLineEndingTests`; not duplicated.
- **Fix kept-break axis** (verbatim, raw, interpolated verbatim, block comment, disabled text): covered by the existing `VerifyLineEndingsInside*` and `…StaysExemptWhenTheSameEndingIsReportedElsewhere` tests.
- **`/** */` with a Unicode terminator** for the fix: the `///` rows and `VerifyMixedLineEndingsInsideDelimitedXmlDocumentationAreDetectedAndFixed` reach the same token kind (`XmlTextLiteralNewLineToken`) and the same `Text != endOfLine` comparison; one carrier row per token kind.
- **FC-008 Fix All**: covered by the existing `GetFixAllScenario` (one iteration, two diagnostics); every diagnostic yields the same whole-file rewrite.
- **FC-009 fix "nothing to rewrite" path**: unreachable — the analyzer reports only when two distinct texts occur, so at least one break differs from the predominant one (01-skeptic).
- **Auto-generated guard**: the analyzer does not analyze generated code, so the fix is never offered there; nothing to pin.
- **FC-028 detached root (`FormatNode`)**: covered through the existing entry-point tests (`VerifyFormatterEntryPointsAndIdempotency` in the RH5604 formatter tests).
- **FC-031** (phase summary stays true): a text check for the spec checker/auditor, not a test.
- **A unit test of the new Core owner**: not written; it would fix the method's name and signature before the implementer chooses them. Both callers are pinned through their public behavior, and the identity promise through the phase.
- FC-003 (conditions on each side): the change adds no condition; identity (nothing differs) and rewrite (only tokens differ; only trivia differ, existing tests) are both pinned.
- Declared-shape classes (FC-038..FC-043 beyond the guard hazard): no shape is declared.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604"
scripts/test.sh --project formatter --filter "FullyQualifiedName~LineEndingNormalizationPhaseTests"
```

Results on `158c0ba` plus these tests: analyzer 47/47 passed; formatter 5/5 passed.

## Follow-up candidates

_None._
