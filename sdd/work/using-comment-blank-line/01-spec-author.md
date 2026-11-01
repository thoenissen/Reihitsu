Role: spec author
Commit: db61ed3
Verdict: change doc written

Change doc: `sdd/changes/2026-11/using-comment-blank-line.md` (`Status: Draft`). It lists five visible output changes (RH7207 diagnostic and RH7201–RH7207 code-fix results); the formatter's output changes for no input.

## Today's behavior, established on `db61ed3`

Fixtures under the scratchpad, run with `scripts/trace.sh --passes 2` (full formatter, per phase) and `scripts/apply-fix.sh RH7207` (analyzer, then code fix):

- `using System;⏎⏎// About Linq⏎using System.Linq;` and the same with `/** About Text */`: the using phase removes the blank line and the blank-line phase inserts it again; the formatter's output keeps it. RH7207 reports this output, and its fix removes the blank line.
- `using System;⏎// About Linq⏎using System.Linq;`: not reported by RH7207 (no-diagnostic); the formatter adds the blank line in pass 1 and the using phase removes it again in pass 2 (phase reversal, recorded today as `PhaseReversalCauses.BlankLineAboveCommentLedUsingDirective`).
- Reorders (inputs of visible output changes 2 to 5 and the "Unchanged" reorder case): the fix output has no blank line above the moved or kept comment in every case; the formatter's output has one.
- `using System;⏎⏎/* c */ using System.Linq;` (ordinary comment glued to the directive): the using phase removes the blank line and no phase re-inserts it; formatter output and fix output have none.
- `using System; /** first⏎   second */ using System.Linq;`: the documentation phase moves the comment onto its own line, the blank-line phase adds the blank line, the using phase removes it in pass 2 (the pinned case of `UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt`).

Issue statements checked against current `main`: the code-fix path through `UsingDirectiveCodeFixUtilities` and the blank-line phase's insertion hold as the issue describes; the two phase-reversal exceptions it names are the entries for `DelimitedDocumentationBlankLineConvergenceTests.SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass` and `UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentKeepsTheCommentWithIt` in `PhaseReversalExceptionList`. No statement was made moot by thoenissen/Reihitsu#1496.

## Changed requirements

- SHARED-USING-008 — A blank line between groups, none within a group — changed (points to SHARED-USING-011 for the exception)
- SHARED-USING-011 — A blank line may stand above a comment that leads a directive of the same group — added
- FMT-USING-002 — Exactly one blank line between groups — changed (keeps the FMT-USING-010 blank line; the sentence about the re-inserted blank line is gone)
- FMT-USING-010 — The blank line above a comment that leads a directive of the same group is kept — added
- FMT-USING non-goal on the blank line the blank-line phase inserts — changed to "where none stood"
- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — changed (the open sentence is replaced by the decided case; new example)
- RH7207-001 — A block that is not organized is reported — changed (new glued-comment example)
- RH7207-002 — An organized block is not reported — changed
- RH7201-004, RH7202-004, RH7203-004, RH7204-004, RH7205-004, RH7206-004, RH7207-004 — The fix organizes the whole block — changed (wording "none inside a group other than the one kept above a comment …", `Uses` gains FMT-USING-010; RH7207-004 gains an example)
- Ledger: FMT-USING-010 and SHARED-USING-011 inserted. `Uses` updated in `shared/using-directive-ordering.md` (FMT-BLANK-011, FMT-BLANK-018), `formatter/using-directives.md` (SHARED-USING-011, FMT-BLANK-011), `formatter/blank-lines.md` (SHARED-USING-008 replaced by SHARED-USING-011), RH7201–RH7207.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Requirement"`: 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-USING-010 and SHARED-USING-011 only, which have no covering test yet.

Rule page `documentation/rules/RH7207.md` is left unchanged: it never said that a blank line inside a group is reported, so nothing on it becomes wrong, and the accepted blank line is a narrowed report around comments, not a change of what is reported or how to fix it in the normal case.

## Examples for the tests

