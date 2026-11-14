Role: skeptic
Commit: 6fb7ee4
Verdict: #1074: proceed; #1079: proceed (premise partly wrong, defect real); #1276: proceed; #1613: proceed; #1708: proceed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issues' own prescribed solutions are treated as proposals below.

Evidence: fixtures in a scratch directory outside the repository, run through `scripts/apply-fix.sh <ID> <dir>` (LF and CRLF arms) and `scripts/format.sh --force <dir>` (CLI, whole-document formatting), on 6fb7ee4. `apply-fix` turns every lone `\r` into `\n` before it runs (`FixtureLineEndings.Normalize`), so lone-`\r` documents cannot be run through it (FC-033); U+2028, U+0085, and U+2029 survive and were used instead for #1613.

## thoenissen/Reihitsu#1074

### Premise

Real, for all four rules. Callers: the analyzers report in any compilation; the CLI formats the same inputs.

| Rule | Fixture | Today (`apply-fix`) | Formatter (`format.sh`) |
|---|---|---|---|
| RH5601 | `string b =⏎→"abc";` | no-diagnostic | tab gone |
| RH5601 | `string c = /** c */→"abc";` | no-diagnostic | refused: unsupported (comment inside a variable declaration, FMT-UNSUP-004) |
| RH5601 | `string f = "a"→+ "b";` / `'a'→+` / `"a"→;` / `@"a"→+` | no-diagnostic (all four) | tab gone |
| RH5601 | `$"{→"x"→}"` | only the first tab reported | both tabs gone |
| RH5601 | `string l =⏎→"""…"""` (multi-line raw) | no-diagnostic | tab gone |
| RH5602 | `"a";··`, `$"a{1}b";··`, `@"a";··`, `"a"u8;··`, `""" … """;··`, `@"a··⏎b";··` | no-diagnostic (all) | trailing whitespace gone; `a··` inside the verbatim literal kept |
| RH5602 | `$@"{1 +··⏎ 2}"`, `$"""⏎ {1 +··⏎ 2}⏎ """`, whitespace-only line in a raw hole | no-diagnostic (all) | removed |
| RH6009 | `{` of array initializer, lambda block, anonymous object, switch expression in a hole; regular, `$@`, `$$"""` | no-diagnostic (all 7) | space written in front of each |
| RH6010 | `}` of array initializer in a hole, also `{ 1}}`, `$$"""{{… { 1}}}"""`, empty `{}` | no-diagnostic (all 6) | space written in front of each |

The issue's claim "in each case the formatter already treats the whitespace as code" holds for every fixture above (SHARED-DIAG-010 is not in the way: these are shapes the formatter changes, so the rules must report them).

### Mechanism and candidates

- **RH5601.** `IsStringContentToken` classifies a tab by the kind of the token whose *full* span holds it (`root.FindToken(index)`), so a tab in the leading or trailing trivia of a string, character, UTF-8, or non-interpolated raw literal token counts as literal text. Dispatch: the token kinds in `RH5601UseTabsCorrectlyAnalyzer.IsStringContentToken` — `StringLiteralToken`, `CharacterLiteralToken`, `SingleLineRawStringLiteralToken`, `MultiLineRawStringLiteralToken`, `Utf8StringLiteralToken`, `Utf8SingleLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken` (fail in trivia); `InterpolatedStringTextToken` (carries no trivia, holds); `InterpolatedMultiLineRawStringStartToken`, `InterpolatedRawStringEndToken` (already span-checked, hold); every other token kind, including `InterpolatedStringStartToken` and `InterpolatedStringEndToken`, is code today (`$"a"→+` is reported). The code fix registers through the same predicate, so analyzer and fix have one owner.
- **RH5602.** The analyzer skips every line that the span of a string literal or an interpolated string expression intersects (`FormattingTextAnalysisUtilities.GetStringLineIndices`), so whitespace at the end of such a line is never reported, also where it lies behind the literal's end or inside a hole. Dispatch: `GetStringLineIndices` switch — `LiteralExpressionSyntax` of kind `StringLiteralExpression` or `Utf8StringLiteralExpression` (regular, verbatim, raw single- and multi-line, UTF-8), and every `InterpolatedStringExpressionSyntax` (regular, verbatim, raw; holes included). Character literals are not tracked and are reported today (`char c = 'a';··` fixed).
- **RH6009 / RH6010.** `FormattingTextAnalysisUtilities.IsInsideInterpolatedString` exempts every brace token that has an `InterpolatedStringExpressionSyntax` ancestor, so the braces of code inside a hole are exempt together with the interpolation's own braces. Dispatch: every `OpenBraceToken` / `CloseBraceToken`; inside a hole the brace owners are `InitializerExpressionSyntax` (array, object, collection), `AnonymousObjectCreationExpressionSyntax`, `BlockSyntax` (lambda, anonymous method), `SwitchExpressionSyntax`, `PropertyPatternClauseSyntax`, `WithExpression` initializers, and the `InterpolationSyntax` of a nested interpolated string (its own braces, must stay exempt).

