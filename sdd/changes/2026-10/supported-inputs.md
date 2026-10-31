# Components have to work for the inputs the repository's own callers pass

| | |
|---|---|
| Status | Accepted |
| Date | 2026-10 |
| Affected requirements | none — process change |

## What

No assembly of this repository is a public API, the formatter included. A component has to work for the inputs the repository's own callers pass — the analyzers and their code fixes, the CLI, and whole-document formatting — and not for every input its signature admits. `AGENTS.md`, *Supported inputs*, states the rule once; the squad's process files point to it:

- **At intake.** Whether a caller reaches the reported case is part of an issue's premise. It is checked before anything is built; in the squad, the skeptic checks it. An issue whose case no caller reaches is not implemented. A test that was already written stays, the pull request links the issue with `Refs #<N>`, and the lead reports this as the run's result, so the maintainer can close the issue. It is not an escalation.
- **During a run.** A case no caller reaches is neither a defect nor a follow-up. Failure-class enumerations, review findings, and follow-up candidates leave it out, and the issue manager files no issue for it.
- **Evidence.** A role that says a caller reaches a case names the caller and the path. An input that only a direct call of a method produces does not count.
- **Tests.** A test or a spec example may still call a method directly to check what a caller passes.

## Why

The formatter's node-level entry points, `FormatNode`, `FormatNodeInDocumentAsync`, and `FormatNodeInDocumentWithContextAsync`, are public methods. A run that reads them as an API to be correct for every syntax node treats every node shape as a supported input. The run of issue #1489 did that. It stopped for, and drafted follow-ups about, node-level inputs that no code fix, no CLI path, and no whole-document formatting passes:

- inner parts of member-access chains;
- initializers formatted as targets of their own;
- the column of a node formatted on its own inside an initializer.

None of these could reach a user, yet each cost a stop or an issue. The repository already says that no assembly is used as a library (`squad/README.md`, *What reaches the maintainer*). What was missing was the consequence for which inputs count, and that the question belongs at intake, before a run builds anything for a case no caller reaches.

## Decisions

### One statement, in `AGENTS.md`

The rule applies to every contributor, not only to the squad, so `AGENTS.md` states it. `squad/README.md`, `squad/lead.md`, and the skeptic, auditor, issue-manager, and spec-author role files point to it where it takes effect, without restating it.

### An unreachable issue ends like a narrowed one, without a stop

A case no caller reaches is not a defect, so the run has nothing to ask. Unlike a bug that does not reproduce, it needs no evidence from the issue's author either. The handling borrows the not-reproduced path's ending — a test is kept if one exists, `Refs #<N>`, the remaining roles skipped except the spec checker and the validator — but the run does not stop. The maintainer decides when reviewing the pull request whether to close the issue.

### "Reaches" names a caller

A claim that a case is reachable, or that it is not, is checked like any other claim: name the code fix and the node it formats, the CLI option, or the document shape. This keeps the rule from becoming a way to wave away a real defect.

## Alternatives considered

- **Keep the node-level entry points correct for every node** — rejected: it makes every syntax shape a supported input of a method only the code fixes call, and every run pays for cases no user reaches.
- **File unreachable cases as low-priority follow-ups** — rejected: an issue nobody will implement is noise in the tracker, and it carries the same cost at intake when it is picked up.
- **Ask the maintainer about each unreachable issue during the run** — rejected: nothing visible is in question, and the pull request's `Refs` link and run report already let the maintainer close the issue.
