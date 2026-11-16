Role: auditor
Commit: 8fde9c4
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict; none was used.

## Scope

Full audit. Head `8fde9c45b9634a7c1bf8cd083fba28ea2c0df519` (checked out, clean tree). Base `origin/main` = `git ls-remote origin refs/heads/main` = `372daeee7a757fe7a3026896a594ece4df03fa8a`, an ancestor of the head. The diff is what the handovers describe: `PrecheckPhase.FindCommentInDeclaration` (6 insertions, 4 deletions), 14 tests in `UnsupportedVariableDeclarationTests.cs`, and `sdd/work/precheck-declaration-scan/`. `scripts/lane-check.sh bug-fix`: `confirmed — claimed bug-fix, diff needs bug-fix`.

**Equivalence of the production change (static trace).** The old loop paired each token with the first value whose `Span` contains the token's span (`values.Find`) and compared the token with `value.GetFirstToken()` / `value.GetLastToken()` (default arguments). The new loop builds the same list in the same order, carries the same `Span`, the same `GetFirstToken()` / `GetLastToken()` results computed once, and selects with the same predicate (`FindIndex` on `candidate.Span.Contains(token.Span)`). `valueIndex < 0` is exactly `value == null`. `SyntaxToken` equality is unchanged. A value of only missing tokens gives `default` in both versions. The `isFirst`, terminator, and trivia-selection logic is untouched. No verdict, reason, or location can differ.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedVariableDeclarationTests.cs`, `CommentInFrontOfTheFirstTokenOfALeftNestedValueIsUnsupported` | FC-017, FC-003 | – | PR-introduced | The test's input is `string s = /* c */ "x" + "p0" + "p1" + "p2";`. Roslyn attaches `/* c */` to the trailing trivia of `=`, a token outside every value, so the scan refuses the input without consulting the value's first token. The test cannot falsify the cached first token of a left-nested value, which is the case where `GetFirstToken` walks a deep spine and which the test implementer's handover claims this test covers ("leading trivia ... unsupported in front ... for a deep-left first token"). Shown by mutation in a separate worktree: with `FirstToken: default(SyntaxToken)` in `PrecheckPhase.cs`, this test still passes; only `CommentOnItsOwnLineInFrontOfAValueIsUnsupported` (single-token value) and `CommentOnItsOwnLineInFrontOfALaterValueOfSeveralTokensIsUnsupported` fail. The same mutation with `LastToken: default(SyntaxToken)` fails 12 tests, including the deep-right `CommentBehindTheLastTokenOfARightNestedValueIsUnsupported`, so the last-token side holds. | Put the comment on its own line in front of the value, so that it is the leading trivia of `"x"`: `string s =` / `// c` / `"x" + "p0" + "p1" + "p2";`. The expected verdict (unsupported, at `// c`) is demanded by FMT-UNSUP-004 ("a comment in front of the value, also on its own line ... refuse[s] the input"). |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which value a token of the declaration's span belongs to | The value's `Span` (unchanged; now read from the cached tuple) | The pre-change `values.Find` on the same span and order | Identical predicate and order | ok |
| Whether a token's leading trivia is checked | The value's first token, `GetFirstToken()` with default arguments, now computed once | The pre-change per-token `value.GetFirstToken()`; FMT-UNSUP-004's "leading trivia of the value's first token" | Identical | ok |
| Whether a token's trailing trivia is checked | The value's last token, `GetLastToken()` with default arguments, now computed once | The pre-change per-token `value.GetLastToken()`; FMT-UNSUP-004's "trailing trivia of its last token" | Identical | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| FMT-UNSUP-004 declaration-comment check | `rg -n "FMT-UNSUP-004\|FindCommentInDeclaration\|VariableDeclarationCommentReason" src` outside tests: `PrecheckPhase.FindCommentInDeclaration` / `GetSpanTokens` (the owner) and the shared `VariableDeclarationCommentReason`; `FieldDeclarationSplitTransform.cs:44,96` only refers to the precheck in comments and holds no copy | one owner |
| Value edge-token lookup | `rg -n "Initializer\?\.Value\|GetFirstToken\(\)\|GetLastToken\(\)" src/Reihitsu.Formatter/Pipeline/Precheck/`: the changed lines; `FindTriviaBehindConditionalOperator` (FMT-UNSUP-008, a different decision, one call per conditional) | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `CommentBehindTheLastTokenOfALaterValueOfSeveralTokensIsUnsupported` | Trailing trivia of a later value's last token is checked against that value's own edge | Supported, or a different location | `AssertUnsupported` (reason, unchanged output, location in caller's tree, LF and CRLF) | adequate |
| `CommentOnItsOwnLineInFrontOfALaterValueOfSeveralTokensIsUnsupported` | Leading trivia of a later value's first token is checked | Supported; fails under the `FirstToken: default` mutation | `AssertUnsupported` | adequate |
| `CommentInsideALaterValueBehindItsFirstTokenIsSupported` | Trailing trivia of a first token that is not the last is excepted | Refused | `AssertSupported` (no refusal, LF and CRLF, second pass stable) | adequate |
| `CommentInsideALaterValueInFrontOfItsLastTokenIsSupported` | Leading trivia of a last token that is not the first is excepted | Refused | `AssertSupported` | adequate |
| `CommentBetweenAValueOfSeveralTokensAndASeparatingCommaIsUnsupported` | Trailing trivia of the first value's last token is checked when a later value follows | Supported | `AssertUnsupported` | adequate |
| `CommentInFrontOfTheFirstTokenOfALeftNestedValueIsUnsupported` | Leading trivia of a deep-left value's first token is checked | None reachable: the comment is trailing trivia of `=`; passes under the `FirstToken: default` mutation | `AssertUnsupported` | weak — finding #1 |
| `CommentBehindTheFirstTokenOfALeftNestedValueIsSupported` | Trailing trivia of a deep-left first token is excepted | Refused | `AssertSupported` | adequate |
| `CommentBehindTheLastTokenOfARightNestedValueIsUnsupported` | Trailing trivia of a deep-right last token is checked | Supported; fails under the `LastToken: default` mutation | `AssertUnsupported` | adequate |
| `CommentInFrontOfTheLastTokenOfARightNestedValueIsSupported` | Leading trivia of a deep-right last token (own line) is excepted | Refused | `AssertSupported` | adequate |
| `CommentOnItsOwnLineInsideAFluentChainValueIsSupported` | A comment between chain links is inside the value | Refused | `AssertSupported` | adequate |
| `CommentBehindAFluentChainValueIsUnsupported` | Trailing trivia of a chain's last token `)` is checked | Supported; fails under the `LastToken: default` mutation | `AssertUnsupported` | adequate |
| `CommentInFrontOfTheValueOfADeclarationInsideALaterValueIsUnsupported` | FC-039: the inner declaration is checked with its own span although the outer value excepts its inside | Supported. The comment is trailing trivia of the inner `=`, so this tests the span, not the inner value's first token; that matches its stated invariant | `AssertUnsupported` | adequate |
| `CommentBehindTheValueOfADeclarationInsideALaterValueIsUnsupported` | FC-039 for the inner value's last token | Supported; fails under the `LastToken: default` mutation | `AssertUnsupported` | adequate |
| `CommentInsideTheValueOfADeclarationInsideALaterValueIsSupported` | The inside of the inner value is excepted | Refused | `AssertSupported` | adequate |

Visible output: every expected value is demanded by the unchanged FMT-UNSUP-004, traced against its exception list (`sdd/specs/formatter/unsupported-syntax.md:129-135`). No expected value of an existing test changed. No rule page changed.

## Failure classes dismissed
- FC-001, FC-002 — the inspected spans and qualifiers ("first token", "last token", "inside") are unchanged; see *Guard scope*.
- FC-004, FC-005, FC-019, FC-020, FC-021 — the precheck changes nothing in a tree; no edit, join, or move.
- FC-006, FC-022 to FC-025, FC-029, FC-036 — no rewrite. FC-022 also: the existing comment-looking-text-in-literal test is unchanged and classification uses trivia, not text.
- FC-026 — tokens are compared by `SyntaxToken` equality (position-based), not re-resolved by text; unchanged.
- FC-027, FC-039 — nesting is covered by the three FC-039 tests and the pre-existing `SpecExampleDeclarationInsideAValueIsCheckedOnItsOwn`. The tuple list is local to each declaration, so outer data cannot leak inward.
- FC-028, FC-041 — `isInputRoot` and `GetSpanTokens` are untouched.
- FC-007 to FC-009, FC-018, FC-030, FC-043 — no code fix changes. `ReihitsuFormatter.FindUnsupportedSyntax` still calls the same predicate (FMT-UNSUP-007).
- FC-010 to FC-014, FC-038 — no phase output changes.
- FC-015, FC-042 — one owner; no guard is added or kept for a declared shape.
- FC-016 — the sibling scans in the same phase: `FindTriviaBehindConditionalOperator` makes one edge lookup per conditional (no per-token cost). `FindTriviaInFrontOfComma`'s cost is already follow-up candidate 1 in `00-lead.md`.
- FC-031 — the summary of `FindCommentInDeclaration` now states that the edge tokens are taken once and why; the rest of it still describes the code.
- FC-032 — the skeptic's harness measured a copy at 24aeea5, not HEAD. Equivalence at HEAD rests on the static trace above and the implementer's full formatter run (5257 passed) on the final code.
- FC-033, FC-034 — no reproduction (time-only change).
- FC-035 — expected values come from the unpatched code (tests were written and run before the change) and from the spec.
- FC-037 — no comment-only claim.
- FC-040 — trivia classification (`SyntaxTriviaUtilities.IsCommentTrivia`) is untouched.
- FC-044 — no modelled result; the edge tokens are read from Roslyn.

## Architecture findings
_None._

## Hints
- `CommentInFrontOfTheValueOfADeclarationInsideALaterValueIsUnsupported` places its comment in the inner `=`'s trailing trivia as well. That is enough for FC-039, its stated invariant. It does not pin the inner value's first-token leading trivia; the fix to finding #1 covers the first-token side generally.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
