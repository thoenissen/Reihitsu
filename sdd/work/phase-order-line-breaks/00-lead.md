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
| 6 | Skeptic | FMT-BLANK-021 breaks idempotency after the reorder (a switch section whose `;` is followed by `return`/`throw` on the same line is braced only in run 2) — which repair? | Option 1A of 02-skeptic: the line-break phase starts a statement on its own line where a blank-line rule separates it from the empty statement before it; both phases read one shared statement-kind predicate (moved next to `BlankLineSpacingPolicy` in Core). FMT-BLANK-021 is dropped, FMT-BREAK-006's empty-statement exemption narrowed, FMT-PIPE-025 worded to allow the shared predicate, and the change doc's rejected alternative updated | Option C is forbidden by FMT-PIPE-025; option B changes output; a private copy would add a second statement of the rule (architecture finding: change the structure in this run, within the run's components) | Output stays as on `main` (skeptic traced it), idempotency restored | 700cdcf |
| 7 | Skeptic | Decision 2 (narrowed Done-when) | Reverted: with decision 6 the blank-line phase inserts no line break that ends a statement's line, so the issue's Done-when holds unnarrowed | Follows from decision 6 | Output as on `main` | 700cdcf |
| 8 | Skeptic | Architecture finding: `BraceBlankLinePolicy.FindTokenLineStartIndex` calls `ChainLineBreakRewriter.BreaksLineBehindMultiLineCommentTail` to predict a later line break | Change the structure in this run: remove the prediction, guarded by the FMT-BLANK-018 chain tests; update the stale remarks in `RemovesBlankLinesBehindOwnLineComment` and `GetWrappedLayoutLineBreaks` | After the reorder it predicts a line break that already exists; it is a second statement of the line-break rule in components the run changes | Output unchanged; guarded by existing tests | 700cdcf |
| 9 | Skeptic | RH5405/RH5406/RH5407 code fixes (via `FormatNodeInDocumentAsync`) no longer move `// c` above the `if` | Covered by maintainer answer 1; the change doc lists it as a visible output change | Same decision, a different surface reaching the same requirement | Decided by the maintainer | 700cdcf |
| 10 | Skeptic | FMT-PIPE-025 wording "line break between two tokens" is too wide | Narrow it as 02-skeptic proposes: whether two tokens with text stand on one line, with blank lines named separately; cleanup's end-of-file/root removals outside it | Accuracy | Spec wording only | 700cdcf |
| 11 | Skeptic | FMT-SWITCH-016 "removes none"; FMT-SWITCH-015 names a mechanism | Keep "removes none"; state FMT-SWITCH-015 as the observable line positions, not the mechanism | Skeptic's check shows output unchanged; requirements must be testable | Output unchanged | 700cdcf |
| 12 | Spec author (amendment) | FMT-BLANK-021 deleted or retired? | Deleted | The ledger rule covers IDs ever issued; this one existed only on the run's branch and never reached `main` | Spec bookkeeping | 700cdcf |
| 13 | Spec author (amendment) | FMT-BREAK-006 split in both directions (statement behind `;` and `;` behind a statement)? | Both | One direction would leave a statement-ending line break to the blank-line phase, against decision 7 | Output as on `main` | 700cdcf |
| 14 | Spec author (amendment) | Shared statement-kind rule: own requirement? | No; stated in the `Why`s and FMT-PIPE-025's allowance. If the implementer makes it a new Core type, the spec lists it as code owner in a repair | Not a separate behavior | Internal | 700cdcf |
| 15 | Spec author (amendment) | `BracePlacer` in the `AGENTS.md` switch-brace row | Keep: the issue's Done-when has the switch-brace phase place braces through `BracePlacer`; if the implementation cannot, the row is corrected in a repair | `agent-filed` goal kept while it holds | Internal | 700cdcf |
| 16 | Lead | Accept the change doc | Accepted after the skeptic and its amendment; no output question open before the test implementer (visible changes 1–5 are covered by answer 1) | Lead playbook, sections 4–5 | — | 700cdcf |
| 17 | Test implementer | `RH5104CommentsMustBeOnTheirOwnLineFormatterTests.VerifyFormatterFixesViolation` expects the formatter to clear RH5104 on `if (left == right) // …`, which FMT-BREAK-036 now keeps | Rewrite the test: the input stays reported by RH5104 and the formatter keeps it; RH5104 and its rule page are unchanged | SHARED-DIAG-010 covers only a rule whose fix lays the code out as the formatter does; RH5104's fix moves comments, which the formatter never does (on `main` RH5104 already reports `while (a) // w` and `int _a; // first`, which the formatter keeps). No exception to record | RH5104's diagnostics, fix, and rule page are unchanged; the formatter output is maintainer answer 1 | dbf2f1d |
| 18 | Test implementer | `AGENTS.md` `BlankLinePhase` row still names the empty-statement line break | Spec author corrects it | Decisions 6, 7, 12 removed that rule | Text | 66ed0a8 |
| 19 | Test implementer | FMT-SWITCH-015 "the line below the opening brace" contradicts FMT-SWITCH-016's carried blank line | Spec author rewords to the next line with text | Consistency of two requirements | Output unchanged; tests hold under either reading | 66ed0a8 |
| 20 | Implementer | Test-file edit in `ContainedBlockRewriterTests.cs` (removed constructor argument) | Accept | Touches no assertion, input, or expected value; keeping an unused parameter would be dead code | Internal | b48a47e |
| 21 | Implementer | Architecture finding: `SwitchCaseBraceRewriter.AddBraces` places braces with its own code, not `BracePlacer`; outputs agree | Fix narrowly: drop the issue's `agent-filed` Done-when "places braces through `BracePlacer`", correct the `AGENTS.md` `SwitchCaseBracePhase` row to name the real guards, and file the structural change as a follow-up | The copy is pre-existing; this run's change adds no copy, prediction, or second statement of the rule, and the class has not repeated in an audit (`README.md`, *Architecture findings*); a section without a terminal `break` needs a token outside the section moved, beyond the switch-brace phase's reach | Output unchanged; FMT-SWITCH-015 states line positions, which the tests check | pending |
| 22 | Implementer | FMT-SWITCH-016 "writes no blank line and removes none" vs the wrap dropping comment-free blank lines in front of the first statement and the terminal `break`, as on `main` | Spec author words FMT-SWITCH-016 to match the code: blank lines that lead a comment are carried; the code is not changed | Carrying comment-free blank lines could change output; `main`'s output is the target | Output as on `main` | pending |
| 23 | Implementer | Code owners for the statement-kind rules now in `Reihitsu.Core/BlankLineSpacingPolicy.cs` | Spec author lists the owner where the FMT-BLANK requirements name their code owners | Decision 14 | Spec bookkeeping | pending |
| 24 | Implementer | Stale order remark in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs:233` | Adjacent fix: the cleaner corrects the comment | Text only, in a file the run's subject covers | Comment only | pending |

## Follow-up candidates

- Switch-brace phase places its braces with its own code instead of the line-break phase's `BracePlacer` (FMT-BREAK-008); a section without a terminal `break` needs the token after the section moved. Structural change. — 07-implementer.md, decision 21
- `ControlFlowBraceTransform` drops the blank lines in front of a braced body's first comment; output not checked. — 07-implementer.md
- `BlankLineEditor.InsertBlankLineBeforeToken` still writes a line-ending line break when a token sits behind a multi-line comment's closing line (statements and switch sections no longer reach it). — 07-implementer.md
