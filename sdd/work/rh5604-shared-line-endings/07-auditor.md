Role: auditor
Commit: 1fd503e5d49a1811f62ba36016a51233640791fa
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head is `1fd503e`; the prompt named `909b83b`, and the one commit between them changes only `sdd/work/rh5604-shared-line-endings/00-lead.md` (`git diff 909b83b 1fd503e --stat`), so the audited code, tests, and specs are byte-identical to `909b83b`. Base `origin/main` = `31d2931af8e2894fba9e66a281edd23b4de9ae6c` (fetched), an ancestor of the head. The diff (`31d2931...HEAD`) matches the handovers: `LineEndingUtilities.NormalizeLineEndings` added in Core, the phase and the RH5604 fix delegate to it, five RH5604 test methods and one new phase test class added, two spec intros clarified. `scripts/lane-check.sh bug-fix`: confirmed. Issue thoenissen/Reihitsu#1874 read via the GitHub MCP server.

Issue goals: rewrite written once and used by both (met: `rg` finds `XmlTextNewLine(`, the `EndOfLineTrivia` rewrite, and the differing-newline-token filter only in `LineEndingUtilities.cs:90-104`); existing RH5604 and line-ending tests unchanged (met: the analyzer test diff is additions only, the formatter test file is new); no code-fix code calls a phase (met: Core is not a banned type under FMT-PIPE-009; architecture 198/198 per 06).

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.Test/Formatting/RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests.cs`: `VerifyForeignTerminatorBetweenTokensIsFixedToThePredominantLineEnding` (8 rows), `VerifyForeignTerminatorAtTheEndOfADirectiveIsFixedToThePredominantLineEnding` (4 rows), `VerifyForeignTerminatorInsideADocumentationCommentIsFixedToThePredominantLineEnding` (4 rows) | – (check 10) | – | undecided output | These tests add expected visible output — an RH5604 diagnostic on a line ending in a lone `\r`, U+0085, U+2028, or U+2029, and the fix result that turns that terminator into the predominant line ending — that no unchanged requirement demands and no maintainer answer covers. RH5604-001 and RH5604-005 are both scoped to "a file whose line breaks are `\r\n` and `\n`"; FMT-EOL-001 decides all six terminators for the formatter only. Example: input `internal class TestClass\n{\n    internal int X;     internal int Y;\n}` — the test pins a diagnostic on line 3 and the fix output with `\n` in place of U+2028. The output equals `main` (skeptic, *Settled 1*), so nothing changes; the gap is only that the tests lock an undecided output. The members that hold: the two RH5604-005-tagged tests (unsupported-shape file, syntax-error file) use only `\r\n`/`\n` and are demanded by RH5604-005; the phase tests are demanded by FMT-EOL-003 or assert reference identity, which is not visible output | Lead asks it as an output question (input: a file mixing `\n` with one lone `\r`/U+2028 line; before = after on `main`: reported, fix converts it; A: keep, and the spec author extends RH5604-001/005 to every terminator FMT-EOL-001 names; B: leave it undecided and the test implementer removes the three characterization methods). A matches FMT-EOL-001 and costs only spec text |
| 2 | low | `sdd/specs/formatter/line-endings.md` intro (line 10), `sdd/specs/analyzer/RH5604.md` intro (line 14) | – (spec convention) | – | PR-introduced | The added sentences state an implementation, not behavior: "The rewrite itself is `LineEndingUtilities`, which the RH5604 code fix runs too" and "The fix runs the same rewrite as the formatter's line-ending phase, `LineEndingUtilities`". `sdd/README.md` (*Layout*): "A spec specifies behavior, never an implementation … a rule may stop using it without a spec change" — after this text, the fix could not stop using the helper without making two specs false. The `line-endings.md` sentence also restates the code-owner rule of `sdd/README.md` ("a type is a code owner of one spec only"), a second statement of one rule | Keep the behavioral part (RH5604: the fix converts a file the formatter refuses as unsupported; RH5604-005 already ties the fix to FMT-EOL-001..003) and drop the implementation sentences and the restated code-owner rule. Text only: cannot reach compiled behavior, diagnostics, or rule metadata |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which newline tokens are rewritten | `DescendantTokens(descendIntoTrivia: true)` of the whole input node, kind `XmlTextLiteralNewLineToken`, `Text != endOfLine` | The removed fix and phase copies (identical filter); `DetectEndOfLine` counts the same kind over the same span; RH5604 analyzer collects the same kind | Same span and boundary as both removed copies | holds |
| Which end-of-line trivia are rewritten | `DescendantTrivia(descendIntoTrivia: true)` of the token-rewritten node, kind `EndOfLineTrivia`, `ToString() != endOfLine` | Same as above | Same | holds |
| "Nothing changed" | Reference equality: `NormalizeLineEndings` returns its input when no token or trivia differs; the fix compares `normalizedRoot == root` (`SyntaxNode` has no `==` overload, so reference equality) | Old fix: returned `document` when both arrays were empty; old phase: returned its input | Same condition; the removed "tokens only" early return in the fix produced the same text as the general path | holds |
| Formatter refusals | The fix applies none (no precheck, no syntax-error or generated-code guard) | Before: the fix applied none either | Same; pinned by the two RH5604-005-tagged tests | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Rewrite of line breaks to a line ending | `rg -n "XmlTextNewLine\(" src --glob '!*Test*'` and the `EndOfLineTrivia … != endOfLine` filter: only `src/Reihitsu.Core/LineEndingUtilities.cs` (`NormalizeLineEndings`). `rg "NormalizeLineEndings\|ReplaceLineEndings"`: the other hits (`CopyrightHeaderTemplateResolver.NormalizeLineEndings`, `ConfigurationManager`) normalize configured header text, not a tree | one owner — holds |
| Detected line ending in the fix | Fix now calls `LineEndingUtilities.DetectEndOfLine` directly; `ReihitsuFormatterHelpers.DetectEndOfLine` (`src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs:32-34`) only forwards to it | same function — holds |
| Which syntax elements carry a line break | `LineEndingUtilities.DetectEndOfLine`, `LineEndingUtilities.NormalizeLineEndings`, `RH5604CodeMustNotContainMixedLineEndingsAnalyzer.OnSyntaxTree` | three statements (four before); all agree today. Pre-existing, recorded as a follow-up candidate under decision 2 of `00-lead.md`; not PR-introduced |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyForeignTerminator…` (three methods, 16 rows) | Fix rewrites a lone `\r`/U+0085/U+2028/U+2029 in code trivia, at a directive's end, inside `///` to the predominant ending | Terminator left in place, or a different ending written; exact text compare | `Verify` (exact fixed text, diagnostics) with fixtures built byte-exact after `ConvertEndOfLine` (FC-033 avoided) | adequate as assertions; output undecided, see #1 |
| `VerifyCodeFixConvertsTheLineEndingsOfAFileTheFormatterRefusesAsUnsupported` | The fix applies no formatter refusal | A fix routed through a formatter entry point would return the file unchanged and `Verify` would fail; `FindUnsupportedSyntax` asserts the fixture really is refused (the check does not depend on line endings, so asserting it on the expected root is sound) | `Verify` + `Assert.IsNotNull` | adequate |
| `VerifyCodeFixConvertsTheLineEndingsOfAFileWithASyntaxError` | The fix applies no syntax-error guard | Unchanged output fails `Verify` | `Verify` with `CS1002` markup in both states | adequate (CRLF-predominant only; a guard would not depend on the ending) |
| `ExecuteReturnsItsInputWhenNoLineBreakDiffersFromTheDetectedLineEnding` | Phase returns its input reference when nothing differs, also with other breaks in literal, block comment, disabled text | A copy returned (`AreSame` fails), or a kept break counted as differing | direct `Execute`, `Assert.AreSame` | adequate |
| `ExecuteRewritesAForeignLineBreakThatOnlyADocumentationCommentHolds` | Token-only path rewrites the `///` newline (FMT-EOL-003) and nothing else | Any other character changes; exact `ToFullString` compare | direct `Execute`, `Assert.AreEqual` | adequate; it covers the path the fix's removed early return used to take |

