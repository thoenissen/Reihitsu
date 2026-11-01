# Lead — spacing-sole-gap-owner

## Run

| Issue | `agent-filed` | Goals |
|---|---|---|
| thoenissen/Reihitsu#1493 — FMT-SPACE: the spacing phase is the only owner of same-line gaps | yes | proposals; the issue's own "Before the run" calls its target behavior and "Done when" binding, but the label makes them proposals under `squad/README.md`, *Issues are hypotheses*; any change to visible output is an output question either way |

- Slug: `spacing-sole-gap-owner`
- Branch: `claude/focused-brown-4e3k0u` (session-designated branch; see decision 1)
- Draft PR: thoenissen/Reihitsu#1548
- Base: `origin/main` at `e3842c8`
- Blocker thoenissen/Reihitsu#1488 (B3) is closed (merged by #1513).

## Lane

**Behavior change.** The issue requests changed formatter behavior (attribute-target colon and arrow spacing, joined-pair separators) and requires a change doc under `sdd/changes/`.

Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager once before publishing if follow-up candidates exist. Skipped: none.

## Maintainer clarifications for this run

- The maintainer expects the issue to be complex and asks the run to plan carefully up front, so that repair rounds do not keep finding new cases at the end. Every role therefore enumerates complete inventories and failure classes in its first pass (every rewriter of the line-break phase and every token pair it can join or space; every attribute-target and arrow construct; existing formatter, code-fix, and analyzer tests whose expected output depends on the changed gaps), rather than sampling.
- Context from earlier runs (the blank-line and using-comment runs of 2026-11-01): the maintainer considers comments that share a line with code, and comments around `=`, unwanted; the formatter must not grow complexity to lay them out. Reachability (`AGENTS.md`, *Supported inputs*) is checked before any case is measured.

## Maintainer answers

Batch 1 (before the test implementer), 13 output questions from `03-spec-author.md`. The maintainer answered that the recommendations are correct: **1B, 2B, 3B, 4B, 5B, 6C, 7C, 8B, 9B, 10B, 11B, 12B, 13B**.

1. Join separator: no space unless the two tokens would run together (1B).
2. One space on each side of every `=>` (2B).
3. Attribute-target colon: no space before, one space behind, for every target (3B).
4. Conditional `? :`: one space on each side (4B).
5. Name colons of arguments, tuple elements, subpatterns: `name: value` (5B).
6. One space in front of every `when`, catch filter included (6C).
7. One space between a type and every declared name, including `foreach`, `catch`, query, and pattern designations (7C).
8. One-line accessor list spaced `{ get; set; }`, attributed lists too (8B).
9. Relational pattern `is > 5` (9B).
10. One space behind the listed keywords in front of their operand (10B).
11. One space on each side of `with` (11B).
12. One space between an explicit lambda return type and its parameter list (12B).
13. `delegate* managed` / `delegate* unmanaged` (13B).

Question 14 (asked before the implementer; one-line forms of the FMT-SPACE-024 sweep — ref expression in a statement, lambda `ref`/`static`/`async` in front of a tuple return type, function pointer parameter `ref`): **14A**, one space behind the keyword in every listed position.

Standing instruction from the maintainer for the rest of the run: any further output question is decided by the lead's recommendation without stopping, and every such decision is reported to the maintainer at the end of the run (run report). Record each one here as "decided on recommendation under the standing instruction".

