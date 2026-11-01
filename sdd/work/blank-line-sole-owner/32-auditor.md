Role: auditor
Commit: ddcdbf41741999290f92aa24c5359c5279d2a0db
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict; none was used.

## Scope

Repair round 3 of thoenissen/Reihitsu#1496, delta against b20875e (`24-auditor.md`). Head ddcdbf4 (checked out, clean). `origin/main` = 741157c9ca6fd4e16b144da4cbd7be2f354aec8b (`git ls-remote`), the run's base and an ancestor of the head. `scripts/lane-check.sh behavior-change`: confirmed. The diff `b20875e..ddcdbf4` matches handovers 25–31: FMT-BLANK-011/018/020 and the change doc (entries 20–22, the classification decision), `BraceBlankLinePolicy`, the two blank-line rewriters, `ChainLineBreakRewriter`, `TokenGapNormalizer`, `TriviaListUtilities`, two new test classes, and one RH5204 expected value. The spec changed again (FMT-BLANK-011/018/020), so I audited those three requirements, the new predicates, and the chain-rewriter refactor in full. For the parts the repair did not touch (FMT-BLANK-017, RH5025-005, the directive answers, FMT-BREAK-030's initializer guard), I reused the evidence of `14-auditor.md` and `24-auditor.md`, because that code is byte-identical. I trusted the cleaner's build (0 warnings) and the roles' full suite results (formatter 3711/3711, analyzer 6351/6351, core, CLI, architecture).

Method: I built the CLI from ddcdbf4 (head), 741157c (`main`), and b20875e (previous audit) in scratch worktrees, which I have since removed. I formatted five fixtures under LF and CRLF, two passes each, and diffed the outputs between revisions:
- `t1`: about 35 token kinds behind `/* x⏎ */`: namespace, type, accessor, lambda, switch and if braces, `else`, `catch`, `finally`, do-`while`, a `while` statement, `case`, ternary `?` and `:`, a binary operator, query clauses, the switch expression `{` and arm, an argument, an argument-list `)`, `where`, attribute lists, `=>`.
- `t2`: chain links behind a multi-line comment. It covers link 0, a later link, a prefix link, `?.`, a chain inside an argument, a lambda, an interpolation hole, a field initializer, and an expression-bodied member. It also covers a chain rooted in an initializer `}`, a trailing `/* …⏎ */`, `// c` plus a multi-line comment, `/* a */ /* x⏎ */`, and two links in a row.
- `t3`: an enum member, a base list, a constructor initializer, attribute lists, parameters, the ternary in both positions, a switch `when`, a lambda `=>`, a declaration `;`, `else if`, type arguments, initializer elements, and object-initializer braces.
- `t4`: every failing member of #6, with and without the author's blank line.
- `t5`: a single-line comment in front of a multi-line glued one, two multi-line comments in a row, a documentation comment that leads the line, `#if`, a `#pragma` gap, a trailing multi-line comment, statements and block statements after `}` and after `a++;`, and a comment at the end of the file.

