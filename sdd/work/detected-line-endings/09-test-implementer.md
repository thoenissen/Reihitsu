Role: test implementer
Commit: e4f90d6 (tests written on top of it; see the commit carrying this file)
Verdict: #1695: nothing in this round; #1696: nothing in this round; #1707: tests written (repair round 1, finding #1 of 08-auditor.md); #1721: nothing in this round

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from SHARED-FIX-003 (the result must not gain a compile error or change meaning), SHARED-FIX-006 (format only the replaced node; keep the author's spacing around it), RH3003-004 (replace with `string.Empty`, keep the literal's trivia), and Decision 10 (one space next to a word token; none next to punctuation). None comes from a patched copy of the code.

## thoenissen/Reihitsu#1695

Nothing in this round (audit: pass).

## thoenissen/Reihitsu#1696

Nothing in this round (audit: pass).

## thoenissen/Reihitsu#1707

Eleven new tests in `src/Reihitsu.Analyzer.Test/Clarity/RH3003UseStringEmptyForEmptyStringsAnalyzerTests.cs`. Each runs under LF and CRLF through `VerifyCodeFixWithEndOfLine`, which gives 22 cases. They cover finding #1's failing members on both sides of the literal, both sides at once, and the holding members with punctuation neighbours.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixOfLiteralDirectlyBehindReturnSeparatesTheReplacementFromTheKeyword` (`return""` → `return string.Empty`) | SHARED-FIX-003, RH3003-004 | `VerifyCodeFixWithEndOfLine` (LF, CRLF) | fails (`returnstring.Empty`) |
| `FixOfLiteralDirectlyBehindYieldReturnSeparatesTheReplacementFromTheKeyword` | SHARED-FIX-003, RH3003-004 | same | fails (`yield returnstring.Empty`) |
| `FixOfLiteralDirectlyBehindTheInKeywordOfAForEachSeparatesTheReplacementFromTheKeyword` | SHARED-FIX-003, RH3003-004 | same | fails (`instring.Empty`) |
| `FixOfLiteralDirectlyBehindSelectSeparatesTheReplacementFromTheKeyword` | SHARED-FIX-003, RH3003-004 | same | fails (`selectstring.Empty`) |
| `FixOfLiteralDirectlyInFrontOfIsSeparatesTheReplacementFromTheKeyword` (`""is string`) | SHARED-FIX-003, RH3003-004 | same | fails (`string.Emptyis`) |
| `FixOfLiteralDirectlyInFrontOfAsSeparatesTheReplacementFromTheKeyword` | SHARED-FIX-003, RH3003-004 | same | fails (`string.Emptyas`) |
| `FixOfLiteralDirectlyInFrontOfSwitchSeparatesTheReplacementFromTheKeyword` | SHARED-FIX-003, RH3003-004 | same | fails (`string.Emptyswitch`) |
| `FixOfLiteralBetweenTwoKeywordsWithoutTriviaSeparatesTheReplacementFromBoth` (`return""is string`) | SHARED-FIX-003, RH3003-004 | same | fails (`returnstring.Emptyis`) |
| `FixOfLiteralDirectlyBehindAnOperatorKeepsTheAuthorsSpacing` (`a+""` → `a+string.Empty`) | SHARED-FIX-006, RH3003-004 | same | passes (other side: punctuation in front) |
| `FixOfLiteralBetweenAParenthesisAndAnOperatorKeepsTheAuthorsSpacing` (`(""+s)` → `(string.Empty+s)`) | SHARED-FIX-006, RH3003-004 | same | passes (punctuation on both sides) |
| `FixOfLiteralDirectlyBetweenAnAssignmentAndASemicolonKeepsTheAuthorsSpacing` (`s=""` → `s=string.Empty`) | SHARED-FIX-006, RH3003-004 | same | passes (punctuation on both sides) |

Every failing case fails on the glued token alone, under both line endings. The verifier's diff shows exactly that one line (observed above in brackets), and no case has a compile error in its input. The existing tests with trivia between the literal and a word token (for example `EmptyStringInReturnIsReportedAndFixed`, `return ""`) still pass. They are the trivia-present side of each new condition. Class count on this head: 16 failing and 44 passing, and every failure is one of the eight tests listed as failing.

The punctuation tests pass on head and fail on `main`, where the code-action clean-up wrote `a+ string.Empty` (08-auditor.md *Hints*). They pin the requirement that a repair adds a space only next to a word token.

### Not covered, and why

- `case"":` and `s is""`: these are constant contexts, so the analyzer does not report them (RH3003-002). They are not members of the finding.
- Verbatim `@""` touching a word token: one call site handles it. `rg "LiteralExpressionSyntax literalExpression" src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3003*` finds the single `ApplyCodeFixAsync`, which replaces the literal node whatever its token kind, so the word-neighbour tests cover it.
- A comment or directive between the literal and its neighbour: trivia present means no adjacency. Comment cases already exist (the RH3003-004 example and the drift tests). A directive cannot stand inside an expression between two tokens of one line, and the 04 handover records why none sits in front of the literal.
- Fix All with glued literals: replacements are per literal and do not overlap (FC-008). The existing Fix All scenario covers batching, and the adjacency is decided per replaced node.
- Formatter counterpart: the fix does not delegate to the formatter.

## thoenissen/Reihitsu#1721

Nothing in this round (Decision 6).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3003UseStringEmptyForEmptyStringsAnalyzerTests"
```
