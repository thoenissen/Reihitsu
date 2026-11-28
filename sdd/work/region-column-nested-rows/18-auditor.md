Role: auditor
Commit: ce27b81
Verdict: pass

My prompt named the issue, the slug, the SHAs, the audit to check against, and the handovers. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, `13-auditor.md`, `14-test-implementer.md`, `15-implementer.md`, `16-cleaner.md`, FMT-BREAK-046, FMT-BLANK-011/-018, FMT-BREAK-015/-021/-022, FMT-UNSUP-023, and the repair diff.

## Scope
This is a repair delta of 9911c8e..ce27b81, checked against the audit of 78e7b5c (`13-auditor.md`, findings 1–3).

State:
- Head: ce27b81 (`claude/epic-rubin-2swis1`).
- `origin/main`: 5ef477ef96cd565ab4b919d6fc9227c1d84df7ce, confirmed by `git ls-remote`. It is an ancestor of the head.
- While I audited, the spec checker committed 2ce6af5 on top of ce27b81. It adds only `17-spec-checker.md`, so the audited code is unchanged.
- The repair diff matches handovers 14–16:
  - `LineBreakPhase` order: the brace rewriter now runs last.
  - `ArrayInitializerBraceLineBreakRewriter`: `MoveTokenBelowItsLine` on `}`, and the remarks are rewritten.
  - `PrecheckPhase.RegionInsideMember`: early exit, plus the cleaner's form-only LINQ rewrite.
  - Seven tests in `ArrayInitializerBraceLineTests` and three in `ListElementInteriorJoinTests`.
- `scripts/lane-check.sh behavior-change --base 5ef477e --head ce27b81`: `LANE CHECK: confirmed`.
- CLI `--check` over `src tests` at the head: `0 of 2098 file(s) need formatting`.

The repair did not grow into an unrelated surface and did not change a spec.

How I gathered evidence:
- I built Release CLIs from scratch worktrees at ce27b81 (head) and 9911c8e (the previously audited code) and removed the worktrees afterwards.
- I formatted five scratch fixtures (b1–b5) with the head CLI twice, and diffed the output against the 9911c8e CLI. b1–b5 cover about 70 array initializers in different contexts.
- I ran `apply-fix.sh` for RH5020, RH5023, RH5024, RH5111, RH5402, and RH5404 over the head output of the new comment-led `}` layouts. All reported `no-diagnostic` under LF and CRLF.

### Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every failing member that finding 1 listed is now on one line and stable: `{ z⏎.Length }`, `{ z?⏎.Length }`, `{ N(⏎1, 2) }`, `{ w[⏎0] }`, `{ typeof(Dictionary<⏎int, int>) }`, `{ (⏎int q) => q }`, `stackalloc int[] { z⏎.Length }`, and the row `{ { z⏎.Length, 1 }, … }`. I also found joiners the enumeration had not named, and they are fixed too: `{ z.Trim().⏎Length }`, `{ w[⏎0], w[1] }`, `{ new C(⏎1) }`, `{ nameof(⏎a) }`. The members that held still hold: cast, object initializer, binary and ternary wraps, a chain with two wrapped links, the attributed lambda, `default(⏎int)`, and a wrapped tuple. Moving the rewriter to the end changed no other output: the diff of 9911c8e against ce27b81 over b1, b2, and b5 shows only these joins. b1, b2, and b5 also place the initializer in an argument, a second argument, a ternary branch, the right side of `??`, a chain root, a chain argument, a lambda body, a switch arm, a `when` clause, an attribute argument, a property initializer, an `if` condition, `foreach`, `using`, a nested creation, and a jagged array. Every output is stable on the second pass. Nothing predicts a later join (FC-044). |
| 2 | closed | `/* c */ }` behind `new[]`, `=`, `stackalloc[]`, a row, a jagged element, an empty initializer (`{⏎/* c */ }`), and two comments (`/* a */ /* b */ }`) all leave `}` starting the line below the comment. So does a multi-line comment, whether it leads the line (`/* x⏎y */ }`) or trails the last element (`2 /* x⏎y */ }`, `2 /* a */ /* b⏎*/ }`). Holding members: `// c⏎}`, `#endif⏎}`, and `2 /* c */⏎}`. Every output is stable. The blank line above a comment that now stands on its own line comes from FMT-BLANK-011, which this run does not change. The glued-comment rule (FMT-BLANK-018) no longer applies to that comment, because `}` no longer shares its line. No spec example pins a glued comment in front of a multi-line array's `}`. |
| 3 | closed | Three regression tests now pin the base defect, for a collection expression, a collection initializer, and a complex element initializer, each paired with the plain-literal layout. 14 confirms that each one fails on 5ef477e. The second half of the suggested change, naming the fix in the PR, stays on the lead's worklist in `00-lead.md` (A3) and cannot be checked in the tree. |

