Role: skeptic
Commit: 568f167
Verdict: proceed

Inputs read: thoenissen/Reihitsu#894 and its one comment (the maintainer's triage of 2026-11-17, which overrides the body), `00-lead.md`, `01-spec-author.md`, the change doc `sdd/changes/2026-11/unsupported-link-operator-trivia.md`, the spec diff of 568f167 (FMT-UNSUP-015, FMT-BREAK-019, RH2001-007, RH7101-007, AGENTS.md row), FMT-UNSUP-014, SHARED-CHAIN-006, FMT-INDENT-029, RH5201-003, RH5048-001, and the code regions `Reihitsu.Core/FluentChain.cs` (`Collect`, `CollectMemberAccess`, `GetConditionalTokens`, `ContinuesChain`), `FluentChainLink.cs`, `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, `ChainLineBreakRewriter` (`CloseGap`, `CloseInnerGaps`, `NormalizeChain`, `GetWrappedLayoutLineBreaks`, visit overrides), `LineBreakPhase.CreateRewriters`, `LineBreakTriviaUtilities.JoinGaps`, `MethodChainAlignmentContributor` (`AlignRestOfRun`, `Contribute`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

All fixtures below were run on this branch (code identical to `main`; 568f167 changes only text), with `scripts/format.sh <dir>` from `/home/user/Reihitsu`, in temporary directories that are deleted afterwards.

## Premise

Real, and reached by whole-document formatting and the CLI (any file holding the shape); code fixes reach it through the node they format (SHARED-FIX-010).

