# Lead — ordering-move-directives

- Issues: thoenissen/Reihitsu#970 (`agent-filed`), thoenissen/Reihitsu#1668 (`agent-filed`) — goals of both are proposals.
- Slug: `ordering-move-directives`
- Branch: `claude/squad-970-1668-3bsm54` (session-designated); draft PR #1940
- Lane: **behavior-change**
  - #970: the issue text says bug fix, but the triage comment reclassified it as an extension: SHARED-ORDER-006 currently forbids a move across any other directive kind, a balanced `#pragma` pair included. Offering a fix that is withheld today changes a visible output and a spec requirement → behavior change with a change doc.
  - #1668: a decision between two rules for conditional blocks in the moved/crossed span of the member-moving fixes (RH7102, RH7103, RH7109, RH7309). Choosing the simpler rule withholds fixes `main` offers (visible), with a spec and change-doc change → behavior change. Shared mechanism with #970: the directive guards of the ordering move (`OrderingDeclarationUtilities`, `SHARED-ORDER-*`).
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; none skipped.

## Maintainer clarifications (from the issues, summarized)

- #970: triage moved it to an extension because SHARED-ORDER-006 forbids moves across any other directive kind, including a balanced `#pragma` pair. An earlier decision (#927 comment, `ordering-move-safety.md` Q1) is cited by the issue as having chosen to allow balanced pairs inside a crossed member's body.
- #1668: the maintainer kept it open for closer analysis: if checking precisely which conditional blocks matter turns out to be a non-trivial check, main's current behavior should not be kept, and the fixes should refuse any conditional block in the moved span instead (AGENTS.md, *Special cases do not grow the source*).

## Maintainer answers

Batch 1 (before test implementer), questions as drafted in 01-spec-author.md and 02-skeptic.md. The maintainer answered "as recommended":
- 970-1: A — any `#pragma warning` in front of the moved code withholds the fix, whatever IDs it names (unless 970-3 pairing applies).
- 970-2: A — RH7309 is included.
- 970-3: B — a closed pair in front of the move does not withhold the fix; unpaired directives in front still do.
- 970-4: B — RH7309: pairs in any block / between blocks are checked alike; "in front" = in front of the first block.
- 1668-1: B — any `#if` block in moved or crossed code withholds RH7102/RH7103/RH7109/RH7309 fixes; excluded-text parse dropped for these fixes.
- 1668-2: A — any `#if` block in the class in front of the moved fields, or in front of the class, withholds an instance-field reorder; exception: an `#if` without `#else` wrapping the whole class keeps main's result.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Architecture finding: the rule for which position-sensitive directives a move may carry is stated twice (`OrderingMoveSafety.RelocationChangesDirectiveScope` and RH7309's `ContainsUnsafeDirectives`) | Change the structure in this run: SHARED-ORDER-011 is implemented once and RH7309 uses the shared check; routed to the implementer | The narrow fix would otherwise add a second copy of the new pairing rule; both components are already changed by the run | Internal structure; outputs decided by SHARED-ORDER-011 and the answers | – |
| 2 | Skeptic | Declaring a shape unsupported | Not an option for either issue (ordering fixes do not use the formatter for the guarded text; no formatter special case deleted) — no escalation 10 | skeptic handover | – | – |
| 3 | Skeptic | Stale goals of #970: remove an open-question non-goal that does not exist on `main`; bug-fix lane; RH7309 missing from ID list | Drop the non-goal goal; lane is behavior-change; RH7309 decided by 970-2 | agent-filed goals are proposals | No visible output; RH7309 goes to the maintainer as 970-2 | – || 4 | Spec author (03) | Reading of 1668-2 A "in front of the class declaration" | The leading trivia of the first token of each declaration of the type (where `main` reads); groups elsewhere earlier in the file stay unread | Covers both shown inputs (the earlier type's `#endif` lands on the class's first token); a wider reading would add machinery | The shown outputs of 1668-2 A all hold | b6390d6 |
| 5 | Spec author (03) | Stale `Uses` entries in RH7102 and code-fixes.md | Fixed in passing | adjacent text fix | spec text only | b6390d6 || 6 | Test implementer (04) | SHARED-ORDER-008 RH7103 example "group in the body in front of the spans" puts the group in the target's leading trivia, i.e. inside the crossed span, so the requirement (and answer 1668-1 B) refuse it | The requirement wins: the example's input is corrected by the spec author (group separated from the target, as in `FixMovesAStaticMethodBehindAGroupInTheBodyInFrontOfTheSpans`); `NoCodeFixWhenAGroupStandsInTheLeadingTriviaOfTheTarget` stays | 1668-1 B (any group in moved or crossed code withholds the fix) decides it | Covered by maintainer answer 1668-1 | 3369d50 |
| 7 | Test implementer (04) | Where the #1668 refusal lives | In `ChangesMemberOrder` (skeptic's recommendation); Core tests `MoveRangeContainsDirectivesReturnsFalseWhen{Crossed,Moved}MemberBodyContainsBalancedConditional` stay `false` | One change serves all four fixes | Internal | – |
| 8 | Test implementer (04) | No separate SHARED-ORDER-011 matrix for RH7102/RH7109 | Accepted: one shared call site `TypeMemberOrderingCodeFixProviderBase.IsMoveSafe` | Coverage first | Internal | – || 9 | Implementer (06) | RH7309: a group inside a kept block that moved blocks cross refuses the fix | Intended reading: crossed code counts (answer 1668-1 B); spec author adds a sentence to SHARED-ORDER-008 | Answer 1668-1 B names moved or crossed code | Covered by 1668-1 B | – |
| 10 | Implementer (06) | `SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives` lost its last production caller in this run | Adjacent cleanup in the run: test implementer retargets its tests to `GetPositionSensitiveDirectives`, implementer deletes the method | Dead code the PR introduces; small, same component | Internal | – |
| 11 | Implementer (06) | Reuse `DirectivePairSplitScan` for pairing | No: different question and ID normalization SHARED-ORDER-011 deliberately does not use | – | Internal | – |

## Follow-up candidates

- If 1668-1 is answered B: RH2001's fix (RH2001-010, `ConditionalGroupsWrapWholeMembers`) remains the only place parsing excluded branches; same criterion would apply (01-spec-author.md).

## State

- Skeptic done (c72272a): extra Q 970-3, 970-4; spec wording fixes pending for spec author after answers (RH7309 clause wording, intro paragraph naming RH7309 for SHARED-ORDER-011, RH7103 split-pair example → state for RH7107; SHARED-ORDER-006 Why sentence on `#if DEBUG` bodies if 1668-1 B); test implementer must update three existing tests (02-skeptic.md).
- Last step: spec author (88ae496): #970 spec + change doc written; #1668 draft change doc, spec waits for answer. Open output questions 970-1, 970-2, 1668-1, 1668-2.
- Batch 1 answered (see Maintainer answers).
- Spec author follow-up done (b6390d6): answers applied, both change docs Accepted.
- Test implementer done (f9b2eda): 80 red analyzer + 2 red Core tests, all new behavior.
- Spec author 05 done (3369d50).
- Implementer done (44bd79c code, aeac0a8 handover): all tests pass.
- Spec author 07 done (c0e555b); test implementer 08 done (443fee2).
- Next: test implementer (continued) adds tests for the two new RH7309-005 examples of 07; then implementer deletes the method (decision 10) and makes any new test pass; then cleaner.
