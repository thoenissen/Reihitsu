Role: auditor
Commit: 22015e924f17ad4b920e43f97298e957e3ab7959
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `22015e9` (checked out, clean tree). Base `origin/main` = `c81091b883383d0735af167e15603fb702f1d7b4`, matching the SHA in the prompt, and an ancestor of the head (`git merge-base --is-ancestor`). The diff against the base holds `squad/failure-classes.md` (FC-013 row), `squad/roles/skeptic.md` (check 3), and the four run handovers, as `01-implementer.md` describes. Nothing outside `squad/` and `sdd/work/fc013-p11/`. Since the cleaner's build (`5abde22`) only handovers changed. `scripts/lane-check.sh tooling` on the head: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

Issue requirements (thoenissen/Reihitsu#1419 and the clarifications in `00-lead.md`):

| Requirement | Status |
|---|---|
| FC-013 and check 3 no longer treat analyzer-clean code that the formatter changes as a failure or conflict where P11 calls the rewrite intended | met: `squad/failure-classes.md:59`, `squad/roles/skeptic.md:23` |
| Both point at the P11 section by its anchor | met: `../sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11` and `../../sdd/...`; targets exist, anchor matches the heading at `sdd/README.md:117` |
| "Formatter output is flagged by the analyzer" stays a failure, with SHARED-DIAG-010 (P31) cited as its counterpart | met (see hint H1 about its breadth) |
| The skeptic still names such a case in its handover and classifies it as P11 does | met in wording, see finding 2 |
| A new-rule candidate is mentioned in the run report, with no issue created | not delivered: finding 1 |
| Other failure classes and skeptic checks unchanged; no code, specs, or tests changed; nothing in `sdd/` outside `sdd/work/` | met: the diff changes one row and one check only |
| `scripts/lane-check.sh tooling` passes | met |

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `squad/roles/skeptic.md:23` ("a new-rule candidate for the run report"); counterpart `squad/lead.md` §7 steps 3 and 5, §Follow-ups | FC-015 (the run report's contents have one owner, which does not carry the new item) | same requirement | The skeptic is told a new-rule candidate is "for the run report", but the run report's only owner, `squad/lead.md` §7 step 5, lists its contents (lane and `LANE CHECK:` line, roles run or skipped, escalations, repair round) and has no item for it, and the skeptic handover template (`squad/roles/skeptic.md`, Handover) has no slot for it. Counterexample: a run on an issue asking RH5101 to report `[A(\n    1)]` (P11's own example). The skeptic names the attribute argument list as a new-rule candidate in its handover. The lead writes the run report from §7 step 5, which does not mention it, and §7 step 3 then deletes `sdd/work/`. The candidate leaves no trace in the published PR. The only other route a handover find has to the lead is the follow-up path (`squad/README.md:103` → `lead.md` §Follow-ups), which drafts an issue, the opposite of "no issue is created". The maintainer's clarification ("mentioned in the run report, with no issue created") therefore depends on the lead improvising. | Give the lead's playbook the counterpart: in §7 step 5, add the new-rule candidates a handover names to the run report, and say in §Follow-ups that they are not drafted as issues. A matching slot in the skeptic handover template would make the hand-off explicit. `lead.md` is not named by the issue, so the lead decides whether this is in scope or asks the maintainer. |
| 2 | low | `squad/roles/skeptic.md:23` ("— the rule's own gap is a bug or a behavior change of that rule, a different construct is a new-rule candidate for the run report, and no issue is created") | FC-002 | PR-introduced | "and no issue is created" is the third clause of a list with no subject tying it to the new-rule candidate, so it most naturally reads as covering both kinds. The clarification in `00-lead.md` ties "no issue created" to the new-rule candidate only. For the rule's own gap, P11 (`sdd/README.md:121`) calls it a bug against the rule's spec or a behavior change of the rule. When a skeptic finds that gap in passing in another rule, it is an out-of-scope find, which `squad/README.md:103` and `lead.md` §Follow-ups turn into an issue once the maintainer approves it. Read as written, check 3 tells the lead to drop that follow-up. | Tie the qualifier to the new-rule candidate explicitly, for example "a different construct is a new-rule candidate, named in the run report without an issue". |

Why not `pass, text cleanup`: both findings are in the run's deliverable. The tooling lane ships agent instructions, and those instructions are the behavior being changed. Finding 1 also needs a new instruction in a file outside the diff (`squad/lead.md`) and its interaction with §Follow-ups. That is a process change the repair round should audit, not a wording fix that a text-only proof settles.

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Analyzer-clean code that the formatter changes: FC-013 / conflict or not | FC-013 sentence 2 and check 3 sentence 2: "where P11 calls the rewrite intended" | `sdd/README.md:119`: the formatter rewriting a construct that no rule reports is intended, filed as the rule's own gap or a new-rule candidate; `:137` for everything else | Agree by reference. Neither text restates P11's condition, so none of it can drift | ok |
| Formatter output flagged by the analyzer: failure | FC-013 sentence 1 and check 3 sentence 1: any analyzer rule | SHARED-DIAG-010 (`sdd/specs/shared/diagnostics.md:531-533`): only a layout rule whose fix lays out code as the formatter does; order and content rules (RH7103 example at `:552`) and rules without such a fix are exempt | FC-013 is wider than the requirement it now calls its counterpart | pre-existing and kept by explicit instruction: hint H1 |
| Whether an issue is created for a P11 case | check 3, final clause "and no issue is created" | `00-lead.md` clarification: new-rule candidate only; `squad/README.md:103` and `lead.md` §Follow-ups: out-of-scope find → follow-up candidate | The qualifier "for the new-rule candidate" is missing | finding 2 |
| Where a new-rule candidate ends up | check 3: "for the run report" | `lead.md` §7 step 5 (run report contents), §7 step 3 (deletes `sdd/work/`), skeptic Handover template | The receiving side has no counterpart | finding 1 |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| P11 classification: the rule's own gap vs a new-rule candidate | `sdd/README.md:117-137` (owner); restated in `AGENTS.md:59`, `squad/roles/skeptic.md:23`, `squad/failure-classes.md:59` (`rg -n -i "FC-013\|formatter-stable\|analyzer-clean\|flagged by the analyzer\|formatter output\|counterpart surface\|P11\b\|may do more than"`, `sdd/work/` excluded) | One owner. Each restatement links the P11 anchor and agrees in substance. Check 3 leaves out P11's deciding condition between a bug and a behavior change (does the spec already cover the form) and sends the reader there with "classify it as P11 does" | ok |
| Formatter output is not flagged by an analyzer rule | `sdd/specs/shared/diagnostics.md:529-535` (SHARED-DIAG-010, owner), `sdd/README.md:137`, `squad/failure-classes.md:59`, `squad/roles/skeptic.md:23` | The squad texts are broader than the owner. Pre-existing: both texts and SHARED-DIAG-010 were on `main` before the run, and the issue says to keep the clause as stated. Hint H1 |
| Analyzer-clean code stays untouched by the formatter | `squad/roles/test-implementer.md:51`, the helper row for `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source)` (`src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs:32`) | The row maps a concern a test asserts to its helper. It does not require all analyzer-clean code to be formatter-stable, so it is no second owner and does not contradict P11 | ok |
| Handling of a new-rule candidate and of a find made in passing | `squad/roles/skeptic.md:23` (new), `squad/lead.md` §7 step 5 and §Follow-ups, `squad/README.md:103` | The new instruction has no counterpart on the lead's side: finding 1. Its qualifier is ambiguous against the follow-up path: finding 2 |
| References to FC-013 elsewhere | `rg -n "FC-013"` over the repository: only `squad/failure-classes.md:59` and `squad/roles/skeptic.md:23` (no decisions entry, no `.claude/agents/` wrapper, no test) | ok |

## Assertion adequacy

_N/A — the run adds or changes no test, and no test reads `squad/` files (`rg -l "failure-classes|squad/roles|skeptic\.md" src scripts .github` finds nothing)._

## Failure classes dismissed

- FC-001: can reach the change, since the narrowed exemption is a predicate. It defers to P11 by reference, and the spans agree (guard scope, row 1).
- FC-003, FC-017, FC-033, FC-034, FC-035: no test added or changed, and none covers these Markdown files.
- FC-004 – FC-012, FC-014, FC-018 – FC-030, FC-036: these concern compiled analyzer, formatter, or code-fix behavior. The diff holds no compiled file (lane check: 0 C# files).
- FC-013: the subject of the change. Its new definition is audited above (guard scope rows 1–2).
- FC-016: no sibling copy of the old disjunct "analyzer-clean code is changed by the formatter" remains. The rg above finds only the helper row at `test-implementer.md:51`, which is not a copy (policy ownership).
- FC-031, by analogy for text: no other squad text still describes the old FC-013 second half.
- FC-032: no compiled change. Since the cleaner's build (`5abde22`) only handovers changed.
- FC-037: no C# file in the diff. The lane check's text-only proof passed: "4 non-compiled, 0 C# comment/layout-only" at `69c4ecc`, and since then only handovers changed.

## Hints

- H1 (pre-existing, not blocking; follow-up candidate): FC-013 sentence 1 and check 3 sentence 1 call any analyzer report on formatter output a failure. SHARED-DIAG-010, which both now cite as the counterpart, covers only layout rules whose fix lays code out as the formatter does. It explicitly exempts order and content rules: its own RH7103 example reports `Create` in code the formatter keeps (`sdd/specs/shared/diagnostics.md:533`, `:552-565`). A skeptic reading check 3 literally ("formatter output must not be flagged by the analyzer") on an RH7103 or comment-content issue would raise a conflict that the spec calls intended behavior. That is the same kind of instruction-versus-spec gap this run closes for P11. It is not a finding, for two reasons. The issue and the maintainer's clarification say to keep the clause as stated. And the gap predates the run: SHARED-DIAG-010 and both texts were already on `main`. The new citation's wording ("under which a layout rule does not report…") narrows the reading slightly rather than widening it. If the maintainer wants it closed, narrowing the clause to layout rules within SHARED-DIAG-010's scope is a separate change.
- H2: the FC-013 cell is now the only row of the catalog with two sentences. The first ends with a period and the last does not, like the single-sentence rows. Style only.