## Findings
_N/A — no confirmed finding in the repair._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| When an array initializer counts as multi-line (`IsMultiLineArrayInitializer`, unchanged; the order is what changed) | The text from the end of `{` to the start of `}`, on the lines every other rewriter of the phase leaves | FMT-BREAK-046 *On one line* ("once the other requirements of this phase have placed and removed their line breaks"); batch 2 answer 6A | Agrees now: no line-break rewriter runs after it. The later phases (switch-case braces, blank lines, spacing, indentation, cleanup) add no line break inside an initializer. | ok |
| Closing brace starts its line (`NormalizeGapBeforeOwnedToken` + `MoveTokenBelowItsLine` on `}`) | Normalization: the gap's line breaks. Move: `SyntaxTokenPositionUtilities.StartsLine(})`, meaning the line in front of `}`, including a multi-line comment's tail and trailing trivia of the previous token | The opening side and the element behind a row (same `MoveTokenBelowItsLine`); FMT-BREAK-046 comment paragraph; batch 3 answer A | Agrees: all three tokens now use the same predicate and edit. The previous token is always inside the initializer, so a node-level input cannot make `StartsLine` read outside it. | ok |
| Region directive inside a member, early exit (`node.ContainsDirectives == false`) | The node's full span, inactive directives included | The per-token search it short-circuits | Agrees: a node without directive trivia holds no active region directive. A node-level input's first-token leading trivia is inside its full span. | ok |
| Region directive inside a member, cleaner's restructured condition | Same tokens. A token is now excepted only after a region is found in its leading trivia | The old condition `skip ⇔ E ∧ (first ∨ between)` | `return ⇔ ¬E ∨ (¬first ∧ ¬between)` is its exact negation, and the first active region of the trivia is returned as before. Fixtures: a region in a body is refused with exit 2 and the FMT-UNSUP-023 message; regions inside `#if false` and between members are formatted. | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Placement of array-initializer braces | `rg -n "ArrayInitializerExpression\|ArrayInitializerBrace" src/Reihitsu.Formatter`: `ArrayInitializerBraceLineBreakRewriter` only. `LineBreakInitializerRewriter` returns early for arrays. | One owner. ok |
| "Token starts the line below a comment, and the comment keeps its line" | `MoveTokenBelowItsLine` (array rewriter: now `{`, row `{`, the element behind a row, and `}`) and `ChainLineBreakRewriter.EnsureLineBreak`, unchanged | Same as audit 13 (hint 3, not taken in decision 9). The repair adds a call, not a copy. ok |
| Phase order statement | `AGENTS.md` `LineBreakPhase` row ("judged … on the lines the other line-break rules leave"), the rewriter's `<remarks>`, and `CreateRewriters` | The three statements agree. No other comment refers to the old position (`rg ArrayInitializerBrace src`). ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `InitializerWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine` | A chain join leaves the initializer on one line (`=`, `new object[]` with `?.`, `stackalloc`) | Braces moved onto lines of their own | `AssertRuleResult` (exact output, LF and CRLF, second pass, per-phase stability) | adequate |
| `InitializerWhoseOnlyLineBreakTheListLayoutJoinsStaysOnOneLine` | The same for four list joins | Same | `AssertRuleResult` | adequate |
| `RowWhoseOnlyLineBreakTheChainLayoutJoinsStaysOnOneLine` | A row and its outer initializer stay on one line | The row or the outer initializer spread | `AssertRuleResult` | adequate |
| `InitializerWithAWrapTheOtherRulesKeepPutsItsBracesOnLinesOfTheirOwn` | The other side of the boundary: wraps that stay put the braces on lines of their own | One-line braces around a multi-line element | `AssertRuleResult` | adequate. The boundary is tested on both sides (FC-003). |
| `ClosingBraceBehindACommentThatStartsItsLineStartsTheLineBelowIt`, `ClosingBraceOfARowBehind…`, `ClosingBraceBehindAMultiLineComment…` | `}` starts the line below a leading or trailing comment, also multi-line, for a creation, a declaration, and a row. The comment text is unchanged. | `/* c */ };` or `y */ };` | `AssertRuleResult` | adequate |
| `InterpolatedStringStartingTheFirstElementOf…` (3) | FMT-BREAK-015/-021/-022: no join onto `[`/`{` | `[$"a{y}",` / `{$"a{y}",` | `AssertRuleResult`, paired with the plain-literal input | adequate. Each fails on the base (14). |

