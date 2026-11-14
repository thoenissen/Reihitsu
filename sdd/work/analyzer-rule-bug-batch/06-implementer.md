Role: implementer
Commit: 7d29e2b
Verdict: #1074: tests pass; #1079: tests pass; #1276: tests pass; #1613: tests pass; #1708: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Work follows `00-lead.md` decisions 3, 5 and 6, the changed specs, and `03`/`05-test-implementer.md`.

## thoenissen/Reihitsu#1074

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/FormattingTextAnalysisUtilities.cs` | New `IsInsideLiteralText(SyntaxToken, int)`: a position is literal text only inside the *span* of a string, character, UTF-8, raw, interpolated-text, or interpolated-raw start/end token; the trivia around these tokens is code. Moved here from `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken` (deleted), which classified a token's trivia as content | RH5601-001, RH5601-002, RH5602-001, RH5602-002, SHARED-DIAG-003 |
| same file | `IsInsideInterpolatedString` (any brace under an interpolated string) replaced by `IsInterpolationBrace` (only the interpolation's own `{`/`}`) | RH6009-001/-002, RH6010-001/-002 (decision 6) |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5601UseTabsCorrectlyAnalyzer.cs`, `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5601UseTabsCorrectlyCodeFixProvider.cs` | Call `IsInsideLiteralText`; unused `using` dropped | RH5601-001, RH5601-004 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.cs` | Per-line skip of every line a literal touches (`GetStringLineIndices`) replaced by a per-run check: the run is skipped only when its start lies in literal text (`IsInsideLiteralText`). `GetStringLineIndices` itself is unchanged (RH5204 and others keep using it) | RH5602-001, RH5602-002 |
| `src/Reihitsu.Analyzer/Rules/Spacing/RH6009…Analyzer.cs`, `RH6010…Analyzer.cs` | Exempt only `IsInterpolationBrace` | RH6009-001/-002, RH6010-001/-002 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.cs` | Private `IsOpeningBraceOfInterpolation` deleted; uses `IsInterpolationBrace` (equivalent, the caller filters `OpenBraceToken` first) | RH5025-006 (decision 6) |

### Policy owners
- `rg -n "InterpolatedStringTextToken" src --glob '!*Test*/**'`: the RH5601 predicate (now Core) and formatter `RawStringLineShifts`/`RawStringLineUtilities` (formatter-internal layout decisions, not "is this literal text" for a diagnostic; untouched).
- `rg -n "Parent is InterpolationSyntax" src --glob '!*Test*/**'`: only the RH5025 copy, now deleted. `IsInterpolationBrace` is the one owner for RH5025, RH6009, RH6010.

### Tests run
See *Whole run*. Every #1074 test listed in `03` passes.

### Disputed tests or scope
_None._ Scope note: `IsInsideLiteralText` is a new public Core member with three consumers (RH5601 analyzer, RH5601 fix, RH5602 analyzer); it replaces a public analyzer member with two, so RH5602 does not depend on the RH5601 analyzer type. `IsInsideInterpolatedString` was public in Core and is removed (no other consumer, tests included).

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1079

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5603FileMustNotEndWithANewlineAnalyzer.cs` | Reported only when the file's last character is `\r`/`\n`; walks back over line breaks and whitespace (`SyntaxFacts.IsWhitespace`) to the last non-whitespace character, then starts the tail at the first line break after it, so whitespace-only lines belong to the tail and whitespace behind the last character on its line stays. The fix (unchanged) deletes the span, so one application removes the whole tail | RH5603-001, RH5603-003, RH5603-004 |

### Policy owners
`rg -n "'\\\\r' or '\\\\n'|== '\\\\r'" src/Reihitsu.Analyzer` — no other owner of RH5603's tail; the formatter's FMT-CLEAN-005 is a separate surface and not touched.

### Tests run
See *Whole run*. All RH5603 tests pass, including the lone-`\r` characterization.

### Disputed tests or scope
_None._ As decision 9 records, RH5603 does not report `}⏎···`; the fix now removes the whole tail.

### Architecture findings
_None._

### Follow-up candidates
_None_ beyond `00-lead.md` candidate 1 (unchanged by this work: a tail behind a final directive line or `///` comment is still reported).

