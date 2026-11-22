Role: auditor
Commit: 023c066632d1a50fb938aff36b437feef845653d
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to ignore.

## Scope

This is a delta audit of the decision-17 architecture repair, against 15526cc9a988579d10c39395fcd2f14e1ca26fc6 (the previous audit, `17-auditor.md`). The head 023c066 is checked out and the tree is clean. `origin/main` and `git ls-remote origin refs/heads/main` are both 1788f94d265c4e13052d5145404ae3ffd21e6acc, which is an ancestor of the head. `scripts/lane-check.sh bug-fix` gives: confirmed.

`git diff 15526cc..023c066` matches handovers 18–22 and decisions 17–20 in `00-lead.md`:
- FMT-BREAK-044's structural part is rewritten as an interpolation-level refusal. Its *Why* gains one clause, and the second example gains `u`, `v`, `t`, and `w`. The structural-transforms non-goal pointer is reworded.
- In `ControlFlowBraceTransform`, the per-brace `forbidsLineBreak` guard is gone (`rg forbidsLineBreak` finds nothing). The new private `IsInSingleLineInterpolationWithDocumentationComment` takes its place, and `VisitIfStatement` returns before any wrap when it is true.
- The `DocumentationCommentFormattingPhase` summaries are comment-only changes (finding #4).
- The test file has 17 new tests and 1 re-derived test.

Scope did not grow. The production delta is one file (+31/−16) plus the two summaries. No new type and no public member were added.

For evidence I used the Release CLI built at 20:04, after the last `src` commit 6626218 (20:02). I copied it to the scratchpad and ran it with `--lang-version 10` and `--lang-version 11`. I compiled every input and output with the scratchpad Roslyn probe, under `CSharp10` and under `CSharp11` for the C# 11 outputs. Everything compiles. A second C# 10 pass with `--check` gave 0 of 11 files to change. The spec's second example run through the CLI equals the spec's C# 10 and C# 11 blocks, apart from the final newline that the cleanup phase removes.

### Previous findings
- **#3: closed.** All three listed members are now pinned by `AssertKept` tests and keep their input: nested `if`, nested `else`, and three levels. The members the old audit listed as holding were re-derived under decision 17, and each test matches the amended spec. The refusal no longer depends on the transform's prediction of an outer inserted `}`. Inside a refused interpolation, every `if` gives the same answer: a nested `if` is the direct body of its parent, so it has exactly the parent's interpolation ancestors. `GetTokenBehindInsertedCloseBrace` therefore returns `default` only where the outer body is in fact wrapped.
- **#4: closed.** Both summaries now state the FMT-BREAK-044 exception. The cleaner's text-only proof passed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | medium | `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/NonVerbatimInterpolationLineBreakTests.cs`, which covers `ControlFlowBraceTransform.IsInSingleLineInterpolationWithDocumentationComment` (`ControlFlowBraceTransform.cs:166–168`) | FC-003, FC-002 | – | PR-introduced | **Why it matters.** The new predicate has a condition, `&& IsInsideSingleLineInterpolation(interpolation, languageVersion)` inside the `Any`, that is tested on its true side only. It implements decision 19's qualifier "an interpolation **of a non-verbatim interpolated string** that contains the body". The false side is a documentation comment in a verbatim (`$@"`) interpolation that stands in no `$"` interpolation and contains the `$"` hole with the `if`, outside that hole. **What the tests cover.** The two mixed-nesting tests and the sibling test exercise other branches. In the `$@"`-outer test the comment sits inside the inner `$"` hole. The `$"`-outer test makes the condition true, and in the sibling test the comment's interpolation is not an ancestor. **The gap.** If the condition were deleted, every test would stay green, yet this input would wrongly keep its `if` unbraced. Today's output is correct (the CLI at C# 10): `var s = $@"a {$"x {F(() => { if (b) return 1; return 2; })}" /** d */} b";` gives `var s = $@"a {$"x {F(() => { if (b) {return 1; }return 2; })}" /** d */} b";`. It compiles and is stable. The input is reached through whole-document formatting (CLI `--lang-version 10`) | Add the holding test: input `var s = $@"a {$"x {F(() => { if (b) return 1; return 2; })}" /** d */} b";` (parameter `bool b`), and expect the output above at C# 10 through `AssertResult`. No production change |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The brace transform wraps no `if`/`else` body in a C# 10 `$"` hole whose containing interpolation holds a documentation comment (`IsInSingleLineInterpolationWithDocumentationComment`) | The `if` node of the original tree first gets the Core `IsInsideSingleLineInterpolation` short-circuit. After that, the predicate checks the full `DescendantTrivia` of every ancestor `InterpolationSyntax` that is itself, or stands in, a `$"` interpolation. That includes the alignment clause and the leading trivia of the interpolation's closing `}` | FMT-STRUCT-026's separators in `WrapWithBraces`, which read the leading trivia of the body's first token and of the token behind the inserted `}` (`GetTokenBehindInsertedCloseBrace`) | **Both tokens are inside the checked span.** They belong to the block that holds the `if`, and that block lies inside every interpolation that contains the `if`. So the inspected span is a superset of the decision's span. In a one-line hole, only a documentation comment can lead a token, so no separator can be a line break once the predicate is false. **Constructed boundaries (C# 10):** e01 (the #5 input) gives braced. e02, a doc comment in a nested `$"` inside the same hole, keeps the input. e03, a doc comment in a sibling hole of a nested string inside the same outer hole, keeps the input: the outer hole contains both, as decision 19 says. e04 (else-if chain with a doc comment in a sibling hole) gives braced (decision 20). e05 (an `if` outside the hole whose body holds a `$"` string with a doc comment) gives braced outside, with line breaks only outside the hole. e06 (doc comment in the alignment clause) keeps the input. e07 (`/** d */` inside the format string, which is text, not trivia) gives braced. e08 (local function) and e09 (anonymous method) keep the input. e10 (an already braced outer body) keeps the input. e11 (a `$@"` string with a doc comment on another line) does not refuse the `$"` string on the previous line. The C# 11 outputs are unchanged by the delta | holds (test gap: #5) |
| Nested bodies inside a refused interpolation | The same predicate, evaluated for each `if` | `GetTokenBehindIfStatement`/`GetTokenBehindInsertedCloseBrace`, which predict an outer inserted `}` | A direct body `if` shares its parent's interpolation ancestors, else-if included, so the prediction holds wherever it is read | holds |
| The short-circuit `IsInsideSingleLineInterpolation(ifStatement)` | Language version and the `if`'s ancestors | The `Any` condition, which implies it | It is logically redundant and only saves the trivia scan outside `$"` holes and at C# 11+. It is not dead: removing it would cost time, not change behavior | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| C# 11 boundary and "`$"` hole" | `rg -n "IsInsideSingleLineInterpolation\|CanInterpolationsHoldLineBreaks" src --type cs`. The boundary is only in `StringInterpolationUtilities.CanInterpolationsHoldLineBreaks` (Core:33). The `$"` test is only in `IsInsideSingleLineInterpolation` (Core:46). The callers are `ControlFlowBraceTransform:164,168`, `SingleLineInterpolationUtilities:78,85–86` (outermost-hole selection, a different decision), `DocumentationCommentFormattingPhase:215`, `SwitchCaseBraceRewriter:776`, and RH5417:67–85 | one owner |
| Documentation-comment trivia kind | Core `SyntaxTriviaUtilities.IsDocumentationCommentTrivia`, reused. `CommentPositionUtilities.IsDocumentationComment` and `IsMovableDelimitedDocumentation` predate the run and are untouched (`20-implementer.md`) | one owner |
| FMT-STRUCT-026 separators | Only in `WrapWithBraces`, which no longer has a refusal branch | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 11 `AssertKept` tests: `ElseBodyLedBy…` (re-derived), `NestedIfBody…`, `NestedElseBody…`, `ThreeNestedIfBodies…`, `OuterIfBodyLedBy…`, `InnerIfBodyLedBy…`, `DocumentationCommentInFrontOfNestedElse…`, `ElseIfChain…`, `ElseIfBodyLedBy…`, `…WithDocumentationCommentElsewhere…`, `IfStatementInVerbatimStringNestedInNonVerbatimInterpolationWithDocumentationCommentOutsideIt…` | At C# 10, every `if`/`else` body in a `$"` hole whose containing interpolation holds a doc comment stays as written | Any inserted brace or line break | `AssertKept` → `AssertRuleResult`: exact output, LF and CRLF, a second pass, phase stability. Under `CSharpParseOptions.Default`, `/** */` is a doc comment (FC-040) | adequate |
| `IfStatementInVerbatimStringNestedInNonVerbatimInterpolationWithDocumentationComment…`, `IfStatementInNonVerbatimStringNestedInVerbatimInterpolationWithDocumentationComment…` | The refusal reaches a doc comment in the innermost hole, in both nesting directions | A braced body | Same | adequate. Both also passed before the delta |
| `NestedIfBodiesInInterpolationGetBraces…`, `NestedIfBodiesFollowedByOrdinaryComment…`, `…WhenAnotherInterpolationOfTheStringHolds…`, `…WhenAnotherStringHolds…` | Holding sides: no doc comment, an ordinary comment, a sibling interpolation, or another string means the body is braced | A body left unbraced | `AssertResult`, exact text | adequate (FC-003, true/false of the "holds a doc comment" condition) |
| `NestedIfBodyFollowedByDocumentationCommentInInterpolationIsBracedInCSharp11` | The refusal does nothing at C# 11 | An unbraced or unbroken body | `AssertRuleResult`, full expected text | adequate |
| Missing | The false side of the "non-verbatim interpolation" condition | — | — | finding #5 |

## Failure classes dismissed
This covers the delta only. Everything else is byte-identical to `17-auditor.md`, so its dismissals stand.
- FC-001: see *Guard scope*. The refusal inspects a superset of the separator decision's span.
- FC-002: the qualifiers "non-verbatim", "contains the body", and "documentation comment" are each explicit conditions. The untested side of the first is #5.
- FC-004, FC-005, FC-019, FC-020, FC-021: the refusal only withholds a wrap and moves no trivia. Directives, `//`, and disabled text cannot stand in a one-line hole. `CanWrap` is unchanged.
- FC-006: all 22 C# 10 and C# 11 outputs of the delta corpus compile.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix changed.
- FC-010, FC-011, FC-012: `--check` on the C# 10 outputs gives 0 of 11 to change. The refusal returns the visited node, with no loop and no reported change.
- FC-013, FC-014: no analyzer changed.
- FC-015: see *Policy ownership*.
- FC-016: the round-1 per-brace guard was the only other place that decided this. It is deleted, not kept alongside (decision 17).
- FC-017: see *Assertion adequacy*.
- FC-022: verbatim and non-verbatim nesting in both directions (e01, tests), plus comment-looking text in a format clause (e07, braced because it is not trivia).
- FC-023, FC-024, FC-025, FC-029, FC-036: the delta cannot reach them.
- FC-026, FC-027: nesting was enumerated as nested `if`, nested `else`, three levels, else-if, a braced outer body (e10), a local function (e08), an anonymous method (e09), and nested strings in both directions. All hold.
- FC-028, FC-041: the predicate reads only ancestors of the `if` in the tree the phase receives. A node-level target inside a hole is already follow-up candidate 1.
- FC-031: the new summary and the inline comment in `VisitIfStatement` describe the new code. The `WrapWithBraces` summary lost its FMT-BREAK-044 sentence along with the guard. The summaries of `GetTokenBehindInsertedCloseBrace`/`GetTokenBehindIfStatement` and the `nextToken` parameter still hold (see *Guard scope*). No `TODO` and no tracker number.
- FC-032: the CLI was built after the last `src` commit.
- FC-033, FC-034: the tests use finding #3's exact members and the spec's `u`/`v`/`t`/`w` inputs.
- FC-035: the expected values come from the amended FMT-BREAK-044 (input unchanged) and from FMT-STRUCT-026 with decision 7's spacing (`19-test-implementer.md`), not from a patched copy.
- FC-037: the comment-only change was proven by `scripts/verify-text-only.sh` (`21-cleaner.md`).
- FC-038, FC-039, FC-042: no unsupported shape is involved, and decision 5 stands. The trivia scan descends fully (e02, e03).
- FC-040: with DocumentationMode `None`, `/** */` lexes as an ordinary comment that trails the token in front of it. The predicate then does not fire, and no separator is a line break. This is the `/* c */` holding case, which is tested.
- FC-044: the refusal is one cheap check on the cause (a doc comment in the hole) and models nothing.
- Tab-width matrix: no column changes.

## Architecture findings
_None._ Decision 17 removed the per-brace decision that depended on the transform's prediction of its own outer wrap. #5 is a test gap on a holding side, not the same decision failing, so it does not repeat #1 or #3.

## Hints
- `{x, /** d */ 5}` and `{x, /* c */ 5}` both become `/* … */5` in an alignment clause (e06, and probe `h`). This happens at C# 10 and at C# 11, with ordinary comments too, so it predates the run and comes from horizontal spacing, not from FMT-BREAK-044. It compiles. I did not check it against `main` or the spacing spec.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
