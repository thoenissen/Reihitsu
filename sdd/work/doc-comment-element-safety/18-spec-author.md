Role: spec author
Commit: 2069be0
Verdict: F1: spec clarified (FMT-CLEAN-009)

Repair round 3. This handles the one finding of `17-spec-checker.md` and also covers the auditor's Indentation hint in `13-auditor.md`. This file lists only the changes. I edited and staged only `sdd/specs/formatter/cleanup.md` and this handover.

## F1 — A `U+FEFF` that is not at the start of the text (#1389)

### Decision: a plain clarification, written into FMT-CLEAN-009

Why it is a clarification and not an open question:

- **The behavior is unchanged from `main`, and it is already decided by other requirements.** The parser reads a `U+FEFF` that does not start the text as white-space trivia. On a blank line elsewhere, two existing requirements therefore decide it:
  - FMT-CLEAN-001 removes the white space on a line that holds nothing else.
  - FMT-BLANK-007 and FMT-BLANK-008 remove the blank line itself where they apply: inside braces, and before an opening brace or a chained keyword.
- **Nothing about that looks wrong.** FMT-CLEAN-009's reason is the encoding signature, and only a `U+FEFF` at the start of the text is one. Elsewhere it is the zero-width no-break space. A blank line that holds it is still a blank line, and removing it destroys no comment, literal, or code (FMT-PIPE-002).
- **The tests already pin exactly this.** The eight tests the checker lists tag FMT-CLEAN-009 for this claim. Inputs and outputs, from the test sources:
  - `ByteOrderMarkCharacterOnBlankLineBeforeElseIsRemoved`, `…BeforeFinallyIsRemoved`, `…BeforeOpeningBraceIsRemoved`, and `…BeforeClosingBraceIsRemoved`: the blank line is removed together with the `U+FEFF`.
  - `ByteOrderMarkCharacterOnBlankLineBeforeStatementIsRemoved`: the blank line between `int x = 0;` and `x++;` stays as an empty line without the `U+FEFF`.
  - `CleanupPhaseTests.RemovesByteOrderMarkCharacterOnBlankLineInTheMiddleOfTheText`: `U+FEFF` and two spaces on a blank line become an empty line.

Where I wrote it, and why there:

- I wrote it in FMT-CLEAN-009, the requirement those tests tag. It also states where "byte order mark" ends.
- The blank-line rules are only referenced (FMT-BLANK-007, FMT-BLANK-008), so `formatter/blank-lines.md`, which a parallel run owns, stays untouched.

### Changed requirements

- **FMT-CLEAN-009 — The byte order mark is kept — clarified.**
  - New paragraph: only a `U+FEFF` at the start of the text is a byte order mark. A `U+FEFF` on a line that holds nothing but white space anywhere else is white space of that line. It is removed with the line's white space (FMT-CLEAN-001), and with the line itself where the formatter removes that blank line: before a closing brace (FMT-BLANK-007), or before `else` (FMT-BLANK-008).
  - New coverage sentence: a `U+FEFF` that starts the white space in front of a token on a later line is not covered.
  - The Why gains one sentence: elsewhere `U+FEFF` is the zero-width no-break space, which the parser reads as white space.
  - New prose example, both blank-line outcomes:
    - `int x = 0;`, a line holding only `U+FEFF`, then `x++;` gives an empty line.
    - `}`, a line holding only `U+FEFF`, then `else` gives `else` directly after `}`.
  - `Uses` gains FMT-BLANK-008. FMT-BLANK-007 was already listed.
- No ID is added or retired, so the ledger is unchanged.

### Examples for the tests

The eight existing tests above already derive from the new paragraph and its example. Their `FMT-CLEAN-009` tags now state their claim, and nothing needs to change.

## Open questions for the maintainer

Found in passing, not blocking. It is outside the files of this run.

1. **A `U+FEFF` at the start of an indented code line elsewhere in the text.**
   - Input: `class C⏎{⏎    void M(bool b)⏎    {⏎        if (b)⏎        {⏎        }⏎U+FEFF        else⏎        {⏎        }⏎    }⏎}`.
   - Real output, per the auditor's hint (pre-existing on `main`, not re-run by me): the Indentation phase keeps the `U+FEFF` at the start of the `else` line, in front of the indentation it writes.
   - Why it may look wrong: the same character is removed on a blank line (FMT-CLEAN-009 as clarified, FMT-CLEAN-001) but kept in front of code, as if it were a byte order mark. The rule-of-thumb "only the start of the text has a byte order mark" does not hold there.
   - Requirement IDs touched: FMT-CLEAN-009 (its coverage sentence leaves this case out), and the FMT-INDENT requirements of the Indentation phase, which a parallel run owns.
   - Readings:
     - (a) Keep it as today. The character is user text that the formatter does not drop, and FMT-PIPE-002 prefers keeping.
     - (b) Treat it as white space, like on a blank line, so the indentation replaces it.
   - Recommendation: (a), recorded as a follow-up issue for the Indentation run only if the maintainer wants one uniform rule. The behavior is non-destructive, and (b) would delete a character the user wrote in a phase this run may not touch.

## Whole run

- Changed files: `sdd/specs/formatter/cleanup.md` and this handover.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`: 51 passed, 0 failed.