- `class C { object M(dynamic a) { var r = a? // note⏎            .B(); … } }` (the change doc's first input) → `.B()` moves under the `?` (`                 .B();`). Confirmed.
- Same with `///` own-line between `?` and `.` (`var r = a?⏎            /// doc⏎            .B();`) → both the `///` line and `.B()` move to column 17, under the `?`.
- Directives between `!` and `.`, each in its own method (`return Get()!⏎#pragma warning disable CS0168⏎            .Trim();⏎#pragma warning restore CS0168`; `return Get()!⏎#if false⏎            .ToLower()⏎#endif⏎            .Trim();`; `return u!⏎#if DEBUG⏎            ?⏎#endif⏎            .Length;`) → `.Trim()` moves from column 12 to 20, `.Length` to 16: the rest of the operator goes under its first token. No precheck refuses any of them today.
- A justification comment behind a `!` that starts a `!.` link:

```csharp
var a = services.GetService(typeof(C))! // registered at startup
    .ToString();
```

  gives on `main`

```csharp
var a = services.GetService(typeof(C))
                ! // registered at startup
                .ToString();
```

  (and the same for the wrapped form `services⏎        .GetService(typeof(C))! // …⏎        .GetHashCode();`). The issue's claim that "the operator stays split" is true, and the output is worse than the issue says: the `!` is moved onto a line of its own.
- `scripts/apply-fix.sh RH5201` on the change doc's RH5201 input: `fixed (1 action(s), 1 iteration(s))` under LF and CRLF, output `var r = a.B()⏎                 ? // note⏎                 .C()⏎                 .D();` — the fix is offered today, as the change doc says.

The `?[` part of the maintainer's answer is already met on `main` by FMT-UNSUP-014: `return n? /* c */ [0];`, `return n! /* c */ ?[0];`, and `return n!?⏎#if DEBUG⏎            [0];⏎#else⏎            [1];⏎#endif` are all refused today ("Skipped 3 file(s) with unsupported syntax"; e.g. `E1.cs(7,19): unsupported syntax: a comment or directive in front of an argument list, an element access, or a null-forgiving operator`). The spec author's choice to leave `?[` to FMT-UNSUP-014 therefore fulfils that part of the answer without a second statement; the `?[` reason text stays today's.

The deletion premise holds as well. With only whitespace inside an operator, the line-break phase joins it in every context I tried, so `AlignRestOfRun` only ever acts on a gap that trivia keeps — i.e. on the declared shape. Input (one method):

```csharp
var p = a?
    .B();
var q = s!
    .Length;
var r = u!
    ?
    .Length;
var t = a.B()
         .C()?
         .D();
var i = $"{s?
    .Length}";
var l = xs.Select(y => y?
    .Length);
var f = a // c
    ?
    .B();
var g = w!
    !
    .Length;
```

Output: `a?.B()`, `s!.Length`, `u!?.Length`, `.C()⏎                 ?.D()`, `$"{s?.Length}"`, `y => y?.Length`, `a // c⏎                ?.B()`, `w!!.Length`. Why it holds in code: `ChainLineBreakRewriter` visits every node kind that can be an outermost chain node (invocation, conditional access, member access, element access, postfix unary; member and element bindings are never outermost), `NormalizeChain` calls `CloseInnerGaps` for every link whether or not the chain is wrapped, `JoinGaps` has no further guard, and `WouldJoinAcrossUnjoinableTrivia` refuses exactly comments (documentation comments included), directives, and disabled text — the same set FMT-UNSUP-015 declares. `FluentChain.Create` builds links only from nodes that hold all of the operator's tokens (`GetConditionalTokens` takes the `!` from the conditional access's own expression; `CollectMemberAccess` takes it from the member access's own expression; a member binding without its conditional access yields no link), so a node-level input never gives the phases a link whose operator tokens the input only partly holds. This is the same boundary FMT-UNSUP-015's node-level paragraph draws.

## Mechanism and candidates

_N/A_ for a bug mechanism: this is a declared shape decided by the maintainer. The token set that decides it, by defining type:

- `?.` — `ConditionalAccessExpressionSyntax.OperatorToken` followed by `MemberBindingExpressionSyntax.OperatorToken` (also when the branch is an `AssignmentExpressionSyntax` whose left side starts with the member binding — the null-conditional assignment, which `FluentChain.Collect` follows).
- `!.` — `PostfixUnaryExpressionSyntax` (`SuppressNullableWarningExpression`) that is the `Expression` of a `SimpleMemberAccessExpression`, and that member access's `OperatorToken`.
- `!?.` — the same `!` as the `Expression` of a `ConditionalAccessExpressionSyntax` whose branch starts with a member binding, that `?`, and that `.`.
- Not in the set: a `!` in front of another `!` (`w! /* c */ !.Length` is the front gap of the outer `!.`; the inner `!` is an attached part, FMT-UNSUP-014), `(a!)?.B` (the `!` is followed by `)`), `!?[`, `?[` (FMT-UNSUP-014).

These definitions agree with `FluentChain`'s and are the complement of FMT-UNSUP-014's `!` ("not followed by `.` or `?.`"): no gap is in both shapes and no gap inside an operator is in neither.

## Breaking inputs

Failure classes that reach the change: FC-003, FC-004, FC-005, FC-015, FC-019, FC-020, FC-021, FC-027, FC-028, FC-031, FC-038, FC-039, FC-040, FC-041, FC-042, FC-043, FC-017 (LF and CRLF, two passes).

The shape's matrix (every cell refused unless marked; the test implementer covers each axis at least once per member, and the operator × trivia-side cross fully):

| Axis | Members |
|---|---|
| Operator and gap | `?`–`.` of `?.`; `!`–`.` of `!.`; `!`–`?` of `!?.`; `?`–`.` of `!?.` |
| Trivia side | trailing trivia of the earlier token (same line); leading trivia of the later token (own line) |
| Trivia kind | `//`; `/* */` on one line (no line break in the gap — FC-003); `/* */` spanning lines; `///` and `/** */`, each under `DocumentationMode.None` and `Parse` (FC-040); `#pragma`; `#if`/`#else`/`#endif` with both branches between the tokens (refused in every build); `#if false … #endif` (disabled text only); `#region`/`#endregion` pair (FC-020) |
| Context | sole link; later link of a wrapped chain; prefix link of a call-less chain; null-conditional assignment target (`a? /* c */ .B = 1`); inside an interpolation hole (`$"{s? /* c */ .Length}"`); inside a lambda in an argument; nested inside another link's argument list or a `?[ ]` index (FC-039: `a?.B(s? /* c */ .Length)`, `n?[s? /* c */ .Length]`); local initializer value; field with one declarator (FMT-UNSUP-015's reason); field with several declarators — a comment gives FMT-UNSUP-015's reason, a directive or disabled text gives FMT-UNSUP-010's (overlap paragraph) |
| Entry point | whole document / CLI (reason, location, file unchanged); `FormatNode` and `FormatNodeInDocumentAsync` on the statement (refused); node-level input holding only part (FC-041): the conditional access's `WhenNotNull` with a comment in the `.`'s leading trivia, and the null-forgiving `PostfixUnaryExpressionSyntax` alone with a comment in the `!`'s trailing trivia — **not** refused |
| Code fix (SHARED-FIX-010, FC-043) | the change doc's RH5201, RH5112, RH3202, RH2001 (`= Source? /* three */ .Length;`), RH7101 inputs: diagnostic stays, no fix offered |

The other side (must stay formatted, and a second pass must leave the output unchanged and not refused — FC-038):