- SHARED-USING-011 (organized and not-organized blocks, both comment kinds, multi-line documentation comment on the directive's line, glued ordinary comment)
- FMT-USING-010 (in place, moved, several blank lines, header blank line, no blank line in source, glued comment) — these are phase-level outputs; run the using phase alone.
- FMT-BLANK-011 (new using example: full formatter output)
- RH7207-001 (glued comment still reported), RH7207-002 (blank line above comment not reported, also several), RH7207-004 (fix keeps the blank line)
- The issue's two inputs (`// About Linq`, `/** About Text */` with `class C { }` → `class C;`) are the change doc's visible output change 1 and need regression tests for the formatter (unchanged output) and for RH7207/RH5020 (nothing reported on it).

## Output questions

1. **RH7201–RH7207 code fixes · a comment-led directive moved into the middle of its group, no blank line above the comment in the source.** The issue asks the change doc to decide this case.

   Input (RH7207 on `System`):
   ```csharp
   using System;
   using Microsoft.CodeAnalysis;
   // About Linq
   using System.Linq;

   class C
   {
   }
   ```
   Before (`main`) and reading A — keep what the source had (recommended; spec written on it, FMT-USING-010):
   ```csharp
   using System;
   // About Linq
   using System.Linq;

   using Microsoft.CodeAnalysis;

   class C
   {
   }
   ```
   Reading B — the using phase inserts the blank line above every comment that leads a same-group directive:
   ```csharp
   using System;

   // About Linq
   using System.Linq;

   using Microsoft.CodeAnalysis;

   class C
   {
   }
   ```
   Requirements: FMT-USING-010, SHARED-USING-011. The formatter's output is B in both readings. Recommendation A: the using phase would otherwise decide the comment rule's blank line a second time (two owners, the class the parent issue removes), and under B every fix on a block would also add blank lines above comments it does not move; under A the fix leaves the comment exactly as RH5020 already reported it in the input.

2. **RH7207 and the formatter · a blank line above an ordinary comment glued to a directive of the same group.** The issue's "a blank line followed by a comment does not split a using group" admits it; its "the formatter's output does not change" excludes it.

   Input:
   ```csharp
   using System;

   /* c */ using System.Linq;

   class C
   {
   }
   ```
   Before (`main`) and reading A (recommended; spec written on it, SHARED-USING-011): RH7207 reported on `System`; the fix and the formatter output
   ```csharp
   using System;
   /* c */ using System.Linq;

   class C
   {
   }
   ```
   Reading B: not reported; the fix and the formatter keep the input unchanged (a formatter output change).

   Requirements: SHARED-USING-011, RH7207-001, FMT-USING-010. Recommendation A: the comment rule gives a glued comment no blank line (FMT-BLANK-018), so the blank line would be the using rules' own layout for a comment the blank-line rules deliberately leave to the author, and the formatter's output would change.

The visible output changes 1–5 of the change doc are to be asked as well (the change doc lists the chosen output for each; change 4 — one blank line where several stood — has the alternative "keep them all" under *Alternatives considered*).

## Internal choices

- The allowance is a new requirement SHARED-USING-011 rather than a sentence in SHARED-USING-008, because it has its own reason; SHARED-USING-008 points to it. Alternative: rewrite SHARED-USING-008 alone.
- The using-phase behavior is a new requirement FMT-USING-010 rather than a clause in FMT-USING-002, for the same reason. Alternative: extend FMT-USING-002.
- SHARED-USING-011 states the comment classes in shape terms (own line, or documentation comment) and cites FMT-BLANK-011 and FMT-BLANK-018 only in `Why:`, so the analyzer spec reads without the formatter's classification; the shared spec's `Uses` therefore lists two FMT-BLANK IDs. Alternative: define the allowance as "a comment that FMT-BLANK-011 separates".
- RH7201-004 to RH7206-004 are reworded in the same way as RH7207-004, since each restated "none inside a group". Alternative: leave them, which would contradict FMT-USING-010.
- RH7207 rule page left unchanged (see above). Alternative: add one sentence on comments; rejected as a corner-case exception the page format excludes.

## Follow-up candidates

_None._
