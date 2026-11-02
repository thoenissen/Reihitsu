Role: lead
Commit: 250aa7c
Verdict: lane claimed — behavior change

# Run: phase-order-line-breaks

## Issues

- thoenissen/Reihitsu#1498 — "FMT-PIPE-001: line breaks are decided before switch braces and blank lines". Label `agent-filed`: its goals, Done-when items, target output, and prescribed solution are proposals. Parent: thoenissen/Reihitsu#1482. The issue comment of 2026-11-02 extends its Done-when to switch sections (the FMT-BLANK-004 sentence that lets the blank-line phase end a `break`'s line; input `switch (x) { case 1: break; case 2: break; }` must be stable in one run after the reorder).
- Blockers named by the issue (C1 thoenissen/Reihitsu#1490, C2 #1491, C3 #1496, C7 #1495) appear merged on `main` (#1535, #1532, #1541, #1558). Every statement the issue makes about today's code must be re-checked against current `main`; statements already made true or moot are noted and dropped.

## Lane

Behavior change: the issue changes the phase order and visible formatter output (the comment move below), and requires a change doc. Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. No role is skipped.

## Maintainer answers

1. Formatter · comment behind an `if` condition (asked before the claim). The maintainer chose to **remove FMT-BREAK-005's move in this run**: a comment trailing the closing parenthesis of an `if` condition stays where the user wrote it, as it already does for `while`, `foreach`, and `else if`. The phase reorder still happens in this run, motivated by the issue's other defects (the blank-line phase splitting same-line statements; the switch-brace phase's copy of brace placement and blank-line rules). Agreed output for the issue's example:

   ```csharp
   while (b)
       if (a) // why
       {
           A();
       }
   ```

   Evidence shown to the maintainer: on `main` at aec8dc6, `while (a) // w {…}` and `foreach (var x in xs) // f {…}` keep the comment behind the parenthesis, stable in one pass; only `if (a) // i` and `if (a) /* m */ { A(); }` move the comment above the `if`. The requirement's stated reason (the comment would end up between condition and brace or glued to the brace) does not occur for those statements.

   Consequence for the issue: its example input and "Desired Formatted Output" are superseded by the answer; the Done-when "the input above is stable in one run, under a regression test" still holds, with the comment staying behind the condition.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | The session's designated branch `claude/confident-rubin-j3pkrc` instead of `claude/issue-1498-<slug>` | The session may push only to its designated branch | Branch names are internal | 250aa7c |
| 2 | Spec author | The Done-when "the blank-line phase inserts no line break that ends a statement's line" cannot hold where FMT-BREAK-006 leaves a statement next to an empty statement on its line (`a++; ; return a;`, `if (a) { } ;`) | Narrow the goal: the blank-line phase still ends that line when it inserts a blank line there (FMT-BLANK-021); every other same-line split is the line-break phase's | `agent-filed` goal; keeping it fully would change output on `main` | Output stays as on `main` | 1a072d5 |
| 3 | Spec author | FMT-PIPE-024 loses the `else if` fact once FMT-BREAK-005 is retired | Take it in: remove `RootConstruct.ElseIf`, its computation, and `IsElseIfBranch` in this run | Only the retired move reads it; leaving it would be dead code in components the run already changes | Internal API with no other reader | 1a072d5 |
| 4 | Spec author | Do visible output changes 1–4 of the change doc need an output question? | No: all four are the maintainer's answer 1 (the comment stays behind an `if` condition) applied to its shapes | Covered by answer 1 | Decided by the maintainer | 1a072d5 |
| 5 | Spec author | Accept the change doc | Accept after the skeptic and any amendment | Lead playbook, section 4 | — | 1a072d5 |

## Follow-up candidates

None yet.
