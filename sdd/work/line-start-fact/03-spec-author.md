Role: spec author
Commit: 235f67c
Verdict: spec written

Repair round on the skeptic's handover `02-skeptic.md` (9a51602) and the maintainer's answer to the architecture finding (option B, `squad/decisions/2026-10-30-line-start-fact-architecture-finding.md`). The change doc stays `Accepted`. Every edit below is a clarification made in place, and none changes What or Why: each follows from the approved rule (a token starts its line after the same line breaks as trivia; a line behind a stranded `///` gets its `//` counterpart's column) or from an existing requirement. The approval of What and Why is recorded in `squad/decisions/2026-10-30-line-start-fact-change-doc.md`, see `01-spec-author.md`.

## Items re-derived

1. **SHARED-BLANK-008 contradicted itself at the start of a file.** Confirmed. The first sentence makes a token behind a `///` comment or a directive start its line. The last sentence, read alone, would make the first token of a file behind such a comment not start its line. Today `/// <summary>x</summary>` / `    public class C` puts `public` in column 0 (trace: `IndentationPhase`), and RH5204 reports it (per the skeptic). I reworded the last sentence as the fallback SHARED-BLANK-002 already uses: "Without a preceding token … and without a line break in front of it, …; a `///` comment or a directive in front of it ends its line like anywhere else". I also added the file-start example with RH5204 and the formatter output.
2. **End-of-file token.** It is a token without text, so under SHARED-BLANK-008 it never starts a line. Trivia in front of it is judged by SHARED-BLANK-002, which is unchanged, so the trivia keeps being placed. Today `class C { }` / `    // end` puts `// end` in column 0 (trace: `IndentationPhase`). SHARED-BLANK-008 now states this, with an example (`int _a;` keeps the type from becoming `class C;`). The change doc's *What* bullet names the end of the file.
3. **RH5048.** The change doc now states its consequence: RH5048-001 reports an ordinary comment in front of a first link that starts its line behind a `///` comment, as it does behind `//`. Example: `items /* c */ /// x` / `.Where(…)` reports `/* c */`; today nothing is reported. Whether the `///` itself is reported is not part of this change; see *Follow-up candidates*.
4. **Newly in-scope inputs.**
   - The specs already cover the stranded `///` before `?.`, `?`, `:`, and do-while `while`: FMT-DOC-003 forbids the move ("MUST NOT move … such as … the `.` of a member access", a list that opens with "such as"), and FMT-INDENT-002 with SHARED-BLANK-008 places the line. I did not change any spec for them. The change doc's *Outputs that change* now names them explicitly, together with `,`, `is`, `=>`, `=`, and the directive or own-line comment between the `///` and the line.
   - Omitted array size: named under *Outputs that change*. Its target is the block column, as the text-bearing control `int[2,` / `3] a` gets today.
   - Disabled text and other terminators: FMT-INDENT-006 (with FMT-INDENT-011 for the disabled text itself) already covers them. The change doc's FMT-INDENT-006 bug bullet now names U+2029 and U+0085 too, and an own-line comment in the same leading trivia as a multi-line comment or disabled text that holds such a terminator.

## Changed requirements

- SHARED-BLANK-008 — A token starts its line after the same line breaks as trivia — changed in this run before any test: the start-of-file sentence is repaired, the end-of-file sentence is added, and there are two new examples (end-of-file comment, file-start token behind `///`).
- Change doc `sdd/changes/2026-10/line-start-fact.md` — clarified in place: *What* (end of the file), *Analyzer diagnostics* (RH5048 bullet), *Outputs that change* (`?.`, `?`, `:`, `while`, `,`, `is`, `=>`, `=`, directive or own-line comment in between, omitted array size), *Bugs fixed* (FMT-INDENT-006 terminators and carriers).
- No other spec changed. The `Uses` rows and the ledger are unchanged.

Architecture check (`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`): 49 passed and 2 failed, the same two as in `01-spec-author.md` (the retired FMT-SPACE-015 tags, and no test for SHARED-BLANK-008 yet).

## Examples for the tests

- SHARED-BLANK-008: all examples, including the two new ones (end-of-file `// end` to column 0; `public` behind a file-start `///` reported by RH5204 and moved to column 0).
- Change doc *Outputs that change*, *Analyzer diagnostics* (RH5048 `items /* c */ /// x`), and *Bugs fixed*. The skeptic's M1, M2, and M3 matrices in `02-skeptic.md` list the cells.
- Unchanged from `01-spec-author.md`: FMT-INDENT-002, FMT-DOC-003, FMT-SPACE-001, FMT-INDENT-015, FMT-INDENT-006.

## Open questions for the maintainer

_None._

## Follow-up candidates (outside this run's scope)

- **RH5048 excludes every documentation-looking comment, against SHARED-COMMENT-001.** `CommentPositionUtilities.IsOrdinaryComment` rejects `SingleLineDocumentationCommentTrivia`, `MultiLineDocumentationCommentTrivia`, and `//`/`/* */` text that starts with `///` or `/**`. Its doc comment states that as deliberate ("excluded in every documentation mode"). SHARED-COMMENT-001, which RH5048 lists under `Uses`, counts a `///` or `/** */` comment that documents nothing as an ordinary comment. So by RH5048-001, a stranded `/** x */` before a first link that starts its line should already be reported today, and after this change a stranded `/// x` before one should be too. This is derived from reading code only; RH5048 has no fix, so `apply-fix.sh` cannot run it. Since the `/** */` case exists independently of this run, I recommend a follow-up issue rather than a fix here. Meanwhile, this run's RH5048 tests should not assert that the stranded `///` itself is unreported.
- The skeptic's follow-up candidates stand as listed in `02-skeptic.md`: the RH5204 fix does not converge at the start of a file; RH5204 skips predefined-type fields; the documentation-comment phase relocates comments that FMT-DOC-001 leaves undecided.
