Role: lead
Commit: 0fa1778c (origin/main at claim)
Verdict: lane claimed — bug fix

# Run withhold-unsafe-fixes

Branch: `claude/sweet-archimedes-fzb7hv` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1599.

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1257 | yes | bug fix | RH3001/RH3005/RH3101/RH3105 code fixes produce a generic invocation that does not compile; SHARED-FIX-003 already demands withholding. |
| thoenissen/Reihitsu#1279 | yes | bug fix | RH3004 code fix. Narrowed by triage comment: keeping `static`/`async` and parenthesizing moved to #1436 (out of this run). Remaining: withhold the fix where the rewrite would not compile or would change meaning (cast and binary operands, e.g. `() => 1 + h`, CS0019; SHARED-FIX-003), and lay out the result as the formatter does (SHARED-FIX-007, e.g. `= delegate (int x)` with `{` on the next line). |
| thoenissen/Reihitsu#1281 | yes | bug fix | RH3005, RH3101, RH3106, RH3201 fixes break code or change what runs; report stays, fix withheld (P1, P23). Its RH3103 part is superseded by #1417 (triage comment) and is not handled under #1281. |
| thoenissen/Reihitsu#1282 | yes | bug fix | RH3101, RH3104, RH3105, RH3106, RH3107 fixes delete comment text; fix withheld (SHARED-FIX-003). The issue suggests doing RH3106/RH3107 on their shared empty-list removal. |
| thoenissen/Reihitsu#1417 | yes | bug fix | RH3103 must not report where `T?` is not valid C# (P38, SHARED-DIAG-011); `using N = System.Nullable<int>;` reported only from C# 12; extra instance `using static System.Nullable<int>;` (issue comment). Triage comments: bug-fix lane, no change doc; RH3103-006 and other rule-spec text aligned to SHARED-DIAG-011 as a clarification in the same PR. The body's "behavior change / change doc" line no longer applies. |
| thoenissen/Reihitsu#1422 | yes | bug fix | RH3104 must not report `new S();` as a statement (`default(S);` is CS0201); RH5417 must not report below C# 11 where the line breaks would be CS8967. Triage comment: bug-fix lane, no change doc; rule-spec text aligned to SHARED-DIAG-011 as a clarification. |

All six issues are `agent-filed`: goals are proposals.

## Roles

