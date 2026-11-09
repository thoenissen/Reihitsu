# Declaring a shape unsupported is a refusal

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |
| Depends on | [Special cases do not grow the source](special-cases-do-not-grow-source.md), [The formatter's Unsupported outcome](unsupported-precheck.md) |

## What

`AGENTS.md`, *Special cases do not grow the source*, names a third refusal for a rare input class next to withholding the fix and leaving the construct as written: **declaring the shape unsupported**. Its contract is the one the first shape was built under — one FMT-UNSUP requirement per shape, checked cheaply on exactly what the phases receive, from the tokens and trivia inside that input alone, never produced by a phase, and declared together with the deletion of every special case it makes unnecessary; a shape that deletes nothing is not declared.

The three refusals differ in who decides:

- withholding the fix and leaving the construct as written are local and silent, and the rule decides them for a rare class;
- declaring a shape unsupported refuses and reports whole files, so it is always a visible output change: an output question that shows the input unchanged with the CLI's line and names the special cases the shape would delete.

It is the refusal to propose when a comment or directive shape forces guards into several phases, or keeps coming back in findings. A new shape is a change of its own, unless the work's issue is about that shape.

The squad follows it:

- *What reaches the maintainer* lists declaring a shape unsupported as in question, and escalation 9 offers it among the refusals;
- *Architecture findings* proposes it as the refusal for a repeated or multi-phase shape, as an output question, and *Scope* makes a shape outside the run's issues a follow-up candidate that carries the question, while the run refuses the class locally;
- the lead's table proposes it for findings on rare classes, guards beyond a cheap check, repeated audit classes, and production growth, keeps such a follow-up instead of dropping it as rare, and never routes a declaration without the maintainer's answer;
- the skeptic names the option with the shape's exact span, the special cases it would delete, and the common code it would refuse;
- the auditor reports a special case that only serves a shape the precheck could refuse, and a guard that survives for a declared shape;
- the implementer deletes the listed special cases as part of the change and adds no throw site without a requirement and a test, and the test implementer covers the requirement's examples, both sides of the boundary, nested occurrences, every entry point, closure, and every throw site;
- the failure-class catalog gains FC-038 to FC-042 for a shape a phase produces, a check that skips nested subtrees, an outcome that depends on the documentation mode, a mix of node-level and whole-document answers, and a guard that survives for a declared shape.

## Why

The precheck gave the formatter an outcome that removes a shape from every phase at once, but the squad's instructions knew only the two silent refusals. Without a third option, a comment or directive shape that keeps returning is guarded phase by phase again, or left as written construct by construct — the cost the precheck was built to end. Declaring a shape is not silent: the whole file is refused and reported, so the lead cannot take it the way it takes the other two refusals, and the question has to show what the refused files buy.

## Visible output changes

None — no output an end user sees changes. Every shape declared later reaches the maintainer as an output question of its own.

## Decisions

### The maintainer always decides a new shape

Unlike the other refusals, rarity does not decide it: refusing a whole file and reporting it as an error is visible even for a rare shape.

### The output question names what the shape deletes

The output-question form otherwise shows outputs only. For a new shape, the deleted special cases are the benefit the refused files are weighed against, so one line behind *Why* names them.

### A shape outside the run's issues is a follow-up

The precheck's contract makes a shape a change across the formatter, the code fixes, the CLI, and their specs. Inside a run on another issue it would grow the run past its scope, so it is filed with its question, and the run refuses the class locally meanwhile. The rule that drops a follow-up which only serves a rare class does not apply to it.

### The failure classes come from the first shape

FC-038 to FC-041 are the questions the first shape had to settle: closure, nested declarations, the documentation mode, and the node-level boundary. FC-042 is the removal the contract demands.

## Alternatives considered

- **Let the lead declare a rare shape like the other refusals** — rejected: it refuses and reports whole files, which is visible output.
- **Ask a new shape inside any run that finds it** — rejected: it would grow the run beyond its issues and mix two changes in one pull request.
- **State the contract in the squad files** — rejected: the rule has one statement in `AGENTS.md`, and the squad files point to it.