### Breaking inputs

Failure classes: FC-002, FC-003, FC-016, FC-022, FC-027, FC-033, FC-040.

RH5601 matrix — axes: trivia side (leading / trailing) × literal kind × context (code / hole):

| Cell | Fixture | Today |
|---|---|---|
| leading, string, code, line start | `string b =⏎→"abc"` | fails |
| leading, string, code, behind `/** */` | `/** c */→"abc"` | fails (FC-040: with `DocumentationMode.Parse`/`Diagnose` the `/** */` and the tab are the literal's leading trivia; with `None` they are the `=`'s trailing trivia, like `/* c */→"abc"`, which is reported today) |
| leading, multi-line raw | `string l =⏎→"""…` | fails |
| trailing, string / char / verbatim | `"a"→+`, `'a'→+`, `@"a"→+`, `"a"→;` | fails |
| trailing, string, hole | `$"{"x"→}"` | fails |
| trailing, UTF-8, single-line raw, multi-line raw closing `"""→;` | not run; same predicate branch as the failing cells | fails by the code path |
| tab in the previous token's trailing trivia | `=→"abc"`, `=→'d'`, `=> →"a"u8`, `/* c */→"abc"`, `$"{→1→}"`, `"a"; →// x` | holds |
| trailing, interpolated string end token | `$"a"→+` | holds |

Content side, must stay unreported (FC-003): a tab inside the text of each literal kind and comment, inside the closing line and after the opening quotes of an interpolated raw string — covered by existing tests (`VerifyNoDiagnosticsForTabIn…`, `VerifyCodeFixLeavesTheTabsOfAnInterpolatedRawStringUntouched`).

