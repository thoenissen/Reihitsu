Role: spec author
Commit: 58438e4
Verdict: spec written

Repair round for the auditor's findings in 07. My items are 1, 2 (the spec side) and 3 of the worklist in 00-lead.md. My prompt carried no intended fix or verdict. The auditor's suggestions were treated as suggestions: I re-ran each line-ending observation myself before deciding.

`␍` is a lone `\r` and `⏎` is `\n`.

## How the statements were established

I ran a scratch harness outside the repository at `/tmp/claude-0/-home-user-Reihitsu/916c84b7-f164-50d0-90d2-176b1c91c894/scratchpad/crfix/crfix.cs`. It references `src/Reihitsu.Tooling`, builds the document itself without normalizing line endings, applies the first offered fix, and prints every line break. Output: `.../scratchpad/repair-run.txt`. Each fixture ran in an all-`␍` document and in an LF control. The LF controls are all-`⏎`.

| Fix | Input in an all-`␍` document | Result around the fix |
|---|---|---|
| RH5405 | `if (a) M(a, b);␍` | `if (a)⏎{⏎M(a, b);⏎}␍` |
| RH5405 | `if (a)␍            M(a, b);␍` | `if (a)⏎{⏎M(a, b);⏎}␍`. The input's break behind `if (a)` became `⏎`. |
| RH5406 | `if (a) M(a,␍                 b);␍` | `if (a)⏎{⏎M(a,⏎b);⏎}␍`. The input's break between `M(a,` and `b);` became `⏎`. |
| RH5406 | `if (a)␍            M(a,␍              b);␍` | `if (a)⏎{⏎M(a,⏎b);⏎}␍` |
| RH5417 | `void M() { }␍` | `void M()⏎{⏎}␍` |
| RH5011 | `a++;␍break;␍a++;␍` | `break;␍␍a++;`. The created blank line ends in `␍`. |
| RH8304 | `/// <summary>Runs</summary><remarks>Remarks</remarks>␍` | `/// <summary>Runs</summary>␍    /// <remarks>…`. The created break is `␍`. |

For FMT-PIPE-007, a second scratch harness, `.../scratchpad/fmteol/fmteol.cs`, ran `ReihitsuFormatter.FormatSyntaxTree` on strings it built itself, twice. Results:
- `class C␍{␍void M(){}␍\r\n}` (two lone `\r`, one CRLF) gives `class C⏎{⏎    void M()⏎    {⏎    }⏎}`.
- The same input with a second `\r\n` behind `}` is a 2:2 tie and gives the same layout in CRLF, with no final line break.
- An all-`␍` input gives `⏎` throughout.
- Every output is stable on the second pass.

## Changed requirements

- **SHARED-FIX-008 — clarified (worklist item 1)** in `sdd/specs/shared/code-fixes.md`. Nothing else in that file changed.
  - **Removed** the clause "and a line break the fix keeps MUST stay a lone `\r`", the Why sentence about kept lone `\r` breaks, and the example's closing clause about the `␍` behind the closing brace.
  - **Why removed and not narrowed.** The clause is false, as the table shows:
    - RH5406 (both shapes) and RH5405 (body on the next line) turn a break that was already in the input into `⏎`. So which kept breaks stay `␍` depends on the code a fix happens to lay out through the formatter, not on a rule.
    - Narrowing the clause to "inside the laid-out node vs. outside it" would write that implementation boundary into a shared requirement.
    - The kept-break behavior is not part of the decided P35 text.
    - Whether the mixed result is right is the maintainer's call, so it is an open question below.
  - **What remains.** The decided P35 text is unchanged: a fix writes the line ending FMT-PIPE-007 detects, and a lone `\r` counts as `\n`. So in an all-`␍` document, every break the fix creates MUST be `\n`. For CRLF and LF documents, "creates or keeps" is unchanged; it holds for every fix in the table's LF controls.
  - Heading, ID and `Uses` are unchanged.
- **FMT-PIPE-007 — example added (worklist item 3)** in `sdd/specs/formatter/pipeline.md`.
  - A second example: `class C␍{␍void M(){}␍` then `}` gives the formatted class with `\n` throughout. Two lone `\r` count as `\n` and outnumber the one `\r\n`.
  - Its notation extends the first example's `␍` legend: a `␍` at the end of a line is half of a `\r\n`, and one inside a line is a lone `\r`.
  - The prose after it adds the tie variant: a second `\r\n` behind `}` gives `\r\n` throughout. The all-`\r` sentence stays.
  - Requirement text and Why are unchanged.
  - This is the code example thoenissen/Reihitsu#1081 item 2 asks for. The example discriminates: if a lone `\r` were not counted, the count would be 0:1 and give `\r\n`.

No ID was minted, retired, moved or renumbered. `requirement-ids.txt` is unchanged.

## Contradictions left between active requirements (worklist item 2)

This adds to the list in 01:

