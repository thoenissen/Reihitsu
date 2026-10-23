Role: implementer
Commit: 99f789d
Verdict: tests pass

Repair round 2. The prompt named the issue, slug, lane, branch, and handovers; it carried no intended fix or verdict. I treated the auditor's suggested changes in `08-auditor.md` as suggestions and worked out the fix again from the maintainer's decision `squad/decisions/2026-10-23-1403-second-repair-round.md`. The run has one issue, thoenissen/Reihitsu#1403, so this handover is not split by issue.

No tests were handed over. The diff is Markdown only, so `tests pass` means no test was affected.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/README.md` | New bullet *An issue's own lane widens like a run's lane* under *Several issues in one run*. Each issue's own lane is checked through the run the way a single run's lane is; it says why `lane-check.sh` cannot see this. It names three triggers: the paths a writing role lists for the issue need a wider lane by the path classes; a docs issue adds a requirement to an enforced spec; a role reports that the lane has to widen (the spec author's failed text-only proof, or the spec checker's missing covering test for a docs issue). Before the run's implementer has started, the lead widens the issue's lane along the orders, records why in `00-lead.md`, works out the claimed lane again (no lane left is escalation 10), and runs the roles the new lane adds for that issue, in order. A role that has already run is continued, not spawned again. Once the implementer has started, the widening is escalation 10. | Finding 1; decision 2026-10-23, first part |
| `squad/README.md` | *The roles are the union of the lanes*: dropped the last clause ("as the claimed lane asks"). It now says the four tree-checking roles check the tree whole, but a lane-keyed clause in their role files still means each issue's own lane, "as for every role". This agrees with the *One agent per role* bullet and uses spec-checker step 7 as the example. | Finding 1; decision, "the two README sentences are made consistent" |
| `squad/README.md` | Escalation 10: "(in a run of several issues, also an issue's own lane)". | Finding 1 |
| `squad/README.md` | *The prompt sets the batch*: the issue references in the prompt select the run's issues (points to `lead.md` section 1 step 1); other text is context. | Finding 2 |
| `squad/lead.md` | Section 1 step 1: only issue references select issues; other prompt text is context, never an issue. A prompt with a single reference names that issue (`#123 keep it small`). When a prompt holds several references and it is unclear whether one names an issue of the run (it stands inside other text, as in `#123, see #124 for the background`), or when no issue can be extracted, the lead asks. | Finding 2; decision, second part |
| `squad/lead.md` | Section 3: one sentence. For several issues, each issue's own lane is also checked and widened after each writing role, as the README describes (it points there and does not repeat the rule). | Finding 1 |
| `.claude/commands/squad.md` | Line 7 defers to `lead.md` section 1 step 1 for which references select issues, what other text means, and when to ask. | Finding 2 |

Judgment calls for the auditor to challenge:

- **Cutoff per run.** The decision says "a widening after the implementer has started is escalation 10". The run has one implementer agent, so I applied the cutoff once that agent starts, whichever issues it works on, not per issue. As a result, the spec checker's step-7 signal always becomes escalation 10 in a run that has an implementer. The lead's own check after the spec author (trigger: a docs issue adds a requirement to an enforced spec) catches the counterexample from `08-auditor.md` earlier, before the implementer starts. Then the skeptic and the test implementer run for #D.
- **How the lead sees a per-issue widening.** `lane-check.sh` has no per-path or per-issue mode, and changing it would be a C# change outside this Markdown repair. So the per-issue check uses two inputs: the per-issue *Changes* sections that the handovers already carry, read against the path classes in *Lanes*, and the widening signals the roles already report.
- **Single-issue runs.** Escalation 10 only gains a parenthesis, and `lead.md` section 3 gains a sentence for several issues only. Step 1 keeps the base rule that a single reference names the issue. The question for references inside other text applies only when a prompt holds several references, so `please fix #123` still starts a run without a question. Compared with the base `/squad` command (which asked when `$ARGUMENTS` was "not an issue number or URL"), a single-issue prompt with extra prose now runs instead of asking. The maintainer's decision ("other prompt text is context") sets this. The `issue 123` form noted in `05-implementer.md` is the other single-issue difference.
- **Placeholder numbers.** The examples in `lead.md` use `#123`/`#124`, not the real numbers from the decision text, so the playbook names no real tracker issue.

Not changed: the role files. The README's general rule now governs their lane-keyed clauses (`roles/spec-author.md:12`, spec-checker step 7) for every role without exception, so they stay in their single-issue form. The auditor's hints (decisions-entry escalation range, `batch-<a>` branch visibility, the `#<a>` Verdict example, the C# comment prediction in *One lane*) were not findings, so I left them alone.

## Policy owners
- Widening an issue's own lane: owner `squad/README.md` *Several issues in one run*, the bullet *An issue's own lane widens like a run's lane*. `squad/lead.md` section 3 and README escalation 10 point to it. Single-run widening stays owned by `lead.md` section 3 and README *Lanes*. `rg -n -i "widen|own lane|claimed lane" squad .claude CLAUDE.md AGENTS.md sdd/README.md` also finds `roles/spec-author.md:12` and `roles/spec-checker.md:23`. Both are single-run widening signals that the README reads per issue, and neither states a procedure of its own.
- Which lane a lane-keyed clause means: one rule, `squad/README.md` *One agent per role* ("that issue's own lane"). The *union* bullet now restates it for the tree-checking roles and agrees with it.
- Which prompt text selects issues: owner `squad/lead.md` section 1 step 1. README *The prompt sets the batch* and `.claude/commands/squad.md:7` point to it. `rg -n "ARGUMENTS|issue 123|issue references" squad .claude CLAUDE.md AGENTS.md` finds no other statement.

## Tests run
None, because the change is Markdown only. `scripts/lane-check.sh tooling --head worktree`: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.

## Disputed tests or scope
_None._ Three Markdown files changed, all already in the run's diff.

## Follow-up candidates
- Pre-existing, carried over: the XML summary of `LaneClassifier.Covers` (`src/Reihitsu.Tooling/LaneClassifier.cs`) says a claimed lane "runs every role the required lane runs". The lane table contradicts this for tooling against docs and tests.
- `scripts/lane-check.sh` could take a path subset (one issue's share of the diff). Then the lead could check each issue's own lane mechanically instead of from the handovers' per-issue *Changes* sections. This is a new mechanism, not part of this repair.
