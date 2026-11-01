Role: spec author
Commit: 5cda010
Verdict: spec written

Routed-findings round after the implementer (`05-implementer.md` D3, D4, D5, Scope; lead decisions 11–13 and the maintainer's implementer-stage answer). The prompt carried no intended fix or verdict to ignore. I re-derived each item: I built the CLI at 741157c (`main`, in a scratch worktree) and at 5cda010, formatted the same fixtures with `--force` on both, and formatted every input example of FMT-BLANK-009, -011, -017 and -018 at 5cda010. Each one gives its stated output. Each new example also gives its stated output on `main`, except the new FMT-BLANK-009 example, which is the visible change. The change doc keeps `Status: Accepted`; see the D3 note on `]`.

## What re-deriving found

- **D3 confirmed, and the class is wider than the implementer measured.**
  - Without a blank line between the directive and `#endregion` in the input (`a++;⏎#pragma …⏎#endregion⏎}`), FMT-BLANK-013 inserts the region's blank line. `main` also inserts one in front of `#pragma`; the branch does not.
  - A collection expression's `]` behaves the same: `1,⏎#pragma …⏎⏎#region R⏎⏎#endregion⏎]` gains a blank line above `#pragma` on `main` and not on the branch. The implementer's "`]` holds" fixture had no region directive.
  - Case that keeps its output: `#endregion⏎#pragma …⏎}` keeps the blank line below `#endregion` (SHARED-BLANK-006) on both.
  - Case that keeps its output: an author's blank line above the first directive is kept on both.
- **D4 confirmed for all three shapes**, on `main` and on the branch with identical output:
  - The opening brace of a multi-line complex element initializer gets no blank line above a glued comment. A one-line complex element initializer gets one.
  - The closing brace of a collection initializer assigned to a member (`L = { … /* l */ }`) gets one. The closing brace of an unassigned collection initializer gets none.
  - `/* x */ /** Doc */ {` gets one. `/* y */ {` gets none.
  - FMT-BLANK-020 follows the same set on both: `// c1⏎⏎{` (multi-line complex) is removed, and `// c2⏎⏎{ 5, 6 }` and `// l⏎⏎}` (assigned) are kept.
- **D5 confirmed.** The implementation skips FMT-BLANK-011 for a comment glued to a block-statement brace that follows a statement, as FMT-BLANK-017 and FMT-BLANK-018's example require.
- **D1 side note acted on.** The FMT-BLANK-017 example and change-doc entry 2 used `#if DEBUG` with formatted content inside. Without the symbol defined that content is disabled text, so both now use `#if !NEVER`, as FMT-BLANK-016 already does. The outputs are unchanged otherwise.

## Changed requirements

- FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed — changed. It now inserts no blank line in front of the first non-region directive or disabled text of the gap, also when a region directive ends the gap. A directive directly below a region directive is left to FMT-BLANK-013. `Why` is extended. A new example covers a type body (blank line kept) and a method body (blank line inserted by FMT-BLANK-013) (D3).
- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — changed. The exemption now covers a comment glued (FMT-BLANK-018's definition) to a token FMT-BLANK-018 names or to the opening brace of a block statement that follows a statement (D5).
- FMT-BLANK-017 — directive example changed from `#if DEBUG` to `#if !NEVER`; no change to the text.
- FMT-BLANK-018 — A comment glued to a brace gets no blank line of its own — changed (D4):
  - "glued" is defined as an ordinary comment with nothing but whitespace and ordinary comments up to the token;
  - the excluded tokens add the one-line complex element initializer's `{` (the multi-line one is no longer excluded) and the closing brace of a collection initializer assigned to a member;
  - a comment in front of a documentation comment on the token's line falls under FMT-BLANK-011;
  - `Why` is updated;
  - the complex-element example now shows both sides, and examples are added for assigned vs. unassigned and for a documentation comment vs. none.
- FMT-BLANK-020 — no text change; its token set follows FMT-BLANK-018's corrected list.
- FMT-BLANK code owners: `BraceBlankLinePolicy` added, in the spec header and in `sdd/specs/README.md`.
- Change doc `sdd/changes/2026-11/blank-line-sole-owner.md`:
  - `What`: the FMT-BLANK-018 token list, the FMT-BLANK-011 exemption, and a new FMT-BLANK-009 bullet;
  - `Precedence`: the FMT-BLANK-011 bullet;
  - visible output change 11 is new (D3, before/after measured);
  - entry 2 now uses `#if !NEVER`;
  - `Unchanged`: the complex-element bullet;
  - `Decisions`: the maintainer's implementer-stage answer, the FMT-BLANK-018 and FMT-BLANK-020 token lists, and a new decision for the region-directive case;
  - `Alternatives`: the last entry.

The SDD architecture tests (`--filter FullyQualifiedName~Sdd`, including SpecConsistency, SpecStructure and RequirementCoverage) pass: 51/51. No new requirement ID.

## Examples for the tests

- FMT-BLANK-009, the new region-directive example (`class C` + `class D`). It is the visible change. The existing test `ReihitsuFormatterLineEndingTests.LineBreaksEndingPragmaAndRegionDirectivesAreRewritten` gets the new expected output, as the lead recorded. Also pin these, LF and CRLF:
  - `#if NEVER … #endif⏎⏎#endregion⏎}` in a method body: no blank line above `#if`;
  - `#endregion⏎#pragma …⏎}`: blank line below `#endregion` kept or inserted;
  - the author's blank line above the first directive: kept.
- FMT-BLANK-018, the three new D4 examples. They are today's output and already pinned in part by the RH5303 and `DelimitedDocumentationBeforeBlockStatementTests` tests the implementer named. A test that covers both sides of each qualifier is enough.
- FMT-BLANK-011, the D5 exemption: `A();⏎/* a */ {` gets no blank line (already `KeepsCommentsGluedToBracesWithoutBlankLine`).
- No new requirement IDs need tests.

## Output questions

_None new._ One scope point for the lead: the maintainer's implementer-stage answer names "a closing brace". The same output, with no blank line inserted above the directive, also occurs before a collection expression's `]` (`1,⏎#pragma …⏎⏎#region R⏎⏎#endregion⏎]`, where `main` inserts a blank line above `#pragma`). I read the answer as covering it, because the rule and the mechanism are the same. Change-doc entry 11 names it in words, without an example, so as not to pin the region directive's column (the existing follow-up candidate). If the lead reads the answer as limited to braces, this member needs its own output question, and the change doc goes back to `Draft` until it is answered.

## Internal choices

- D3 is stated in FMT-BLANK-009 (the gap before a closing token with a non-region directive), not by widening FMT-BLANK-016. Alternative: make FMT-BLANK-016 forbid every inserted blank line in front of a directive. That would contradict FMT-BLANK-013, which inserts blank lines in front of region directives and below `#endregion` (e.g. `#endregion⏎#pragma`).
- `BlankLineSpacingPolicy` is not added to RH5025's code owners. `sdd/README.md` allows a type to own only one spec, and `BlankLineSpacingPolicy` is already a SHARED-BLANK owner. RH5025-005 and FMT-BLANK-017 each state the "block statement that follows a statement" definition. Alternative: a new SHARED-BLANK requirement for `IsBlockStatementAfterStatement`, used by both. It would give the planned direct Core test its own requirement, but it restructures two specs for a one-line definition.
- The complex-element qualifier is written as "whose closing brace stands on the same line" (as the phase sees the input), and the assigned-initializer qualifier as "assigned to a member inside an object initializer". Both are facts about the construct, not about the line-break phase.

## Follow-up candidates

- The D4 shapes join the existing follow-up about braces the line-break phase never normalized (`03-spec-author.md`). Whether a glued comment gets a blank line now depends on whether a complex element initializer spans lines, whether a collection initializer is assigned, and whether a documentation comment shares the line. That is today's output, kept on purpose, and RH5020 reports every member. It belongs in that one issue rather than a new one.