RH5602 matrix — axes: where the run is (behind a literal's end on its last line / end of a hole line / whitespace-only line in a hole) × string kind (regular, verbatim, UTF-8, raw single-line, raw multi-line closing line, interpolated regular/verbatim/raw):

- Fails today, every cell run: `"a";··`, `$"a{1}b";··`, `@"a";··`, `"a"u8;··`, `""" … """;··`, `@"a··⏎b";··` (second line), `$@"{1 +··`, `$"""` hole `{1 +··`, whitespace-only line in a `$"""` hole. A regular `$"…"` hole that spans lines (C# 11) is a further cell, not run.
- Holds (content, must stay unreported): `@"a··⏎` (fixture `g`, not reported today and formatter keeps it), whitespace behind a hole's `}` on a raw content line (existing test `VerifyNoDiagnosticsForTrailingWhitespaceBehindAHoleInAnInterpolatedRawString`), interpolated verbatim text (existing test). Not covered by an existing test, and needed on the content side of the new qualifier: whitespace after the opening `"""` / `$"""` of a multi-line raw literal (part of the start token's text), a whitespace-only content line of a multi-line raw or verbatim literal.
- Holds today and must keep holding: `string s =··⏎"a";` (line does not intersect the literal, reported).

RH6009/RH6010 matrix — axes: brace owner in the hole × string kind (regular, `$@`, `$$"""`) × single- or multi-line hole. All seven RH6009 and six RH6010 cells run fail today. The side that must stay exempt (FC-003): the interpolation's own braces, also of a nested interpolated string (`$"{$"{1}"}"`), and the `}}` of a `$$` raw interpolation directly behind an initializer's `}` (`$$"""{{new int[] { 1 }}}"""`, formatter output, parses).

Notes for the test implementer:
- The spec example `string c = /** c */→"abc";` is a field with a comment inside its declaration, which the formatter refuses as unsupported (FMT-UNSUP-004). Fine for an analyzer test; a formatter-parity test (`RH5601UseTabsCorrectlyFormatterTests`) on that input gets the input back unchanged. Use a statement or an argument for parity.
- Run the `/** */` cell under the documentation mode the harness uses and say which (FC-040).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH5601UseTabsCorrectlyAnalyzer` (`IsStringContentToken`) | must change |
| `RH5601UseTabsCorrectlyCodeFixProvider` | must stay; it calls `IsStringContentToken` and follows it |
| `RH5602CodeMustNotContainTrailingWhitespaceAnalyzer` | must change |
| `FormattingTextAnalysisUtilities.GetStringLineIndices` / `GetNonFormattableLineIndices` (`Reihitsu.Core`) | must stay: also used by RH5204, RH5023, RH6004, RH8301, RH8302, RH8303; changing the helper changes those rules |
| `RH5602…CodeFixProvider` | must stay (deletes the reported span) |
| `RH6009…Analyzer`, `RH6010…Analyzer` | must change |
| `FormattingTextAnalysisUtilities.IsInsideInterpolatedString` | used only by RH6009 and RH6010; may change or go |
| `RH6009`/`RH6010` code fixes | must stay (insert one space at the diagnostic) |
| Formatter | not involved (already treats all of it as code) |
| Rule pages | not involved (spec author's reading confirmed) |
| Tests | existing RH5601/RH5602/RH6009/RH6010 tests asserting "not reported" were checked; none asserts a case the new spec reports |

### Refuse option

Refusing means leaving these runs unreported, which is what SHARED-DIAG-003 forbids; there is no fix to withhold, and the cases are common code (`"a";··` is ordinary trailing whitespace). The requested change is small per rule: one predicate for RH5601 (position instead of token kind), one per-run check instead of a per-line skip for RH5602, one narrower predicate for RH6009/RH6010. Declaring a shape unsupported is not an option: the formatter handles every one of these inputs, so there is no formatter special case to delete. _No simpler option._

### Output questions

_None._ Every output follows SHARED-DIAG-003 (maintainer's P5) and the triage comment on the issue.

### Internal choices

- RH5602: the change can stay inside the analyzer instead of changing `GetStringLineIndices`, which six other rules consume. Recommendation: do not change the shared helper's behavior.
- RH6009/RH6010: `RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.IsOpeningBraceOfInterpolation` already decides "is this the interpolation's own brace" privately. A third private copy for RH6009/RH6010 would be FC-015. Recommendation: one predicate that RH6009, RH6010, and RH5025 share, or reuse; the lead decides whether touching RH5025 is in scope.

### Issue goals

- "Done when: the rules report and fix these cases while still leaving literal text untouched, with regression tests for each input" — keep.
- The body's lane line ("change doc required") was superseded by the triage comment; nothing else to drop.

### Architecture findings

_None._ (Each rule's analyzer and fix share one decision; the RH5025 copy is a hazard, listed above, not a defect of the run.)

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1079

Scope: the RH5603 part only (triage comment); the other instances are #1444.

### Premise

The defect is real; the issue's account of it is partly wrong.

- `class C⏎{⏎}⏎···⏎` → RH5603 fix (`apply-fix`): `}⏎···` (LF and CRLF). Reproduced.
- "…which RH5603 reports again" — false. `class C⏎{⏎}⏎···` (no final break) is `no-diagnostic` for RH5603: the analyzer walks back over `\r`/`\n` only and stops at the spaces. What reports `···` is RH5602 (fixed to `}⏎`), after which RH5603 reports `⏎` again. So the tail is removed in three applications by two rules.
- The real defect: the RH5603 diagnostic covers only the line breaks behind the last whitespace-only line, so one application leaves a tail the formatter removes as a whole (FMT-CLEAN-005: `format.sh` turns all of `a`, `c`, `d` below into `class C;` with no tail). The maintainer decided the result (fix removes the whole tail); the spec author's rewrite follows that decision, not the "reported again" claim.
- Also observed: RH5602 reports the `···` line of `}⏎···⏎` as well (fixed to `}⏎⏎`). Overlapping diagnostics of two rules on one line; both orders converge. Not a defect.

### Mechanism and candidates

The analyzer's tail starts after the last character that is not `\r` or `\n`, so whitespace-only lines inside the tail end it early: whitespace before a line break is taken for the file's last character. Dispatch: the backward loop in `RH5603FileMustNotEndWithANewlineAnalyzer.OnSyntaxTree`; the fix deletes exactly the diagnostic span.

### Breaking inputs

Failure classes: FC-002, FC-003, FC-007, FC-013, FC-017.

Matrix — axes: what precedes the tail × what the tail holds × line ending:

| Cell | Fixture | Today (RH5603) |
|---|---|---|
| code token, `⏎···⏎` | `a_ws_line` | fails: `}⏎···` |
| code token, `⏎··⏎→⏎⏎` | `d_two_ws_lines` | fails: `}⏎··⏎→` |
| code token, `··⏎` (whitespace behind last char) | `b_trailing_ws_on_last` | holds: `}··` (spec keeps it) |
| code token, `⏎⏎` | `g_plain` | holds: `}` |
| code token, `⏎···` (no final break) | `c_ws_line_no_final_break` | not reported (RH5602's, spec non-goal) |
| code token, `··⏎···⏎` (both) | not run | the spec's two examples combined; should give `}··` — needs a test (FC-003) |
| directive line, `#endif⏎` | `e_directive_end` | reported and fixed — see follow-up 1 |
| `///` comment, `/// x⏎` | `f_doc_comment_end` | reported and fixed — see follow-up 1 |
| `//` and `/* */` comment | existing tests | holds |
| line endings LF, CRLF | every fixture, both arms | as above in both |
| lone `\r` | not runnable through `apply-fix` | analyzer loop handles `\r`; cover in a unit test |

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH5603FileMustNotEndWithANewlineAnalyzer` | must change (where the tail starts) |
| `RH5603…CodeFixProvider` | must stay (deletes the diagnostic span) |
| RH5602 analyzer and fix | must stay |
| Formatter (FMT-CLEAN-005) | not involved |
| Rule page | not involved |

### Refuse option

Withhold the RH5603 fix when the tail holds a whitespace-only line. Cost: a guard of about the same size as the requested change, and the diagnostic then stays without a fix for a shape editors produce; the requested change (start the tail at the last non-whitespace character) is a few lines in one loop and is decided by the maintainer. Declaring a shape unsupported: not an option (the formatter handles it; nothing to delete). _No simpler option._

### Output questions

_None._ The maintainer decided the fix result (issue comment of 2026-10-18); the diagnostic span follows from it.

### Internal choices

- The spec keeps whitespace behind the last character on its line out of the tail (`}··⏎` → `}··`), as today. Recommendation: keep it; including it would make RH5603 delete what RH5602 owns.
- The spec's RH5603-001 qualifier ("a code token … or a `//` or `/* */` comment that is not on a directive line") was already in the spec before 7d38931, and the analyzer has never implemented it (see follow-up 1). Recommendation: keep today's behavior for a tail behind a directive or `///` in this run, so no visible output outside the maintainer's decision changes, and file the follow-up.

