# Lead — csharp15-closed-labeled-break

- Issues: thoenissen/Reihitsu#1764 (`agent-filed`, goals are proposals), thoenissen/Reihitsu#1765 (`agent-filed`, goals are proposals). Both are sub-issues of #1716 (C# 15 support); their dependency #1761 (Roslyn 5.12 / C# 15) is on `main` (commit 1d2d29a).
- Slug: `csharp15-closed-labeled-break`
- Branch: `claude/ecstatic-feynman-ef6145` (session-mandated branch, see decision 1). Draft PR: #1783.
- Lane: **bug fix**.
  - #1764: RH7105 reports a false positive on `public closed partial class` because the C# 15 `closed` and preview `safe` keywords have no rank; an observed-versus-expected difference. The spec gets a clarification naming the new modifiers.
  - #1765: RH5011 reports a false positive on labeled `break outer;` before `}`; an observed-versus-expected difference. Sibling rules get tests.
- Roles: spec author (clarification, RH7105 for #1764; and any spec clarification for labeled break/continue) → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. If a change doc turns out to be required, the lane widens to behavior change.
- Maintainer clarifications: none.

## Maintainer answers

- Output question 1 (#1764, where `closed` stands): **B**. `closed` takes the place of `abstract`/`sealed` (treated like `sealed`). So `public closed partial class A` and `internal closed unsafe partial class U` are not reported; `public new closed partial class N` is reported on `new` and fixed to `public closed new partial class N`.
- Output question 2 (#1764, where preview `safe` stands): **B**. `safe` takes the place of `unsafe` (treated like `unsafe`): after `extern`, before `required`. So `public safe partial void P();`, `public safe required int R`, and `public safe ref struct S` are not reported; `public static safe extern void M();` is reported and fixed to `public static extern safe void M();`.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use `claude/ecstatic-feynman-ef6145` instead of `claude/issues-1764-1765-<slug>` | The session environment permits pushing only to this branch | Branch names are internal | 4457f19 |
| 2 | Skeptic | #1764: are the spec author's options "reported as today, no fix" (1A/2A) still options? | Dropped from the output questions | SHARED-DIAG-011 (unchanged) forbids reporting an order that cannot be written | An unchanged requirement decides it | — |
| 3 | Skeptic | #1764: change doc required? | Yes; the lane widens from bug fix to behavior change once the maintainer answers | RH7105-001's order list changes under every answer (`sdd/README.md`, *When a change doc is required*) | Lane and change-doc requirement are internal; the visible changes are asked as output questions | — |
| 4 | Skeptic | #1764 issue goals | Drop "explicit ranks" (prescribed solution); widen "never behind `partial`" to `ref` too; the reproduction test compiles the fixed code; the spec may state the rule generically with `closed`/`safe` as examples | Skeptic evidence (`public safe ref struct` → CS1585) | Goal edits that change nothing visible beyond what the output questions ask | — |
| 5 | Skeptic | #1764 architecture finding: RH7105-001 order list contradicts its Why, the rule page, and the code remark | Resolved through output question 1/2; under answer A the order list is aligned with the other three statements | Same rule stated in four places | Settled by the maintainer's answer | — |
| 6 | Skeptic | #1765 architecture finding: RH5011 analyzer and code fix compute "the token behind the break" differently | Fix in this run: the analyzer derives the gap from the statement's end (its semicolon), as the code fix does, so both agree; whether in the RH5011 override or the base class is the implementer's choice, provided no other consumer of the base class changes output | Two components own one decision; the change is small and within RH5011's components | Restores RH5011-001 behavior; no new visible output | — |
| 8 | Maintainer answer | Lane | Widen bug fix → behavior change for the run | Change doc required for RH7105-001's order list (decision 3) | Lane is internal | — |
| 7 | Skeptic | #1765 issue goals | Drop RH5009 tests (rule only judges `goto`); keep RH5010, RH5012, RH7501 tests; widen "done when" to the fix converging (FC-007) and a formatted labeled-`break` input (FC-013) | Skeptic evidence | Test scope is internal | — |



## Follow-up candidates

(none yet)

## State

- Last step: spec author (01, commit 87f9643), skeptic (02, commit 8233fbe).
- Answers recorded (1B, 2B). Lane widened to behavior change (decision 8).
- Spec author 03 done (e9a6ecf); change doc accepted by the lead; lane check confirmed behavior-change.
- Test implementer 04 done (159666b): both reproduced.
- Implementer 05 done (50321f7): tests pass.
- Cleaner 06 done (f2e55bd): clean.
- Spec checker 07 (da6a450): consistent. Auditor 08 (450249a): pass. Audited SHA da6a450 (base 1d2d29a).
- Next: validator (09), then publish.
