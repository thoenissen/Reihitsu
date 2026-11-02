Role: spec author
Commit: 2fa9ca6
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decisions 6–11 and lists the RH5405/RH5406/RH5407 code-fix result as a visible output change covered by maintainer answer 1 (decision 9).

Files: `sdd/changes/2026-11/phase-order-line-breaks.md` (still `Status: Draft`), `sdd/specs/formatter/line-breaks.md`, `sdd/specs/formatter/blank-lines.md`, `sdd/specs/formatter/pipeline.md`, `sdd/specs/formatter/switch-case-braces.md`, `sdd/specs/requirement-ids.txt`. No rule page changes; `AGENTS.md` unchanged in this round.

## Changed requirements

- FMT-BLANK-021 — A blank line in front of a statement on its predecessor's line ends that line — **removed** (decision 6). It was issued in 1a072d5 on this branch only and never reached `main`, so the heading and its ledger line are deleted rather than retired; the PR's ledger diff against `main` removes no line. See *Internal choices*.
- FMT-BREAK-006 — One statement per line — changed: the empty-statement exemption stops where FMT-BLANK-001 to FMT-BLANK-005 ask for a blank line between the pair; then the second statement starts its own line in this phase and the blank-line phase adds only the blank line. The Why states the reason (the switch measure, FMT-SWITCH-003) and that the pairs are read from the one statement of the blank-line rule, not a copy. New second example (block, `if (a) { } ;`, switch section `case 1: ; return 1;`). `Uses` gains FMT-BLANK-001/002/003/005, FMT-SWITCH-003, SHARED-STMT-001 and loses FMT-BLANK-021.
- FMT-PIPE-025 — No phase decides a line break or a blank line from a later phase's — changed (decision 10, and the shared-predicate allowance of decision 6): the decided fact is whether two tokens with text stand on one line, and blank lines are named separately; two phases may share one statement of a condition read from the syntax; the phases after the blank-line phase change neither; the cleanup removals of FMT-CLEAN-005/007/008 are outside. The Why names the chain prediction (decision 8). `Uses` gains FMT-CLEAN-007, FMT-CLEAN-008.
- FMT-BLANK-018 — A comment glued to a token belongs to the token's line — changed (decision 8): the chain-link case is no longer a prediction; the line break is the line-break phase's and stands when the blank-line phase runs, so the comment is on its own line by the ordinary rule, and the phase must not predict it. Examples unchanged.
- FMT-BLANK non-goal "Line breaks that are not blank lines" — changed: the phase writes no line break that ends the line of a statement or a switch section (decision 7). `Uses` loses FMT-PIPE-004 and SHARED-STMT-001 (only FMT-BLANK-021 used them).
- FMT-SWITCH-015 — The inserted braces are placed as the block of a section — changed (decision 11): the mechanism sentence ("apply the line-break phase's placement … not a placement of its own") removed; the requirement states the line positions only. The Why still gives the reason for one placement.
- Change doc: FMT-BLANK-021 replaced by FMT-BLANK-018 in *Affected requirements*; *What* bullets for FMT-PIPE-025, the blank-line phase, and the switch braces rewritten; visible output change 5 added (RH5405/RH5406/RH5407 code-fix result, "before" reproduced with `scripts/apply-fix.sh RH5405` on 2fa9ca6, which carries no code change from `main`); the visible-output intro names every surface; maintainer answer 1 points at change 5 too; *Decisions* sections "A statement next to an empty statement is still split by the blank-line phase" replaced by "The line-break phase splits an empty statement from a neighbour that a statement rule separates" (the Done-when holds unnarrowed, decision 7), "The blank-line phase no longer predicts chain line breaks" added, the switch-brace section restated as line positions; *Alternatives* updated (the FMT-BLANK-021 variant, an unbraced section, a switch-phase prediction, and a private copy of the statement list are the rejected ones).

## Examples for the tests

- FMT-BREAK-006 — the new second example (block, `if (a) { } ;`, switch section), in one run; verified on 2fa9ca6 with `scripts/format.sh`: identical output, stable on a second run. Over two runs (FC-017) also `case 1: ; throw null;` and `case 1:` / `; throw null;` from 02-skeptic, and the holding rows (another multi-line section in the switch; terminal `break` behind `;`; `; Foo();`, which stays as written).
- FMT-BLANK-018 — its existing chain-link example ("A comment in front of a chain link that the formatter moves onto a line of its own …") guards the removal of the prediction.
- FMT-SWITCH-015 — its example; line positions only.
- Change doc visible output change 5 — an RH5405 code-fix test (`if (a) // c` + unbraced body); RH5406/RH5407 for an `if` they brace if the test implementer finds an existing test shape for them. The existing `else if (y) // note` tests in `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs` must stay unchanged.
- Unchanged from 01-spec-author: FMT-BREAK-036, FMT-PIPE-025's switch example, FMT-PIPE-026, FMT-SWITCH-016, FMT-PIPE-001's runtime order, and the unchanged-output seeds listed there. FMT-BLANK-021's tests are no longer wanted; its example lives on in FMT-BREAK-006.

`SpecStructureTests` and `SpecConsistencyTests` pass. `RequirementCoverageTests` fails as expected until the tests exist: the retired FMT-BREAK-005 tags in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ElseIfConditionCommentTests.cs` (lines 22, 48) and the uncovered added IDs.

## Output questions

_None._ Every output in this round is `main`'s, except visible output change 5, which is maintainer answer 1 on the code-fix surface (decision 9).

## Internal choices

- FMT-BLANK-021 is deleted, heading and ledger line, instead of retired: it was issued on this branch only, no test or code names it, and a retired heading would carry an ID that never shipped. Alternative: keep it with `Status: Retired` — `sdd/README.md` says "never remove … a line", read literally for the branch. Recommend deletion; if the lead reads the rule as covering branch-only IDs, retire it instead (a two-line change).
- FMT-BREAK-006's split covers both directions — a statement behind an empty statement (`; return a;`) and an empty statement behind a statement (`if (a) { } ;`, FMT-BLANK-005). Decision 6 says "from the empty statement before it"; covering only that direction would leave `if (a) { } ;` to a line break written by the blank-line phase, against decision 7. Alternative: the narrow reading, which reintroduces a blank-line-phase line break.
- The requirement list in FMT-BREAK-006 is spelled out (FMT-BLANK-001, …, FMT-BLANK-005) instead of a range, because the `Uses` check reads single IDs only.
- The shared predicate is stated in Whys (FMT-BREAK-006, FMT-PIPE-025) and FMT-PIPE-025's allowance, not as a normative mechanism, consistent with decision 11. Where it lives in code is the implementer's: if it becomes a new type in `Reihitsu.Core` rather than a member of `BlankLineSpacingPolicy`, `sdd/specs/README.md` and a spec's code owners need it (FMT-BLANK owns the statement-kind conditions; `BlankLineSpacingPolicy` is owned by `shared/blank-line-spacing.md`).
- `AGENTS.md` still lists `BracePlacer` among the `SwitchCaseBracePhase` guards (added in 1a072d5). Left in place, since decision 11 moved only the spec wording; whoever implements FMT-SWITCH-015 without `BracePlacer` must remove it there.

## Follow-up candidates

_None._
