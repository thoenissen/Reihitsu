Role: auditor
Commit: 889dba07936312767f7a0b6d1e20d0cf4a7b48d8
Verdict: findings

This audits repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict. The run has one issue, thoenissen/Reihitsu#1403, so this handover is not split by issue.

## Scope

This is a repair delta against `f7eebabf8390a6afd39da744ffdb81815cc38096`. Head `889dba07936312767f7a0b6d1e20d0cf4a7b48d8` is checked out with a clean worktree. Base `origin/main` and `git ls-remote origin refs/heads/main` both give `f52df4598d6e5634ca0a00d5a0444137d253ca72`, which is an ancestor of the head.

The repair commits match the handovers:
- `859ed25` adds the decisions entry `2026-10-22-1403-batch-roles-and-change-doc.md` and the *Repair round 1* section of `00-lead.md`.
- `a7b7f7b` changes `squad/README.md`, `squad/lead.md`, and `.claude/commands/squad.md`, as `05-implementer.md` lists.
- `b49d534` and `f40c812` add handovers only.
- `889dba0` changes only the `Escalation` cell of `2026-10-22-1403-multi-issue-run-design.md`. The question and answer are unchanged.

No C# file, test, script, or workflow changed, so the cleaner's Release build (`0 Warning(s) 0 Error(s)` at `a7b7f7b`) and the spec checker's lane check and architecture run still hold.

Scope did not grow: the repair touches only files that were already in the run's diff, plus decisions entries. Evidence from `04-auditor.md` for the unchanged parts of the diff is reused: D1–D4, the branch, claim, and trigger names, the handover split, and the follow-up rule.

Status of the previous findings:
- **Finding 1 (medium, which roles a run spawns): closed.** All three counterexamples are now covered when the lead picks each issue's lane up front:
  - The docs issue together with the tooling issue now runs the spec author for the docs text (`squad/README.md:93`).
  - The tests-lane spec issue together with the tooling issue now runs the spec author, the skeptic, and the test implementer.
  - In the bug together with the new rule, the bug is still reproduced first, because a lane-keyed duty now means the issue's own lane (`squad/README.md:90`, `:93`).

  The implementer's claim that the union never adds a tree-checking role the claimed lane lacks holds. Along both widening orders, the cleaner, spec checker, auditor, and validator columns of the lane table only gain checkmarks. A sibling shape stays open: an issue's own lane that has to widen in the middle of the run. That is new finding 1.
- **Finding 2 (low, accepted prompt forms): closed.** `.claude/commands/squad.md:7` now defers to `squad/lead.md:9`, which is the single owner and names both separators. Lines 3 and 7 now cover one or several issues. The new boundary of the ask is new finding 2.
- **Previous hints:**
  - The `Escalation | 1` cell was corrected (`889dba0`). The new text, "none of 1–10; the issue left four design points to the maintainer", is accurate, and 1–10 was the range when that question was asked. `squad/decisions/README.md` says entries "are not edited afterwards". The edit was made before the merge, touches metadata only, and leaves the answer as it was, so it is noted, not a finding.
  - The maintainer decided to leave `sdd/changes/2026-10/establish-squad.md` unchanged, so that hint is closed.
  - The other hints are unchanged (see *Hints*).

## Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `squad/README.md:93` (last clause: "the cleaner, spec checker, auditor, and validator check the finished tree whole, as the claimed lane asks") against `squad/README.md:90` ("wherever it keys a duty to a lane, it means that issue's own lane"); `squad/lead.md:13` and `:28`; `squad/README.md:45`; `squad/roles/spec-checker.md:23` | FC-016, FC-001 | same requirement (thoenissen/Reihitsu#1403; `2026-10-22-1403-batch-roles-and-change-doc.md` answer 1: "each issue receives the duties of its own lane") | The union of roles depends on each issue's own lane, but that lane is picked only once (`lead.md:13`). The only way a lane is re-checked during a run is `lane-check.sh <claimed lane>` plus section 3, which inspects the claimed lane. That lane already admits the paths of a narrower issue that has to widen. The role-file signal that a docs-lane issue must widen is spec-checker step 7: "in the docs lane it means the lane has to widen". `README.md:90` reads that clause against the issue's own lane, while `README.md:93` makes the spec checker act "as the claimed lane asks". The two sentences give opposite answers for the same clause, and the second one suppresses the signal in every run of several issues that claims more than docs. It also departs from answer 1 of the decision, because this clause is a docs-lane duty. Counterexample: `/squad #D #T`, where #D is an SDD text follow-up the lead classifies as docs and #T changes `scripts/test.sh` (tooling, no tests). The run claims tooling and runs the spec author for #D. While writing #D's spec text, the spec author adds a requirement to an enforced spec. On its own, #D would have to widen to tests (`README.md:45`; spec-checker step 7), which adds the skeptic's tests-lane check ("does every requirement state what the code does today") and the test implementer's tests-lane duties for #D. In the run of several issues, `lane-check.sh tooling` still confirms, because tooling admits `sdd/` and test paths. The spec checker reports the missing covering test only as "a finding for the test implementer". The test implementer "has no duty" for #D (`README.md:93`), yet `README.md` *Repairs* routes the missing test to it. The skeptic does not run at all in this run, so no skeptic ever checks the new requirement against the code. This is the hazard of the previous finding 1, reached through a lane that widens instead of a lane picked up front. | State how an issue's own lane widens in a run of several issues. For example: when a role reports that an issue needs a wider lane, the lead picks that issue's lane again along the orders, records it in `00-lead.md`, derives the claimed lane again, and runs for that issue the roles its new lane adds, with an explicit escalation-10 cutoff (per issue or per run). Also make the lane-keyed widening clauses of the tree-checking roles read per issue, so `README.md:90` and `:93` no longer conflict. Whether this extends answer 1 or only applies it is for the lead to judge; the maintainer may need to choose the cutoff. |
| 2 | low | `.claude/commands/squad.md:7` with `squad/lead.md:9` | FC-001 | PR-introduced (by the repair) | The command used to ask whenever `$ARGUMENTS` held anything other than issue references, both at the base (`f52df45`: "empty or not an issue number or URL") and at `f7eebab`. Now it asks only when no issue can be extracted, and neither file says what the lead does with other text in the prompt. In a run of several issues, that text decides which issues the run includes. Counterexample: `/squad #1401, see #1377 for the background` has a comma, so it can be read as the accepted list form "`#123, #124`". The run then becomes a two-issue run with a `Closes #1377` line for an issue the maintainer named only as background, and the README rule "the prompt sets the batch" does not prevent it, because #1377 is in the prompt. A single-issue prompt with prose (`/squad #1403 keep it small`) also no longer asks. That is a second difference for single-issue runs, besides the `issue 123` form that `05-implementer.md` names as "the one single-issue difference" (the issue asks that a single-issue prompt keep working unchanged). My previous suggestion ("ask only when no issue can be extracted") led to this change. | In `lead.md` step 1, the owner, state what non-reference text does. For example: ask when the prompt holds anything besides issue references in the accepted forms and their separators, or when prose names an issue. The command keeps deferring to step 1. |

The verdict is `findings`, not `pass, text cleanup`, for the same reason as in `04-auditor.md`. In this run the Markdown process files are the deliverable, so they are the lane's behavior. Finding 1 changes which roles serve an issue, and finding 2 changes which issues a prompt puts into a run. A `--strict-docs` proof cannot tell either change from a wording cleanup.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Roles of a run of several issues (`README.md:93`, `lead.md:13`) | The claimed lane plus each issue's own lane, as picked once in `lead.md` section 1 step 5 | Lane re-evaluation during the run: `lead.md:28` (`lane-check.sh <claimed lane>`, "run the roles the new lane adds"); `README.md:45` ("that run widens to tests"); spec-checker step 7 ("in the docs lane it means the lane has to widen") | Disagree: role selection is per issue, but re-evaluation inspects only the claimed lane, which already admits a narrower issue's widened paths | Finding 1 |
| Which lane a role file's lane-keyed clause means (`README.md:90` versus `:93`) | `:90`: every lane-keyed duty means the issue's own lane. `:93`: the four tree-checking roles act as the claimed lane asks | The tree-checking roles' lane-keyed clauses: `auditor.md`, `cleaner.md`, `spec-checker.md`, and `validator.md` *When you run*; spec-checker step 7 | Disagree for those clauses. Harmless where the claimed lane only adds work (auditor, cleaner, validator); harmful for spec-checker step 7, whose docs-lane clause is a widening signal | Finding 1 |
| The union never adds a tree-checking role the claimed lane lacks (`05-implementer.md` judgment call) | The cleaner, spec checker, auditor, and validator columns of `README.md:27-33` | Widening orders `README.md:45` | Agree: docs → tests adds the cleaner and validator; tests → bug fix and tests → tooling add the auditor; nothing is lost along either order | ok |
| No duty, one line per issue (`README.md:91`) | Per-issue roles of an issue whose own lane does not run them | `README.md:93` "have no duty for an issue whose own lane does not run them" | Agree | ok |
| Skipping a role (`lead.md:90`) | The claimed lane, or an issue's own lane | `README.md:93` | Agree | ok |
| Not reproduced in a run of several issues (`lead.md:46`) | Only that issue skips the remaining roles | `README.md:93` union; *An escalation stops the whole run* | Agree | ok |
| When the command asks (`.claude/commands/squad.md:7`) | No issue can be extracted | `lead.md:9`: accepted forms and separators; the README rule *The prompt sets the batch* | Agree on the forms (previous finding 2 closed). Text outside those forms is not covered; at the base it made the command ask | Finding 2 |
| A docs issue's C# comment edit together with a tooling issue that changes compiled C# (`README.md:92`) | Docs → tests → tooling predicts that tooling admits both | `LaneClassifier.ClassifyPath(path, isTextOnlyProven)`, where the proof covers the whole diff (`scripts/lane-check.sh` header; `LaneClassifier.cs:220-224`) | Disagree on prediction only: the comment file becomes `Production`, so Production plus Tooling without a change doc gives "no lane", and that shows up after the implementer instead of before the claim. The outcome is escalation 10, which is the designed result, and no role is lost | Hint |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Roles of a run of several issues | Owner `squad/README.md:93`. Pointers and restatements: `squad/lead.md:13`, `:26`, `:32`, `:90`. `rg -n -i "role(s)? (of\|the) (the )?(lane\|run)\|lane (runs\|requires\|asks)\|own lane\|claimed lane\|has to widen\|must widen\|widen" squad .claude CLAUDE.md AGENTS.md sdd/README.md` | One owner, and the restatements agree with it |
| Which lane a lane-keyed duty means | `squad/README.md:90` (own lane, general); `squad/README.md:93` (claimed lane, for the four tree-checking roles) | Two statements that disagree for spec-checker step 7: finding 1 |
| Lane widening during a run | `squad/lead.md:28`; `squad/README.md:45`, `:76` (escalation 10); `squad/roles/spec-author.md:12`; `squad/roles/spec-checker.md:23` | Each is written for the run's lane; none covers an issue's own lane: finding 1 |
| Accepted prompt forms | Owner `squad/lead.md:9`; `.claude/commands/squad.md:7` defers to it (`rg -n "ARGUMENTS\|issue 123\|issue number" squad .claude CLAUDE.md AGENTS.md`) | One owner (previous finding 2 closed); its boundary for other text: finding 2 |
| Escalation range in decisions entries | Template `squad/decisions/README.md:11` (`1–11`); both entries of this run say `none of 1–10` | Hint |

