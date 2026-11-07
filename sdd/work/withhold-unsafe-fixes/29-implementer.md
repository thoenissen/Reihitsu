Role: implementer
Commit: a31baa9d
Verdict: tests pass

Architecture changes after audit 4, lead rows 40–41 (`00-lead.md`), auditor findings 14–15 (`26-auditor.md`), the spec diff of aedc8b5f (`27-spec-author.md`), and the tests of `28-test-implementer.md` with its amendment. My prompt restated rows 40 and 41 as decided. It carried no other intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside. No output questions raised (standing instruction).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/RewriteBindingUtilities.cs` `CountErrors`, new `IsInside` | The walk descends into a child operation only when its syntax is one of the path's nodes or lies inside the replaced expression. Every other child operation, whatever its syntax kind (a sibling statement, a `for` condition or incrementor, a sibling argument), and everything under it, is left out. The exclusion is no longer keyed on `StatementSyntax`. | Row 40; RH3106-003 (finding 14) |
| same file, `BindsAsBefore` | Passes the replaced expression to `CountErrors` on both sides. Summary states exactly what is counted: the path's nodes and the expression's content; the candidate choices are decided by the bound symbols on the path. | Row 40; FC-031 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` `GetLineElements`, new `GetOutermostString`/`IsString`/`AddTriviaElements`, `AddLineElements` | The unit of the line decision is the whole string. Each outermost string literal (also UTF-8 and raw) or interpolated string expression of active code is one code element spanning from its first character through its closing quote and suffix, holes and nested strings included. Its tokens and the trivia inside it are no elements of their own. In disabled text, lexing the text between its directives (`ParseTokens`) already gives one token per string, interpolated strings included (verified: `InterpolatedStringToken` covers holes and nested strings). | RH3004-006 (row 41, finding 15) |
| same file, `IsMovingLine` | Unchanged in code. Its existing rule "the first code element overlapping the line decides: it moves when that element starts on the line" now decides on string elements, so a line whose first character lies inside a string begun earlier stays, whatever follows the string on the line. No case was added. Summary updated. | RH3004-006 |
| same file, `GetFirstStatementLinePosition`, `IsCode` | Summaries only: why the first line on which code starts always moves (a string covering its start would start code on an earlier line); a string element is code. | FC-031 |
| same file, new `LeavesStringOpen`, `IsOpenStringToken`, `IsInBranch`, `ContainsLineStartingWithHash`; `RegisterCodeFixesAsync` | On the block-body path, the fix is withheld when disabled text leaves a string open: a string token lexed from disabled text that runs to the end of it with a lexer error. Also when a string of active code lies inside a branch of a conditional directive of the block and holds a line that starts with `#`: under symbols that disable the branch, that line is a directive and the disabled text leaves the string open (`FixWithheldForAStringThatDisabledTextLeavesOpenWithTheFirstBranchActive`, `A` defined). | RH3004-008 |

## Policy owners

- Rebinding a rewrite or probe: `rg -n "TryGetSpeculativeSemanticModel\(" src --glob '!*Test*'` finds only `RewriteBindingUtilities`. One owner.
- Moving or keeping a line of RH3004's moved code: `rg -n "IsMovingLine|GetOutermostString|KeepsColumns|StaysInColumnZero" src --glob '!*Test*'` finds `IsMovingLine`, its three callers, and `GetOutermostString` feeding `GetLineElements`, all in the RH3004 provider. One owner; no per-token or per-kind predicate.
- Lexing disabled text: `rg -n "ParseTokens" src --glob '!*Test*'` finds RH3004 (`AddTriviaElements`, `ContainsMultiLineRawStringLiteral`, `LeavesStringOpen`), `TokenSeparationUtilities`, and `CasingCodeFixProviderBase`. The latter two lex for other purposes (token separation, identifier search); no shared "strings of disabled text" helper exists to reuse.

## Tests run

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
#   Passed! - Failed: 0, Passed: 162, Total: 162   (the 14 analyzer tests of 28-test-implementer.md that failed now pass)
scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~RewriteBindingUtilitiesTests"
#   Passed! - Failed: 0, Passed: 28, Total: 28     (the 2 Core tests that failed now pass)
scripts/test.sh --configuration Debug --project analyzer      # Passed: 6944, Failed: 0
scripts/test.sh --configuration Debug --project core          # Passed: 649, Failed: 0
scripts/test.sh --configuration Debug --project architecture  # Passed: 115, Failed: 0
scripts/build.sh                                              # Release: 0 Warning(s), 0 Error(s)
scripts/format.sh <both changed files>                        # Formatted 0 of 2 file(s)
```

No test changed. Extra probes with `scripts/apply-fix.sh RH3004`, LF and CRLF, fixtures in the scratchpad:
- a verbatim string of active code outside any branch holding an `#endif` line: fixed, the string's lines keep their columns (the RH3004-008 active-code check needs a branch, so it does not refuse this safe input);
- a comment in a hole that spans lines (`{name /* c⏎   */}`): fixed, the line inside the hole keeps its columns;
- `#if A` around a string holding `  # not a directive`: a parse error without `A`, so no input of the rule.

## Disputed tests or scope

_None_ for the tests. Two notes for the lead:
- RH3004-008's text names only disabled text, but `FixWithheldForAStringThatDisabledTextLeavesOpenWithTheFirstBranchActive` (`A` defined, no disabled text at all) requires the withheld fix too, as the spec's example and Why ("must not depend on which preprocessor symbols are defined") demand. I implemented that reading: an active string inside a branch of the block that holds a line starting with `#`. The spec author may want the requirement sentence to say it.
- Scope: four private helpers for RH3004-008 and three for the string elements, both files named by rows 40–41. No new public member, no new dependency.

## Architecture findings

_None new._ Rows 40 and 41 are carried out as decided: `CountErrors` decides on the path, not on a syntax kind; RH3004 decides on the whole string for active code and disabled text alike, through the one existing `IsMovingLine` rule.

## Follow-up candidates

- `IsInBranch` only knows conditional directives inside the block. A chain whose `#if` lies outside the anonymous method and whose `#else` lies inside it is not seen as a branch; such a shape also makes the anonymous method itself disappear under some symbols, so I left it. Not a defect I could reach with a compiling input in the time; noted for the auditor.
- The shared "move lines by a delta keeping literal values" helper (row 28) stands unchanged.