Lane bug fix runs: spec author (clarifications only: rule specs that still disagree with SHARED-DIAG-011 / SHARED-FIX-003 for #1417 and #1422, and any other clarification the issues expose), skeptic (premise and reachability check, incl. whether #1481 or other recent main commits already fixed instances), test implementer (reproductions), implementer, cleaner, spec checker, auditor, validator. Issue manager once before publishing if follow-up candidates exist.

## Maintainer clarifications

Triage comments on the issues (written under the maintainer's account) narrow #1279 (static/async and parenthesizing moved to #1436), remove RH3103 from #1281 (superseded by #1417), and move #1417 and #1422 into the bug-fix lane without a change doc.

## Maintainer answers

Batch 1 (before the test implementer), all answered with the recommended option:

1. RH3004, anonymous method whose block body is on one line: **A** — the fix lays out its result as the formatter does (the block on its own lines, aligned under the lambda), not on one line.
2. RH3106, `var a = delegate () { };` and likewise `Delegate d = …` / `object o = …` (C# 10+): **A** — not reported (instead of reported with the fix withheld).
3. RH3101 rule page example: **A** — make `Derived` `internal` in both blocks so the example compiles (in addition to dropping `virtual`).
4. RH3106, an anonymous method with an empty parameter list as an argument whose parameter type is not a delegate type (e.g. the only overload takes `object`): **A** — not reported, like answer 2. Arguments where competing overloads or type inference decide the binding stay reported with the fix withheld (RH3106-002).
5. RH3201, `return /**/a;` (whitespace in front of the empty comment, the comment directly in front of the next token): **A** — the fix writes `return a;`, keeping the separating whitespace (today `returna;`). Raised by 04-test-implementer (test `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens`).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/batch-1257-<slug>` per lead.md vs the session's designated branch | Use `claude/sweet-archimedes-fzb7hv` | The session environment allows pushes only to its designated branch | Branch names are internal | claim commit |
| 2 | Claim | Lanes of #1417/#1422 (body says behavior change) | Bug fix for all six issues | Triage comments on both issues move them to bug fix: SHARED-DIAG-011 already requires the behavior | Lanes are internal | claim commit |
| 3 | Skeptic | Architecture finding: "does the rewrite still parse into the same tokens/structure" is needed by the RH3001, RH3005, RH3101, RH3105, RH3201 fixes and exists privately in the RH3002 analyzer | Change the structure in this run: the run's fixes share one check (one place), no per-fix copy. Consolidating RH3002's private copy onto it is a follow-up (RH3002 is not a component this run changes) | A narrow per-fix guard would add four or five copies of one decision (squad/README, Architecture findings) | Internal structure; outputs follow the requirements | pending |
| 4 | Skeptic | Widen RH3101-005/RH3105-005 to the cast reading (`(this.A)(5)`), state RH3001-007/RH3005-008/RH3101-005/RH3105-005 as "would parse as a generic name" with `(` as an example, repair RH3005-008 wording, widen RH3106-002 to every overload-resolution/inference argument position (incl. constructor initializer, object creation, element access; withhold when binding would change, become ambiguous, or fail), RH3201-006 to any comment-only separation that would merge tokens (multi-line and adjacent comments), RH3104-005 naming void/async Task functions, constructors, finalizers, set/init/add/remove accessors, RH3004-005 Why mentions CS8848 | Accept all | Each failing member breaks compilation or changes binding, which SHARED-FIX-003 / SHARED-DIAG-011 already forbid | Withheld fixes / unreported positions demanded by unchanged shared requirements | pending |
| 5 | Skeptic | Retire RH3103-006 without a change doc | Accept | Triage comments on #1417 rule out a change doc; SHARED-DIAG-011 already demands the change | Demanded by SHARED-DIAG-011 | f71365b7 |
| 6 | Skeptic | RH3004-005 withholds every binary operand, incl. `() => { } + g` (compiles with CS8848) | Accept | Triage comment on #1279 names cast and binary operands; one uniform condition | Withheld fix covered by the triage decision | f71365b7 |
| 7 | Skeptic | RH3106/RH3107 fixes leave stray whitespace (`delegate  { }`, `[Obsolete /* x */ ]`), pinned by tests | Adjacent fix in this run | Same shared removal the run changes; small; SHARED-FIX-007 already demands the formatter's layout | Demanded by unchanged SHARED-FIX-007 | pending |
| 8 | Skeptic | Issue #1257's `M(G < A, B > !x)` does not compile as input | Use the valid `switch` form as the reproduction | An input that does not compile is not a case | Internal (test input) | — |
| 9 | Spec author amendment | RH3106: report of a delegate-target argument whose call would bind elsewhere, be ambiguous, or fail | Stays reported, fix withheld (RH3106-002) | The error comes from competing overloads / inference, not the position (SHARED-DIAG-011); matches #1281 and maintainer answers 2 and 4 | Covered by maintainer answer 4 | a68f412f |
| 10 | Implementer | Nine `FixAll…Reported` tests lack `MarkupMode.Allow` for the fixed/batch-fixed state | Test implementer adds the configuration; assertions unchanged | Test-harness configuration, the expected outputs are right | Expected outputs unchanged | pending |
| 11 | Implementer | Spec `Uses` rows miss SHARED-FIX-003/-007/SHARED-DIAG-011, RH3106/RH3107 list SHARED-PARENS-003 without referencing it (`UsesListsExactlyTheReferencedRequirements`) | Spec author repairs the `Uses` rows | Spec structure | Spec text only | pending |
| 12 | Implementer | Architecture finding: RH3004 fix writes `///` comments in the block back after `FormatNodeInDocumentAsync` rewrote them | Fix narrowly in this run; file a follow-up for a layout-only mode of the node-level entry point (and the multi-line `///` continuation-column gap) | The structural change is in the formatter, a component this run does not change; the narrow fix adds no copy of a formatter decision, it only restores user text | RH3004-003 (statements unchanged) and SHARED-FIX-003 decide the output | 6961521a |
| 13 | Implementer | `RewriteParseUtilities` (new, public in Core, five consumers) has no direct unit tests | Test implementer adds focused Core tests | Shared helper should be tested where it lives | Internal | pending |
| 14 | Implementer | RH3103 fix's member-access arm is no longer reached | Keep as a guard | Removing it is a cleanup outside the fix's need; harmless | No output changes | — |
| 15 | Audit 1 | Finding 1: Fix All merges guarded rewrites that break code together (RH3005, RH3101, RH3105, RH3106) | Repair round 1: the guards hold under Fix All (each fix decided on the document the earlier fixes produced); interacting-instance tests per rule | Same requirements (SHARED-FIX-002/-003) | Withheld fixes demanded by unchanged requirements | pending |
| 16 | Audit 1 | Finding 2: RH3106 natural-type members (`var a = (delegate () { })`, array creation element, lambda return) and user-defined operator operands with competing overloads | Natural-type members: not reported (covered by maintainer answer 2: positions where the anonymous method takes its natural type). Operator operands chosen by overload resolution: under RH3106-002, reported with the fix withheld (covered by answer 4, competing overloads) | Same principle the maintainer answered | Covered by answers 2 and 4 | pending |
| 17 | Audit 1 | Finding 3: RH3004 fix formats the block's statements | Repair round 1: the block's statements are moved code under SHARED-FIX-007's moved-code bullet (own layout kept, only indentation follows); the fix uses the mechanism the existing moved-code fixes use rather than restoring per phase. Not an architecture change: an unchanged requirement and an existing mechanism decide it; Decision 12's `///` write-back is superseded if no longer needed | RH3004-003 + SHARED-FIX-007 moved-code bullet | Demanded by unchanged requirements | pending |
| 18 | Audit 1 | Finding 4: `ParsesAsWritten` compares the whole document per diagnostic (quadratic under Fix All) | Repair round 1 (PR-introduced, in a component the run adds): compare the smallest node covering the change | Cheap, in the run's own helper | Internal | pending |

Repair rounds used: 1 (started after audit 1).

## Follow-up candidates

- Consolidate the RH3002 analyzer's private "rewrite still parses the same" helpers onto the shared check this run introduces (Decision 3; 02-skeptic).
- RH3106-001 states the report only for variable initializers but the analyzer reports every empty parameter list; widen after Q2 (01-spec-author, 02-skeptic).
- The four `-004` conditions (RH3001, RH3005, RH3101, RH3105) leave unspecified the safe inputs today's fixes rewrite (01-spec-author).
- RH3201-005 does not cover a comment at the start of a line followed by code, or code on both sides where tokens would not merge (01-spec-author, 02-skeptic).
- RH3201-007 does not decide `return /**//**/a;` (whitespace in front of the first of two adjacent empty comments) and has no test for it (04-test-implementer, amendment).
- Formatter: a layout-only mode for the node-level entry point so code fixes need not restore `///` text the documentation-comment phase rewrites; today RH3004 restores it, and a multi-line `///` inside a moved block keeps its old continuation columns (05-implementer, Decision 12).
