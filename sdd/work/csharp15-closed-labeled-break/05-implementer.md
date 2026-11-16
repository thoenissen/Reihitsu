Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/csharp15-closed-labeled-break/05-implementer.md`)
Verdict: tests pass

Inputs: `00-lead.md` (answers 1B, 2B; decisions 1–8, decision 6 in particular), `02-skeptic.md`, `03-spec-author.md`, `04-test-implementer.md`, change doc `sdd/changes/2026-11/rh7105-closed-safe-modifiers.md`, specs RH7105 and RH5011. The prompt carried no intended fix or verdict. The issues' prescribed solutions (#1764 "explicit ranks", #1765 `GetNextToken`) were not taken as given; the route below follows the spec and decision 6.

Every handed-over "fails" test was seen failing on 060d127 as `04-test-implementer.md` lists (the test implementer's run; the counts match: 35 analyzer and 7 Core failures, all new rows) before the production change, and passes after it.

## thoenissen/Reihitsu#1764

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/ModifierOrderingUtilities.cs` | `GetRh7105Rank`: `ClosedKeyword` gets the rank of `sealed` (4); a modifier whose text is `safe` gets the rank of `unsafe` (11) through a `when` arm before the default; new private constant `SafeModifierText`; remarks updated to state both places and why `safe` is matched by text | RH7105-001, -002, -003, -004 |

`SyntaxKind.SafeKeyword` is experimental in Roslyn 5.12 (`RSEXPERIMENTAL006`), so referencing it would need a suppression, which I must not add. The arm compares the token text instead: only modifier tokens reach the switch, and a verbatim identifier `@safe` never stands in a modifier list, so the text identifies the keyword. It is a cheap syntactic check, no new type. When Roslyn drops the experimental marker, the arm can become `SyntaxKind.SafeKeyword => 11` (not a follow-up issue on its own; noted for the lead).

Shared ranks: `closed`/`sealed` and `safe`/`unsafe` tie; the stable `ThenBy(index)` keeps their written order, matching the spec's "order relative to `sealed` and `unsafe` is not decided".

### Policy owners
`rg -n "GetRh7105Rank|OrderModifiersForRh7105|SyntaxKind.SealedKeyword =>|UnsafeKeyword =>" src --type cs -g '!*Test*'`: the one owner is `ModifierOrderingUtilities.GetRh7105Rank` (via `OrderModifiersForRh7105`/`TryGetRh7105Violation`); consumers `RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider` and `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` (its `safe` test passes, output unchanged). No private copy of the order exists.

## thoenissen/Reihitsu#1765

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs` | The token behind the statement is now `statement.GetLastToken().GetNextToken()`, the same last token the gap is already measured from; the abstract `GetNextToken` is removed; `OnStatement` summary states that the gap starts at the statement's terminator | RH5011-001, -002, -008 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.cs` | `GetNextToken` override (keyword-relative, skipping one `;`, which a label defeated) removed | RH5011-001, -002, -008 |

Decision 6 left the place to me: the base class, as the skeptic recommended. The base class has exactly one subclass (skeptic's `rg`; its constructor is `private protected`), so no other consumer changes output. Analyzer and code fix now both measure from the statement's end (the fix from `SemicolonToken`, the analyzer from `GetLastToken()`, which is the semicolon in syntax-valid code). The fix additionally passes `includeZeroWidth`/`includeSkipped`; they differ only on syntax-invalid code, which the formatter and fixes leave alone, so I did not align the flags.

### Policy owners
`rg -n "GetNextToken\(" src/Reihitsu.Analyzer/Base src/Reihitsu.Analyzer/Rules/Layout/RH5011* src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011*`: the base class (line 80) and the RH5011 code fix (lines 66, 116), both from the statement's end. No keyword-relative derivation remains.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (after this commit): 3 files changed, 31 insertions(+), 32 deletions(-). No new type, no new public member (one abstract protected member removed from a base with a `private protected` constructor).

## Deleted special cases
_N/A._ The run declares no shape unsupported.

## Tests run
- `scripts/build.sh --configuration Debug` — Build succeeded, 0 warnings.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests|FullyQualifiedName~VerifyPrivateGetOnlySafeAutoProperty|FullyQualifiedName~RH5011BreakStatements|FullyQualifiedName~RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH7501Break|FullyQualifiedName~RH2001"` — Passed! Failed: 0, Passed: 469, Total: 469.
- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~ModifierOrderingUtilitiesTests"` — Passed! Failed: 0, Passed: 35, Total: 35.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 6045, Total: 6045.
- `scripts/test.sh --configuration Debug --project core` — Passed! Failed: 0, Passed: 738, Total: 738.
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 118, Total: 118.
- `scripts/format.sh` on the three changed files — formatted 0 of 3.

## Disputed tests or scope
_None._ No warning needs a suppression or ruleset change; the `safe` text match avoids the `RSEXPERIMENTAL006` suppression the test implementer flagged.

## Architecture findings
_None new._ The RH5011 finding (decision 6) is resolved by this change: the analyzer no longer computes "the token behind the `break`" its own way.

## Follow-up candidates
_None._