- **SHARED-FIX-008 against RH5011-004 and RH8304-004.**
  - RH5011-004 says the inserted line ends "with the same line break as that line" (the `break` line). RH8304-004 says to insert "the line break of the line the element starts on".
  - In an all-`␍` document, both mean `␍`, and both fixes create `␍` today (table above).
  - Amended SHARED-FIX-008 (the decided P35 text) requires every created break there to be `\n`.
  - In CRLF and LF documents, the two requirements and SHARED-FIX-008 agree.
  - Before this PR, SHARED-FIX-008 ("the line ending the document already uses") agreed with both. The contradiction is introduced by writing P35, and the code does not meet P35 for these two fixes. So the issue's "today's behavior, no code change" does not hold for RH5011 and RH8304.
  - I left RH5011-004 and RH8304-004 unchanged. Changing them to `\n` would specify output the code does not produce. The follow-up changes them together with the code.
  - No test tagged RH5011-004, RH8304-004 or SHARED-FIX-008 uses an all-`␍` document, so no tagged test pins either side.
  - Related: thoenissen/Reihitsu#1377 lists RH5011-005 among the restated line-ending clauses, but neither RH5011-004 nor RH8304-004.

**Follow-up candidate for the lead to file** (clarification 9): "Decided by principle P35 (#912)". The RH5011 and RH8304 fixes create a lone `\r` in an all-`\r` document. Under SHARED-FIX-008 they must write `\n`. RH5011-004 and RH8304-004 change with them. The private line-break choices are `RH5011…CodeFixProvider.ApplyCodeFixAsync` and `DocumentationCommentCodeFixUtilities.GetLineBreak`. I found no open `SDD open questions: sdd/specs/shared/code-fixes.md` issue; a new one is needed unless the lead knows of one.

## Examples for the tests

- **SHARED-FIX-008.** The conforming shapes a test may pin:
  - created breaks are `\n` in an all-`␍` document: RH5405 one-line (existing test), RH5417 `void M() { }␍` → `void M()⏎{⏎}␍` (verified, still untested);
  - the CRLF and LF shapes as before.
  - Do not tag RH5011 or RH8304 all-`␍` shapes; they are non-conforming (above).
  - **Existing tagged test:** `RH5405BracesMustNotBeOmittedAnalyzerTests.FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` does not contradict the amended text, because its three created breaks are `\n`.
    - Its exact expected string also pins `}␍`, which the requirement no longer states and which the open question below concerns.
    - Its `<summary>` ("…and that the line break the fix keeps from the input stays a lone carriage return") and its inline comment describe the removed clause. They need rewording so they claim only the created breaks.
    - Whether to keep the exact-string assertion is the test implementer's call.
  - Tagged tests contradicting the amended text: none. The change only removes a constraint.
- **FMT-PIPE-007.** The new example 2, built in the test without a normalizing harness (FC-033):
  - `"class C\r{\rvoid M(){}\r\n}"` gives `"class C\n{\n    void M()\n    {\n    }\n}"`;
  - optionally the tie variant `"class C\r{\rvoid M(){}\r\n}\r\n"`, which gives the same layout with `\r\n` and no final line break.
  - The existing `DetectEndOfLineCountsALoneCarriageReturnAsLineFeed` covers the count. A `FormatSyntaxTree` test of example 2 would cover the written output.
  - Tagged FMT-PIPE-007 tests contradicting the text or examples: none. The text is unchanged, and the new example agrees with the 2:1 and 1:1 `DetectEndOfLine` tests.

## Open questions to file

**Which line ending a line break the fix keeps gets in an all-`\r` document** — for a new `SDD open questions: sdd/specs/shared/code-fixes.md` issue (none found open).

- **Input** (RH5406, every line break a lone `\r`):

  ```
  class C␍{␍    void M(bool a, bool b)␍    {␍        if (a) M(a,␍                 b);␍    }␍}␍
  ```

- **Real output:**

  ```
  class C␍{␍    void M(bool a, bool b)␍    {␍        if (a)⏎        {⏎            M(a,⏎              b);⏎        }␍    }␍}␍
  ```

  RH5405 with the body on the next line does the same to the break behind `if (a)`.
- **Why it looks wrong:**
  - Two line breaks the fix carries over from the input are treated differently. The one between `M(a,` and `b);` becomes `\n`, and the one behind the closing brace stays `\r`.
  - The split follows which code the fix lays out through the formatter, not a stated rule. The result mixes `\r` and `\n` inside the fix's own statement.
  - Text-edit fixes keep every input break as it is.
  - P35 decided only the breaks a fix creates.
- **Requirement IDs it touches:** SHARED-FIX-008 (P35), SHARED-FIX-007, FMT-PIPE-007.

## Decisions needed

_None._

## Checks

`scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests|FullyQualifiedName~RequirementCoverageTests"`: 39 passed, 0 failed.
