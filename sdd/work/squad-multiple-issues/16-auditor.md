Role: auditor
Commit: 000b62a61b99405274ef4aca286f32cd7d7ebc81
Verdict: pass

This audits repair round 3, which the maintainer approved under escalation 7 with a changed design (`squad/decisions/2026-10-23-1403-same-lane-batches.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. The run has one issue, thoenissen/Reihitsu#1403, so this handover is not split by issue.

## Scope

Head `000b62a61b99405274ef4aca286f32cd7d7ebc81` is checked out with a clean worktree. Base `origin/main` and `git ls-remote origin refs/heads/main` both give `070359be885a4ea908527babec903f0047dfde43`, and that commit is an ancestor of the head. The head is the lead's merge of `origin/main` into `9a41fa6`.

- **The merge.** It brings in `daf8e1e`, `20c92ae`, and `070359b`. `comm -12` of their file list with the run's file list (`git diff --name-only f52df45 9a41fa6`) is empty. `git diff --name-only origin/main HEAD`, outside `sdd/work/`, lists only the run's 22 Markdown files under `squad/`, `.claude/`, and `CLAUDE.md`.
- **The repair.** It is a delta against `989dc442d2bb9f345b42be2f333864dd5f2b6de4`:
  - `92723f3` adds the decisions entry and the *Repair round 3* section of `00-lead.md`.
  - `3dc2880` changes `squad/README.md` (4 lines added, 6 removed) and `squad/lead.md` (5 added, 5 removed). That is the 9 insertions and 11 deletions of `13-implementer.md`, and the edits match its *Changes* table.
  - `7897c9d`, `e71902c`, and `9a41fa6` add handovers only.
- **Coverage.** The repair touches no new surface, but it replaces the run's governing design. So I audited every changed file of the run against the new decision, not only the two edited files: the role files, `.claude/agents/*`, `.claude/commands/squad.md`, `CLAUDE.md`, and `squad/decisions/README.md`. Evidence from `12-auditor.md` for the parts the new design leaves untouched is reused.
- **Build.** The cleaner's Release build (`0 Warning(s)`, `0 Error(s)` at `7897c9d`) predates the merge. The run's share of the tree is Markdown that no build step or test reads (FC-032 below), so the merged tree's compiled state is `origin/main`'s. No finding depends on a build or test result.

Status of the previous findings:

- **Finding 1 (medium, the per-issue lane widening has no input before the cutoff): superseded.** The maintainer's decision removed per-issue lanes. `rg -n -i "own lane|union|narrowest lane that admits all|admits all issues|admits them all|naming its lane|issue's lane|per-issue lane"` over `squad .claude CLAUDE.md AGENTS.md sdd/README.md scripts/README.md`, excluding the dated decisions, finds only `lead.md:13` "pick each issue's lane". That is the pre-claim pick, not a lane carried through the run. Each part of the old finding is gone:
  - Both counterexamples now stop before the claim. In `/squad #N #D` (behavior change and docs) and `/squad #D #T` (docs and tooling), the issues do not share a lane, so the lead asks the maintainer to split them (`lead.md:13`, `README.md:76`, `:92`).
  - The path trigger, the prose path classes, the unnamed change-doc signal, and the attribution of a run-level mismatch to one issue no longer drive anything. The only widening left is the single-run `lane-check.sh` widening, applied to the whole run (`lead.md:28`, `README.md:92`).
- **Finding 2 (low, a single reference inside other text is never asked about): closed.** `lead.md:9` now asks "whenever it is unclear whether a reference names an issue of the run, however many references the prompt holds". It names `#99` in `keep the diff small, as we did in #99` as unclear. The count condition is gone.

## Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether a reference names an issue of the run (`lead.md:9`) | The reference is clear when it leads the prompt (alone, or in a list separated by spaces or commas) or when the text names it as the work to do. Any other reference inside other text is unclear and the lead asks, whatever the number of references | Decision `2026-10-23-1403-same-lane-batches.md` ("a single issue reference inside other text is asked about too"). Decision `2026-10-23-1403-second-repair-round.md` ("the lead asks when it is unclear whether an issue is meant"). `README.md:89` and `.claude/commands/squad.md:7` defer to it | Agree. The two definitions cover every case. A reference that does not lead the prompt has text before it, so it is either named as the work or unclear. The examples sit on both sides: `#123 keep it small` and `please fix #123` are clear; `#124` in `#123, see #124 for the background` and `#99` in the prose prompt are unclear. Reading `please fix #123` by clarity rather than by position is the implementer's recorded judgment call, and it matches the suggested change of `12-auditor.md`, finding 2. Single-issue prompts: `/squad #123` and `/squad <url>` run as at the base. The prose-only prompt asks, as the base command did ("not an issue number or URL") | ok |
| Whether the named issues may form one run (`lead.md:13`; `README.md:76`, `:92`) | Each issue's lane as the lead picks it from the issue (`lead.md:26`), compared before the claim | Decision: "every issue of a run must have the same lane … When the issues of a prompt need different lanes, the lead asks the maintainer to split them before claiming" | Agree. All three places name "before the claim" and escalation 10. `lead.md:13` sits in section 1, before section 2 (*Claim*) | ok |
| Widening a run of several issues (`lead.md:28` last sentence; `README.md:92`) | `scripts/lane-check.sh <lane> --head worktree` on the whole diff after each writing role. A widening gives every issue the new lane and runs the roles it adds | Single run: the first three sentences of `lead.md:28`; `README.md:45` (orders); `README.md:76` (escalation 10 after the implementer starts) | Agree. It is the same mechanism and the same cutoff. A whole-run widening only adds roles, so no issue loses a role it would run alone. For lane-keyed duties, see the next row | ok |
| Which lane a role file's lane-keyed clause means (`README.md:90`) | The run's lane, which every issue shares | `test-implementer.md:9` (the reproduction is keyed to the bug-fix lane); `README.md:14` ("for a bug, the first test is the reproduction"); `AGENTS.md:37`, `:56`, `:60`; `lead.md:39` (routes on the verdict `not reproduced`, not on the lane) | Agree in effect. After a whole-run widening from bug fix to behavior change, a co-batched reported bug gets the behavior-change clause of `test-implementer.md:9` from the run's lane. `README.md:14` and `AGENTS.md` still key the reproduction to the bug itself, and the `Refs` and escalation 8 routing works from the verdict. So the reproduction survives (see *Hints*) | ok |
| Handover line for an issue without work (`README.md:91`) | "nothing to do for … saying so and why" | Role files' handover templates; `lead.md:46` (an issue that did not reproduce skips the remaining roles) | Agree. The reason no longer depends on a per-issue lane | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which prompt text selects issues, and when to ask | Owner `squad/lead.md:9`. `squad/README.md:89` and `.claude/commands/squad.md:7` defer to it. `.claude/commands/squad.md:3` describes the command and selects nothing. `rg -n -i "single reference\|several references\|issue references\|leads the prompt\|\$ARGUMENTS\|other text"` over `squad .claude CLAUDE.md AGENTS.md`, excluding the dated decisions, finds no other predicate | One owner. ok |
| Every issue of a run shares one lane; a split before the claim | Owner `squad/README.md:92`. `squad/lead.md:13` gives the procedure and points to it, and `squad/README.md:76` names the escalation trigger. Same `rg` with "share a lane\|share one lane\|split them" | Three statements that agree. ok |
| How the lane of a run of several issues widens | `squad/lead.md:28` (single-run owner, plus one sentence for several issues); `squad/README.md:92` restates it; `squad/README.md:45` gives the orders | They agree, and the restatement adds no condition. ok |
| What a lane-keyed clause means | `squad/README.md:90`, single owner. "As the claimed lane asks", "own lane", and "union" are gone (`rg` in *Scope*) | ok |
| Path classification | `LaneClassifier.ClassifyPath` and `LaneClassifier.TryGetRequiredLane` decide mechanically. `squad/README.md:37-43` describes them in prose | Since round 3, no run step applies the prose by hand. Its divergence from the classifier for a change doc without shipped source is pre-existing (see *Hints*). ok for this run |

## Assertion adequacy
_N/A: the repair adds or changes no test. `git diff --stat 989dc44 9a41fa6` lists Markdown only, and no test reads the content of the changed files. `LaneClassifier` and `LaneClassifierTests` use `squad/` and `squad/lead.md` only as path literals._

## Failure classes dismissed
- **FC-001, FC-002:** traced in *Guard scope*. The decision's qualifiers each became an explicit condition, and every changed predicate agrees with its counterpart:
  - "before claiming" became "before the claim" (`lead.md:13`, `README.md:76`, `:92`);
  - "widens as a whole exactly like a single-issue run" became the `lead.md:28` mechanism applied to every issue;
  - "inside other text" became the unclear branch of `lead.md:9`.
- **FC-003:** there is no test boundary. The text names both sides of each boundary it states: shared lane or not; before or after the implementer starts; a reference that leads the prompt or is named as the work, or one inside other text.
- **FC-015:** each changed policy has one owner (*Policy ownership*).
- **FC-016:** the sibling places that carried the old design were checked. These are `.claude/commands/squad.md`, `CLAUDE.md`, `AGENTS.md`, the role files, `.claude/agents/*`, and `squad/decisions/README.md`. None names per-issue lanes or the union (`rg` in *Scope*).
- **FC-031:** every changed sentence describes the new design. `lead.md:32` "each role of the run" comes from the union design (commit `a7b7f7b`). It is still accurate, because the run's roles are the lane's roles (`README.md:92`). See *Hints*.
- **FC-032:** the cleaner's build at `7897c9d` predates the merge `000b62a`, which brings compiled changes from `origin/main`. Outside `sdd/work/`, the head differs from `origin/main` only by the run's Markdown, which no build input or test reads. No finding depends on a build result, and the validator runs on the final tree.
- **FC-037:** no C# file is in the run's diff. The lane was confirmed by `scripts/lane-check.sh tooling --head worktree` (`15-spec-checker.md`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`). After the merge, the run's path set against the new merge base is unchanged.
- **FC-004 to FC-014, FC-018 to FC-030, FC-036:** no syntax, trivia, directive, rewrite, analyzer, code fix, formatter phase, or rename is in the run's diff.
- **FC-017, FC-033, FC-034, FC-035:** no test, reproduction, or expected value is involved.

## Hints
- **A co-batched bug after a whole-run widening.** Take `/squad #B1 #B2`, two reported bugs claimed as bug fix. The spec author writes a change doc for #B1, and the run widens to behavior change before the implementer starts. By `README.md:90`, #B2 then gets the behavior-change clause of `test-implementer.md:9` (spec examples), not the bug-fix reproduction. `README.md:14` ("for a bug, the first test is the reproduction") and `AGENTS.md:37`, `:56`, and `:60` still require the reproduction and the `Refs` link. `lead.md:39` and `:46` route on the verdict. So nothing is lost, but a literal reader of the role file alone could miss it. The implementer recorded "every issue gets the new lane" as a judgment call, and the decision does not address this case.
- **A whole-run widening can add roles no issue needs alone.** The text-only proof covers the whole diff (`scripts/lane-check.sh`). Take two docs issues: one makes a comment-only C# edit under `src/`, and the other adds a requirement to an enforced spec. Together they widen docs → tests → bug fix, although run alone they would stay docs and widen only to tests. This adds roles and loses none. It is what "exactly like a single-issue run" prescribes.
- **`lead.md:32`: "for several issues, each role of the run".** The union design introduced this wording to tell the run's roles apart from the lane's. They are now the same, so the qualifier is redundant but not wrong.
- **A reference that leads the prompt is always clear (`lead.md:9`).** `/squad #1377 is only background, fix #1401` would claim #1377. This follows the documented invocation syntax `/squad <issue> [<issue> ...]` (`CLAUDE.md:13`, `.claude/agents/README.md:3`). So it is a syntax rule, not a gap in the clarity predicate.
- **General statements that the split rule qualifies.** These sentences say or imply that the named issues make one run with one pull request. Issues that do not share a lane are now split. Each sentence defers to *Several issues in one run* or sits next to it, so none of them drives a decision.
  - `.claude/commands/squad.md:3`: "Several issue numbers make one run with one pull request".
  - `README.md:87`: several issues "become one run".
  - `lead.md:3`: the lead takes the issues the prompt names to "one published, green pull request".
  - `lead.md:83`: "the run implements exactly the issues the prompt names".
  - `README.md:94`: after an escalation, the run "resumes for all of them". Nothing says what the session runs after a split answer; the maintainer's answer decides.
- **Superseded hints from `12-auditor.md`:**
  - Reading the tree-checking roles' *When you run* per issue no longer applies, because issues no longer have lanes of their own.
  - "A docs issue's C# comment edit together with a tooling issue's compiled C# change" is now a split before the claim.
- **Carried over, unchanged:**
  - `2026-10-22-1403-batch-roles-and-change-doc.md` says "none of 1–10", although escalation 11 existed by then.
  - `squad/README.md:79` requires a reason for a new rule, but the entry that introduced escalation 11 gives `Reason: none given`. The new entry's *Question* names the failure it prevents.
  - `claude/batch-<a>-<slug>` (`lead.md:17`) hides issues 2..n from the branch half of the claim check.
  - The `Verdict` example `#<a>` at `README.md:91` does not match the `thoenissen/Reihitsu#<N>` form.
  - This run's handovers mix layouts.
- **Follow-up candidates, pre-existing:**
  - The XML summary of `LaneClassifier.Covers` (`src/Reihitsu.Tooling/LaneClassifier.cs:128`) says a claimed lane "runs every role the required lane runs". The lane table contradicts this for tooling against docs and tests.
  - `squad/README.md:39-43` classes `sdd/` as docs, while the classifier gives a change doc without shipped source behavior change (`LaneClassifierTests.cs:87`).
  - The path-subset mode of `scripts/lane-check.sh` (`09-implementer.md`) is no longer needed by this design.
