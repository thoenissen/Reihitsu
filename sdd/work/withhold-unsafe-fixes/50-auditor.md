Role: auditor
Commit: 816960bf
Verdict: findings — #1257: pass; #1279: findings (#25, low); #1281: pass; #1282: pass; #1417: pass; #1422: pass

My prompt named the run, the SHAs, the delta (`f1e3f033..816960bf`, rows 50 and 52), the handovers to read, and what to check: whether finding 23 (a)–(c) is closed for its class inside the anonymous method, whether finding 24 is closed, and that member (d) is out of scope by row 51. It carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to set aside.

## Scope

Head `816960bff20b031a2f7c08f8b680fe3f0a8813d7`, checked out and clean. Base `origin/main` = `0fa1778c875793aaba22d10fef14747d4988f2ed`, fetched; it is an ancestor of head.

Repair delta against `f1e3f033`. It matches handovers 45–49 and rows 50 and 52:
- `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs`:
  - `ContainsStringOrCommentSpanningLines` now takes the anonymous method and `keepsBlockBody`, and runs on both paths over `anonymousMethodExpression.Span`;
  - new `ContainsLineStartingWithHash`;
  - private `SpansLines` deleted, replaced by `SyntaxNodeUtilities.IsSingleLineSpan` (Core).
- Spec: RH3004-007 and RH3004-008.
- Tests: 1 RH3004 test flipped to withheld and renamed, 13 new RH3004 tests (10 withheld, 3 offered).
- `00-lead.md`: rows 50–52 and the reworded follow-up candidate.

Scope did not grow. I trusted the cleaner's build and the roles' focused test results.

Method:
- static tracing of the widened guard against RH3004-007/-008, SHARED-FIX-003 (build configurations) and the lexer's directive rule;
- a lexer argument for class completeness (below);
- `scripts/apply-fix.sh RH3004`, LF and CRLF, on a fresh build of head, over 30 fixtures in the session scratchpad (`audit8/fx`, `audit8/fx2`): the four audit-7 fixtures (`e1`, `e2`, `e3`, `e3b`), the twelve audit-6 fixtures, and 14 new ones.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 23 | closed for (a)–(c); (d) out of scope (row 51) | (a) `e2_expression_body`, (b) `e3_param_comment` and (c) `e3b_param_comment_expr` give `no-fix-offered` under LF and CRLF, and each has a test of its own (`FixWithheldForAReturnedStringThatHidesTheElseOfAChainAroundTheAnonymousMethod`, `…ParameterListCommentThatHidesTheElseOfAChainOnTheBlockBodyPath`, `…OnTheExpressionBodyPath`). The class holds inside the anonymous method (see *Class completeness*). (d) `e1_comment_before` is still fixed, as on `main`. It is the member row 51 files as a follow-up, and RH3004-008's *Place* bullet and Why now state it as out of scope. I report it here as status only, not as a finding. |
| 24 | closed | `SpansLines` is gone. The only line test is `SyntaxNodeUtilities.IsSingleLineSpan(syntaxTree, carrierSpan)`, which measures the unmapped `GetLineSpan`, the same lines the deleted copy measured. A `#line` directive before the anonymous method does not change it. `rg "SpansLines" src` finds only the formatter-private `ListElementInteriorJoinRewriter.SpansLines`, which a code fix cannot reach. |

## Class completeness inside the anonymous method

The class: a compiling input where RH3004 offers a fix, and an edit of that fix lies in text that is the inside of a string in some build B. The analyzed build is A.

In B, the string that holds the edit E opens before E. Let D be the last line before E that is a directive in B. From D on, B lexes from a fresh state up to E. If A lexed the same text from the same state, E would be code in A and in B alike. So in A, one of these holds:

1. **D lies inside a token or trivia that spans lines** (a carrier). That carrier opens on an earlier line, because D's line holds only lexer whitespace before `#`. D is then a *later* line of the carrier whose first non-whitespace character is `#`, and that `#` lies inside the carrier, because a string or comment cannot close in whitespace. If the carrier starts inside `anonymousMethodExpression.Span`, condition 1 of RH3004-008 withholds the fix. If it starts before the first token, the input is member (d).
2. **D is disabled text or a directive in A.** Then a directive of A lies between D and E, and in B it is hidden in a string that opens before it. RH3004-009 (`ContainsDirectiveOrDisabledText`) puts no directive inside the span, so this directive stands before the first token. The hiding string of B then starts before the anonymous method, which is again member (d).

The carriers that can span lines in A are verbatim, raw, interpolated and UTF-8 raw strings (`IsString`), `/* */` and `/** */` comments, and `///` runs. A `///` run hides no `#` line, because each of its lines starts with `///`. A character literal and a `//` comment end on their line, and a directive or disabled text is RH3004-009's.

