Role: auditor
Commit: ff6c88c6ebf0e298d7813183a7ab04aa11e4e45e
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Findings #1 and #2 are confined to text that no build or diagnostic reads: #1 is an XML `<param>` description of a Core helper (not public API documentation of a shipped assembly's user surface, not rule metadata), #2 is a line in `00-lead.md`.

## thoenissen/Reihitsu#1074

### Scope
Full audit. Head ff6c88c, base origin/main f81cb53 (`git ls-remote` agrees; merged). Production files: `FormattingTextAnalysisUtilities.cs` (`IsInsideLiteralText`, `IsInterpolationBrace`; `IsInsideInterpolatedString` removed), RH5601 analyzer and fix, RH5602 analyzer, RH6009/RH6010 analyzers, RH5025 analyzer (decision 6).

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | low | `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` `IsInsideLiteralText`, `<param name="position">Position of the tab</param>` | FC-031 | – | PR-introduced | The helper moved from RH5601 to Core and now also serves RH5602, which passes the start of a whitespace run; the parameter still describes a tab | "Position to classify" or similar |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whitespace/tab is literal text (RH5601, RH5602) | `token.Span` of the token whose full span holds the position, for string, char, UTF-8, raw (single/multi-line, UTF-8), interpolated text, interpolated multi-line raw start/end tokens; trivia never | SHARED-DIAG-003 (literal text only); RH5601 fix uses the same helper; formatter cleanup/tab phases touch trivia only | Agree. Boundaries: position == `Span.End` (whitespace directly behind a literal) is code — tested (`VerifyTabBehindAStringLiteral…`, `VerifyWhitespaceBehindAStringLiteral…`); position in front of `Span.Start` is code — tested (`VerifyTabStartingALineInFrontOfAStringLiteral…`). Token kinds left out (`InterpolatedStringStartToken`, `InterpolatedVerbatimStringStartToken`, `InterpolatedSingleLineRawStringStartToken`, `InterpolatedStringEndToken`, single-line `InterpolatedRawStringEndToken`) can hold no whitespace in their text | ok |
| RH5602 per-run instead of per-line skip | start of the trailing run only | — | A run cannot start in a token's text and continue into trivia on the same line: every literal token whose text ends a line ends with its line break, and trivia cannot precede a text token inside a string. Constructed `$"""…{x}··⏎` and `$@"a{x}··⏎b"`: not reported (`apply-fix`, LF/CRLF) | ok |
| Brace is the interpolation's own (RH6009, RH6010, RH5025) | `token.Parent is InterpolationSyntax` and token is its Open/Close brace | RH5025's deleted private copy (open brace only; caller pre-filters `OpenBraceToken`, so equivalent); formatter `RawStringLineShifts` private open-brace check (different component, layout) | Agree; nested interpolation braces and `$$` `{{`/`}}` stay exempt (tested) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Position lies in literal text | `FormattingTextAnalysisUtilities.IsInsideLiteralText` (RH5601 analyzer, RH5601 fix, RH5602 analyzer); `GetStringLineIndices`/`GetNonFormattableLineIndices` (RH5204, RH5023, RH6004, RH8301–RH8303: line-level skips for other decisions, unchanged) | one owner for this decision; ok |
| Interpolation's own brace | `FormattingTextAnalysisUtilities.IsInterpolationBrace` (RH5025, RH6009, RH6010); `RawStringLineShifts` line 267 (formatter-internal) | ok; see Hints |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5601 `VerifyEveryTabOfTheRequirementExampleIsReported` and the `…IsDetectedAndFixed` family | every tab in code, literal-adjacent and in holes, reported and replaced | missing/extra diagnostic or different fixed text | `Verify(source, fixed)` (exact text, Roslyn iteration check); CRLF arms on two | ok |
| RH5601 `/** */` in `Diagnose`/`Parse`/`None` | outcome independent of documentation mode (FC-040) | one mode not reporting | `Verify` + mode transforms | ok |
| RH5602 example + literal-kind family, `VerifyNoDiagnosticsFor…` content side | runs behind literal ends and in holes reported; literal text not | as above | `Verify(source, fixed)`; FC-003 both sides | ok |
| RH6009/RH6010 hole families, nested and `$$` exemptions | code braces in holes checked; interpolation braces exempt | as above | `Verify`, `MarkupMode.None` with explicit spans for `$$` | ok |
| Formatter parity (four rules) | analyzer reports what the formatter changes, formatter output clean | analyzer step or formatter output differs | `VerifyFormatter` (LF+CRLF, 2 passes) | ok |

