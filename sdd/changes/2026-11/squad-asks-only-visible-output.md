# The squad asks the maintainer only about what an end user sees

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |

## What

The squad's lead asks the maintainer only when an output an end user sees is in question, and decides every internal question itself.

- **Visible output** is the formatter's output, analyzer diagnostics, code-fix results, rule pages, and CLI behavior. It is in question when no unchanged requirement and no earlier answer decides it. A bug fix that restores an unchanged requirement puts nothing in question.
- **Output questions** are asked as outputs: the rule's ID and title, the input, the output before, and one output per option as C# blocks, and one sentence of reason. They are batched once before the tests are written and once before the final audit.
- **Change docs need no approval of their own.** A change doc lists its visible output changes, each of them is an output question, and an amendment is asked only for the visible changes it adds or alters.
- **Every other question is decided by the lead**, including change-doc acceptance, spec wording, architecture findings and repeated audit classes, scope growth, failures on `main`, lane widening, and follow-ups. It records each decision in a `Decisions taken` table in the pull request's run report, with the commit that carries it out.
- **Scope is not frozen.** A small adjacent fix in a component the run already changes may join the run instead of becoming an issue.
- **Issues are hypotheses, goals included.** An issue an agent writes carries the label `agent-filed`. The goals of such an issue are proposals that the lead may drop or adjust when nothing visible is in question; the goals of the maintainer's own issues stay binding.
- **An issue manager writes issues.** A new role, spawned once before publishing, files every follow-up without the maintainer approving its text, labeled `agent-filed`, and the pull request lists them. An open question outside the run's issues becomes such an issue instead of a chat question.
- **Safety rails stay.** A run that drops, relocates, or alters user code, produces code that does not compile, or breaks idempotency or convergence, and cannot repair it, still stops. So do a new rule, a suppression, a bug that does not reproduce, a skeptic verdict against a binding issue, and a failed narrowing after the third repair round.
- **Run notes replace decisions entries.** `squad/decisions/` becomes `squad/notes/`: one temporary note per run for improving the squad, never referenced from another file. The entries of the earlier runs are folded into this change and deleted.

## Why

The run of issue #1489 stopped four times before a single test was written: to approve the change doc, for a question about a case outside the run, for the skeptic's findings together with an architecture finding, and to approve the change doc again after the skeptic amended it. Every follow-up issue waited for the maintainer to approve its exact text. Every stop costs a session round trip, and resuming makes the agents re-read the run's handovers.

The decisions entries of the eight runs before it (issues #1463, #1467, #1483, #1484, #1485, #1487, #1488, #1489) show the pattern:

- 61 questions reached the maintainer. 34 of them concerned no end-user-visible output: which component owns a decision, requirement wording, test structure, scope, and lanes. 23 were architecture findings.
- None of the entries records the maintainer choosing an option other than the one recommended. All 12 change-doc approvals were "as written".
- In three of the eight runs, the maintainer handed every further question to the lead's recommendation partway through, because the stops had become the bottleneck.
- Where the maintainer's answer added something of their own, it was mostly about visible output: preferring one layout, setting a condition on a spacing outcome, deciding which keyword a rule checks.

The maintainer's attention is worth most on what users see. Internal questions get the lead's recommendation anyway, and a review of a table of decisions in the pull request costs less than a stop per decision. The auditor's new check that every changed visible expectation is covered by an unchanged requirement or an answer keeps a visible change from slipping through as an internal one.

The decisions entries had also drifted from their purpose. They were meant to collect answers until a pattern showed, but merged change docs began citing them as the record of a decision, which made temporary notes into sources. Answers to output questions now live where they take effect — in the change doc or the spec — and notes stay temporary and unreferenced.

## Decisions

### Change docs

A change doc is not approved as a document. The maintainer decides the visible output changes it lists, and the lead accepts the rest. An internal-only change doc is accepted by the lead.

### Architecture findings

The lead changes the structure in the run when the class has repeated in an audit of the run, or when the narrow fix would itself add a copy, a prediction, or a second statement of a rule, provided the structural change stays within the components the run changes. Otherwise it fixes narrowly and files the structure as a follow-up. A fixed "always narrow" default was rejected: the maintainer's earlier answers chose the structural option about as often as the narrow one.

### After the third repair round

The run narrows instead of stopping: the failing part goes back to `main`'s behavior, the issue that is then not fully met is linked with `Refs`, and the narrowed part becomes a follow-up. It stops only when the narrowing would change an output the maintainer answered, or the run still blocks after it.

### Defects on `main`

A loss of user code that exists only on `main`, outside what the run changes, becomes a `bug` follow-up that the run report names first. It is not asked, because the output is not in doubt; only where to fix it is.

### The `agent-filed` label

The label marks the text as not reviewed by the maintainer, not merely as written by an agent. It was applied once to every open issue, and only the maintainer removes it. `/triage` leaves it alone, because assigning a milestone is not a review of the text.

## Alternatives considered

- **Keep approving change docs, but only at batched points** — rejected: the approvals never changed a change doc, and they still cost a stop.
- **Ask every visible output change compared with `main`** — rejected: every bug fix would come back to the maintainer, though the spec already decides it.
- **A decisions entry per lead decision** — rejected: it would have produced about 34 files in a week for questions the maintainer never saw. The pull request's table serves the review.
- **Drop the notes folder entirely** — rejected: per-run notes are what made this analysis possible, and a pull request description is no longer read once the pull request is merged.
