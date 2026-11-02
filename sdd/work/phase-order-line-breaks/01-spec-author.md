Role: spec author
Commit: 7ab1427
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The maintainer answer in `00-lead.md` (remove FMT-BREAK-005's move) overrides the issue's example output and is recorded in the change doc's `Decisions`.

Files: `sdd/changes/2026-11/phase-order-line-breaks.md` (Status: Draft), `sdd/specs/formatter/pipeline.md`, `sdd/specs/formatter/line-breaks.md`, `sdd/specs/formatter/blank-lines.md`, `sdd/specs/formatter/switch-case-braces.md`, `sdd/specs/requirement-ids.txt`, `AGENTS.md` (order sentence, ownership table rows and order). No rule page changes: no analyzer's report or fix changes, and rule pages never mention the formatter.

## Changed requirements

- FMT-PIPE-001 — Fixed phase order — changed: table order LineBreak (6) → SwitchCaseBrace (7) → BlankLine (8); the "columns only" sentence now points to FMT-PIPE-025; Why rewritten for the new order.
- FMT-PIPE-024 — Node-level input facts are plain values — changed: the `else if` fact row and its bullet clause removed (only FMT-BREAK-005's move read it, via `RootConstruct.ElseIf` / `LineBreakContainedBlockRewriter.IsElseIfBranch`); the Why's example that relied on it replaced by the list-position case.
- FMT-PIPE-025 — No phase decides a line break or a blank line from a later phase's — added.
- FMT-PIPE-026 — Whether a comment starts its line is final after the line-break phase — added.
- FMT-BREAK-005 — A comment behind an `if` condition moves above the `if` — retired (maintainer answer; replaced by FMT-BREAK-036).
- FMT-BREAK-006 — One statement per line — changed: one clause pointing to FMT-BLANK-021 for an empty-statement pair that a blank-line rule separates.
- FMT-BREAK-030 — The phase keeps the blank lines of every gap it does not join — changed: Why and the example's closing sentence ("removes the second after this phase runs").
- FMT-BREAK-033 — Every switch section starts its own line — clarified: the `/** d */` example's lead-in says the FMT-BLANK-012 blank line is added after this phase. No normative change.
- FMT-BREAK-036 — A comment behind the head of a block statement stays there — added (new for `if`; existing behavior for `else if`, `else`, `while`, `for`, `foreach`, `using`, `lock`, verified on this commit with both `// c` + brace on next line and `/* c */ { … }` on one line).
- FMT-BLANK-004 — `break` is separated on both sides — changed: the sentence that let the phase write the line break ending the `break`'s line is reverted (issue comment); Why says the line-break phase runs first; the same-line example stays with a new lead-in.
- FMT-BLANK-005 — The blank line after a closing brace goes behind a directive — changed: same-line sentence and the "as if they were already split" Why sentence replaced/removed; examples unchanged.
- FMT-BLANK-021 — A blank line in front of a statement on its predecessor's line ends that line — added (clarification of existing output, see Internal choices).
- FMT-SWITCH-007 — Comments survive brace insertion — changed: "already in the output of the pass that inserts the braces" → in the same run, inserted by the blank-line phase; the carried blank lines are collapsed/removed by the blank-line phase after the wrap. Examples unchanged.
- FMT-SWITCH-015 — The inserted braces are placed as the block of a section — added.
- FMT-SWITCH-016 — Brace insertion writes no blank line and removes none — added.
- Non-goals: FMT-BLANK (line breaks — names FMT-BLANK-021 as the one exception), FMT-SWITCH (third non-goal reconciled with FMT-SWITCH-015/016). Intro of FMT-BLANK states its position after the line-break and switch-brace phases.
- `Uses` headers of all four specs updated; checked with a script against the rule "exactly the active IDs of other specs referenced outside header and non-goals" (blank-lines.md drops SHARED-BLANK-008, which only the reverted sentence used).

## Examples for the tests

- FMT-BREAK-036 — its example; visible output changes 1–4 of the change doc (4 has an unbraced `if` body that FMT-STRUCT braces). `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ElseIfConditionCommentTests.cs` tags the retired FMT-BREAK-005 twice (lines 22, 48); `RequirementCoverageTests.EveryRequirementReferenceNamesAnActiveRequirement` fails until they are retagged.
- FMT-PIPE-025 — its example (`switch (x) { case 1: break; case 2: break; }`, the issue comment's input), stable in one run.
- FMT-PIPE-026 — its example (the issue's input), kept unchanged in one run.
- FMT-BLANK-021 — its example (`a++; ; return a;`, `if (a) { } ;`).
- FMT-SWITCH-015 — its example.
- FMT-SWITCH-016 — its example (blank line after the label, a run of two blank lines before `// b`).
- FMT-PIPE-001 — the runtime order (`PhaseOwnershipDocumentationTests.OwnershipMapMatchesRuntimePhaseOrder` fails now: `AGENTS.md` shows the new order, `FormattingPipeline.CreatePhases` the old).
- Unchanged-output regression seeds that the reorder touches: FMT-BLANK-004 same-line section example; FMT-BLANK-005 same-line pair example; FMT-SWITCH-007's five examples; FMT-BREAK-033's `/** d */` example (FMT-BLANK-012 blank line now decided two phases after FMT-DOC-001's move); FMT-BREAK-030's example.

Every "before" output and every unchanged-output example named above was run through `scripts/format.sh` / `scripts/trace.sh` on this commit, except the `if` parts of FMT-BREAK-036 and FMT-PIPE-026, whose output is the change.

`RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` fails for the six added IDs until tests exist. `SpecConsistencyTests` and `SpecStructureTests` pass.

## Issue statements checked against this commit

- Two-run blank line above the moved `if` comment: reproduced (trace, pass 2 inserts the blank line above `// why`). Moot after the maintainer's answer.
- `BlankLineEditor.InsertBlankLineBeforeToken` writes the line break that ends the line together with the blank line (`RequiredLineBreakCountForBlankLine`): true; used by `EnsureBlankLineBeforeStatement`, `EnsureBlankLineBeforeSection`, `EnsureBlankLineAfterClosingBrace`.
- After the reorder the line-break phase still leaves a pair on one line next to an empty statement (FMT-BREAK-006); on this commit `a = false; ; return;` and `if (a) { } ;` are split with a blank line by the blank-line phase. This is why the issue's Done-when "the blank-line phase inserts no line break that ends a statement's line" cannot hold without a visible change (see Internal choices).
- `SwitchCaseBraceRewriter.AddBraces`: after thoenissen/Reihitsu#1490 it carries trivia through `BlockWrapTriviaUtilities` and writes its own end-of-line trivia for the braces (no `BracePlacer`). It writes no blank line; it drops blank lines above the first leading comment of the first statement (`keepBlankLinesAboveFirstComment: false`) and keeps them at the terminal `break` (`true`), whose code comment says "the blank lines the blank-line phase placed in front of them".
- FMT-SWITCH-007's clause and FMT-SWITCH's third non-goal: present as the issue quotes them; reconciled.
- FMT-BLANK-005: the issue's "second sentence" is now its third; the "as if they were already split" text is in its Why. Both replaced.
- SHARED-BLANK-003's Why ("splits … (FMT-BREAK) and then separates … (FMT-BLANK)") describes the new order; left unchanged (change doc, *Decisions*).
- Code comments that state the old order, for whoever changes that code: `src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockWrapTriviaUtilities.cs` (type summary), `src/Reihitsu.Formatter/Pipeline/SwitchCaseBraces/Rewriter/SwitchCaseBraceRewriter.cs` (`AddBraces`, terminal-break comment), `src/Reihitsu.Formatter.Test/Regression/LineBreaks/AutoPropertyTrailingCommentTests.cs:233`.

## Output questions

_None._ The only visible output change (FMT-BREAK-005 retired) is the maintainer's answer 1; the change doc lists it as visible output changes 1–4 for the lead to confirm against that answer. Every other output is specified as unchanged.

## Internal choices

- FMT-BLANK-021 keeps `main`'s output for a statement next to an empty statement (blank-line phase writes the line break and the blank line) instead of the issue's "blank-line phase writes no line break that ends a statement's line". Alternative: no line break from the blank-line phase, so `; return a;` stays on one line without a blank line — a visible change, not proposed; recommend the lead record the narrowed Done-when under *Decisions taken*.
- The line-break/blank-line rule is a new FMT-PIPE-025 rather than more text in FMT-PIPE-001, and the comment rule a separate FMT-PIPE-026 (different reason). Alternative: one extended FMT-PIPE-001 as the issue proposed.
- FMT-BREAK-005 retired and replaced by FMT-BREAK-036, which also states the existing behavior for `else if`, `else`, `while`, `for`, `foreach`, `using`, `lock`, with a coverage sentence. Alternative: retire without replacement and leave the case to FMT-BREAK-001/002 — rejected, nothing would then state that the comment stays.
- Switch-brace layout split into FMT-SWITCH-015 (placement as FMT-BREAK-008, by the line-break phase's placement) and FMT-SWITCH-016 (no blank line written or removed). Alternative: only reword the non-goal.
- FMT-SWITCH-016 also forbids *removing* a carried blank line (the drop above the first statement's first comment). Output is unchanged because FMT-BLANK-007 removes a blank line behind `{` (verified on a plain block: `{`, blank, `// c` → blank removed). Alternative: forbid only writing one.
- FMT-PIPE-024 loses the `else if` fact, since no decision reads it once the move is gone (`RootConstruct.ElseIf`, its computation in `ReihitsuFormatterHelpers`, and `IsElseIfBranch`). This adds production paths beyond the issue's list; reported for the lead's scope decision.
- `AGENTS.md` table: `BracePlacer` added to the `SwitchCaseBracePhase` guards column, matching FMT-SWITCH-015 and the issue's Done-when.

## Follow-up candidates

_None._