### Visible output
Every new expected value is demanded by SHARED-DIAG-003 (unchanged); the -001 rewrites only remove exemptions that contradicted it (triage comment on the issue, decision 4). Rule pages unchanged and still true (RH6009/RH6010 pages exempt only `{value}`-style braces).

## thoenissen/Reihitsu#1079

### Scope
Full audit of `RH5603FileMustNotEndWithANewlineAnalyzer`; fix unchanged (deletes the span).

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `sdd/work/analyzer-rule-bug-batch/00-lead.md`, `## Maintainer answers` ("none yet") | — (check 10, record only) | – | same requirement | The new RH5603 expected values (span from the first line break after `}` through `···`, fix result `}`; `}··⏎···⏎` → `}··`) are not demanded by an unchanged requirement: old RH5603-001 did not report `}⏎···⏎` at all, and SHARED-FIX-001 was not violated by the old fix (RH5603 does not report `}⏎···`). They rest on the maintainer's decision posted on the issue (comment of 2026-10-18 by the owner account, "decided in question round 2 (maintainer)"; verified). That answer is cited in 01/02 and decision 9 but not recorded under `Maintainer answers` | Record the 2026-10-18 answer under `Maintainer answers` and in the run's note; the PR states that its premise ("RH5603 reports again") was wrong and the decided result stands |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Where the reported tail starts | walk back over `\r`, `\n`, `SyntaxFacts.IsWhitespace`; forward to the first `\r`/`\n` | FMT-CLEAN-005 (formatter removes the whole tail); RH5602 owns whitespace behind the last character | Agree. The forward loop always terminates (last char is a line break, checked first). Constructed: `}⏎→⏎·⏎` → `}` in one iteration; whitespace-only file `⏎···⏎` → empty file in one iteration (LF/CRLF) — harmless | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| End-of-file tail | RH5603 analyzer only; formatter FMT-CLEAN-005 is the counterpart surface | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyOneApplicationOfTheFixDeletesTheWholeTail` | one application removes the tail, exact text, LF and CRLF | `}⏎···` left | `ApplyCodeFixAsync` once + `Assert.AreEqual` | ok (FC-017) |
| `…TrailingLineBreaks…`, `…CarriageReturnLineFeeds…`, `…CarriageReturns…` (lone `\r`, untagged), `…Several…`, `…BehindAFinalComment…` | span and fix per spec | wrong span start | `Verify(source, fixed)` | ok |
| `VerifyWhitespaceBehindTheLastCharacterStays…` (both) | FC-003 other side | `··` deleted | `Verify(source, fixed)` | ok |
| Formatter `VerifyFormatterRemovesATailWithAWhitespaceOnlyLine` | parity | span or output differs | `VerifyFormatter` | ok |

## thoenissen/Reihitsu#1276

### Scope
RH5404 analyzer (location) and fix (withheld for extension blocks).

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Fix withheld | declaration resolved from the diagnostic start is `ExtensionBlockDeclarationSyntax` | analyzer locates the block on `Keyword`, so the fix resolves the block itself, never another type | Agree; the containing type's fix stays (tested both actions) | ok |

### Policy ownership
_N/A — one analyzer and its fix._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifySingleLineExtensionBlockIsFlagged`, `…Empty…`, `…Generic…`, `…BehindAnotherType…`, `…BehindAUsingDirective…` | location on `extension`, no action for any diagnostic | `Location.None` or any registered action | `VerifyNoCodeFixIsOffered` (asserts exact diagnostics and zero actions) | ok |
| `VerifySingleLineTypeHoldingASingleLineExtensionBlockKeepsItsFixWhileTheBlockGetsNone` | type keeps its fix, block none | either list wrong | `GetCodeFixActionsAsync` | ok |

### Visible output
Location demanded by SHARED-DIAG-007 (unchanged); withholding the fix is a rare-class refusal that the published rule page already states.

