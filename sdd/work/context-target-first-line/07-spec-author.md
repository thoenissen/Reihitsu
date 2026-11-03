Role: spec author
Commit: 3332bca
Verdict: spec written

Wording round for decision 8 in `00-lead.md`. The prompt carried the change to make (decision 8) and no suspected cause or hoped-for verdict. I checked the wording against the production diff of f2aa0eb:

- `TargetAnnotation` and `TargetFirstTokenColumn` on `FormattingContext` are read only by `LayoutComputer.UseTargetPosition`.
- No other phase reads them.

## What changed

- **FMT-PIPE-017.** "The phases take that column … from the target-position fact" now says "The indentation phase takes that column, and the target's first token in the root, from the target-position fact". The result sentences and every example are unchanged.
- **FMT-PIPE-018.** The paragraph "The phases MUST keep that trivia …" is replaced by a result:
  - Writing the trivia back MUST NOT move a line of the target that the indentation phase lays out. The written-back trivia puts the first token in its document column, and the indentation phase lays the other lines out from that column, which the fact gives it.
  - Whatever the phases do in the root with the trivia and the code in front of the target decides no line or column of the result: blank lines, comments, directives, a line break added or removed, spacing, or a moved line.
  - The write-back also restores the tabs and line endings the preparation phase rewrote.
  - The *Why* is restated to match: no phase needs to leave the trivia alone. The example is unchanged.
- **FMT-PIPE-024.**
  - The line-break bullet is removed. It said the line-break phase neither breaks nor joins in front of the target, and that the documentation, line-break and blank-line phases keep the trivia.
  - The indentation bullet no longer says that phase "leaves the lines of that target's leading trivia in their columns", nor that "no phase other than the horizontal-spacing phase decides the spaces in front of that first token".
  - It now says the indentation phase alone reads the target position. Where the earlier phases leave the first token in the root, and what they write in front of it or in its trivia, decides no column of the result.
  - The *Why* says the context entry point "has the indentation phase lay out the target's other lines from the column that trivia leaves its first token in".
  - The fact table row is unchanged. It already lists exactly the two values the code carries: the document column, and the annotation on the first token.
- **Change doc** `sdd/changes/2026-11/context-target-first-line.md`:
  - *What* drops the three per-phase bullets. It now says the fact is read by the indentation phase alone, that nothing the earlier phases do in front of the target or in its trivia reaches the result, and that no other phase changes for the target.
  - The decision *The write-back of the leading trivia stays, and changes no column* is restated the same way, and says why the earlier phases are not guarded.
  - One alternative is added and rejected: guarding the documentation, line-break and blank-line phases.
  - Status stays `Accepted`. Every visible output change (1–18) and every example is unchanged.
- **`AGENTS.md`.** The architecture sentence now reads: the write-back keeps the target's first token in its document column, and the indentation phase lays out the other lines from that column, which the entry point passes as a fact, so the write-back moves no line the indentation phase laid out.
- **Checked and left as they are:**
  - SHARED-FIX-007 and RH5111-004 already state results only.
  - FMT-PIPE-010 and FMT-PIPE-022 do not state the mechanism.
  - No other spec under `sdd/specs/` names the target position.
  - The source comments in f2aa0eb (`FormattingContext`, `ReihitsuFormatter`, `LayoutComputer`, `LayoutModel`) already describe the indentation-phase-only mechanism.

## Changed requirements

- FMT-PIPE-017 — Node-level targets take their columns from the indentation phase — clarified (which phase reads the fact)
- FMT-PIPE-018 — The context entry point writes back its target's leading trivia as in the document — changed (the phase guard is restated as a result)
- FMT-PIPE-024 — Node-level input facts are plain values — changed (the guard bullet is removed, and the indentation bullet is restated)

## Examples for the tests

Unchanged: the FMT-PIPE-017 and FMT-PIPE-018 examples, and changes 1–18 in the change doc. No test needs to change.

## Output questions

_None._ No visible output changes.

## Internal choices

- I removed the FMT-PIPE-024 line-break bullet instead of rewording it as a result. The alternative was to keep a bullet that says the line-break phase reads nothing for the target. The result it would state is already in the indentation bullet and in FMT-PIPE-017.
- The `LayoutModel.UseTargetFirstToken` summary says the first token "stands in the given column in the output of the indentation phase". In the root, the indentation phase may place a line-starting target's first token by depth, and the write-back puts it in that column afterwards. This is a comment-precision note for the cleaner, not a spec matter, and I did not touch it (code lane).

## Follow-up candidates

_None._