The fix makes no edit behind the closing `}` of the anonymous method. A carrier that starts in the trailing trivia of `}` therefore has its `#` lines behind every edit:
- `f01_trailing_comment_block` (block path) and `f02_trailing_comment_expr` change no character behind `} /*`, the trailing spaces of `*/   ` included.
- Without `A`, `s` keeps its value.

Holding (all `no-fix-offered`, LF and CRLF):
- `#` behind U+00A0 (`f03`);
- a U+2028 line separator before `#` (`f04`);
- `#` behind a vertical tab (`f05`);
- a `#` line inside an interpolation hole's comment (`f06`);
- an interpolated raw string (`f07`);
- a nested anonymous method in the returned expression (`f08`);
- `async delegate` with a parameter-list comment (`f09`);
- a returned raw string that hides `#else` (`f10`);
- a UTF-8 raw string (`f11`).

`SourceText.Lines` and the lexer agree on the line-break set (CR, LF, U+0085, U+2028, U+2029). `SyntaxFacts.IsWhitespace` is the set the lexer's `ScanWhitespace` skips.

Offered and correct:
- `@"a⏎  b #c"` returned (`f12`): a `#` behind other text, kept as is;
- a parameter-list comment over two lines with no `#` line on the expression-body path (`g01`);
- a returned string with no `#` line inside an `#if A` chain (`g02`): ` else` is no directive, so without `A` the whole chain is disabled.

The audit-6 fixtures are all still `no-fix-offered`.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 25 | low | `RH3004UseLambdaSyntaxCodeFixProvider.cs:483` `ContainsStringOrCommentSpanningLines(…, bool keepsBlockBody)` and its call at `:699` | – (naming) | – | PR-introduced | The name says "holds a string or comment over several lines", but on the expression-body path the method returns `false` for such a carrier unless it has a `#` line. The summary and the `<returns>` text say "…that withholds the fix"; the name does not. At the call site, `ContainsStringOrCommentSpanningLines(…) == false` reads as "no multi-line string", while `g01` and `FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBody` are fixed with one. No behavior is wrong. | Rename it to say what it decides, e.g. `ContainsWithholdingSpanningCarrier` or `IsWithheldBySpanningStringOrComment`. No test change. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH3004-008 condition 1 (`ContainsStringOrCommentSpanningLines`, both paths, `ContainsLineStartingWithHash`) | every `IsString` node in the anonymous method; every `/* */`/`/** */` trivia whose span lies in `anonymousMethodExpression.Span`; on each later line of the carrier, the first character after `SyntaxFacts.IsWhitespace`, if it lies inside the carrier | RH3004-008's span "from its first token through the block's closing brace — its parameter list included"; every edit of the fix (the deleted `delegate`, `=>` behind the list, the deleted `{ return`/`; }`, the moved block lines) lies in the same span | identical. Trailing trivia of `}` is outside the span and receives no edit (`f01`, `f02`); leading trivia of the first token is member (d) | holds |
| RH3004-008 condition 2 (block path, any spanning carrier) | same carriers, without the `#` condition | `GetExpressionBody(…) == null`, the same test `ApplyCodeFixAsync` uses to choose the path | identical | holds |
| RH3004-009 (`ContainsDirectiveOrDisabledText`) | unchanged, `anonymousMethodExpression.Span` | the span of condition 1 | identical spans, so case 2 of the completeness argument can only arise before the first token | holds |
| RH3004-004 (`ContainsDiscardedTrivia`) | unchanged gaps | now also inside condition 1's span | overlapping, no gap | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| A span starts and ends on different lines | `rg -n "SpansLines\|IsSingleLineSpan\(" src`: `SyntaxNodeUtilities.IsSingleLineSpan`/`IsSingleLine` (Core), used by RH3004 and RH5408. `ListElementInteriorJoinRewriter.SpansLines` is formatter-private and not reachable from a code fix | one owner reachable from code fixes; #24 closed |
| A line inside a token starts with `#` behind lexer whitespace | `rg -n "'#'\|SyntaxFacts.IsWhitespace" src`: only `ContainsLineStartingWithHash`. RH6004's analyzer checks real directive lines, a different question | one owner |
| A node is a string | RH3004's private `IsString`, unchanged | unchanged by the delta |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixWithheldForASingleReturnOfAStringThatHoldsALineStartingWithHash` (+ CRLF, + form feed, + spaces and a tab) | expression-body path; withheld by condition 1 alone (no comment, no directive, no block path) | a fix offered | `VerifyNoCodeFixIsOffered` (form feed and tab inserted by ordinal `Replace`, not normalized by the harness) | adequate |
| `FixRewritesASingleReturnOfAMultiLineVerbatimStringWithoutAHashLineAsAnExpressionBody` (+ CRLF), `FixRewritesASingleReturnOfAStringWhoseLineHoldsHashBehindOtherTextAsAnExpressionBody` | the other side of condition 1 (FC-003): no `#` line, or `#` behind text, is offered with exact output | no fix, or a different text | `Verify` | adequate |
| `FixWithheldForAReturnedStringThatHidesTheElseOfAChainAroundTheAnonymousMethod`, `…ParameterListCommentThatHidesTheElseOfAChainOnTheBlockBodyPath`, `…OnTheExpressionBodyPath` | finding 23 (a)–(c) verbatim | a fix offered | `VerifyNoCodeFixIsOffered` | adequate |
| `FixWithheldForASingleReturnOfAMultiLineRawStringLiteralThatHoldsALineStartingWithHash` | RH3004-007/-008 boundary for raw strings | a fix offered | `VerifyNoCodeFixIsOffered` | adequate. Its other side, `FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody`, is unchanged |
| `FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath` (+ CRLF), `FixWithheldForAParameterListCommentWithAHashLineOnTheExpressionBodyPath` | the parameter list is in the span on both paths | a fix offered | `VerifyNoCodeFixIsOffered` | adequate. The expression-body side without a `#` line in the parameter list has no test; see *Hints* |