## Assertion adequacy
_N/A: the repair adds or changes no test (`git diff --stat f7eebab..HEAD` lists Markdown only). `rg "squad/|lead\.md|commands/squad" src --glob '*.cs'` finds only path classification in `LaneClassifier.cs:20` and `LaneClassifierTests.cs:32`. No test reads the content of the changed files._

## Failure classes dismissed
- FC-002: every qualifier the repair adds is an explicit condition: "every role the claimed lane runs and every role any issue's own lane runs", "in the order above", "whose own lane does not run them", and "separated by spaces or commas" (both separators named, with examples). The text the guard no longer asks about falls under FC-001 in finding 2.
- FC-003: no test can cover a boundary. The boundaries the repair states name both sides: own lane runs or does not run the role; per-issue role or tree-checking role.
- FC-015: covered under *Policy ownership*. The remaining double statement is part of finding 1.
- FC-004 to FC-014, FC-018 to FC-030, FC-036: no syntax, trivia, directive, rewrite, analyzer, code fix, formatter phase, or rename changed. The repair is Markdown only.
- FC-017, FC-033, FC-034, FC-035: no test, reproduction, or expected value is involved.
- FC-031: every changed step describes the new text. `lead.md:3` already says the steps are written for one issue, with the differences for several issues stated where they occur, and the new clauses follow that pattern. `squad.md:3` now covers one or several issues. `05-implementer.md` describes the diff accurately, apart from the understatement noted in finding 2.
- FC-032: no compiled change since the cleaner's build at `a7b7f7b`. Only `sdd/work/` and one decisions entry changed after it.
- FC-037: no C# file changed. The lane was confirmed by `scripts/lane-check.sh tooling` (`07-spec-checker.md`), and `889dba0` touches only `squad/`, which is a tooling path.

## Hints
- **A run cannot be predicted to need escalation 10 before the claim when a docs issue edits a C# comment.** `scripts/lane-check.sh` proves text-only over the whole diff, so take a docs issue whose fix is a comment-only edit in shipped C# (an allowed spec-author duty, `spec-author.md:12`). Combined with a tooling issue that changes compiled C# (for example `src/Reihitsu.Tooling/` and its tests), the run gives "no lane" (exit 3) once the implementer has run. `README.md:92` predicts that docs → tests → tooling admits that run and that a no-lane case is "asked before the claim". The outcome is the designed escalation 10, no role is skipped, and the property predates the run, so this is not a finding. One sentence in *One lane* would make the case predictable.
- **Escalation range in the new decisions entry.** `2026-10-22-1403-batch-roles-and-change-doc.md` says "none of 1–10", but escalation 11 already existed on the branch when that question was asked (`a4f84c7`). "none of 1–11" would be exact. The statement is not false.
- **Carried over from `04-auditor.md`:**
  - `squad/README.md:79` requires a reason for a new rule, but the escalation-11 entry names none (`Reason: none given`).
  - `claude/batch-<a>-<slug>` hides issues 2..n from the branch half of the claim check (`lead.md:11`).
  - The `Verdict` example at `squad/README.md:91` writes `#<a>`, while `:56` asks for `thoenissen/Reihitsu#<N>`.
  - This run's handovers mix layouts.
- **Follow-up candidate, pre-existing.** The XML summary of `LaneClassifier.Covers` (`src/Reihitsu.Tooling/LaneClassifier.cs:128`) still says a claimed lane "runs every role the required lane runs". The README no longer relies on that claim, but it is still false for tooling against docs and tests.