## thoenissen/Reihitsu#1276

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5404ElementMustNotBeOnSingleLineAnalyzer.cs` | An `ExtensionBlockDeclarationSyntax` is reported on its `Keyword` (`extension`) instead of its empty identifier | RH5404-001, RH5404-002, SHARED-DIAG-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5404ElementMustNotBeOnSingleLineCodeFixProvider.cs` | No fix registered when the resolved declaration is an extension block (decision 3); since the diagnostic is now on the keyword, the declaration resolves to the block itself, never to another type | RH5404-003 |

### Policy owners
_N/A_ — one analyzer and its fix.

### Tests run
See *Whole run*. All RH5404 tests pass.

### Disputed tests or scope
_None._

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1613

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RawStringLiteralUtilities.cs` | New `GetClosingLineShift(SourceText, SyntaxNode)`: column of the opening quotes (behind the `$`s) minus the whitespace width of the closing line, both on `SourceText.Lines`, so every C# line terminator ends a line; 0 for a one-line literal. Moved from the fix's private `GetShift` | RH5202-001, RH5202-003, SHARED-FIX-008 (decision 5) |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5202…Analyzer.cs` | Both paths (plain/`u8` and interpolated) report when `GetClosingLineShift != 0`; the token-text splitting on `\r\n`/`\n` and `LastIndexOf('\n')` is gone | RH5202-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5202…CodeFixProvider.cs` | Private `GetShift` deleted; uses `GetClosingLineShift`; summaries updated | RH5202-003 |

### Policy owners
`rg -n "GetLeadingWhitespaceLength|GetQuoteOffset" src --glob '!*Test*/**'`: RH5202 analyzer and fix now share `GetClosingLineShift`. The formatter's `RawStringLineShifts` measures the same quantities but after earlier literals moved (`GetShift(int, Func<…>)`), a different decision, and keeps its own; it already uses `SourceText` lines.

### Tests run
See *Whole run*. All RH5202 tests pass, including `VerifyFormatterOutputForLoneCarriageReturnDocumentIsNotReported`.

### Disputed tests or scope
_None._

### Architecture findings
_None_ (decision 5 implemented).

### Follow-up candidates
`00-lead.md` candidate 2 (formatter output of a lone-`\r` document has mixed line endings): the RH5202 analyzer no longer reports that output; whether mixed line endings remain is a formatter question I did not examine.

## thoenissen/Reihitsu#1708

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4105MethodParameterCasingAnalyzer.cs` | `GetLocations` skips a parameter whose identifier has an empty span (unnamed extension receiver), as RH8032 does | RH4105-004 |

### Policy owners
`rg -n "Identifier.Span.IsEmpty" src`: RH8032 uses the same check.

### Tests run
See *Whole run*. Three of four new RH4105 tests pass; one fails as below.

### Disputed tests or scope
- `src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.VerifyUnnamedReferenceReceiverParameterIsNotReported` asserts no diagnostic for `extension(ref int)`, but that input does not compile: the verifier reports `error CS9305: Cannot use modifiers on the unnamed receiver parameter of extension block` at (3,19)–(3,22). RH4105 now reports nothing there, as RH4105-004 says; the test fails only on the compiler error. The test input is invalid C#; an unnamed receiver with a modifier is not code any caller analyzes in a compiling project. Suggest the test implementer drop the `ref` cell (or expect CS9305). Not changed by me.

### Architecture findings
_None._

### Follow-up candidates
_None._

## Whole run

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 14 files changed, 137 insertions(+), 163 deletions(-). No new type. New public members: `FormattingTextAnalysisUtilities.IsInsideLiteralText`, `FormattingTextAnalysisUtilities.IsInterpolationBrace`, `RawStringLiteralUtilities.GetClosingLineShift`; removed: `FormattingTextAnalysisUtilities.IsInsideInterpolatedString`, `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken`.

### Deleted special cases
_N/A._

### Tests run
`scripts/format.sh` over the 14 changed files: formatted 0 of 14.

```shell
scripts/build.sh --configuration Debug                       # Build succeeded, 0 warnings
scripts/test.sh --configuration Debug --project analyzer     # Failed: 1, Passed: 9118 (the disputed RH4105 test)
scripts/test.sh --configuration Debug --project core         # Passed: 651
scripts/test.sh --configuration Debug --project architecture # Passed: 117
```

The formatter project was not run: the formatter's only change is an added Core method it does not call, and the build is clean.
