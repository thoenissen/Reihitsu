Role: auditor
Commit: 888654727cd862479760152f6835166e0655009e
Verdict: findings (#1283: pass; #1284: pass apart from low #2 and #3; #1285: findings, #1 high)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

## Scope

- Head: `888654727cd862479760152f6835166e0655009e`. The checkout is at this commit, and the working tree is clean.
- Base: `origin/main` = `cf6d26b03fe1917f193d412c8540916f93a00dc4`. It matches the prompt, and the head contains it (`git merge-base --is-ancestor`).
- Diff audited: `114ae37..HEAD`. It changes eight production files under `src/Reihitsu.Analyzer.CodeFixes`, eight test files, the specs, and the change doc. That matches `04-implementer.md` and `07-spec-author.md`.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Full audit. This is the run's first audit.
- Evidence: static tracing, plus `scripts/apply-fix.sh` (LF and CRLF arms) on scratch fixtures outside the repository. Each fixture ran on the head and on a detached worktree of `origin/main`, which has since been removed:
  - RH3001: 50 fixtures;
  - RH3202: 17 fixtures;
  - RH3203, RH3205, and RH3208: 10 fixtures.

  No test was re-run. I relied on the cleaner's Release build and the roles' focused results.

## thoenissen/Reihitsu#1283

### Findings
_None._

The RH3006 fix now builds `(` with the operand's leading trivia and `)` with its trailing trivia, and gives neither token elastic trivia. That trivia matches the former `WithTriviaFrom` result. The expression branch and the pattern branch are built the same way. The ten tests run under LF and CRLF through `VerifyCodeFixWithEndOfLine`, which compares the exact text. They cover every member of the skeptic's matrix: the issue's shape, a line comment on its own line, a blank line in front of the comment, `;` on its own line, the left operand, a whole argument, the pattern operand, and Fix All. They also cover both sides of the boundary: no comment, and a comment trailing the operator. SHARED-FIX-006, SHARED-FIX-008, and RH3006-003 hold.

### Guard scope
_N/A — the RH3006 change adds or changes no predicate._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Fix-created tokens carry no elastic trivia | `rg -n "SyntaxFactory\.(ParenthesizedExpression\|ParenthesizedPattern\|BinaryExpression\|Token\(SyntaxKind)" src/Reihitsu.Analyzer.CodeFixes`: RH3006 and RH3001 now use explicit tokens; RH3003 (`Token(StringKeyword)`) and RH3005 (`BinaryExpression(kind, …)`) still build elastic tokens | ok for the run. RH3003 and RH3005 are already follow-up candidates in `00-lead.md` (decision 9). |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixOfCommentedOperandOnTheNextLineKeepsTheLineBreakInFrontOfIt` and the six sibling shapes | Every line break stays as written, byte for byte, under CRLF | One LF in the CRLF arm | `VerifyCodeFixWithEndOfLine`: exact text through the framework verifier, then the raw `ApplyCodeFixAsync` | adequate (FC-017, FC-033) |
| `FixAllOfTwoCommentedOperandsOnTheirOwnLinesKeepsTheLineBreaks` | Same, on the Fix All path and on two single applications | LF lines after Fix All | `Verify` on CRLF and LF text, plus `ApplyCodeFixAsync` twice | adequate (FC-008) |
| `FixOfOperandOnTheNextLineWithoutACommentKeepsTheLineBreakInFrontOfIt`, `FixOfOperandBehindALineCommentOnTheOperatorsLineKeepsTheLineBreak` | The other side of the boundary | Any changed line break | `VerifyCodeFixWithEndOfLine` | adequate (FC-003) |

### Failure classes dismissed
- FC-004, FC-019, FC-021: the fix carries the trivia over unchanged, and the tests put a comment in every position of the gap.
- FC-005, FC-020: no directive guard is added or changed.
- FC-006, FC-022–FC-024, FC-036: the fix only adds parentheses.
- FC-010–FC-014, FC-028, FC-038–FC-043: the formatter is not involved.
- FC-025, FC-027, FC-029: they do not apply to a parenthesizing fix.
- FC-035: the expected values come from the requirements, and the CRLF arms were red before the change (`03-test-implementer.md`).

### Architecture findings
_None._

## thoenissen/Reihitsu#1284

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 2 | low | `src/Reihitsu.Analyzer.CodeFixes/Core/ExpressionBodyCodeFixUtilities.cs` `FormatNodeAsync` and `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` `FormatNodeAsync` | FC-015 (minor) | – | PR-introduced | Two identical private wrappers of `ReihitsuFormatter.FormatNodeInDocumentAsync(...).Output` with the same delegate shape, both added in this diff to two internal helpers of the same assembly | Keep one wrapper and pass it to both. |
| 3 | low | `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs:118-128` (summary of the three-argument `FormatMemberOnLinesOfItsOwnAsync`) | FC-031 | – | PR-introduced | The summary still says "only the member is formatted (RH5417-003, RH3202-002)". RH3202 no longer calls this overload: it calls the new four-argument overload with `ConvertBodyAsync`, which converts only the body. The four-argument overload's summary defers to this text ("as … does"). | Describe the line separation once, where both overloads can point to it, and drop RH3202 from "only the member is formatted". Text only. |

Neither finding can reach compiled behavior. #3 is confined to a comment, and #2 is a refactoring with identical behavior.

Traced and probed, and holds. Each was compared with `main`, and each new block body is byte-identical to `main`'s block body:

- **Kinds and gaps.** All six kinds: RH3202, RH3203, RH3205, RH3206, RH3207, and RH3208, including a labeled local function on its label's line, a labeled local function with a wrapped signature, and one in a lambda. The tested gap shapes: a constructor initializer with a trailing comment, a constraint clause with a comment, and the arrow on the next line with a comment on its own line in front of it. A block comment in front of `=>`, comments after `=>` and before `;`, and a trailing comment after `;` are all placed by the formatter as on `main`: `{/* b */` is `main`'s output too.
- **Other shapes.** Trailing whitespace in front of a wrapped arrow, wrapped parameter lists in both styles, a raw-string body, an interface member, a nested type, a lambda body with alignment, and a CRLF arm for every fixture.
- **Nothing is offered that changes nothing (FC-009, FC-043).** `ConvertBodyAsync` returns the input unchanged only when the formatter keeps the expression body. The formatter keeps it only for `BlocksRewrite` members, which the analyzers do not report, and for the async and unsupported-shape cases, which `RegisterCodeFixesAsync` already withholds.
- **RH3208.** The RH3208 tracked-node path keeps working: the next statement's tracking annotation survives, because `ConvertBodyAsync` replaces only the member in the caller's root.
- **Tabs (decision 13).** The block's column comes from the formatter's layout of the member: FMT-PIPE-017 counts a target's own leading tabs at four columns. The signature line keeps its tabs under SHARED-FIX-006. Both expected values are demanded by unchanged requirements.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| What the fix writes back (`ConvertBodyAsync`) | The formatted block, plus the formatted trailing trivia of the token in front of `{`, which replaces the original trailing trivia of the token in front of `=>`. Everything else of the member stays as written. | SHARED-BODY-003: everything in front of `=>` stays; the whitespace between the last token in front of `=>` and `=>` goes with the arrow; a comment there is placed as FMT-STRUCT-006 states | Agree. The replaced trivia is exactly that gap. Comments in it, and in the leading trivia of `=>`, come back from the formatter's placement (probed). | holds |
| When the fix does nothing | The formatted member still has an expression body, or the mark is not found | `RegisterCodeFixesAsync`: `RequiresAsyncExpressionBodyPreservation` and `FindUnsupportedSyntax(GetFormattingInput(node))`; the analyzers: `ExpressionBodyRewriteUtilities.BlocksRewrite` | Agree. Every refusal of the six transforms is covered before registration. | holds |
| RH3208 formatting input | `FormatLocalFunctionAsync` formats `GetLineStartingLabel(localFunction)` | `GetFormattingInput` returns `GetLineStartingLabel(node)` | Same node and same text; the mark adds no text | holds (FC-043) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Write-back of a converted body (SHARED-BODY-003) | `rg -n "ConvertBodyAsync\|WithExpressionBody\(null\)" src --glob '!*Test*'`: `ExpressionBodyCodeFixUtilities` is used by the base provider and by RH3202, RH3203, and RH3208 (RH3205–RH3207 inherit the base). The formatter transforms own the conversion itself, and the fix reads their result. | one owner; listed in the spec (07) |
| Line separation for a member that shares its line | `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`, used by RH3202 (new overload) and RH5417 (old overload) | one owner (see hint 1 on the providers that do not call it) |
| Expression-body and block-body accessors | `rg -n "=> method.ExpressionBody" src --glob '!*Test*'`: only `ExpressionBodyCodeFixUtilities` | one owner |
| Formatter call wrapper | the two `FormatNodeAsync` copies | finding #2 |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH3202 `FixKeepsTheSpacingOfTheSignature` (the SHARED-BODY-003 example) | The signature stays byte for byte, and only the body changes | `int GetValue()` normalized, or a CRLF arm drifts | `VerifyCodeFixWithEndOfLine` | adequate |
| RH3202 `FixLeavesNoWhitespaceBehindTheSignatureWhenSeveralSpacesStoodInFrontOfTheArrow` | No trailing whitespace on the signature line | Trailing spaces behind `)` | `Verify` (exact text) | adequate |
| RH3202 `FixKeepsABlockCommentBehindTheSignatureOnItsLine`, `FixKeepsALineCommentBehindTheSignatureWhenTheArrowStartsTheNextLine` | The comment in the replaced gap is kept where FMT-STRUCT-006 puts it | A lost or moved comment | `Verify` | adequate (FC-021) |
| RH3202 `VerifyFixKeepsTwoBlankLinesAboveTheMethod` / `…BelowTheMethod` | The blank lines around the member stay | One blank line collapsed | `Verify` | adequate (FC-003, both sides) |
| RH3202 tab tests | The signature keeps `\t\t`, and the block is written in spaces at column 8 | Tabs replaced on the signature, or the block in another column | `Verify` (exact strings) | adequate |
| RH3202 `FixAllKeepsTheSignaturesOfTwoMethods`, RH3203 `VerifyFixAllKeepsTheSignaturesOfTwoConstructors` | Fix All keeps both signatures | One signature normalized | `Verify` (iterative and Fix All) | adequate (FC-008) |
| RH3202 `FixIsOfferedWhenAnotherMemberHoldsACommentTheFormatterRefuses` / `NoFixIsOfferedForAMethodWhoseBodyHoldsACommentTheFormatterRefuses` | SHARED-FIX-010 on both sides | A fix offered or withheld on the wrong side | `Verify` / `VerifyNoCodeFixIsOffered` | adequate (FC-043) |
| RH3203 initializer tests, RH3205–RH3208 signature tests | The initializer and the signature stay as written | Any of them normalized | `Verify` | adequate (FC-025, FC-027) |

### Failure classes dismissed
- FC-001, FC-002: no new predicate. The write-back span is checked under *Guard scope*.
- FC-005, FC-020: a directive in the member blocks the report (`BlocksRewrite`). The signature, where a directive could sit, is no longer touched at all.
- FC-006: the block comes from the formatter's transform, as on `main`.
- FC-007: one iteration in every probe. The converted member is no longer an expression body.
- FC-010–FC-012: no formatter phase changed.
- FC-013, FC-014: the analyzers are unchanged.
- FC-016: all six providers go through the helper.
- FC-022: raw-string body probed. It matches `main`.
- FC-028: no detached `FormatNode` call is involved.
- FC-038–FC-042: no shape is declared.

### Architecture findings
_None._ The fix reads the formatter's result and makes no layout decision of its own. The two decisions the skeptic named are both read from that result: the gap in front of `{` and the column of the block.

## thoenissen/Reihitsu#1285

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs` `FindReplacement` (`ParsesAsWritten` on the unparenthesized comparison) | FC-001, FC-002 | – | PR-introduced | See *Finding 1* below | Refuse this class: withhold the fix where `!` touches the keyword in front of it with no whitespace between them (and, likewise, where the operand's last token touches a following keyword). That is a rare input: RH6001 reports `return!a`, and RH3007 reports query syntax. |

#### Finding 1

RH3001-004 now says the fix MUST NOT put the comparison in parentheses where it binds as the negation did. `FindReplacement` also falls back to parentheses when the unparenthesized text fails `ParsesAsWritten` only because the keyword in front of `!` would merge with the operand: `return` and `a` lex as `returna`. Binding is not the reason in that case. The output breaks the new MUST NOT, and it changes `main`'s output; no maintainer answer covers the change.

Counterexample:

```csharp
class C
{
    bool M(bool a, bool b)
    {
        if (b) return!a;
        return a;
    }
}
```

- `main`: `if (b) return a == false;`
- head: `if (b) return(a == false);`

The class is a keyword in front of `!` with no whitespace between them. Every member probed under LF and CRLF, compared with `main`:

| Keyword in front of `!` | `main` | head |
|---|---|---|
| `return`, `yield return` | `kw a == false` | `kw(a == false)` |
| `case` (switch statement label) | `kw a == false` | `kw(a == false)` |
| `when` (case guard and switch-expression arm guard) | `kw a == false` | `kw(a == false)` |
| `where`, `on`, `equals` | `kw a == false` | `kw(a == false)` |
| `select`, `group`, `by`, `goto case` | `kw(a == false)` | `kw(a == false)`: unchanged, but the change doc and RH3001-004 now say these positions get no parentheses |
| `orderby` | no fix | `orderby(x == false)`: new output |

- Traced, not probed: the trailing side, an operand ending in `)` directly followed by a keyword, such as `where!(x)select`. It takes the same path.
- Holds: every position with whitespace in front of `!`. 34 positions were probed, and the parenthesized and unparenthesized outputs were identical to `main` outside the whole-node and keyword cases.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which node the fix gets | `FindNode(span of !, getInnermostNodeForTie: true)` gives the `PrefixUnaryExpressionSyntax` itself, also where it fills an `Argument`, `AttributeArgument`, `ExpressionElement`, or `Ordering` | RH3001-004's position list | Agree. All other wrappers with the same full span are constant patterns (RH3001-008). An unnamed `AnonymousObjectMemberDeclarator` cannot hold `!x` in compiling code. | holds |
| Constant pattern: no fix | `node.Parent is ConstantPatternSyntax` | RH3001-008: "the whole expression of a constant pattern", including `case !true when x:`, but not `case !true:` (a `CaseSwitchLabel` value) | Agree on the letter. See hint 2 for `x is (!true)`. | holds |
| Parentheses or not | Unparenthesized iff the text parses back into the same tokens and structure within the innermost member | RH3001-004: "where it binds differently" / "where it binds as the negation did" | **Disagree.** Token merging is not binding (finding #1). | finding #1 |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether the rewritten text parses as written | `rg -n "ParsesAsWritten" src --glob '!*Test*'`: `RewriteParseUtilities` (`Reihitsu.Core`), used by RH3001, RH3005, RH3101, RH3105, and RH3201 | one owner |
| Parenthesization of the comparison | Only `RH3001…CodeFixProvider.FindReplacement`. `rg -n Simplifier src --glob '!*Test*'` finds no other user. | one owner |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NegationsThatAreWholeArgumentsAttributeArgumentsOrElementsAreFixed` and the twelve `NegationAsAWhole…IsFixed` tests | Fix offered, with exact output and no parentheses, at each whole-node position | No fix, or parentheses | `Verify` (exact text, compile, iterative and Fix All) | adequate |
| `FixWritesNoParentheses…`, `…AddsNoParentheses` | No parentheses where binding is the same | `(x == false)` | `Verify` | adequate for positions with whitespace. **Missing:** a position where `!` touches the keyword in front of it (finding #1). |
| `NegationAsTheLeftOperandOfIsInAnArgumentIsParenthesized` | Parentheses kept where binding differs | Parentheses dropped | `Verify` | adequate (FC-003) |
| `NegationsThatAreConstantPatternsAreReportedWithoutACodeFix` and the subpattern, list-pattern, logical-pattern, and `case … when` tests; `NegationAsTheValueOfACaseLabelWithoutAWhenClauseIsFixed` | RH3001-008 on both sides of the `case` boundary | A fix offered on a constant pattern, or withheld on `case !true:` | `VerifyNoCodeFixIsOffered` / `Verify` | adequate |
| `FixOfACommentedWholeArgumentOnTheNextLineKeepsTheLineBreaks` | Line endings stay under CRLF | An LF in the CRLF arm | `VerifyCodeFixWithEndOfLine` | adequate (FC-017) |
| `FixAllFixesTwoWholeArgumentsOfOneInvocation`, `FixOfANestedNegationInAWholeArgumentFixesBoth` | Fix All converges on adjacent and nested negations | Lost edit or no convergence | `Verify` | adequate (FC-008) |

### Failure classes dismissed
- FC-004, FC-019, FC-021: comments between `!` and the operand and behind a whole argument were probed. They move as RH3001-005 states.
- FC-005, FC-020: `HasSafeOperandTrivia` is unchanged and still refuses directives, and `DirectiveAfterTheOperatorOfAWholeArgumentIsReportedWithoutACodeFix` covers it.
- FC-006: the outputs compile. `[A(true == false)]`, `goto case true == false`, and `case true == false:` are constant. Bool overloads are unchanged.
- FC-007: one or two iterations, as on `main`. RH3002 and RH3005 do not report the new output (`02-skeptic.md`).
- FC-009: a fix is registered only with a precomputed replacement that differs from the negation.
- FC-010–FC-014, FC-028, FC-038–FC-043: the formatter is not involved.
- FC-022–FC-025, FC-036: they do not reach a `!`-to-comparison rewrite.
- FC-026: the replacement is built from the node, not re-resolved by text.

### Architecture findings
_None._ Finding #1 is one predicate in one provider, and the parse owner is shared correctly.

## Whole run

### Visible output is decided

- **#1283.** The expected values are demanded by unchanged SHARED-FIX-006, SHARED-FIX-008, and RH3006-003.
- **#1284.** Every changed expected value is demanded by unchanged SHARED-FIX-006 (*Formatting*, *Blank lines*), whose RH3203 example is the issue's input. SHARED-BODY-003 was clarified and does not decide beyond it. The tab outputs follow from SHARED-FIX-006 and SHARED-FIX-007 (decision 13). No rule page changed.
- **#1285.** Every changed expected value falls under the change doc's `Decisions` (Q2, Q3 option A, the `orderby` answer, and `case !true:`), as recorded in `Maintainer answers` in `00-lead.md`. The run's tests assert no keyword-touching output, so finding #1 is a behavior finding, not an untested output.

### Comments and conventions

- No `TODO`, no tracker numbers, and no commented-out code in the diff.
- The summaries of `ExpressionBodyCodeFixUtilities`, the base provider, RH3202, RH3203, RH3208, RH3001, and RH3006 describe the new code, except finding #3.

### Cost of special cases

No production code in the diff serves only a rare class. Finding #1's suggested change is a refusal, not machinery.

### Scope

The changes are confined to the components the handovers name. The new overload of `BlankLineCodeFixUtilities` keeps RH5417 on the old path.

## Hints

1. **Pre-existing, unrelated to the run's defects (follow-up candidate).** RH3203, RH3205–RH3207, and RH3208 do not give a member or local function that shares its line with another member or statement a line of its own. Examples: `int _a; public C() => _a = 1;` gives `int _a; public C()` followed by the block at column 12, and `int  L( ) => 1; var x = L();` leaves `} var x = L();`. `main` produces the same output. SHARED-FIX-007 *Shared line* requires the separation for members. Structural cause: the separation is owned by `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`, and of the SHARED-BODY providers only RH3202 calls it.
2. **Pre-existing; RH3001-008's letter and its Why diverge.** `x is (!true)` and `x is not (!true)` are fixed to `x is (true == false)` and `x is not (true == false)`, on `main` and on the head alike. The negation is not "the whole expression" of the constant pattern, because a parenthesized expression is. The fix is therefore offered under the requirement's letter, while its Why names exactly this output as the one to avoid. The input is rare.