- front gap of each operator: `s /* c */ ?.Length`, `s // c⏎!.Length`, `t /* c3 */ !.Length`, `w! /* c */ !.Length`, `a.B() /// d1⏎?.C()`, `a.B() /** d3 */⏎?.C()` — I ran the last three plus `s /// d2⏎!.Length` and the spec's third example: formatted, second pass `Formatted 0 of N file(s)`; no phase moved a comment into an operator;
- `.`-to-name gap: `t?. /* c */ Length` (formatted to `t?. /* c */Length` by FMT-SPACE-013, specified);
- inside a link's argument list, behind a link, `?[` gaps (FMT-UNSUP-014's reason), `c ? /* c */ .5 : 1` (FMT-UNSUP-008), logical `!`, `..`, `->`, `::`.

Whitespace-only operator splits must keep joining in every context after `AlignRestOfRun` and the inner-gap check are removed — the eight-statement fixture under *Premise* is the regression guard (its output is quoted there).

Existing tests that pin today's output for the shape and must change to the refusal (expected outputs of a visible surface; covered by the maintainer's answer): in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConditionalAccessWrapBetweenOperatorTokensTests.cs` — `LineCommentBetweenQuestionMarkAndDotKeepsCommentAndAlignsDotUnderQuestionMark`, `BlockCommentBetweenQuestionMarkAndDotKeepsCommentAndAlignsDotUnderQuestionMark`, `PragmaBetweenQuestionMarkAndDotKeepsDirectiveAndAlignsDotUnderQuestionMark`, `DisabledTextBetweenQuestionMarkAndDotIsPreservedAndDotAlignsUnderQuestionMark`, `OwnLineCommentBetweenQuestionMarkAndDotAlignsCommentAndDotUnderQuestionMark`, `CommentBetweenQuestionMarkAndDotInShortChainAlignsDotUnderQuestionMark`, and probably `CommentOnOneLinkOnlyLeavesOtherLinksJoined`, `CommentOnSoleLinkMovesQuestionMarkOntoItsOwnLine`, `CommentAfterQuestionMarkOnIdentifierRootKeepsQuestionMarkOnRootLine`; in `ChainGapJoinTests.cs` — `SingleLineCommentInsideLinkOperatorKeepsItsLine`, `DirectiveInsideLinkOperatorKeepsItsLine`; in `ChainLinkLayoutTests.cs` — `CommentInsideNullForgivingConditionalOperatorKeepsOperatorStartTogether`. I located these by name; the test implementer reads each to confirm. Analyzer-only tests on the shape stay valid (`RH5201…VerifyCommentHeldLineBreakInsideOperatorOfCallLessChainIsNotReported`, `…InsideLaterLinkOperatorIsReportedForSharingALine`, `RH5048…VerifyCommentInsideConditionalOperatorIsNotReported`): they verify no fix.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `PrecheckPhase` (FMT-UNSUP-015) | must change |
| `ChainLineBreakRewriter` | must change: the trivia check for gaps inside an operator goes (FC-042); `CloseGap`'s check stays for the `.`-to-name gap and the first link's front gap; class summary ("the indentation phase aligns whatever such a gap keeps") must be updated (FC-031) |
| `MethodChainAlignmentContributor` | must change: `AlignRestOfRun` and its call go; class summary sentence about "the rest of an operator" (FC-031) |
| `Reihitsu.Core` `FluentChain` / `FluentChainLink` | must stay: `HasInnerLineBreak(false)` decides `IsWrapped` from the user's input before the join; `HasInnerLineBreak(true)` serves RH5201 only |
| RH5201, RH5048, RH5112 analyzers | must stay (analyzers do not consult the precheck; RH5201-003 unchanged) |
| Code fixes RH5201, RH5112, RH3202, RH2001, RH7101 and every other formatter-delegating fix | no code change; withheld through SHARED-FIX-010 on the shape; covered by tests |
| Fix All | not involved beyond the withheld fix |
| CLI | no code change; reports through the existing unsupported path |
| Rule pages | not involved (`general-notes.md` covers a diagnostic without a fix) |
| Specs / AGENTS.md | done by the spec author; SHARED-CHAIN-006, FMT-INDENT-029 need no change (no requirement states the column `AlignRestOfRun` wrote) |

## Refuse option

The run *is* the refuse-by-declaration option the maintainer chose (escalation 10, answered). The local alternative — leave the construct as written — is today's behavior in substance and would keep both special cases, which is what the maintainer rejected.

Cost of the declaration: one cheap syntactic check in the precheck (token kinds and parent kinds, within the input); deletions of `AlignRestOfRun` (~12 lines and one call) and of the trivia test for inner operator gaps. The latter is a small deletion in lines: `CloseInnerGaps` keeps selecting gaps by line break, only without the trivia test. No new type, no new mechanism.

Span: each gap between two tokens of one `?.`, `!.`, or `!?.` operator — the trailing trivia of every operator token except the `.`, and the leading trivia of every operator token except the first; on any line; excepting the front gap, the `.`-to-name gap, and the `?[` gaps (FMT-UNSUP-014).

What it refuses that people may commonly write: a comment justifying a null-forgiving operator, placed behind the `!` of a wrapped chain (`services.GetService(typeof(C))! // registered at startup⏎    .ToString();`). This is the most plausible common member of the shape. `main` already formats it badly (the `!` moves onto a line of its own, *Premise*), and the maintainer's answer names `!.` explicitly, so it is covered and no new question arises. The PR's review notes should name this input as the visible cost. The workaround the user has is to put the comment in front of the `!` (`GetService(typeof(C)) // …⏎    !.ToString()`), which stays supported as the front gap.

Simpler option: _None._ The guard-only alternative (refuse only gaps with a line break) is the spec author's output question 1, and the maintainer's wording already decides it.

## Output questions

- Spec author's question 1 (a `/* */` inside an operator on one line): **not in question.** The maintainer's answer covers "a comment … between the tokens of" the operators with no condition on line breaks, so the one-line case is part of the decided shape. I recommend the lead does not ask it.
- Spec author's question 2 (the reason text `a comment or directive inside a ?. or !. operator`): the CLI line is visible output, and the maintainer's answer requires "a reason" without wording it. Whether the answer covers the wording is the lead's call. If it is asked, I agree with recommendation A: it names the operators users write and has the same form as the other reasons.

No other case within #894 has an undecided visible output.

## Internal choices

- **One owner for "which tokens form a link operator."** With the inner-gap trivia test removed from `ChainLineBreakRewriter`, preserving a comment inside an operator depends entirely on the precheck refusing exactly the token gaps that `FluentChain` builds into `OperatorTokens`. If the precheck carries its own token definition and the two drift, the line-break phase joins across a comment (FC-004, a safety rail). Option A: the precheck and the chain model read the operator's tokens from one statement of the rule. Option B: the precheck states the syntax-kind definition separately, as FMT-UNSUP-014's `!` test does today, and tests pin the agreement for every row of the token set above. Neither changes visible output. Recommendation: A where it stays a cheap, local check (FC-015, FC-042). Otherwise B with the full operator × side matrix under test. This is the lead's decision, not an implementation proposal.
- **FMT-UNSUP-015's overlap sentence about FMT-UNSUP-004** ("the two meet only outside a value"). I found no input in which a link operator stands inside a variable declaration outside its initializer values. If there is none, the sentence is vacuous and could say the two never meet. It is wording only; I did not compile a search for such an input, so I leave it to the spec checker.
- The spec author's choices (a new requirement rather than widening FMT-UNSUP-014; `?[` left to FMT-UNSUP-014; SHARED-CHAIN-006 and RH5201-003 unchanged) hold against the code and specs I read. I agree with each.

## Issue goals

thoenissen/Reihitsu#894 is `agent-filed`, and the maintainer's triage overrides its body.

- New Layout rule without a code fix — dropped by the maintainer. Visible, but decided.
- "Use `FluentChainLink.OperatorTokens` and `WouldJoinAcrossUnjoinableTrivia` so the rule matches the gaps the formatter refuses" — a prescribed implementation of the dropped rule. Drop it. Its intent, that refusal and phases agree on the gaps, survives as the internal choice above. Not visible.
- `?[` among the operators (issue and triage) — already met by FMT-UNSUP-014 on `main` (refused today, evidence under *Premise*). Not re-declared, and its reason text is unchanged. Not visible.
- Issue's statement "only RH5104 reports this" and its pointer to a parity exception in #887 — stale context. No spec or document records such a parity exception today (`rg -i "parity exception"` over `sdd`, `documentation`, `squad`, `AGENTS.md`: no match). Nothing to change.
- Method name `TryCloseGap` in the triage — it is `CloseGap` today, as the change doc records.

## Architecture findings

_None_ in the run's defects. The change removes the existing shared decision: the line-break phase refused the join, and the indentation phase placed what the refusal kept. The new risk of two statements of the operator-token rule is listed under *Internal choices*. It is not yet a defect.

## Follow-up candidates

- The spec author's follow-up candidate (a comment between a link's `.` and its member name puts the name at the statement's column) stands as they stated it. I did not re-run it. It is escalation 10 for that shape, in a change of its own; I agree with their recommendation.
- _None_ else.