## thoenissen/Reihitsu#1613

### Scope
`RawStringLiteralUtilities.GetClosingLineShift`; RH5202 analyzer and fix.

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Closing-line shift | `SourceText.Lines` of `literal.Span.Start` / `.End`, quote offset behind `$` | fix (same helper); formatter `RawStringLineShifts` (same Core primitives on `SourceText` lines, shift measured against the moved column) | Agree. For LF/CRLF the new closing line equals the old token-text segment (same start), and the opening column is computed as before (tabs count one char in both), so no LF/CRLF output changes | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Opening column minus closing whitespace | `GetClosingLineShift` (RH5202 analyzer, fix); `RawStringLineShifts` ctor stores the same parts for a different decision | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| lone-`\r` tests | aligned not reported; misaligned fixed in one application, `\r` kept | report on aligned; second iteration | `Verify(ConvertEndOfLine(…,"\r"))`, `VerifyCodeFixWithEndOfLine` (one `ApplyCodeFixAsync` + exact text) | ok |
| U+0085/U+2028/U+2029 families | same, per terminator | as above | `Verify(source, fixed)` | ok |
| `VerifyFormatterOutputForLoneCarriageReturnDocumentIsNotReported` | formatter output clean (FC-013) | report | `Format` + `Verify` | ok |

## thoenissen/Reihitsu#1708

### Scope
`RH4105MethodParameterCasingAnalyzer.GetLocations` guard.

### Findings
_None._

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Parameter has no name | `Identifier.Span.IsEmpty` | RH8032's identical check | Agree; named receivers still reported and fixed (tested) | ok |

### Policy ownership
_N/A — one guard, same check as RH8032._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4105-004 tests (three) | unnamed receiver not reported; named neighbor reported and renamed | any extra diagnostic | `Verify` (exact diagnostics) | ok |

## Whole run

### Failure classes dismissed
- FC-004, FC-005, FC-019, FC-020, FC-021: no change moves or joins code; the fixes changed only by reach (RH5601 replaces a tab, RH5602/RH5603 delete whitespace) touch whitespace outside comments and directives (`IsInsideCommentOrDisabledText` unchanged). A directive cannot stand in a hole.
- FC-006, FC-022: no rewrite of literal text; RH5601/RH5602 now act only outside token spans; RH6009/RH6010 insert a space in hole code, which does not change a literal's value.
- FC-007, FC-008, FC-009: checked per issue above; RH5603 and RH5202 converge in one application (tests and constructed fixtures); RH5404 offers no action for the block.
- FC-010–FC-012, FC-014, FC-028, FC-029, FC-038–FC-043: no formatter or precheck change.
- FC-013: skeptic's formatter runs show the formatter changes every newly reported shape; parity tests added for all five rules where layout is involved.
- FC-018, FC-023, FC-036: RH4105 change only removes a report.
- FC-024–FC-027: RH5404 extension blocks (generic, empty, nested in one-line type) covered; other kinds unchanged.
- FC-030: RH5404 fix now formats only the type that holds the diagnostic.
- FC-032: implementer's full analyzer run postdates the production change; later commits are test/doc only.
- FC-033: lone-`\r` cases run through unit tests, not `apply-fix`.
- FC-034, FC-035: reproductions use the issues' shapes (RH5202 uses a genuinely aligned literal, as the issue's was misaligned); expected values come from the specs.
- FC-037: not a text-only claim.
- FC-042: no shape declared.

### Scope and lane
`scripts/lane-check.sh bug-fix`: confirmed. RH5025 edit is decision 6 (deletes the private copy). No `TODO`, no tracker numbers in changed source.

### Architecture findings
_None._ Decision 5 (one closing-line measure) and decision 6 (one interpolation-brace predicate) are carried out.

### Hints
- `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs:267` keeps a private "opening brace of an interpolation" check now that `FormattingTextAnalysisUtilities.IsInterpolationBrace` exists; formatter-internal, not part of this run.
- The three new public Core members (`IsInsideLiteralText`, `IsInterpolationBrace`, `GetClosingLineShift`) are covered only through analyzer tests, not by `Reihitsu.Core.Test`; no convention requires more.