Question 15 (decided on recommendation under the standing instruction; to be reported at the end): change-doc entry 14 — the gap behind a `/** */` comment carried by the right token follows the pair's rule, e.g. `b /** sum */+b` → `b /** sum */ + b` (`main`: `b /** sum */+ b`), `c ? b /** yes */:b` → `c ? b /** yes */ : b`. Decided: **apply the pair's rule** (recommended), because a comment in a gap should not change how the pair is spaced.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name: playbook's `claude/issue-1493-<slug>` or the session-designated branch | Use `claude/focused-brown-4e3k0u` | The session environment permits pushing only to that branch | Branch names are internal | claim commit |
| 2 | Skeptic | Amend change doc before the batch? | Yes: the skeptic's OQ-1 (complete list of pairs spaced today that 1B would make tight) replaces Q10; OQ-2 (`foreach`/`catch`/`from` declarations under FMT-SPACE-021) and OQ-3 (catch-filter `when` under FMT-SPACE-020) join the batch; the change doc's "Unchanged" paragraph and "Why" are corrected | The skeptic showed them false under 1B; the batch must be complete before tests | Internal step; every visible change is asked | pending |
| 3 | Skeptic | `MergeAttributeListGroup` writes a space into a joined gap | Record it as an exception in FMT-BREAK-031 pointing at #1492; do not change the Core helper | Output identical either way (FMT-SPACE-003); the helper is shared with code fixes and its move is #1492 | Output identical | pending |
| 4 | Skeptic | FMT-BREAK-024 "MUST stay as written" | Reword to "stays on its line" | FMT-SPACE-022 changes the list's spacing | Spec wording | pending |
| 5 | Spec author | Scope: the rules for arrows, conditional operator, name colons, relational patterns, attribute-target colon complete parts of #958, #1514, #1440, #1383 | Take them into the run as far as the separator choice needs them; link those issues with `Refs`, not `Closes` | Without them a neutral join changes outputs the maintainer already decided; the other issues are not in the run | Each visible change is still asked in the batch | pending |
| 6 | Skeptic | Present Q2, Q4, Q5 (arguments), Q9 as decided? | Ask them in the batch, with the earlier answer as the recommendation and its citation | The issue proposes otherwise for arrows; one answer confirms them cheaply | Asked | — |
| 7 | Spec author / skeptic | Issue goals: "SpacingPolicy only in the spacing phase" already true; "arrow keeps author's spacing" superseded by Q2; `NoSpaceSpacingRule` remark premise wrong | Drop the first, follow Q2's answer for the second, keep the third as a comment fix explaining why the pairs are tight | Facts on `main` | Not visible except through Q2 | pending |
| 8 | Spec author (amendment) | Accept the amended change doc (dfb3dd8) | Accepted; its 13 output questions asked as one batch before the test implementer | The skeptic has run and the amendment it caused is written | Every visible change is asked | dfb3dd8 |
| 9 | Test implementer | FMT-SPACE-025's example and change-doc entry 11 show a one-line `with` initializer the pipeline never produces | Spec author corrects the example and entry to the real multi-line layout; answer 11B (which showed that layout) stands | Text error; the asked question showed the real layout | Answer 11B covers the output | pending |
| 10 | Test implementer | `x!⏎with { }` is not joined today | Test only the one-line form | Fact on `main` | Covered by answer 11B | 0920f3c |
| 11 | Test implementer | Doc-comment-on-the-right and operator/conversion/constructor/finalizer arrows tested at phase level only | Accept | Earlier phases move or convert these inputs before spacing; whole-document formatting never presents them to the spacing phase as such | Internal test level | 0920f3c |
| 12 | Test implementer | FMT-SPACE-024 ambiguous for `ref` in front of a local's tuple type | Spec author decides from the existing requirements and the answers; if no answer decides it, it becomes an output question | — | Pending check | pending |
| 13 | Spec author | `ref`/`ref readonly` in front of a local's, field's, `foreach` variable's or return type's tuple type: undecided, and no join reaches it today | Outside the run's issue: FMT-SPACE-024 states these positions are outside the rule (today's output kept, as for `public(int, int) G;`); the output question goes to a follow-up issue | The issue is about pairs a join produces; these are one-line-only cases with no change from `main` | Output unchanged from `main`; the question is carried by the follow-up | pending |
| 14 | Spec author | `SameLineGapRulePhaseTests.WithRequirementExampleGetsOneSpaceOnEachSide` no longer matches the example; newly decided positions of FMT-SPACE-024 need one-line tests | Test implementer renames the test and adds a full-pipeline test of the example, and the one-line tests for the positions `05-spec-author.md` lists | Coverage for decided positions | Tests only | pending |
| 15 | Lead | Spec author's sweep (05) extends FMT-SPACE-024 to one-line forms not shown in question 10: ref expression in a statement (`return ref(x);`), lambda ref return type (`ref(int, int) () => …`), lambda `static`/`async` in front of a tuple return type, function pointer parameter `ref` | Ask as an output question now, before the implementer, rather than at the pre-audit batch | They change visible one-line output not literally covered by answer 10; asking now avoids rework after implementation | Asked; answered 14A | 161d307 |
| 16 | Implementer | 10 `LineBreakRewriterTests.Collapses…Assignment…`, 2 `ExpressionBodiedTransformTests`, `LineBreakPhaseIntegrationTests.HandlesExpressionBodiedPropertyAcrossLines` assert phase-level spaces FMT-BREAK-031 / FMT-STRUCT-026 forbid | Tests are wrong; test implementer updates them to the spec | Phase-level text is internal; the final output is unchanged | Internal phase output | pending |
| 17 | Implementer | `ConditionalExpressionFullPipelineTests.SingleLineConditionalWithoutColonSpaceIsNotRewritten` (`a :b`), `IndentationTests.ImplicitStackAllocInitializerAlignsToStackAllocKeyword` (`stackalloc[]`), `SpacingPolicyTests.NoRuleAppliesBetweenTypeAndIdentifier` | Tests are wrong; update to answers 4B, 10B, 7C | The maintainer's answers decide these outputs | Covered by answers 4B, 10B, 7C | pending |
| 18 | Implementer | `RelationalPatternSpacingTests.DocumentationCommentInFrontOfTheOperandKeepsItsPlace` expects an output the documentation phase never lets through (also on `main`) | Test implementer replaces the full-pipeline expectation with the pipeline's real output where every gap is decided by the spec, or keeps the case at phase level only | Unreachable expectation | Comment shape is one the maintainer considers unwanted; phase-level coverage stays | pending |
| 19 | Implementer | FMT-SPACE-014 states only removing whitespace behind a `/** */` that belongs to the next token; the implementation also inserts the one space a rule demands | Spec author widens FMT-SPACE-014 to the general form (the gap behind such a comment is decided by the rule for the pair) — in-run spec gap, not a follow-up | PR-introduced gap between spec and tests | Follows the maintainer's answers for each pair; standing instruction | pending |
| 20 | Implementer | Bundled fix: the gap behind a rewritten token was decided from a detached copy and skipped in that pass | Accept as an adjacent fix the run's tests need; covered by the new comment tests; auditor to check it | Small, same component, tested; repository self-format unchanged | No test outside the run changes output; auditor verifies | cab4c49 |
| 21 | Spec author | Follow-up candidate: spaces behind a right-carried `/** */` in a gap without a rule are not collapsed (`new[] { /** z */   1 }`) | Dropped, not filed | The maintainer's guidance treats comments sharing a line with code as unwanted and asks the formatter not to grow complexity for them | No change in this run | — |
| 22 | Test implementer | `ExpressionBodiedTransformTests` get-only property/indexer: the spec leaves zero or one space next to the created arrow when the author's removed tokens carried one | Accept the tolerant assertion (zero or one space each side) | FMT-STRUCT-026 leaves it open; the strict case is covered by `StructuralTransformSeparatorTests` | Phase-level text only | ed6b80f |
| 23 | Test implementer | Row 18 case | Kept at phase level only (`SameLineGapRulePhaseTests`) | The full pipeline moves the comment; the gap it leaves is undecided and the shape is unwanted | Phase-level | ed6b80f |
| 24 | Spec checker | Repair round 1 (Finding 1: change doc cites run-internal bookkeeping; entry 14's decision basis) | Route to spec author; cleaner skipped (no code changed); spec checker re-runs on the repair | Text-only repair | Wording only | 6ef3a59 |

## Follow-up candidates

- A wrap inside a declaration's attribute list (`[return` ⏎ `:Obsolete]`) is never joined — 01-spec-author, 02-skeptic.
- Wrapped pattern parts lose their column (property-pattern member `{ X:` ⏎ `1 }`); check against #1542 / #953 — 01-spec-author, 02-skeptic.
- A member's own modifier in front of a tuple type (`public(int, int) G;`) keeps the author's spacing — candidate for #1440 — 03-spec-author.
- A lambda with an attribute list is broken after it in whole-document formatting (`[return:Obsolete]` ⏎ `(int x) => x;`) — 04-test-implementer.
- Output question: spacing of `ref`/`ref readonly` in front of a local's, field's, `foreach` variable's or return type's tuple type (`ref(int, int) r`), with the question in `05-spec-author.md` — 05-spec-author.
- A wrapped `ref` ⏎ `(int, int)` of a local, field, property or `foreach` variable is never joined — 05-spec-author.
- A local function's modifier in front of a tuple type (`static(int, int) K()`) keeps the author's spacing — add to the #1440 member-modifier candidate — 05-spec-author.
- Wrapped `with` expression loses its column — same class as #953, comment there at most — 02-skeptic.
- The space behind `when` (`when(x)`, catch filter) is undecided — candidate for #1440 — 02-skeptic.
