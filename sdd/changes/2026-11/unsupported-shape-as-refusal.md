# Declaring a shape unsupported is a refusal

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [Special cases do not grow the source](special-cases-do-not-grow-source.md), [The formatter's Unsupported outcome](unsupported-precheck.md) |

## What

`AGENTS.md`, *Special cases do not grow the source*, names a third refusal for a rare input class next to withholding the fix and leaving the construct as written: **declaring the shape unsupported**. Its contract is the one the first shape was built under — one FMT-UNSUP requirement per shape, checked cheaply on exactly what the phases receive, from the tokens and trivia inside that input alone, never produced by a phase, a requirement and a test for each phase that throws for it, and declared together with the deletion of every special case it makes unnecessary, listed in the change doc's *Special cases* section; a shape that deletes nothing is not declared.

The three refusals differ in who decides:

- withholding the fix and leaving the construct as written are local and silent, and the rule decides them for a rare class;
- declaring a shape unsupported refuses and reports whole files, so it is always a visible output change: an output question with the options to declare the shape or not, whose declare option shows the input unchanged with the CLI's line, the fixes no longer offered, and whether it is declared in the work or in a change of its own, and names in plain words the special cases the shape would delete.

Declaring a shape is an option wherever a shape costs a special case or a refusal — in a formatter phase, a code fix, or a spec — and the precheck could recognize it under the contract; it is not limited to comments and directives. Whoever sees the option names it, and it is escalated to the maintainer, never decided or dropped on the way. Declaring is the recommended option when the shape forces guards into several phases or keeps coming back in findings; a shape the work's issue is not about is usually declared in a change of its own. Until the answer, the work refuses the class locally where the rule allows it, for a rare class.

The squad follows it:

- *What reaches the maintainer* lists declaring a shape unsupported as in question, escalation 9 offers it among the refusals, and a new escalation 10 asks it wherever it is an option, also for a shape outside the run's issues: with the next batch of output questions, or before publishing when it is raised at or after the auditor; the run refuses the class locally and keeps working until then, and waits at that point as for every batched question;
- *Architecture findings* and the lead's table escalate it for findings on rare classes, guards beyond a cheap check, repeated audit classes, and production growth, while the local refusal is made in any case; *Scope* and *Follow-ups* turn a shape the maintainer wants declared in a change of its own into a follow-up, even for a rare shape; the lead never routes a declaration without the answer and never drops the option without asking;
- the spec author writes the *Special cases* section, with *Deleted* and *Checked and kept*, into a change doc that declares a shape, and `sdd/README.md`, *Change docs*, names that section;
- the skeptic names the option wherever it is one, also for a shape the issue is not about, with the shape's exact span, the special cases it would delete, and the common code it would refuse;
- the auditor reports a special case that only serves a shape that could be declared, and a guard that only serves a declared shape; a guard that also serves a node-level input holding part of the shape, or other constructs, stays;
- the implementer deletes the listed special cases as part of the change and adds no throw site without a requirement and a test, and the test implementer covers the requirement's examples, both sides of the boundary, nested occurrences, each documentation mode, every entry point, closure, and every throw site, and rewrites the existing tests of deleted special cases to the refusal;
- the failure-class catalog gains FC-038 to FC-043 for a shape a phase produces, a check that stops descending, an outcome that depends on the documentation mode, a check that is not local to its input, a guard that survives for a declared shape, and a code fix that asks about the wrong node.

## Why

The precheck gave the formatter an outcome that removes a shape from every phase at once, but the squad's instructions knew only the two silent refusals. Without a third option, a shape that keeps returning is guarded phase by phase again, or left as written construct by construct — the cost the precheck was built to end. Declaring a shape is not silent: the whole file is refused and reported, so the lead cannot take it the way it takes the other two refusals, and the question has to show what the refused files buy.

## Visible output changes

None — no output an end user sees changes. Every shape declared later reaches the maintainer as an output question of its own.

## Decisions

### The maintainer always decides a new shape

Unlike the other refusals, rarity does not decide it: refusing a whole file and reporting it as an error is visible even for a rare shape.

### The declare option shows its whole cost and benefit

The output-question form otherwise shows outputs only. The declare option shows the input unchanged with the CLI's line and names the fixes no longer offered, because the refusal reaches further than the shape's own construct; one line says whether the shape is declared in this work or in a change of its own; and one line behind *Why* names, in plain words, the special cases the shape would delete — the benefit the refused files are weighed against.

### Every case where the option exists is escalated

The maintainer asked that the option not be limited to comment and directive shapes that force guards into several phases, and that every case where it is an option be escalated. The squad cannot judge which shapes are worth refusing whole files for, so no role and not the lead filters the option out; the trigger only decides which option the question recommends.

### Where a shape is declared is part of the answer

The precheck's contract makes a shape a change across the formatter, the code fixes, the CLI, and their specs. Inside a run on another issue it would grow the run past its scope, so the question recommends a change of its own for such a shape, which becomes a follow-up carrying the answer. The rule that drops a follow-up which only serves a rare class does not apply to it. The escalation waits for the next batching point because the run refuses the class locally and can continue; one raised by the auditor or later has no batching point left and is asked before publishing.

### The failure classes come from the first shape

FC-038 to FC-043 prevent the failures the first shape had to rule out: the formatter refusing its own output, a nested declaration missed or wrongly excepted, the CLI and a code fix answering differently for the same text, a node-level target refused for text outside it, a guard kept for a shape no phase sees any more, and a code fix offered for a refused node or withheld for one the formatter formats.

## Alternatives considered

- **Let the lead declare a rare shape like the other refusals** — rejected: it refuses and reports whole files, which is visible output.
- **Propose a declaration only for comment or directive shapes that force guards into several phases** — rejected by the maintainer: other shapes can cost special cases too, and the maintainer, not the squad, weighs them.
- **File a shape outside the run's issues as a follow-up without asking** — rejected: the maintainer asked that every case be escalated, and a follow-up filed without asking would carry a proposal nobody decided.
- **Stop the run at once for the escalation** — rejected: the local refusal lets the run continue, so the question waits for the next batch unless a test needs the answer.
- **Offer "declare in this work" and "declare in a change of its own" as separate output options** — rejected: both give the same output, and where a shape is declared is no output; the declare option names it in one line instead.
- **State the contract in the squad files** — rejected: the rule has one statement in `AGENTS.md`, and the squad files point to it.
