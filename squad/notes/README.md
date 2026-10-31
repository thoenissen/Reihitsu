# Notes

Notes are the squad's feedback loop: what a run cost, which questions reached the maintainer, and what in the process got in the way. They are input for improving the squad, never a record of intended behavior — the specs and change docs hold that — and never a source another document depends on.

## Rules

- **One note per run**, `yyyy-mm-dd-<slug>.md`, dated by the day the run publishes its pull request. The lead writes it before publishing ([`../lead.md`](../lead.md), *Publish*) and commits it with the PR, so it survives the deletion of `sdd/work/`. One file per run keeps parallel PRs from conflicting.
- **Other sessions may add one.** A session that works on the agent process, or that hits a process problem worth keeping, may write a note in the same form, named after its topic.
- **Temporary.** A note is deleted once its content is folded into a rule — in the playbook, a role file, `AGENTS.md`, `sdd/README.md`, or `failure-classes.md` — or judged not relevant. The PR that folds or drops notes names them.
- **Never referenced.** No file outside `squad/notes/` names a note: not a spec, change doc, role file, playbook, script, or source file. A rule that comes out of notes states its reason itself, in a process change doc under `sdd/changes/` (`Affected requirements: none — process change`). `SquadNoteReferenceTests` in `Reihitsu.ArchitectureTests` enforces this.
- **The squad never tunes itself.** A note may suggest a rule; only the maintainer, or a session the maintainer starts for it, turns a suggestion into a rule.

## Form

```markdown
# <run slug or topic>

| | |
|---|---|
| Date | yyyy-mm-dd |
| Run | issue #<N> (several: issues #<a>, #<b>), or the topic |
| Pull request | #<N> |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|

## Cost

<stops (output-question batches and escalations), repair rounds, roles spawned more than once and why, and where the run got expensive>

## Decisions taken

<how many the lead took, and any it would have liked the maintainer to see>

## Suggestions

<what in the process should change, with the evidence from this run; or _None._>
```

`Kind` is `output question` or the escalation's number from [`../README.md`](../README.md#escalations).