### Issue goals

- Drop the claim "which RH5603 reports again": RH5603 does not report `}⏎···`. The goal (whole tail in one application) stays. No visible output changes by dropping the claim.

### Architecture findings

_None._

### Follow-up candidates

1. **RH5603 · File must not end with a newline — line break behind a final directive or `///` comment** (outside the run's issues; reachable through any file the CLI or an IDE analyzes).

   Input:
   ```csharp
   class C;
   #if X
   #endif
   ```
   (file ends with a line break after `#endif`; the same holds for a file ending in `/// x` and a line break)

   Before (`main`): RH5603 is reported on the final line break and its fix deletes it. The formatter keeps the file as it is, with or without that line break (two passes, `format.sh`), so formatter output is reported (FC-013). RH5603-001's qualifier excludes a directive line and a `///` comment, so the spec and the analyzer disagree.

   After, A (recommended): not reported.

   After, B: reported as today, and the formatter removes the line break behind a final directive or `///` comment.

   Why A: it is what RH5603-001 already says, and the formatter keeps both forms, so a report there is a diagnostic the formatter never clears.

2. A file ending in U+2028, U+2029, or U+0085 is not reported by RH5603 (the loop knows only `\r` and `\n`; the spec names only `\n` and `\r\n`). Rare; the lead decides whether it is worth an issue.

## thoenissen/Reihitsu#1276

Scope: the location bug only (triage comment); the P11 widening and the formatter's extension-block braces are #1446.

### Premise

Real. `ExtensionBlockDeclarationSyntax` is a `TypeDeclarationSyntax` without an identifier; the analyzer reports `declaration.Identifier.GetLocation()`, which for it has no source position. The code fix then resolves the declaration from position 0 (`root.FindToken(span.Start)…FirstAncestorOrSelf<BaseTypeDeclarationSyntax>()`), i.e. from the file's first token.

`apply-fix RH5404` (LF and CRLF alike):

| Fixture | Result |
|---|---|
| `static class X⏎{⏎    extension(int value) { public int Doubled => value * 2; }⏎}` | "fixed": the fix formatted the whole class `X` (the type holding the first token), giving `extension(int value) {⏎        public int Doubled => value * 2; }` |
| `class A { }` first, then `X` as above | no-progress: the fix formatted `class A` into `class A;`, the extension block stays |
| `using System;` first | no-fix-offered (first token is no type) |
| `extension(int value) { }` (empty), `extension<T>(T[] values) { }` | no-progress |

### Mechanism and candidates

A diagnostic located on the type's identifier has no source position for a declaration without one, and the fix then acts on whatever type holds the file's first token. Dispatch: the kinds registered in `RH5404ElementMustNotBeOnSingleLineAnalyzer.Initialize`; of these only `ExtensionBlockDeclaration` has no identifier (`UnionDeclaration` is not registered).

### Breaking inputs

Failure classes: FC-007, FC-009, FC-030, FC-027.

Matrix — axes: block body (empty / one member / several) × position (first type in file / after another type / after a using) × containing type on one line or not × generic or not. Every cell run fails (table above). Cells the tests must cover besides the spec's: an empty one-line extension block; an extension block in a containing type that is itself on one line (two diagnostics: `X` keeps its fix and the block gets none; not run); another type first in the file (the case where today's fix edits an unrelated type).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH5404ElementMustNotBeOnSingleLineAnalyzer` | must change (location on `extension`, SHARED-DIAG-007) |
| `RH5404…CodeFixProvider` | must change (withheld for an extension block, RH5404-003 as written) |
| Formatter | not involved (#1446) |
| Rule page | not involved; it already says extension blocks are expanded by hand |
| `VerifySingleLineExtensionBlockIsFlagged` | must change (expected location) |

### Refuse option

The spec's choice is the refuse option: withhold the fix for an extension block. It costs one cheap check of the declaration kind; the alternative (format the block) cannot clear an empty block and leaves `extension(…) {⏎ member; }` for a non-empty one, a layout the rule page calls wrong. One-line extension blocks are rare (C# 14 feature, one-line bodies). Declaring a shape unsupported: not an option (no formatter special case exists). _No simpler option._

### Output questions

_None._ Withholding a fix for a rare class puts nothing in question, matches the published rule page, and is recorded as decision 3. Note for the lead: the issue comment of 2026-10-22 quotes a maintainer decision "reported at its keyword and the block is formatted"; the triage comment of 2026-10-24 moved the formatting part to #1446, and today's "formatting" is of a different type, so nothing the maintainer decided is lost — #1446 is where the fix gains the block.

### Internal choices

- RH5404-003's `Why:` says formatting the block "would leave the diagnostic in place". That holds for an empty block only: for a non-empty block, formatting moved the member and the closing brace to another line (fixture 1), which clears RH5404 while leaving the braces on the header and member lines. Recommendation: the spec author rewords the sentence (the formatter does not place an extension block's braces on their own lines, so an empty block stays on one line and other blocks keep their braces on the lines of their header and last member); withholding stays. Internal, no output change.

### Issue goals

- "the block is formatted" — dropped for this run (moved to #1446 by triage; the fix is withheld). Changes visible output (no fix offered instead of a fix on another type); covered by decision 3 and the rule page.
- `class J { int _y; int _z;⏎}` reported — moved to #1446; not in this run.

### Architecture findings

_None._

### Follow-up candidates

_None._ (#1446 already holds the rest.)

## thoenissen/Reihitsu#1613

### Premise

Real, also for U+2028, U+0085, and U+2029 inside the literal, in an otherwise LF/CRLF document. `apply-fix RH5202`, 15 fixtures × 2 arms, all `no-progress`:

- aligned plain, `u8`, and interpolated literals (opening quotes and closing quotes in column 16, or 17 behind `$`): reported, the fix changes nothing;
- misaligned plain and interpolated literals: the fix aligns them in one application (diff shows the shift to column 16 / 17), the analyzer keeps reporting.

The issue's own example input is misaligned (opening quotes in column 12, closing in column 8), so for it the report is correct and the fix would shift; the spec author used a genuinely aligned literal instead. Lone-`\r` documents could not be run here (`apply-fix` normalizes `\r`); the analyzer code takes the same path for them (`Split(["\r\n","\n"])` / `LastIndexOf('\n')`). The analyzer test base can hold a lone-`\r` source (`ConvertEndOfLine(…, "\r")` is used in RH8304 and RH3004 tests); the reproduction must use it, not the default LF/CRLF arms (FC-033).

Formatter: `format.sh` on a lone-`\r` document writes `\n` outside the literals, keeps the literals' lone `\r`, and aligns both literals by `SourceText` lines (16 and 17). By the analyzer's code path, that output is reported (FC-013) — derived from the code, not run.

### Mechanism and candidates

The analyzer finds the closing line of the literal in the token text, splitting only on `\r\n`/`\n` (plain, `u8`) or searching only for `\n` (interpolated), while the fix and the formatter take it from `SourceText.Lines`, which knows every C# line terminator; the opening column is taken from `SourceText` in both. Dispatch: `OnStringLiteralExpression` (`MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`) and `OnInterpolatedStringExpression` (`InterpolatedMultiLineRawStringStartToken`).

### Breaking inputs

Failure classes: FC-007, FC-013, FC-015, FC-017, FC-022, FC-033.

Matrix — axes: literal kind (plain, `u8`, interpolated with one or more `$`) × last line break inside the literal (`\r\n`, `\n`, lone `\r`, U+0085, U+2028, U+2029) × aligned / misaligned:

- `\r\n`, `\n`: hold (existing tests).
- U+0085, U+2028, U+2029: every cell run fails (aligned: false report; misaligned: never clears).
- lone `\r`: fails by the code path; must be reproduced in a unit test, in a document of all lone `\r` and in the formatter's mixed output (LF outside, lone `\r` inside the literal).
- Interpolated with `$$`: not run; same code path as `$`.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH5202…Analyzer` | must change |
| `RH5202…CodeFixProvider` | must stay (already measures by `SourceText.Lines`) |
| `RawStringLiteralUtilities` (`Reihitsu.Core`) | possibly involved, see architecture finding |
| Formatter `RawStringLineShifts` | must stay (already by `SourceText.Lines`) |
| Rule page | not involved |

