# Special cases do not grow the source

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | none — process change |

## What

`AGENTS.md`, *Code-fix coverage*, gains one rule for code fixes and the formatter: a guard is a cheap syntactic check of what the rewrite touches. When making a rewrite safe for a rare class of inputs needs more — evaluating `#if` conditions or reconstructing other build configurations, predicting another component or a later phase, parser-like rules, a new type — the fix is withheld for that class, and the formatter leaves the construct as written. Every input that already violates another Reihitsu rule, such as a comment inside an argument list, is rare by definition. Non-destructiveness still comes first, and a refusal that would hit code people commonly write goes to the maintainer.

The squad follows it:

- the skeptic names the refuse option for every issue and compares its cost with the requested change;
- a finding on a rare input class, and an audit finding that repeats a class, is answered by widening the refusal first, not by a structural change;
- a structural change that adds more than about 100 production lines or a new type is a new escalation (9), which always offers refusing the class instead;
- the auditor reports production growth a refusal would have avoided, and suggests "refuse this class" for findings on rare inputs;
- the implementer reports its production diff size and stops when a guard needs more than a cheap check.

## Why

In the run on issues #1595, #1593, and #1616, the member-moving fixes of RH7102, RH7103, RH7109, and RH7309 had to be made safe around `#if`. Repeated audit findings restructured the shared guard in `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` three times — parsing per configuration, enumerating symbol assignments, limiting the number of configurations — and grew it by about 1000 lines for `#if` shapes nobody writes. The maintainer then decided that `#if` is a special case: a fix decides only from where directives sit in the syntax tree, evaluates no condition, and is withheld where that check cannot classify the input.

Nothing in the instructions prevented the escalation. "Prefer no fix over an unsafe one" said what to do with an unsafe fix but put no limit on what a guard may cost, and the squad's rule for architecture findings changed the structure whenever a class repeated in an audit. Each round of more reasoning found the next uncovered member; refusing the class ends that loop.

## Visible output changes

None — no output an end user sees changes.

## Decisions

### The limit is the kind of reasoning, not a line count

A guard is defined by what it may not do, not by "a few lines": a scan of the trivia of a moved span is generic and cheap even when it is not local to one token. The 100-line threshold applies only to structural changes, and only as an escalation trigger.

### Inputs that violate another rule are rare

A comment inside an argument list or a `#region` inside a member is already reported by its own rule. A fix or a formatter phase that builds machinery to serve such an input pays for a case the user is told to remove anyway.

### Common code is the exception

A refusal that would withhold a fix, or leave a construct unformatted, for code people commonly write would empty a fix of its value. That case is escalated to the maintainer instead of being decided by the rule.

### Withholding for a rare class is not an output question

The rule decides it, so the lead does not ask; the pull request names the refused class.

## Alternatives considered

- **A hard line limit for every guard** — rejected: it is arbitrary and blocks cheap generic checks.
- **Keep refusing as an option the lead may choose** — rejected: the run above shows that the structural option wins by default when nothing ranks the refusal first.