## Failure classes dismissed
- FC-001, FC-002, FC-003: no predicate changed span or qualifier (*Guard scope*); both sides of "nothing differs" vs "differs" and "tokens only" vs "trivia" are pinned.
- FC-004, FC-005, FC-019, FC-020, FC-021: the rewrite replaces only a newline token's text or an `EndOfLineTrivia`; it joins, moves, or deletes nothing. Directive-ending trivia covered by tests.
- FC-006, FC-022: literals keep their breaks (not trivia or tokens of these kinds); existing `VerifyLineEndingsInside*` and the new identity test cover verbatim, raw, interpolated verbatim, comment, disabled text.
- FC-007, FC-008: one application yields a uniform file, so a second pass finds nothing; Fix All unchanged (`BatchFixer`, existing `GetFixAllScenario`).
- FC-009: the fix is offered only when two distinct endings occur; `DetectEndOfLine` returns `\r\n` or `\n`, so at least one break differs and the rewrite always changes the document.
- FC-010: idempotent by construction — after the rewrite every carrier equals `endOfLine`; LF and CRLF rows exist for every new test.
- FC-011, FC-012, FC-013, FC-014: no loop, no neighboring-phase interaction changed, no analyzer/formatter policy changed.
- FC-015, FC-016: closed for the rewrite; the counting siblings are pre-existing and filed per decision 2.
- FC-017: see *Assertion adequacy*.
- FC-018, FC-023..FC-027, FC-029, FC-036: no rename, identifiers, member kinds, nesting, or key/value layout involved.
- FC-028: the phase still receives `context.EndOfLine`, detected per entry point as before; behavior per root kind unchanged.
- FC-030: the fix rewrites the whole file by design (RH5604-005), unchanged.
- FC-031: the summaries of `NormalizeLineEndings`, `LineEndingUtilities`, `LineEndingNormalizationPhase`, and `ApplyCodeFixAsync` describe the new code; the phase's identity promise is still true.
- FC-032: implementer ran the full analyzer, formatter, core, and architecture suites after the compiled change; cleaner's Release build is after it.
- FC-033: fixtures built byte-exact; the Unicode terminators are `\u` escapes in source.
- FC-034, FC-035: no bug reproduction (structural issue); expected values derive from `main`'s behavior, observed before the implementation commit (02 ran on `158c0ba`).
- FC-037: no comment-only change claimed.
- FC-038..FC-043: no shape declared or check changed; the fix asks no `FindUnsupportedSyntax` (FC-043 does not reach it), and the tests pin that it applies no refusal. FC-040: both former copies and the owner rewrite whatever tree they receive; unchanged.
- FC-044: no guard models a computed result.
- Check 11 (cost of special cases): no special case added; production diff is a net deletion.

## Architecture findings
_None._ The rewrite now has one owner. The remaining three statements of the carrier set are the pre-existing follow-up of decision 2.

## Hints
- `ReihitsuFormatterHelpers.DetectEndOfLine` is a one-line forwarder to `LineEndingUtilities.DetectEndOfLine` that many fixes still call; harmless, not a finding.

## Follow-up candidates
_None_ new (the shared carrier-set definition is already in `00-lead.md`).
