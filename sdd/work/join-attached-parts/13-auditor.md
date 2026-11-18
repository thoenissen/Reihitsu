Role: auditor
Commit: fb01a18f2074f51e806acb23483c312bc09b45b7
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit of thoenissen/Reihitsu#895. Head `fb01a18f2074f51e806acb23483c312bc09b45b7`, base `origin/main` `6d574e1159eef2628a716c472a569e37d9676532`. The base is an ancestor of the head, and `origin/main` resolves to the SHA in the prompt. The diff (`6d574e1..fb01a18`, 44 files) matches the handovers 07–12: one new shared type `AttachedPartGapUtilities`, the rename `ArgumentListLineStartRewriter` → `AttachedPartLineStartRewriter`, deletions in `ChainLineBreakRewriter` and `MethodChainAlignmentContributor`, the owner exclusion in `ListElementInteriorJoinRewriter`, the FMT-UNSUP-014 check in `PrecheckPhase`, the specs, the change doc, and the tests. `scripts/lane-check.sh behavior-change --base 6d574e1` reports `confirmed`.

Issue thoenissen/Reihitsu#895: the requested joins (`list⏎[0]`, `Foo⏎(1)`, `Get()⏎!`, `?[`) are all made, in one pass and under LF and CRLF. The proposed root column for a part that trivia keeps below was replaced by the maintainer's answer to question 6 (declare the shape unsupported). The owner hint was dropped (decision 2). Both changes are recorded in `00-lead.md` and the change doc.

