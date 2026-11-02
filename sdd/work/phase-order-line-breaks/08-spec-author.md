Role: spec author
Commit: 1a48cb2
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decisions 21, 22, and 23. No visible output changes; no code, test, rule page, or requirement ID ledger entry is touched.

Files: `AGENTS.md`, `sdd/specs/formatter/switch-case-braces.md`, `sdd/changes/2026-11/phase-order-line-breaks.md`.

## Changed requirements

- FMT-SWITCH-016 — Brace insertion writes no blank line and carries those that lead a comment (title changed from "… and removes none") — clarified (decision 22). The requirement now matches the code and `main`: the phase writes no blank line; the leading trivia of the first wrapped statement and of the terminal `break` is carried as written, blank lines included, when it holds a comment, and not carried when it holds none. Both sides of the qualifier have an example: the existing one (comments `// a` and `// b` with blank lines) and a new second example (blank lines without a comment behind `case 1:` and above `break;`, which the output drops). The second example's output was confirmed with `scripts/format.sh --dry-run` on 1a48cb2. The `Why` explains why neither comment-free blank line belongs in the output (FMT-BLANK-007 behind the opening brace; SHARED-BLANK-004 above the terminal `break`).
- FMT-SWITCH-015 — The inserted braces are placed as the block of a section — clarified in its `Why` only. It no longer implies that the phase applies FMT-BREAK-008's own code. The line positions are unchanged. The non-goal that points to FMT-SWITCH-015 now reads "which they get as well" instead of "which this phase applies to them".
- `AGENTS.md`, `SwitchCaseBracePhase` row (decision 21): `BracePlacer` is removed from the guards. The real ones are named: `BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak`, `IsLastLineEndedByFollowingToken`, and `BlockWrapTriviaUtilities.GetCarriedLeadingTrivia`/`GetCarriedTrailingTrivia`. The decision text reads "places the braces it inserts on the lines the line-break phase gives a section's block, writes no blank line, and carries across the new braces only the leading trivia that holds a comment".
- Change doc (decision 21): the section *The switch-brace phase places its braces on a section block's lines* drops the issue's `BracePlacer` goal and gives the reason, and says the structural change is left to a follow-up. The FMT-SWITCH-016 sentence in that section and the matching bullet under *What* now name the comment-free trivia the wrap drops. *Alternatives considered* gains "route the inserted braces through `BracePlacer` in this change — rejected for this change". `Status` stays `Draft`.
- `scripts/test.sh --project architecture` on the working tree reported `Passed: 92`.

## Examples for the tests

- FMT-SWITCH-016, second example (no comment in front of the first statement or the `break`, blank lines dropped). The test implementer should check whether an existing test already covers it before adding one.

## Output questions

_None._ The comment-free drop is `main`'s output (decision 22, confirmed on 1a48cb2). It is consistent with FMT-BLANK-007 and SHARED-BLANK-004, so it is not an open question.

## Internal choices

- **Decision 23 was not carried out as worded. The lead should confirm this.** `sdd/README.md` (code owners) and the architecture test it describes allow a type to be the code owner of at most one spec. `BlankLineSpacingPolicy` is already the code owner of `shared/blank-line-spacing.md` (SHARED-BLANK). It therefore cannot also be listed in the FMT-BLANK header or the `sdd/specs/README.md` FMT-BLANK row. I left both headers unchanged. `RequiresBlankLineBehindClosingBrace` implements SHARED-BLANK-003, which SHARED-BLANK already specifies. `RequiresBlankLineForStatementKind` and `RequiresBlankLineBetweenStatements` implement FMT-BLANK-001 to FMT-BLANK-005 inside a type that the shared spec owns. FMT-BLANK and FMT-BREAK name those IDs under `Uses`, and the architecture tests pass. Alternative: move the two statement-kind predicates into a Core type of their own and list that type as an FMT-BLANK code owner. That is a code change and not mine; I recommend against it in this run, since decision 14 put the predicates on the existing type.
- The `AGENTS.md` row lists the wrap's trivia helpers and the terminal-`break` predicate as guards, rather than only dropping `BracePlacer`. The alternative was dropping it with no replacement, rejected because the row would then name nothing that decides where the inserted braces and the carried trivia go.
- FMT-SWITCH-016's title still states the "writes no blank line" rule and adds the comment condition. The alternative was keeping "removes none" and adding an exception sentence, rejected because the title would then contradict the requirement.

## Follow-up candidates

- Route the braces `SwitchCaseBraceRewriter.AddBraces` inserts through `BracePlacer` (FMT-BREAK-008), so the switch-brace phase has no placement of its own. A section without a terminal `break` needs the token behind the section moved. Output must stay as FMT-SWITCH-015 states. This is the lead's decision 21 follow-up, restated here for the issue manager.
- Text for the cleaner, not a follow-up: `src/Reihitsu.Formatter.Test/Unit/SwitchCaseBraces/SwitchCaseBracePhaseLayoutTests.cs:12` says the phase "writes and removes no blank line". That no longer matches FMT-SWITCH-016's wording. It is a test file, so I did not change it.
