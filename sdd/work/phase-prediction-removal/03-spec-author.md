Role: spec author
Commit: e2920ed
Verdict: change doc written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It told me to apply the maintainer's answer to escalations 1, 12 and 5: option B, fix D2 in this run, structure first. This round follows that answer, as recorded in `00-lead.md` and `squad/decisions/2026-10-25-phase-prediction-removal-layout-model-column.md`. Every output named below was observed on e2920ed with `scripts/format.sh` on fixtures in the scratchpad, outside the repository. In inputs, `→` stands for a tab and `/` separates lines. Fixtures sit in `class C { void M() { ... } }`, and columns are 0-based.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` is still `Status: Draft`. The maintainer has to approve it again (escalation 2), because it no longer says "output stays as it is". Changes in this round:

- **What.** A third bullet: the layout now counts a comment that stands in front of a line's first token (and the tail of a multi-line comment), so the column it computes is the column the phase writes. The raw-string bullet now says the move uses the layout's own shift, decided once. The "output stays" paragraph is replaced by a table of every changed output found:
  - the skeptic's six stable-but-wrong outputs: g02, g04, g07, g01, g06, g08;
  - a seventh one found in this round, see below;
  - the two outputs that are not idempotent today: t06, t07.
- **Why.** "Every mismatch is non-idempotent output" now also covers a column that is wrong on every pass. A paragraph states the third disagreement, D2, and why the fold forces fixing it.
- **Inventory.** New row: `ReihitsuFormatterHelpers.ComputeTokenColumn` and `AdjustNodeIndentation` (the skeptic's follow-ups 1 and 2), marked "Not changed here".
- **Decisions:**
  - Under "The indentation phase owns the final column": the maintainer's question from escalation 2 about the phase's size. The assessment is the skeptic's: about 7 % growth and no new dependency. The real risk was losing the independent measurement, and the next decision removes it.
  - New: "The layout computes the column the phase writes, and the raw string moves by it once".
  - "The output stays as it is" becomes "Changed output is only output that violates an existing requirement today".
- **Alternatives.** Three added: Q1 C (tabs only, keep `RawStringAlignmentPhase`), Q1 A (measure again in the indented text as the last step), and moving by the layout's shift without fixing the layout. That last one breaks g03: content and closing marker at 16 under quotes at 24.

**Seventh changed output, found in this round (g05b).** Input: `        /* a` / `        b */ Call(1,` / `2);`.
- Today: `2` in column 13, while `1` is in 18. The output is stable.
- After the change: `2` in column 18.
- It belongs to D2: the line is led by the tail of a multi-line comment. FMT-INDENT-012 and FMT-INDENT-030 already rule today's output out, so this is not an open question.
- thoenissen/Reihitsu#1382, which is open, will later make the formatter break before `Call`. That changes this output again.
- I list it explicitly so the maintainer sees it in the re-approval.

## Changed requirements

All requirement changes in this round are wording changes. No new IDs, so the ledger is unchanged.

- FMT-INDENT-030 — Anchors are measured at their final column — changed:
  - A new paragraph says an anchor's column counts everything in front of it on its line. A comment in front of the line's first token, on that token's line, takes its FMT-INDENT-006 column, and the token keeps its gap behind it (FMT-INDENT-005). The same holds behind the last line of a multi-line comment (FMT-INDENT-007).
  - The other side of the qualifier (FC-002) is stated too: a `//` comment, or a delimited comment followed by a line break, is not in front of the token on its line.
  - New examples: g02 (`2` to 21) with its negative side (`// c` on the line above, `2` at 13), g01 (`.ToUpper` to 27), and a one-line chain case (`.C()` to 17).
- FMT-RAW-005 — Alignment follows the final position of the opening quotes — changed (S3). The target column is the column the opening quotes have in the output of the indentation phase, counting everything in front of them on their line, including a leading comment (FMT-INDENT-030). New example: g03, with the literal under the quotes at 24. That output is unchanged today.
- FMT-PIPE-016 — No phase after the first sees or writes a tab in whitespace — changed (S1):
  - It now covers only the whitespace of the code and between directive tokens.
  - Whitespace inside a documentation comment, a comment, a literal, or disabled text, and a directive's free text, keep their tabs and are outside the requirement.
  - A tab in front of a continuation line's `///` reaches the indentation phase, which rewrites that whitespace as a whole (FMT-INDENT-008).
- FMT-TAB-001 — Tabs in whitespace become four spaces each — changed (S1). It defines "the whitespace of the code": whitespace trivia of tokens outside every documentation comment and directive. That is exactly the scope of today's `CleanupPhase.NormalizeWhitespaceTabs`, whose `DescendantTokens()` call does not descend into structured trivia. Whitespace inside a documentation comment is excluded (FMT-TAB-003).
- FMT-TAB-002 — Tabs inside directives become four spaces each — changed (S2):
  - "Four spaces" now holds for this phase's output.
  - Through the pipeline, `#region→Bar` with a bare `#endregion` keeps four spaces (`#region    Bar` / `#endregion // Bar`).
  - `#region→foo` is rewritten by FMT-REGION-001 to `#region Foo`, with one space.
  - `#region→Baz→qux` keeps the inner tab in both directives.
  - All of this was observed on e2920ed.
- FMT-TAB-003 — Tabs in literals, comments, and disabled text are kept — changed (S1). Every tab in a documentation comment is kept, for example `<param→name="x">` (observed kept on e2920ed). The whitespace in front of a continuation line's `///` is not replaced by this phase; the indentation phase rewrites it with spaces (FMT-INDENT-008). New documentation-comment example with both sides.

Header `Uses` rows updated:
- `pipeline.md`: + FMT-INDENT-008, FMT-TAB-003
- `indentation.md`: + FMT-RAW-005
- `tab-replacement.md`: + FMT-REGION-001, FMT-INDENT-008

State of `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd|FullyQualifiedName~PhaseOwnership"` on the working tree: 48 passed, 4 failed. These are the same four pending failures `01-spec-author.md` lists (`TabReplacementPhase` missing, runtime phase order, tests still tagged FMT-CLEAN-002/003, no tests for FMT-PIPE-015/016 and FMT-TAB). `UsesListsExactlyTheReferencedRequirements` passes.

No rule page changes. RH5601 and RH5202 report and fix the same cases as before, and the D2 change is formatter layout only.

## Examples for the tests

- **FMT-INDENT-030**, the new examples:
  - g02 (`2` at 21) and its `// c` side (`2` at 13).
  - g01 (`.ToUpper` at 27).
  - The chain sentence: `/* c */ a.B()` / `.C();` gives `.C()` at 17.
  - The other rows of the change doc's table, as listed there: g04 (22), g06 (28), g08 (32, with the FMT-BLANK-011 blank line above the comment), and g05b (18).
  - Two comments in front of the token, `/* c */ /* d */ Call(1,` / `2);`, are the same member. Expected: `2` at 29. Today it is 13.
- **FMT-RAW-005**: the new g03 example (literal at 24). This is the case that catches a move taken from an uncorrected layout column. Also g08's literal at 29.
- **FMT-TAB-002**: `#region→Bar` / `#endregion` gives `#region    Bar` / `#endregion // Bar`, and `#region→foo` gives `#region Foo`, through the full pipeline.
- **FMT-TAB-003**: the documentation-comment example (`<param→name="x">` kept), and the existing continuation-line test `IndentationTests.DocumentationCommentContinuationLineWithTabIsRealigned`.
- **FMT-PIPE-016**: the observer test scans only the whitespace of the code and of directive tokens (S1).
- **t06 and t07 (FMT-PIPE-004, FMT-TAB-001)**: 29 and 37 in one pass.
- Everything `01-spec-author.md` lists under "Examples for the tests" still stands.

## Open questions for the maintainer

_None new._ The skeptic's follow-up candidates 1–4 (`02-skeptic.md`) stand for the discussion once the PR is ready. Follow-ups 1 and 2 are now recorded in the change doc's inventory as "Not changed here". The maintainer's requested follow-up issue reviewing `IndentationPhase` is also for that discussion; it is not recorded in any spec or the change doc.
