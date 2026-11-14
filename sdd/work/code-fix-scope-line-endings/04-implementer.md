Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/code-fix-scope-line-endings/04-implementer.md`)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

No test assertion, input, or expected output was changed. Every handed-over test was first run red on 4ae6a50 as `03-test-implementer.md` lists (RH3006 8 red, RH3001 25 red, RH3202–RH3208 29 red), then green after the change.

## thoenissen/Reihitsu#1283

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider.cs` | The parentheses are built as tokens with explicit trivia: `(` carries the operand's leading trivia, `)` its trailing trivia, and nothing else. Before, `SyntaxFactory.ParenthesizedExpression(...)` built them with elastic trivia; the code action's clean-up formats the gaps around elastic trivia and wrote the host's line break (LF on Linux) into the line breaks between the preceding token and a comment in front of the operand. | SHARED-FIX-006, SHARED-FIX-008, RH3006-003 |

Mechanism, as confirmed: elastic trivia left on a token the fix creates. No new type, no guard.

Decision 9 (RH3003): the repair is local to the RH3006 provider, not in a component RH3003 shares, so RH3003 stays a follow-up. The mechanism is now known (see *Follow-up candidates*).

## thoenissen/Reihitsu#1284

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/ExpressionBodyCodeFixUtilities.cs` (new, internal static class) | `ConvertBodyAsync`: marks the member, lets the formatter format it where it stands (the caller's entry point), finds the formatted member by the mark, and writes back into the *unformatted* document only the formatted block body plus the formatted trailing trivia of the token in front of `{`. Everything in front of that gap stays as written. Returns the unchanged document when the formatter does not convert the body. | SHARED-BODY-003, SHARED-FIX-006, SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/ExpressionBodyToBlockCodeFixProviderBase{TNode}.cs` | `ApplyCodeFixAsync` calls `ConvertBodyAsync` (RH3205, RH3206, RH3207 inherit it); summaries updated. | SHARED-BODY-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3203ExpressionStyleConstructorsShouldNotBeUsedCodeFixProvider.cs` | Same; the constructor initializer stays as written. | SHARED-BODY-003 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider.cs` | Passes `ConvertBodyAsync` as the format step of `FormatMemberOnLinesOfItsOwnAsync`, so the shared-line separation stays as it was. | SHARED-BODY-003, SHARED-FIX-007 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `FormatMemberOnLinesOfItsOwnAsync` gains an overload that takes the format step; the old signature (used by RH5417) keeps formatting the whole member. | — |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider.cs` | `FormatAsync` calls `ConvertBodyAsync` with the existing label-context choice moved into `FormatLocalFunctionAsync`; the blank-line handling below is unchanged. | SHARED-BODY-003 |

### Does an entry point predict a phase's decision? (skeptic's question)
No. The fix writes no line break, column, or whitespace of its own. The two decisions the skeptic named are read from the formatter's result: the gap between the last token in front of `=>` and `{` (the line break, a comment placed by FMT-STRUCT-006, and the removed whitespace) is the formatted token's trailing trivia, and the block with its `{` indentation is the formatted block. The member's first token keeps its document column under FMT-PIPE-017, so the block's column is the one the formatter computed for that member.

### Tab test (decision 13)
`VerifyFixKeepsTheTabsInFrontOfTheSignatureOfAMethodIndentedWithTabs` and the raw-string tab test pass as written: the signature line keeps `\t\t`, and the block is written in spaces, as the formatter writes it. No special case was needed.

## thoenissen/Reihitsu#1285

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs` | (1) `FindNode(..., getInnermostNodeForTie: true)` finds the negation also where it is a whole argument, element, or `orderby` key. (2) `node.Parent is not ConstantPatternSyntax` withholds the fix for a constant pattern; `case !true:` is a `CaseSwitchLabel` value and keeps its fix. (3) `FindReplacement` replaces the `Simplifier` annotation: it offers the comparison without parentheses when `RewriteParseUtilities.ParsesAsWritten` confirms it parses in the negation's place, else in parentheses when that parses as written, else no fix (RH3001-007 as before). (4) `BuildReplacement` writes `==` and `false` with explicit trivia, not elastic trivia, and puts the operand's trivia outside the comparison. Without (4), `orderby !a` + line break gave `a` ⏎ `== false`. | RH3001-004, RH3001-005, RH3001-007, RH3001-008 |

The parser decides binding. The fix keeps no precedence table (FC-015). `RewriteParseUtilities` is the existing owner of that answer.

## Policy owners
- `rg -n "ExpressionBody" src/Reihitsu.Analyzer.CodeFixes`: the six SHARED-BODY providers, plus RH2001, RH3004, and RH5408. Those three convert other constructs, not member bodies to blocks. `ExpressionBodyCodeFixUtilities` is the one owner of the write-back.
- `rg -n "GetAnnotatedNodes" src/Reihitsu.Analyzer.CodeFixes`: `FluentChainCodeFixHelper`, `BlankLineCodeFixUtilities`, and `CasingCodeFixProviderBase`. Each uses the same mark-and-find pattern, and none splices a body. No existing helper covered this.
- `rg -n "Simplifier" src/Reihitsu.Analyzer.CodeFixes`: none left.
- `rg -n "SyntaxFactory\.(ParenthesizedExpression|BinaryExpression|Token\(SyntaxKind)" src/Reihitsu.Analyzer.CodeFixes`:
  - RH3001 and RH3006 now pass explicit tokens;
  - RH3003 (`Token(StringKeyword)`) and RH3005 (`BinaryExpression(kind, …)`) still build elastic tokens (see *Follow-up candidates*);
  - RH2001 adds a `readonly` modifier.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 8 files changed, 287 insertions(+), 38 deletions(-).

New type: `Reihitsu.Analyzer.CodeFixes.Core.ExpressionBodyCodeFixUtilities` (internal).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzerTests"`: Passed 35, Failed 0
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3001NotOperatorShouldNotBeUsedAnalyzerTests"`: Passed 66, Failed 0
- RH3202|RH3203|RH3205|RH3206|RH3207|RH3208 filter: Passed 190, Failed 0
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 7583, Failed 0 (RH5417 included, which still formats the whole member)
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117, Failed 0
- `scripts/test.sh --configuration Debug --project tooling`: Passed 126, Failed 0
- `scripts/format.sh` over the changed files: 3 of 8 reformatted, then rebuilt and re-tested. The Debug build has 0 warnings.

## Disputed tests or scope
- **New type and a changed shared helper.** `ExpressionBodyCodeFixUtilities` is new, and `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` gained an overload. It has one other consumer, RH5417, whose behavior is unchanged.
  - The six providers needed one shared write-back. The alternative was to have each provider build the block itself, which would copy FMT-STRUCT-004/-005, the formatter's policy.
  - The production paths stay within the handovers' surfaces: the RH3006 provider, the SHARED-BODY base and its providers, and the RH3001 provider.

## Architecture findings
_None in scope._

#1283's cause is elastic trivia on tokens a fix creates, which the code action's clean-up formats with the host line break. That cause is shared by any fix that builds tokens with `SyntaxFactory` defaults and does not go through `ReihitsuFormatter`. It is not a structural cause inside the run's components. Each affected fix is local (below).

## Follow-up candidates
- **RH3003** writes an LF into a CRLF document (lead's existing candidate). The cause is now confirmed: `SyntaxFactory.Token(SyntaxKind.StringKeyword)` and `IdentifierName(...)` carry elastic trivia, and the code action's clean-up re-formats the gap around them. The repair is the RH3006 one: build the tokens with explicit empty trivia.
- **RH3005** builds `SyntaxFactory.BinaryExpression(expressionKind, …)` with an elastic operator token. It is the same hazard, not yet probed under CRLF with a comment next to the operand. Candidate for the same check.