## Failure classes dismissed

- FC-001/FC-002: see *Guard scope*. "Parameter list included" and "first token through the closing brace" are explicit in `anonymousMethodExpression.Span`.
- FC-003: both sides are tested for the `#` condition (with and without a `#` line, `#` behind text, whitespace kinds) and for the path. The place side on the expression path is a hint.
- FC-004/FC-005/FC-019/FC-020/FC-021: no gap rewrite changed. Directives remain RH3004-009's, and gap comments RH3004-004's.
- FC-006: the reparse and the rewrite are unchanged. Only the guard widened.
- FC-007/FC-009: every offered probe converged in one iteration. A withheld fix registers no action.
- FC-008/FC-011: `DocumentOrderFixAllProvider` re-runs `RegisterCodeFixesAsync` per diagnostic on the current document (`GetFixedDocumentAsync`), so the guard applies under Fix All too.
- FC-010, FC-012–FC-014, FC-028, FC-030: no formatter code changed. The fix still formats only the lambda, and `f01`/`f02` show no edit behind it.
- FC-015: see *Policy ownership*.
- FC-016: the class is closed by construction inside the anonymous method (see *Class completeness*). The only remaining member is (d), out of scope by row 51.
- FC-017: see *Assertion adequacy*.
- FC-018/FC-023/FC-024/FC-025/FC-026/FC-029/FC-036: no rename, identifier handling, member kind, token re-resolution, or key/value layout changed.
- FC-022: verbatim, raw, interpolated raw and UTF-8 raw literals are probed (`f03`–`f11`).
- FC-027: nesting is probed with an anonymous method nested in the returned expression (`f08`), and `async` with `f09`.
- FC-031: the summaries of `ContainsStringOrCommentSpanningLines` and `ContainsLineStartingWithHash` describe the new code. No stale "block body only" or "an expression body is laid out by the formatter" claim remains in the provider. No `TODO` and no tracker number.
- FC-032: every probe ran on a fresh build of `816960bf`.
- FC-033: every probe ran under LF and CRLF with the same outcome, and the tests insert form feed and tab after the raw literal.
- FC-034/FC-035: the tests copy the audit-7 fixtures verbatim. The expected values come from RH3004-008's examples. The `b #c` output follows the same layout as the spec's `@"a⏎b"` example.
- FC-037: the delta makes no comment-only change to a `.cs` file.

## Visible output is decided

- The flipped test and the 10 new withheld tests: row 50, decided under the standing instruction, with SHARED-FIX-003 behind it.
- The three offered outputs: RH3004-008's `@"a⏎b"` example (with CRLF), and the `b #c` shape that its *Line start* bullet decides as offered, laid out as in that example.
- No rule page changed.

None is an undecided output.

## Scope

Every changed path is named in handovers 45–48 or rows 50–52.

## Architecture findings

_None new._ The structure behind member (d), "is this span the same code in every build", owned by no component, is recorded in `44-auditor.md` and decided by row 51 as a repository-wide follow-up. One more (d) member follows from the completeness argument, case 2: a directive of the analyzed build that stands before the anonymous method, hidden in another build by a string that opens before it. It belongs to the same follow-up, also on `main`. I did not construct it as a separate fixture.

## Hints

1. The expression-body side of RH3004-008's *Place* bullet has no test: a spanning carrier with no `#` line in the parameter list (`g01`), which the spec offers. The guard has no place-specific branch, because both places go through one `.Any`, so a regression only there is unlikely. Handover 46 left it out because the spec gives no output for it. In `g01`, the fix moves `(int x /* first` left by the width of `delegate `, and the comment's second line keeps its column. The comment text is unchanged, so no value or comment is lost. This layout is not decided by any example. A spec example, or no test, is the lead's call.
2. Earlier hint 1 of `44-auditor.md`: state row 47 in the PR as a reversal of maintainer answer 7, not only as a narrowing. The delta does not change this.
