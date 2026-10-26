Role: spec author
Commit: 714c199
Verdict: spec written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to apply the audit-3 decision, option B (`00-lead.md`, last section; `squad/decisions/2026-10-26-phase-prediction-removal-audit-3.md`):
- narrow FMT-PIPE-017 to a recorded gap for a context target that starts a later line of its context;
- state the reverted behavior in FMT-PIPE-018, in `AGENTS.md`, and in the change doc;
- fix the change doc's count of new pipeline requirements.

This round does all three.

## The reverted behavior

I compared `FormatNodeInDocumentWithContextAsync` on `origin/main`, at 504310f, and at HEAD (714c199).

- **Both `main` and 504310f** write back the target's original leading trivia in every case: `formattedFirstToken.WithLeadingTrivia(originalFirstToken.LeadingTrivia)`. That covers blank lines, comments, directives at their columns, and the whitespace in front of the target's first token.
- **The difference between the two** is the column step:
  - `main` measures the context's first token with `ComputeTokenColumn` and moves the lines afterwards with `AdjustNodeIndentation`;
  - 504310f hands the phases the context's first-token column, counting every character as one column (`countsOwnWhitespaceTabsAsReplaced: false`), and moves nothing afterwards.
- **HEAD** (the #8 repair) writes back the phases' trivia with the document's blank lines (`RestoreBlankLines`) when FMT-PIPE-010's condition does not keep the trivia. The decision reverts that, and the revert goes back to 504310f.

I observed 504310f with `scripts/apply-fix.sh RH5111` in a temporary worktree, since removed, under LF and CRLF:
- an enum member behind two blank lines (`  B =` / `  2`) gives `  B = 2`, with both blank lines kept and `B` at 2;
- with `      // b` above it: the comment stays at 6, `B` at 2, and no blank line is added;
- the declarator (`28-auditor.md` #8a) gives `              b = Other(1,` / `                  2);`: `b` at 14, `2` at 18, `1` at 24.

The auditor reported the same declarator output for the base, so this output is not a regression against `main`.

## Changed requirements

- **FMT-PIPE-010 — clarified.**
  - `FormatNodeInDocumentAsync` keeps the target's leading trivia under the stated condition: the target does not start its line, or is a direct child of the root.
  - `FormatNodeInDocumentWithContextAsync` keeps it in every case (FMT-PIPE-018). Both keep the trailing trivia.
  - This writes down `main`'s behavior. The old wording applied the condition to both entry points.
- **FMT-PIPE-017 — narrowed.**
  - The normative sentence now names its exceptions: the leading trivia the entry points keep (FMT-PIPE-010, FMT-PIPE-018) and the recorded gap.
  - The four-column tab exception is limited to a target of `FormatNodeInDocumentAsync` that starts its line. For the context entry point, every character in front of the context's first token counts as one column.
  - **New paragraph, "Recorded gap":** a target of `FormatNodeInDocumentWithContextAsync` that starts a later line of its context keeps its first line's document column. Its other lines take the phases' columns. A line aligned to an anchor on the first line stands where that anchor stood in the phases' layout. The requirement does not cover the first line or the lines that depend on it.
  - The other side of the qualifier: a mid-line target, or one that starts the context's first line, is not in the gap.
  - Example: the observed declarator output.
- **FMT-PIPE-018 — changed.** New title: "The context entry point writes back its target's leading trivia as in the document".
  - The entry point writes back the trivia as it stands, in every case: blank lines, comments and directives at their columns, and the whitespace in front of the first token.
  - Neither FMT-BLANK-011, FMT-BLANK-013, FMT-BLANK-014, nor the indentation phase changes it.
  - The Why gives three reasons: SHARED-FIX-006; that the alternative needs a restructuring of the entry point; and the cost, which is the gap in FMT-PIPE-017.
  - The two examples are now the observed RH5111 outputs: two blank lines kept with `B = 2` at 2, and the comment at 6 with `B = 2` at 2.
- **`AGENTS.md`.** The sentence about steps after the last phase no longer held: the reverted write-back changes the target's first-line column after the last phase. It now adds: "The one recorded exception is `FormatNodeInDocumentWithContextAsync`, which writes back its target's leading trivia as in the document, so a target that starts a later line of its context keeps that line's column (FMT-PIPE-017, FMT-PIPE-018)."

No new IDs. `scripts/test.sh --configuration Debug --project architecture` on the working tree gives `Passed: 89, Failed: 0`.

## Change doc

`sdd/changes/2026-10/phase-prediction-removal.md` stays **`Status: Accepted`**: the audit-3 decision covers this narrowing. What changed:
- **Affected requirements:** + FMT-PIPE-010.
- **What, node-level bullet:** the sentence about the context entry point's blank lines now says that it writes back its target's leading trivia as in the document, as today, and that a target starting a later line keeps that line's column, a gap FMT-PIPE-017 records.
- **Pipeline paragraph:** "Three new pipeline requirements" becomes "Four", adding FMT-PIPE-018. FMT-PIPE-017 is described with "apart from a recorded gap".
- **Inventory row** `ComputeTokenColumn` / `AdjustNodeIndentation`: it adds that the context entry point's write-back stays, with the recorded gap.
- **Decision "The node-level entry points lay out their node at its own column":** a new paragraph covers the reverted write-back, the gap, why the phases' trivia with restored blank lines was not kept (the `///` behind a token of the context, decided in more than one place), and that restructuring the entry point is left to its own change.
- **Alternatives:** new rejected alternative, "Write back the phases' leading trivia for a context target that starts its line, with the document's blank lines put back" (the #8 repair, rejected after the third audit).

## Examples for the tests

Tests of the #8 repair are to be replaced by tests of the reverted behavior (the decision). Seeds:
- **FMT-PIPE-018:** the two RH5111 examples.
  - Through the public API, the same members leave the leading trivia unchanged. Two blank lines stay, and a comment and the target keep their input columns.
  - The `ReihitsuFormatterNodeColumnTests` and RH5111 tests from `32-test-implementer.md` that assert the phases' columns for a context target's first line, or the restored blank lines, change to these outputs:
    - `B` stays at 2;
    - `// b` stays at 6;
    - `int second` stays at its input column;
    - the declarator `second` stays at 14.
- **FMT-PIPE-017 gap:** the declarator example (`b` at 14, `2` at 18).
  - The #8(b) public-API member (`Call(1,` / `Other(2,` / `3));` with the argument as target) gives `Other` at 17 and `3` at 19 (`28-auditor.md`).
  - Assert these as the recorded gap. They must not be tagged as meeting FMT-PIPE-017.
- **FMT-PIPE-010:** a context target whose leading trivia holds a `///` written behind the token in front of it (#11's members) is written back unchanged. The four inputs are in `37-auditor.md` #11. With the original trivia written back, each line stays as written, as on the base.

## Open questions for the maintainer

_None new._ The restructuring of the context entry point is a follow-up candidate, which the decision names. So is Q1 of `33-spec-author.md`, the blank lines of `FormatNodeInDocumentAsync`.