Evidence beyond reading:
- **Probe 1.** `scripts/format.sh`, run twice on a 26-statement fixture: `s⏎![0]`, `((int[])o)⏎[0]`, `-arr⏎[0]`, `arr⏎[0]++`, `default(string)⏎!`, `f⏎!⏎(1)`, `n⏎!⏎?⏎[0]`, a conditional, `s⏎!⏎.Length`, a chain with links, a tuple, `nameof⏎(arr)`, a switch arm, a lambda, `^1`, a range, an interpolation hole with a format clause, `s⏎!!`, `s⏎! == null`, a query, an anonymous object, and two lists. Every part was joined as FMT-BREAK-040 states. The links stayed links. The second pass changed nothing.
- **Probe 2.** Supported link and directive boundaries, run twice: `s // c⏎!.Length`, `s! // c⏎?.Length`, `#if DEBUG` around a `!.` link, `c /* c */ ? 1 : 2`, `s⏎!⏎?.Length`, and `b!⏎.Append(1)⏎!⏎.Append(2)`. None was refused, and the second pass was stable.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which gap is an attached part's gap | `AttachedPartGapUtilities.IsAttachedPartGap(token)`: the token's kind and parent, plus the next token for `?` and `!` | The same method in the join (`AttachedPartLineStartRewriter.CanJoin`), the precheck (`PrecheckPhase.FindTriviaInAttachedPartGap`), and the owner exclusion (`ListElementInteriorJoinRewriter.IsOwnedGap`) | One predicate, three callers | holds |
| Join versus refusal of a gap that holds trivia | Precheck: `previousToken.TrailingTrivia` and `token.LeadingTrivia` for comments, directives, and disabled text (`FindFirstUnsupportedOperatorTrivia`) | The deleted join guard `WouldJoinAcrossUnjoinableTrivia(previousToken, token)` read exactly these two lists with the same trivia classes (`ContainsUnjoinableTrivia`) | Same span, same classes. `GetPreviousToken` skips zero-width tokens; no zero-width token can stand in front of a part's first token (an omitted array size is followed by `]` or `,`, and missing tokens are syntax errors, which the formatter returns unchanged) | holds |
| Locality (FMT-UNSUP-003) | Precheck: `node.Span.Contains(previousToken.Span)` and `previousToken != default` | Join: `root.FullSpan.Contains(previousToken.Span)`, `previousToken` not `None`, span not empty | Descendant tokens lie in the span; the previous token by default never sits in structured trivia; an empty span means a missing token, which is refused upstream. The two answers agree for whole documents, detached nodes, and in-tree targets | holds |
| Which `!` is a part and which starts a link | `StartsLinkOperator`: the next token is a `.` whose parent is a `MemberAccessExpressionSyntax`, or a conditional-access `?` followed by `.` | `FluentChain.Collect`: a `!` in front of `.` becomes the link `!.`; a `!` in front of `?.` is folded into the link `!?.`; a `!` in front of `?[` is folded into the conditional element access | Traced `x!.y`, `x!?.y`, `x!?.b[0]`, `x!?[0]`, `a?.b!.c`, `x?.y!`, `x!..y` (`..` is not a `.`), and `b! ? 1 : 2` (the `?` of a conditional expression): all agree | holds |
| Which `[` is a part | `BracketedArgumentListSyntax` under `ElementAccessExpressionSyntax`, or under `ElementBindingExpressionSyntax` for the `?`–`[` gap | FMT-BREAK-040's exclusions (implicit element access, `new[]`, `stackalloc []`, rank specifiers, collection expressions, list patterns, attribute lists, indexer parameter lists, calling-convention lists) | Each excluded `[` has another parent kind. The test row "Implicit element access" and FMT-BREAK-040's third example cover the common dictionary initializer | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Joining an attached part's gaps | `rg "SuppressNullableWarning\|ElementBindingExpression\|ElementAccessExpressionSyntax\|ExclamationToken" src/Reihitsu.Formatter src/Reihitsu.Core`: only `AttachedPartGapUtilities`. `ChainLineBreakRewriter` no longer joins parts (`JoinConditionalElementAccesses` and the part loop of `NormalizeChain` are gone). `ListElementInteriorJoinRewriter.IsOwnedGap` defers to the utility. `LineBreakListRewriter:70` only classifies an element access's bracketed list for list layout. `IsArgumentListOpenParenthesis` no longer exists | one owner |
| What an attached part is | `AttachedPartGapUtilities` (token level, formatter) and `FluentChain.Collect` (chain spine, Core, also used by analyzers) | Two statements for two scopes. Parity was traced above; the implementer's handover (08) records the same justification. No divergence |
| Column of a part that starts a line | `MethodChainAlignmentContributor`: `MethodChainAttachedPart` and the part `AlignRestOfRun` calls are deleted. `rg AttachedPart src --glob '!*Test*'` finds no other contributor | No owner remains, and none is needed: outside a refused input, a part's first token starts a line only when it is the first token of the formatting root, and an expression root can never start with a part's first token |
| Refusing the shape | `PrecheckPhase.FindTriviaInAttachedPartGap` only. The code fixes ask `FindUnsupportedSyntax`. The guards of RH5101 and RH5102 (`LeadingAndInteriorContainsCommentOrDirective(argumentList)`) serve a node-level target that holds only the `(` (FMT-UNSUP-003), so they stay | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `AttachedPartLineStartTests` (19 tests) | FMT-BREAK-040: exact output for every position and every surrounding construct | A part left split, a list or conditional judged as written, or a collection collapsed | `AssertRuleResult`: exact output under LF and CRLF, a second pass, and `PhaseStabilityCheck` | adequate |
| `UnsupportedAttachedPartCommentTests`, refusal rows | FMT-UNSUP-014 for every owner, gap, trivia kind, documentation mode, and build | The input formatted, or the wrong reason or location | `AssertUnsupportedFor`: output byte-identical to the input, the reason, the tree identity, and the exact location text, under LF and CRLF | adequate |
| `UnsupportedAttachedPartCommentTests`, supported rows | The other side of every condition: other `(`, `[`, `?`, `!`; inside and behind the part; links | The input refused | `AssertSupported`: not refused, plus second-pass stability and `PhaseStabilityCheck` | adequate |
| `…ArgumentListAloneAndInvokedExpressionAloneDoNotHoldTheShape`, `…ElementBindingAlone…`, `…FindUnsupportedSyntaxAnswersForABuiltNode`, `…GivesTheAnswerOfThePrecheck` | FMT-UNSUP-003 locality and parity between the entry points (FC-041, FC-028) | A partial target refused, or the entry points answering differently | `FindUnsupportedSyntax` and the node entry points | adequate |
| `…OutputOfFormattedAttachedPartsIsNotUnsupported` (12 rows) | FMT-UNSUP-005 (FC-038) | A formatted output refused on the next pass | Two passes under LF and CRLF, in three documentation modes | adequate |
| `CliHiddenCodeEndToEndTests.CommentOrDirectiveInFrontOfAnAttachedPartIsReportedAsUnsupported` (7 rows) | The CLI lines of change 6 | Wrong exit code, wrong stderr line, or a file changed | `Program.Main`; asserts the exit code, the exact stderr line, and the file bytes | adequate |
| The fix-withholding tests of RH3202, RH2001, RH7101, and RH5112 | SHARED-FIX-010 with FMT-UNSUP-014 (change 7) | A fix offered | `VerifyNoCodeFixIsOffered`, LF and CRLF where the change doc's example is used | adequate |
| RH5302 `VerifyFixJoins…` (2) | Change 5 | The old split output | `VerifyCodeFixWithEndOfLine`: an iterative fix and convergence | adequate |
| The converted existing tests (`ChainLinkLayoutTests`, `ConditionalAccessWrapBetweenOperatorTokensTests`, `WithElementSpacingTests`, `ArgumentListLineStartTests`) | The kept-break expectations become refusals or joins | As above | `AssertUnsupportedFor` / `AssertRuleResult` | adequate. Every changed expected value is covered by the maintainer's answers (batch 1: 1A–5A and 6.1A–6.3A; batch 2: 1A and 2A) |