Every fixture was stable on all three revisions. On each revision, the CRLF output equaled the LF output.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 6 | closed | `t4`: every member that 24-auditor listed as failing now produces output byte-identical to `main`, with and without the author's blank line. These are the method `}`, `/* a */ /* x⏎ */ }`, the method-body `{`, the type `{`, the object-initializer `{` and `}`, the assigned collection initializer `{`, the collection-expression `]`, the list-pattern `]`, and the parenthesized-pattern `)`. The one difference from `main` is `a++;⏎// c⏎⏎/* x⏎ */ }`, which now gives `a++;⏎⏎// c⏎/* x⏎ */ }`; maintainer answer 4 and change-doc entry 9 decide that. Structurally, row 30 is in place: `BraceBlankLinePolicy.FindTokenLineStartIndex` is the one classification. `FindGluedCommentIndex` reads it, and through `FindGluedCommentIndex` so do FMT-BLANK-011 and FMT-BLANK-018. The FMT-BLANK-020 trigger reads it directly. The `ContainsLineTerminator` condition that left the third position is gone. No new comment shape fell between the requirements in `t1`–`t5`. |
| 7 | closed | Row 29 is corrected. Entry 21 lists the case, and `KeepsBlankLineBetweenDirectiveAndWhileStatement` and `RemovesBlankLineBetweenDirectiveAndDoStatementWhile` pin both sides. |
| 8 | closed | Entry 22 lists the case, and the change doc's FMT-BREAK-030 bullet names it. The three directive tests in `BlankLineBehindDirectiveBeforeTokenTests` pin it. |
| 9 | closed | `TriviaListUtilities.FindFirstContentIndexOnLine` and `IsOrdinaryComment` are now the formatter's only copies, and both phases call them. `TokenGapNormalizer`'s private copies are removed. The new call `FindFirstContentIndexOnLine(trivia, lastContentIndex + 1)` equals the old `FindLineStartIndex(trivia, lastContentIndex)` for a content index: a content index is never end-of-line trivia, and the `EndsWithLineBreak` case short-circuits before the call. `BlankLineTriviaBoundaryRewriter.FindLineStartIndex` is a different helper, which walks back over whitespace only. `CommentPositionUtilities.IsOrdinaryComment` lives in the analyzer assembly and predates the run. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 10 | low | `BraceBlankLinePolicy.FindTokenLineStartIndex` and `FindGluedCommentIndex`, `<param name="token">` ("still part of its tree"); `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail`, same parameter; the callers `BlankLineTokenCleanupRewriter.RemoveBlankLinesBehindLastOwnLineComment` and `RemoveBlankLinesAboveGluedComment` | FC-031 | – | PR-introduced | The docs state a precondition that two callers do not meet. `BlankLineTokenCleanupRewriter.VisitToken` detaches the token before it calls these two methods; its own comment says "a token it rewrites is detached", and `CollapseLeadingBlankLines`, `RemoveLeadingBlankLines`, and the directive cleanup rewrite it first. For a detached token, `BreaksLineBehindMultiLineCommentTail` finds no parent, so it returns `false`. The answer is still right today, because both callers only pass tokens of the FMT-BLANK-007/008/019 and FMT-BLANK-020 sets, and none of those is a chain-link operator. But the documented contract ("still part of its tree") is not what the code receives. A future caller that passes a detached link operator would get "on the token's line" with no warning. Text only: no output changes. | Make the parameter docs say what the code does. For example: a token no longer in its tree is classified without the chain-link case, which is exact for every token that is not a chain-link operator. Alternatively, pass the original token from the cleanup rewriter. |

