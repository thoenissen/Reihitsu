Role: spec author
Commit: a8c0757
Verdict: text written

Repair round after `04-auditor.md` (`pass, text cleanup`). My prompt carried the auditor's suggested changes and no intended fix, suspected cause, or hoped-for verdict. The auditor's suggestions were treated as suggestions. Finding 1 follows the suggested direction. Finding 2 takes the second of the two offered options, for the reason given below.

## Changed requirements
_None._ No file under `sdd/specs/` changed. Only two lines of Markdown changed, both in commit a8c0757:

| # | Finding | File | Before | After |
|---|---|---|---|---|
| 1 | Field correspondence read in order maps *Today's output* to the expected behavior | `sdd/templates/follow-up-issue.md` (*Spec bug* intro) | "the requirement replaces the description, the three code blocks are the reproducible example, the expected behavior, and the actual behavior" | Each pair is named: *Requirement* takes the place of the description, *Input* is the minimal reproducible example (the formatter form's input code), *Today's output* is the actual behavior, and *Required output* is the expected behavior. |
| 2 | Rulebook bullet routes only some follow-up classes and omits a bug no spec covers | `sdd/README.md` (*Layout*, last bullet) | Lists spec bug, clarification, and internal work → template, and behavior changes and new rules → feature-request forms | A pure pointer: a follow-up issue is drafted in the format that `templates/follow-up-issue.md` picks for it, and that file also names the cases that use a public form under `.github/ISSUE_TEMPLATE/` instead. |

How the fixes were established:
- Finding 1: the public form labels were read with `grep -n "label:" .github/ISSUE_TEMPLATE/*bug*.yml`. `analyzer_bug_report.yml` has *Description*, *Expected Behavior*, *Actual Behavior*, and *Minimal Reproducible Example*. `formatter_bug_report.yml` has *Description*, *Expected/Actual Behavior*, *Input Code*, and *Expected/Actual Formatted Output*. The new sentence names each skeleton field next to its counterpart, so the mapping no longer depends on the order of the fields. The parenthesis covers the formatter form's different label for the input.
- Finding 2: adding the missing bug-report case would keep a third full copy of the routing next to the template (owner) and `squad/lead.md:84`. A future case added in one place could then drift again (FC-015). The bullet now restates no case, so it cannot drift. The template stays the single owner in `sdd/`. `squad/lead.md:84` is outside my allowed files, and the auditor found that it agrees with the template, so it is unchanged.

Checks:
- `scripts/lane-check.sh tooling --head worktree` before the commit printed `TEXT-ONLY PROOF: PASS — base=1b9a028 head=worktree — 8 changed file(s) (8 non-compiled, 0 C# comment/layout-only); xml-doc impact: none` and `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Neither edited line has trailing whitespace.

Files changed by this role: `sdd/templates/follow-up-issue.md`, `sdd/README.md`, and this handover. Nothing under `.github/`, `sdd/specs/`, `src/`, `squad/`, or any test changed.

## Examples for the tests
_None._ No requirement changed, and no test reads `sdd/templates/` or the `sdd/README.md` layout section (`04-auditor.md`, *Assertion adequacy*).

## Open questions for the maintainer
_None._ The auditor's hints (an unspecified rule's own gap matches two routing lines, the Playground has no public form, version fields for an unspecified bug, the undefined word "coordinator") are left as the auditor recorded them. They are not part of the two findings.
