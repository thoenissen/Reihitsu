Role: spec author
Commit: 1ecd1d1
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked for two things:
- specify how `FormatNodeInDocumentWithContextAsync` treats the blank lines in front of a target that starts its line (the spec gap in `32-test-implementer.md`);
- check the `AGENTS.md` sentence about steps after the last phase.

Inputs: `28-auditor.md` #8, the audit-2 decision at the end of `00-lead.md`, `30-` and `31-implementer.md`, and `32-test-implementer.md`.

## Changed requirements

- **FMT-PIPE-018 — The context entry point keeps the document's blank lines in front of its target — added.**
  - **Scope:** a target of `FormatNodeInDocumentWithContextAsync` that starts its line, whose leading trivia FMT-PIPE-010 does not keep. That trivia is written back with the document's blank lines:
    - every blank line above the target, and between the comments and directives of its leading trivia, stays;
    - none is added. This holds also where FMT-BLANK-011 or FMT-BLANK-013 would add one, or where FMT-BLANK-014 would collapse a run of blank lines.
  - The comments, the directives, and the target's first token take the indentation phase's columns (FMT-PIPE-017).
  - A line break that is not a blank line follows the phases. This covers the break that ends a comment's or a directive's line, and a break the phases add or remove behind a delimited comment on the target's line.
  - **Other side of the qualifier (FC-002):** a target that does not start its line, or whose trivia FMT-PIPE-010 keeps, has its trivia written back as it stands. The blank lines of `FormatNodeInDocumentAsync` are left undecided (see the open question).
  - **Why:** SHARED-FIX-006 forbids a fix to change the blank lines between its code and its neighbours. Columns are not a blank-line decision, so they stay with the indentation phase.
  - **Examples**, taken from the passing tests of `32-test-implementer.md`:
    - two blank lines above an enum member `B`: both stay (document-level formatting keeps one), and `B` moves from 2 to 4;
    - `// b` at 6 above `B` at 2: no blank line is added (document-level formatting adds one), and both end at 4.
  - Ledger updated. `pipeline.md` `Uses` + FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014.
  - This writes down behavior the run keeps. Before the run, the entry point restored the target's original leading trivia, blank lines included. After 8cb740c it still writes back the document's blank lines; only the columns are the phases' now.
- **FMT-PIPE-017 — wording.**
  - "MUST NOT move a line after the last phase" becomes "MUST NOT change the column of a line after the last phase". The old wording could be read as also forbidding the blank-line restore, which adds or removes whole blank lines but changes no column.
  - New sentence: which blank lines stand in front of a context target that starts its line is decided by FMT-PIPE-018, not by the phases.

## AGENTS.md

The sentence read "…no step after the last phase moves a line, also in the node-level entry points…". Taken literally, it no longer held: `RestoreBlankLines` runs after the last phase and adds or removes blank lines in the target's leading trivia, which shifts the lines below up or down. It changes no column.

It now reads "…and no step after the last phase changes a line's column, also in the node-level entry points: the indentation phase owns every line's final column." That matches FMT-PIPE-017 as reworded, and it holds for the code at 8cb740c:
- the context entry point's remaining trivia restore is under FMT-PIPE-010's condition only;
- `RestoreBlankLines` keeps the formatted whitespace behind each gap's last line break.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` stays **`Status: Accepted`**. These changes record behavior that the change keeps, and make the wording precise; they change no decision. If the lead reads the added sentence as a change to What, it needs approval again. What changed:
- Affected requirements: + FMT-PIPE-018.
- What, node-level bullet:
  - "move no line after the last phase" becomes "change no line's column after the last phase";
  - new sentence: "The blank lines in front of a target of `FormatNodeInDocumentWithContextAsync` stay the document's, as they are today (FMT-PIPE-018)."
- The same wording change ("change no line's column") in:
  - the pipeline paragraph;
  - the node-level decision;
  - the inventory row for `ComputeTokenColumn` / `AdjustNodeIndentation`.

## Examples for the tests

- **FMT-PIPE-018** is not covered yet: `EveryRequirementOfAnEnforcedSpecIsCovered` fails on it, and every other architecture test passes (88 of 89). The tests already exist: tag them FMT-PIPE-018, in addition to FMT-PIPE-017.
  - The eight `ReihitsuFormatterNodeColumnTests` from `32-test-implementer.md`, which start with `EnumMemberTargetBehind…` or `ParameterTargetBehind…`.
  - The RH5111 tests that assert the document's blank lines:
    - the eight from `32-test-implementer.md`;
    - `VerifyCommentInFrontOfEnumMemberDoesNotWithholdTheFix` and `VerifyCommentInFrontOfParameterDoesNotWithholdTheFix`.
- **Untested members named in `31-implementer.md`.** A context target behind a directive or a `///` comment is covered by `32-test-implementer.md`. A target behind a blank line that the phases would remove is covered by the run-of-two case. No further seed is needed.

## Open questions for the maintainer

**Q1 — Blank lines in front of a target of `FormatNodeInDocumentAsync` that starts its line (found in passing, not blocking).**
- **Behavior:** this entry point decides those blank lines with the formatter's blank-line policy, applied to the surroundings as they stand (`RootListPosition`, `RootPrecedingToken`; see its XML remarks). `FormatNodeInDocumentWithContextAsync` now keeps the document's blank lines (FMT-PIPE-018).
- **What the spec says:** no requirement states the first entry point's behavior. FMT-PIPE-010 says only when the leading trivia is kept, and `blank-lines.md` lists node-level preservation as a non-goal.
- **Why it looks wrong:** a fix that formats a target starting its line through `FormatNodeInDocumentAsync` gets the formatter's blank-line decisions. For example, it could add the blank line FMT-BLANK-011 wants above a leading comment, which SHARED-FIX-006 forbids. I did not run a fix to confirm it. Requirement IDs: FMT-PIPE-010, FMT-PIPE-018, SHARED-FIX-006, FMT-BLANK-011.
- **Readings:**
  - **A**: the first entry point also keeps the document's blank lines in front of a target that starts its line, as FMT-PIPE-018 does for the context entry point. One rule for both entry points. It may change output for fixes that format through `FormatNodeInDocumentAsync`.
  - **B**: keep today's behavior and specify it as the first entry point's own rule. The two entry points then differ.
- **Recommendation:** leave it out of this run and propose it as a follow-up for A, with a probe of the fixes that reach it first. This run did not change that entry point's blank lines, and the change doc does not cover it. A is the reading SHARED-FIX-006 supports.