**Why `pass, text cleanup`.** #10 concerns the XML documentation of internal members. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. The behavior those comments describe holds for every caller in the tree.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment position relative to the token (FMT-BLANK-018 classification) | From the token back to the nearest end-of-line trivia or line-ending structured trivia (`///`, a directive). Every comment in that run is on the token's line, including a multi-line `/* */`. The result is `-1` for an empty token, and for a token for which `BreaksLineBehindMultiLineCommentTail` holds | FMT-BLANK-011's skip (via `FindGluedCommentIndex`); the FMT-BLANK-020 bail-out (`FindTokenLineStartIndex >= 0`); the line-break phase's moves of tokens behind `*/` | Agree. Only the chain rewriter moves a token off a `*/` line (I grepped for `IsBehindMultiLineCommentTail` and strict `StartsLine` readers in the line-break phase). `t1` and `t3` show that no other rewriter moves any of about 50 token kinds, and every output is stable | ok |
| Glued (FMT-BLANK-018) | the first content on the token's line is an ordinary comment that starts its line, followed only by whitespace and comments | FMT-BLANK-011, which applies above a documentation comment that leads the token's line; answer 4 for an own-line comment in front of a glued one | Agree (`t5` M1–M3, `t4` M11) | ok |
| Row 35: chain link moved off a `*/` line | `IsBehindMultiLineCommentTail(token)`, then a line break in the gap, then `GetWrappedLayoutLineBreaks(chain)[link]` for the outermost chain (through `RootChainBoundary`) whose link operator is the token | `ApplyWrappedLayout` (calls `EnsureLineBreak` exactly where `lineBreaks[i]` holds, for i ≥ 1) and `EnsureLineBreak` (appends a line break behind the comment exactly when the gap holds a line break and `IsBehindMultiLineCommentTail` holds) | Agree. With a line break in front of the comment, the link `StartsLine` (`IsFirstOnLine` reads `AreSeparatedByEndOfLine`), so the chain is wrapped and `lineBreaks[i]` is true for every i ≥ 1. The predicate therefore reduces to syntax: a link operator other than link 0, behind a `*/`, with a line break in its gap. That gap holds a comment, so no rewriter in between joins it, and `ListElementInteriorJoinRewriter.IsOperatorWrap` excludes member-access, conditional-access, and member-binding operators anyway. `t2` covers links 0, 1, and n, prefix, `?.`, nested, lambda, initializer root, interpolation, field, and expression body: all stable, with the comment classified as on its own line exactly where the link moves | ok |
| Chain-rewriter refactor (`GetWrappedLayoutLineBreaks`, `CloseGap`) | per link: link 0 closes its gap; for a prefix link the user's wrap decides; the first invoked link breaks when the user wrapped it or the root line is broken; every later link breaks | the old `ApplyWrappedLayout` and `TryCloseGap` | Equivalent: the same reads on the same original tree, the same order, and the same `isRootLineIntact` updates; the link-0 intactness equals `TryCloseGap`'s return value. `t2` output equals `main` and b20875e for every chain that has no glued-comment difference | ok |
| FMT-BLANK-020 trigger | bails out when any content stands on the token's line | the previous `FindFirstContentIndexOnLine(trivia, Count) >= 0` | Identical for every FMT-BLANK-020 token: the empty-token and chain branches reach no token of that set | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Comment position relative to the token | `BraceBlankLinePolicy.FindTokenLineStartIndex` only. Readers: `FindGluedCommentIndex`, and `BlankLineTokenCleanupRewriter.RemoveBlankLinesBehindLastOwnLineComment` | One owner |
| Comment glued to a token | `BraceBlankLinePolicy.FindGluedCommentIndex`. Readers: `BlankLineTriviaBoundaryRewriter.EnsureBlankLineBeforeFirstComment` and `BlankLineTokenCleanupRewriter.RemoveBlankLinesAboveGluedComment` | One owner |
| Per-link line break of a wrapped chain | `ChainLineBreakRewriter.GetWrappedLayoutLineBreaks`. Readers: `ApplyWrappedLayout` and `BreaksLineBehindMultiLineCommentTail` | One owner. The rewriter applies the conjunction "a line break, behind a `*/` tail, broken" through `ApplyWrappedLayout` plus `EnsureLineBreak`, not by calling `BreaksLineBehindMultiLineCommentTail`. The same three helpers decide both sides, and parity holds (see *Guard scope*); hint below |
| First content on a trivia line / ordinary comment | `TriviaListUtilities` (formatter); `CommentPositionUtilities.IsOrdinaryComment` (analyzer assembly, predates the run) | One owner per assembly |
| Layout reads in the blank-line phase | Only through `BreaksLineBehindMultiLineCommentTail`, which reads syntax and the user's line breaks in front of link operators (row 35) | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `MultiLineGluedCommentBlankLineTests`, the #6 members (18 tests: braces, brackets, pattern `)`, with and without the author's blank line) | exact output equals `main`'s; LF and CRLF; the second pass is stable; each phase alone is stable | a blank line inserted or kept above `/* x` | `AssertRuleResult`; `AssertBlankLinesAbove` for the parenthesized-pattern `)` (the label `/* z` is unique) | adequate |
| `MultiLineGluedCommentBlankLineTests`, entry 20 (array, `else`, `catch`, `finally`, do-`while`, complex element, assigned `}`, documentation comment between, argument `)`, first chain link, member, statement) | removed where the token's rule removes; nothing inserted elsewhere; the author's blank line kept | a FMT-BLANK-011 line above a glued comment | `AssertRuleResult`; `AssertBlankLinesAbove` where no requirement decides the column | adequate |
| The FMT-BLANK-018 example, the not-glued side (`/** d⏎ */ {`, `/* x⏎ */ // c⏎⏎}`), and the statement and block-statement rules above a multi-line glued comment | the spec example verbatim; both sides of the classification | a doc-led comment losing its line; a statement rule not inserting above the comment | `AssertRuleResult` | adequate |
| The three chain tests (later link, author's blank line, prefix link) | the moved link's comment gets FMT-BLANK-011's line in pass 1; pass 2 is stable | no line in pass 1 (the FMT-PIPE-004 break that row 35 prevents) | `AssertRuleResult` (second pass) | adequate. A first-invoked link and a nested chain are not pinned, but by the reduction in *Guard scope* they take the same branch as the later-link test; `t2` holds |
| `BlankLineBehindDirectiveBeforeTokenTests` (entries 21 and 22, both sides) | the blank line behind or above a directive is kept or removed as FMT-BLANK-008, FMT-BLANK-009, and FMT-BREAK-030 require | `main`'s removal, or a kept line between the directive and `{` | `AssertRuleResult` | adequate |
| `RH5204…VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` (one expected blank line removed) | the code-fix result, columns included | – | `Verify` | adequate; covered by row 37 (guidance) |

**Visible output is decided.** Every expected value that the repair adds or changes is one of the following:
- `main`'s output: the #6 members, and the chain links that move.
- A comment on a line with code, which the recorded guidance covers: entry 20, and rows 36 and 37.
- An earlier answer: 1, 4, and 7, and the 2026-10-28 revision.
- An output an unchanged requirement demands: entries 21 and 22.

My fixtures found no head-versus-`main` difference outside those groups. The one apparent exception is `a++;⏎#pragma …⏎⏎/* y⏎ */ }`, where `main` inserts a blank line above the directive and the head does not. That is the implementer-stage answer A, unchanged since b20875e.

## Failure classes dismissed
- FC-006, FC-018, FC-022–FC-026, FC-029, FC-036: the repair rewrites no syntax, literal, identifier, or name. It decides blank lines, and the chain refactor applies the same edits as before.
- FC-007, FC-008, FC-009, FC-030: no code fix changed. The RH5204 fix's output differs only by the blank line above a glued comment, through the same node-level pipeline.
- FC-010, FC-011, FC-012: every fixture is stable in two passes under LF and CRLF, including the chain shapes where the line-break phase moves a token after the blank-line phase decided (`t2`).
- FC-013: RH5020 now reports the glued multi-line comments that the formatter leaves without a blank line. Row 33 and the widened change-doc sentence decide this. RH5024–RH5027 report nothing the formatter now writes: the branch only removes blank lines before those tokens, or keeps them where no rule reports.
- FC-004, FC-005, FC-019, FC-020, FC-021: no comment, directive, or disabled text is dropped or moved in any fixture. Directive gaps are covered by entries 21 and 22 and the existing answer-5 and answer-6 tests, and the classification stops at a directive's line break.
- FC-028: the chain predicate reads `FormattingContext.RootChainBoundary`, the immutable value that `ChainLineBreakRewriter.NormalizeIfOutermost` reads, so a node-level root decides "outermost" the same way in both phases.
- FC-015, FC-016: see *Policy ownership*.
- FC-017, FC-033, FC-034, FC-035: see *Assertion adequacy*. The expected values come from the spec examples and change-doc entries and were checked against `main`'s production code (26-test-implementer).
- FC-032: production code is unchanged since the implementer's commit, and the cleaner's and spec checker's runs were on it.
- FC-037: no text-only proof is claimed.
- FC-001, FC-002, FC-003: reached; see *Guard scope*. No mismatch found.
- FC-031: reached; #10.

## Architecture findings

_None._ The class that #2 and #6 shared (comment shapes falling between FMT-BLANK-011, FMT-BLANK-018, and FMT-BLANK-020) is closed by construction. One function now returns one boundary index, every comment on one side of it is on the token's line, and every comment on the other side is on its own line. Row 35's condition holds:
- The chain decision is one function, `GetWrappedLayoutLineBreaks`, owned by the chain rewriter and read by both phases.
- For a link behind a `*/`, its answer reduces to syntax facts that no phase in between changes.
- The blank-line phase keeps no copy of it.

## Hints
- Row 35, literal reading: the chain rewriter reads `GetWrappedLayoutLineBreaks`, `IsBehindMultiLineCommentTail`, and `HasLineBreak` separately, in `ApplyWrappedLayout` and `EnsureLineBreak`. It does not call the composite `BreaksLineBehindMultiLineCommentTail`. Parity holds today. If `EnsureLineBreak`'s leading-trivia branch ever changes its condition, the composite has to follow it. Having that branch call the composite would make the shared predicate literal.
- `FindTokenLineStartIndex` walks every ancestor of a non-link token behind a `*/` tail until it reaches an outermost chain node, and builds that chain only to find no matching link. This is correct, and rare enough not to matter. Checking for a link-operator token first would avoid it.
- Pre-existing and identical on `main`, not this run's:
  - A blank line is inserted inside an interpolation hole above a moved chain link (`$"{l.Where(…)⏎⏎/* c⏎ */⏎.Count()}"`).
  - `l.Where(…) /* c⏎ */ .ToList()` loses the space behind `*/`.
  - `a++; /* t⏎ */ }` keeps the `}` behind the trailing comment (SHARED-COMMENT-012; 25-spec-author noted it).
  - The first line of a comment in front of a ternary, binary, lambda-brace, or switch-expression token sits at an odd column.