### Refuse option

Withholding the fix does not help: the false report on an aligned literal is the analyzer's. Leaving it as is ("no change") is the only refusal; the class is rare (lone-`\r` documents, Unicode line terminators inside raw literals), but it includes the formatter's own output for a lone-`\r` document, and the analyzer change is small. Declaring a shape unsupported: not an option (analyzer-side). _No simpler option._

### Output questions

_None._

### Internal choices

_None._

### Issue goals

- "tests … carry the RH5202-003 tag": the aligned-literal (not reported) tests belong to RH5202-001; only the fix tests are RH5202-003. Change; no visible output.
- "Affected: one shared way of finding the closing line" is a prescribed solution; see the architecture finding. The lead decides.

### Architecture findings

The analyzer and its code fix own the same decision — where the closing line of a multi-line raw string starts and how wide its whitespace is — in two statements: the analyzer from the token text with its own line splitting, the fix (and the formatter's `RawStringLineShifts`) from `SourceText.Lines`. Inputs that show them disagreeing: any multi-line raw literal whose last line break is a lone `\r`, U+0085, U+2028, or U+2029.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1708

### Premise

Real. `apply-fix RH4105`: `extension(long)`, `extension<T>(T[])`, `extension(int)`, `extension(ref int)` are reported, and no fix is offered (`no-fix-offered`); `extension(int Value)` is reported and fixed to `value`. `GetLocations` yields `parameter.Identifier` for every `ParameterSyntax` whose `ValueText` is not `_`, so an unnamed receiver yields an empty name and a location without a source position. The issue's "twice per block" count was not verified (`apply-fix` does not count diagnostics); it does not change the defect.

### Mechanism and candidates

A parameter without an identifier is checked for casing with an empty name and reported on its missing identifier. Dispatch: `SyntaxKind.Parameter` (`CasingAnalyzerBase` registration); the only parameters without a name that compile are extension receivers. Sibling rules on `SyntaxKind.Parameter` run over the same fixtures: RH4120 and RH5111 report nothing; RH4122/RH4123/RH4124 have no fix provider so `apply-fix` cannot run them, and by code they require a one-letter name, which an empty name is not. RH8032 already skips `Identifier.Span.IsEmpty` (the issue says it was fixed separately).

### Breaking inputs

Failure classes: FC-023, FC-027.

Members: receiver type plain (`long`, `int`), array of a type parameter (`T[]`), with `ref` (run, fails), with `ref readonly`/`scoped`/attributes/nullable/tuple types (not run, same path). Must stay reported: a named receiver not in camelCase (`Value`, run, fixed).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `RH4105MethodParameterCasingAnalyzer` | must change |
| `RH4105…CodeFixProvider` | must stay |
| Rule page | not involved |

### Refuse option

Nothing to refuse: the defect is a report without a location, and not reporting is the change itself (one guard). Declaring a shape unsupported: not an option. _No simpler option._

### Output questions

_None._ RH4105-001 reports a name that is not in camelCase; a parameter without a name has none, so "not reported" is the only reading. SHARED-DIAG-007's fallback to the `extension` keyword applies to rules that report a nameless declaration, not to a rule whose subject is the name.

### Internal choices

_None._

### Issue goals

Keep all.

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

- No requirement of the run asks a node-level formatter entry point to predict a phase's decision: the only fix that calls one (RH5404) is withheld for the new case.
- Lane: every change named above is shipped source under `src/` without a change doc → bug fix, as claimed.
