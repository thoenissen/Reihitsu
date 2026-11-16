# The squad refuses by cause, checks its own text, and drops the strict documentation proof

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [The lead chooses between continuing and spawning, and keeps its context small](squad-resume-or-fresh-and-lead-compaction.md) |

## What

- **No strict documentation proof.** `scripts/verify-text-only.sh` loses `--strict-docs`, and the playbook's text cleanup after a passing audit, the cleaner, and the auditor's `pass, text cleanup` no longer ask for it. A documentation change is reported and never fails the proof.
- **Refuse by the cause, not by the cells.** A refusal covers the whole class a role named, stated as a cheap check on the shared cause. A guard that would model a result the compiler, the Renamer, or another build configuration computes is refused, or reads the computed result, from the first finding. The catalog gains FC-044 for that class.
- **Escalation 9 states both halves** (#1725). Design the work needs is built and only confirmed by the escalation. Size that only a rare input class needs is ruled out. The code that implements an issue's own fix, a new type among it, is not escalated. The escalation list holds the one statement; the playbook points to it.
- **The spec author checks its own text.** It runs every example through `scripts/apply-fix.sh` or `scripts/format.sh`, runs the architecture tests after editing requirement references, `Uses` rows, or code owners, and states a boundary by its cause in syntax-tree terms. It names in the change doc the code fixes whose results a formatter change alters, and reads the maintainer decisions an issue links before asking again. `sdd/README.md` no longer says the shared specs need no `Uses` row (#1693).
- **Output questions are checked before a batch.** Neighbouring constructs go into the same batch, every shown output is a real one, a withholding option shows concrete inputs, and option labels stay stable. A routing message quotes the answer and adds nothing.
- **Batches share a mechanism.** The lead proposes a split before the claim when the issues of a batch do not share a mechanism or name more than a handful of code fixes or rules.
- **The session's branch is used.** When the session designates a push branch, the run uses it without recording a decision.
- **Smaller rules:**
  - no `git stash` in the shared tree;
  - escape-carrying fixtures are written byte-exact;
  - the skeptic quotes the fixtures whose results it states;
  - the test implementer searches existing tests, code-fix tests included, for every changed output;
  - the spec author sets a change doc's `Status: Accepted`;
  - the auditor's SHA is read after a repair, too;
  - an issue's "before" output comes from a run, not from reading code;
  - the validator gets a tool allow-list;
  - every handover template that lacked one gets a `Follow-up candidates` section, and the validator's file says why it has none (#1427).
- **Usage names model tiers.** `scripts/usage.sh` writes large, medium, and small instead of model identifiers.

## Why

The run notes from 2026-10-31 to 2026-11-16 show where runs got expensive and what kept coming back.

- **The strict proof cost a repair round in twelve runs.** It classified documentation on `public` members of `internal` types, and of test classes, as public API. It caught nothing in any of them: no assembly here is a public API (`AGENTS.md`, *Supported inputs*), and the package ships no XML documentation. Narrowing the classification to members reachable from outside the assembly (#1545) would still have failed on test methods in public test classes.
- **The most expensive runs modelled a computed result instead of refusing it.** Two of them read about 290 million cached tokens each, against a median of about 70 million. The renamer's reach, interface mappings, documentation expansion, `#if` groups, and a moved statement's column were modelled one audit at a time. Each audit found the next member, until the class was refused by a cheap check. Where a run refused only the cells an audit had listed, the next audit found the neighbouring cell.
- **Spec examples were the most frequent avoidable re-spawn.** They did not compile, contradicted an unchanged requirement, or broke a `Uses` row. The implementer or the spec checker found them one stage too late, at the cost of a spec-author spawn and often a test-implementer spawn.
- **Output questions cost extra stops.** Sibling constructs were asked one at a time, shown outputs were ones no phase produces, and option letters shifted between drafts. A routing message read an answer more widely than it was given.
- **Wide batches made every agent large.** Batches of six to eight issues over different mechanisms did this, and their handovers were re-read in every repair round.
- **Twelve runs recorded the same "decision" to use the session's branch.**
- **Model identifiers were handled inconsistently.** Some sessions forbid pushing them. Their notes replaced the model by placeholders, and one note dropped its usage entirely; the other notes carried full identifiers. Tiers keep the information a cost evaluation needs, with the date and `.claude/agents/README.md`, in a form every session can push.

## Visible output changes

None — no output an end user sees changes.

## Decisions

### Drop `--strict-docs` rather than narrow it

#1545 proposed counting only members reachable from outside the assembly. That still fails a comment fix on a public test class. It also keeps a classification whose premise, a public API, the repository rejects. Removing the option is less code, and the proof still reports every documentation change.

### Refusing from the first finding applies to computed results only

A rare class in general keeps the existing rule: refuse it, and refuse the whole class at the first repeat. Only a guard that would model what the compiler, the Renamer, or another build configuration computes is refused from the first finding. Such a model has no end: every input the computation covers is a possible next finding.

### No leaving a batch, still

A rule that narrows one blocking issue out of a running batch was proposed again. It stays rejected, for the reason the change this one depends on gives. The batch-size check before the claim addresses the same cost without discarding work.

## Alternatives considered

- **Exempt test projects from `--strict-docs`.** Rejected: it keeps the false failures on `internal` types in shipped projects.
- **Record model identifiers and let each session decide.** Rejected: this is how the notes became inconsistent and one run's usage was lost.
- **A hard limit on batch size.** Rejected: the maintainer chooses the batch, and a shared mechanism matters more than the count.
- **Lower the continue-or-fresh threshold, and run the validator project by project.** Not decided here: one run questioned the threshold, and the validator's memory use in the cloud sandbox needs an investigation first.