Visible output (check 10):
- A1 values are demanded by FMT-BREAK-046 and answer 6A.
- A2 values are demanded by FMT-BREAK-046 and answer A. Their blank line above the comment comes from FMT-BLANK-011, which this run does not change.
- A3 values are demanded by FMT-BREAK-015, -021, and -022, which this run does not change.
- The inner columns of the holding test follow FMT-INDENT-018, -020, and -029, which this run does not change.

No expected value is undecided.

## Failure classes dismissed
- FC-001, FC-002, FC-003: checked in *Guard scope*. The boundary of A1 is tested on both sides.
- FC-015, FC-042: see *Policy ownership*. The repair adds no guard for a declared shape.
- FC-016: covered by finding 1's members and the extra joiners above; finding 2's members across creation, declaration, stackalloc, row, jagged, and empty initializers.
- FC-044: the fix is an order change. No component predicts another's join.
- FC-004, FC-019, FC-021: `MoveTokenBelowItsLine` only trims trailing whitespace of `}`'s leading trivia and adds a line break. Every comment keeps its text and its line. Leading, trailing, two-comment, and multi-line comment members were checked.
- FC-005, FC-020: a `}` behind `#endif` already starts its line, so `StartsLine` holds and nothing moves (fixture `m`, stable). The precheck early exit counts inactive directives too.
- FC-006, FC-018, FC-023–FC-026, FC-036: trivia-only edits, no rename. `ReplaceToken` targets the initializer's own `CloseBraceToken`, a unique token.
- FC-007–FC-009, FC-030, FC-043: no code fix changed. The convergence check over the new layouts reports `no-diagnostic` for the six rules above.
- FC-010, FC-012, FC-011: every fixture is a fixed point on the second pass, and the tests run CRLF and per-phase stability. No loop was added.
- FC-013, FC-014: no rule reports the new layouts (above). No new rule.
- FC-017: see *Assertion adequacy*.
- FC-022: a verbatim string and a raw string are pinned by the round-0 tests. The repair changes nothing about literals.
- FC-027: nested and enclosing scopes were sampled (list in finding 1's evidence).
- FC-028, FC-041: `}`'s previous token is always inside the initializer. The precheck's `ContainsDirectives` reads only the input's full span.
- FC-029: no key/value layout.
- FC-031: the rewriter's `<remarks>` and the precheck summary describe the new code. `LineBreakPhase`'s summary has no order claim.
- FC-032: the implementer's full suites ran on b8f1801. The only later production change is the cleaner's form-only precheck rewrite, which the cleaner re-tested with the precheck and brace filters (140/140).
- FC-033–FC-035: the expected values come from requirements and the plain-literal pairing, not from patched code.
- FC-037: not a text-only change.
- FC-038: the moved `}` produces no declared shape. Second passes are not refused.
- FC-039, FC-040: the precheck walk is unchanged in reach. The directive shape does not depend on the documentation mode.

## Architecture findings
_None._

## Hints
- `PrecheckPhase.FindRegionDirectiveInsideMember` (`src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.RegionInsideMember.cs`) now runs `LeadingTrivia.Where(…).Cast<SyntaxTrivia?>().FirstOrDefault()` for every token with leading trivia once the node holds any directive.
  - That covers almost every file in this repository, because they use regions between members.
  - It allocates a boxed trivia list, iterators, and an enumerator per such token. The cost is small next to the phases, and the code is correct.
  - Skipping tokens with `token.ContainsDirectives == false` before the LINQ query would keep the cleaner's form and drop nearly all of it.
- The array rewriter now puts a multi-line array's `}` below a comment that leads its line (FMT-BREAK-046, answer A). An object or collection initializer, a collection expression, and a switch expression keep that comment glued: `/* c */ }` and `/* c */ ]` stay, as FMT-BLANK-018 and `GluedCommentBlankLineTests` pin. The difference is what the maintainer chose for arrays, not a defect. I note it only because a reader may see the two layouts side by side.

## Follow-up candidates
_None._