## Failure classes dismissed
- FC-001, FC-002: one shared predicate; spans traced above.
- FC-003: both sides are tested: a gap with no trivia (joined), and trivia on one line or across lines (refused).
- FC-004, FC-019, FC-020, FC-021, FC-005: the join lost its trivia guard. It is safe only because no gap that reaches it holds trivia. Traced every rewriter that runs in front of the join: the structural transforms, the region, documentation-comment, and using phases, and the block, initializer, contained-block, assignment, and semicolon line-break rewriters. None writes trivia into the trailing trivia of the token in front of a part, or into a part's leading trivia. A part's first token never starts an expression, so leading-trivia moves at statement or expression starts cannot reach it. Copies made by the field split carry the original trivia, which the precheck already checked. Directives and disabled text are refused with comments.
- FC-006: the join changes only trivia. Traced token adjacencies (`s!!`, `x!?[0]`, `-arr[0]`, `default(string)!`): no lexing fusion. The second pass shows `s!!` reads back as two tokens.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code-fix code changed. Withheld fixes follow from SHARED-FIX-010, and RH5302's iterative-fix test covers convergence.
- FC-010, FC-012: the helpers check LF and CRLF, a second pass, and per-phase stability. Probes 1 and 2 were stable. The initializer and collection constructs decided before the join stay expanded on the second pass.
- FC-011: no loop was added.
- FC-013, FC-014: no rule reports a part below its element (P11). No layout rule flags the joined output.
- FC-015, FC-016, FC-042: see *Policy ownership*. The remaining sibling (postfix `++`/`--`) is already a follow-up candidate in `00-lead.md`.
- FC-017: see *Assertion adequacy*.
- FC-022: the raw-string row joins `"""⏎[0]` onto the closing line; the content is untouched.
- FC-023 to FC-026, FC-029, FC-036: no rename, member-kind rewrite, or key/value layout. `JoinGaps` keys on token identity.
- FC-027, FC-039: positions are tested in a lambda, a hole, a switch arm, an initializer, a local function in a lambda, a nested element access, and a chain.
- FC-028, FC-041: see *Guard scope*, locality row.
- FC-031: the summaries of `AttachedPartLineStartRewriter`, `ChainLineBreakRewriter` (class, `CloseInnerGaps`, `NormalizeChain` comment), `MethodChainAlignmentContributor` (class, `AlignRestOfRun`), `ListElementInteriorJoinRewriter`, `LineBreakPhase`, and `PrecheckPhase` describe the new code.
- FC-032: the cleaner's later change was indentation of one pattern; the merge of `origin/main` was text-only. The validator runs the default set next.
- FC-033, FC-034, FC-035: the expected values come from spec examples or from hand-removed line breaks run through `main`'s formatter. No patched code was used.
- FC-037: no text-only claim.
- FC-038: see FC-004 above and the `OutputOfFormattedAttachedPartsIsNotUnsupported` rows.
- FC-040: rows for every documentation mode.
- FC-044: the check reads only the tokens and trivia that are present. Each build's gap is refused on its own (`ConditionalGroupInAGapIsUnsupportedInEveryBuild`).

## Architecture findings
_None._ Architecture finding 02 (two join sites for attached parts) is resolved: `AttachedPartLineStartRewriter` is the one join owner, and `AttachedPartGapUtilities` is the one gap definition shared with the precheck.

## Hints
- `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs:32-34` (remarks): the inserted parenthetical reads "… (`<see cref="AttachedPartLineStartRewriter"/>`) -, operator wraps …". The ` -,` punctuation is awkward, and line 34 is 173 characters wide while its neighbors are wrapped at about 117. This is comment text only; it is not a finding.

## Follow-up candidates
- Query continuation clauses take the enclosing statement's column, also without any attached part, on this head: `var a = from x in list⏎        select x;` keeps `select` at the column of `var`. This is pre-existing and unrelated to the run. It is the same kind of unowned continuation line as the lambda-body candidate already in `00-lead.md`. Structural cause: no layout contributor owns the column of a query clause after the first.
