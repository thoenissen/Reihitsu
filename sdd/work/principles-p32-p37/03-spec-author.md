Role: spec author
Commit: 0e300bd
Verdict: spec written

Revision of the spec diff from 3f07174. It addresses the skeptic's F1 to F5, the minor note on SHARED-COMMENT-012, and the maintainer's answers to Q1 and Q2.

About my prompt: it named the Q1 and Q2 outcomes. Those are recorded maintainer decisions (`squad/decisions/2026-10-23-1380-p12-chain-exemption.md`, `squad/decisions/2026-10-23-1380-p34-family-scope.md`), so I followed them. For every other change I went back to the code, the issue text and the specs myself, and I did not adopt any skeptic suggestion without doing so.

## Changed requirements

- **SHARED-FIX-008 — clarified (F1)** in `shared/code-fixes.md`.
  - The first sentence now states P35 as decided: a fix writes the line ending FMT-PIPE-007 detects.
  - The "creates or keeps" clause now holds only for CRLF and LF documents.
  - In an all-`\r` document, created breaks MUST be `\n`, and a break the fix keeps MUST stay a lone `\r`. This is today's output from #1363 Q6 (`if (b)⏎{⏎M(b);⏎}␍`), which P35 confirmed ("as they do today"). Q6 objected only to the created breaks.
  - The Why now explains the outcome instead of arguing against it. The example's last sentence names the kept `␍` behind the closing brace.
  - Nothing else in the file changed, including its `Uses`.
- **FMT-PIPE-007 — clarified (F2)** in `formatter/pipeline.md`. One added sentence: a one-character line break, such as a lone `\r`, MUST count as `\n`, so an all-`\r` file gets `\n`. There is also one Why sentence and one prose line after the example.
  - This is today's behavior: `LineEndingUtilities.DetectEndOfLine` counts every end-of-line trivia and XML newline token shorter than two characters as `\n`.
  - Without it, the decided P35 text ("the line ending FMT-PIPE-007 detects … a lone `\r` counts as `\n`") pointed to a requirement whose text gives `\r\n` (a 0:0 tie).
  - **Scope note for the lead:** this is the one edit outside the placements listed in 00-lead.md. Clarification 1 fences only `shared/code-fixes.md`, and `pipeline.md` is already edited in this run. Reverting it would leave SHARED-FIX-008 citing a requirement that contradicts it.
- **FMT-STRUCT-024 — changed (F3)** in `formatter/structural-transforms.md`.
  - "would not compile" no longer cites FMT-PIPE-003.
  - The instances are now only FMT-STRUCT-007 (a directive after `return` does not parse, and an orphaned conditional half or region is a compile error) and the comment clauses of FMT-STRUCT-012 (a moved comment would describe other code, or a `//` comment would swallow the new semicolon).
  - A new sentence says the requirement adds to FMT-PIPE-002 and FMT-PIPE-003. FMT-STRUCT-003, -018 and -023 refuse on those grounds (a dropped comment, a dropped or misplaced directive, a changed meaning), not under FMT-STRUCT-024.
- **SHARED-COMMENT-012 — changed (F4)** in `shared/comment-placement.md`. The rule bullet now leads to its own example. Its condition has three parts:
  - the token would start its line without the comment, meaning only whitespace stands between the line break in front of the comment and the comment, and between the comment and the token;
  - the token is one the rule judges where it starts a line (for RH5203, an argument of a wrapped list);
  - when both hold, the rule MUST report it whatever column it reaches, located where its own requirements locate it.

  A token behind code on the comment's first line is judged as a token behind code on its line, so RH5203-001's "behind another argument on the same line MUST NOT be reported" still holds. The example caption now says why `2` is reported.
- **FMT-BLANK-005 — clarified (F5)** in `formatter/blank-lines.md`. "Directly before the statement" now reads "directly before the statement, or, when comments stand between that directive and the statement, in front of those comments (FMT-BLANK-016)". The Why's "directly before the statement" became "after the last directive". I re-verified this as today's behavior with `scripts/trace.sh`: the shape below is stable after pass 2.
- **FMT-BLANK-016 — example added**: the `}⏎#pragma …⏎// why⏎a++;` shape, where the blank line goes between `#pragma` and `// why`. This is the only conforming shape for its "in front of the comments" clause.
- **SHARED-COMMENT-011 — clarified (Q1).**
  - A new sentence: a requirement that keeps the rule from reporting because of a comment in that place still applies. Under SHARED-CHAIN-004, RH5112 does not report a chain whose first link stands behind a comment that starts the link's line.
  - A Why sentence, and a negative RH5112 example: `var x = s⏎/* c */ .Trim();` is not reported. Verified with `scripts/apply-fix.sh RH5112`: `no-diagnostic` under LF and CRLF, and the control without the comment is reported and fixed.
  - `comment-placement.md` `Uses` gained SHARED-CHAIN-004.
- **FMT-PIPE-014 — clarified (Q2).**
  - A new paragraph: the requirement does not override the layout a construct's own requirement chooses inside its family. It names those choices: FMT-BREAK-014 (function-pointer lists joined), FMT-BREAK-021 (array initializers keep their layout, object and collection initializers expand), FMT-BREAK-022 (a one-line collection expression stays on one line), FMT-BREAK-026 (the property or indexer arrow joins), and the switch-arm non-goal of `line-breaks.md`.
  - Within those choices it lists the members P34's sources named: indexer parameter lists, compound assignments, attribute-target colons, a comment before a closing `]` (indented as before `}`, per #1383), and arrow spacing.
  - A Why sentence was added. `pipeline.md` `Uses` gained FMT-BREAK-014, -021, -022 and -026.
- **RH5203-001 — changed (minor note: where SHARED-COMMENT-012 belongs).** One clause was appended: "…and an argument behind the closing `*/` of a comment that spans several lines as SHARED-COMMENT-012 specifies." SHARED-COMMENT-012 was added to the `RH5203.md` `Uses`.
  - Why only RH5203: it is the rule of SHARED-COMMENT-012's own example and conforms today, so a tagged test there is real coverage. Listing it in one enforced spec is enough to make the coverage check enforce it, which it now does.
  - RH5204-001, SHARED-INIT-001 (RH5301 to RH5305) and the FMT-BREAK texts are the requirements #1382 says it changes. Wiring those now would mean writing their non-conforming outcomes into analyzer and formatter texts that #1382 owns.
  - The formatter side of SHARED-COMMENT-012 stays unwired for the same reason: FMT-BREAK-006, -007, -025 and -027 belong to #1382.

Unchanged from 3f07174: FMT-PIPE-013, FMT-STRUCT-002, FMT-STRUCT-006, FMT-SWITCH-007, `switch-case-braces.md`, and `requirement-ids.txt`. No ID was minted, retired, moved or renumbered, and no SHARED-DIAG ID was touched.

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 38 passed, 1 failed. `SpecConsistencyTests` and `SpecStructureTests` all pass. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for FMT-BLANK-016, FMT-PIPE-013, FMT-PIPE-014, FMT-STRUCT-024 and SHARED-COMMENT-012. SHARED-COMMENT-012 is new to that list, which is intended: it is now enforced through RH5203.

## Examples for the tests

Conforming shapes are the ones the test implementer may tag. ⏎ is a line break and ␍ a lone `\r`. Shapes from 01 and 02 that still hold are not repeated, except where the revision changes what may be tagged.

- **FMT-PIPE-007 (clarified):**
  - `ReihitsuFormatterHelpers.DetectEndOfLine` on a source whose line breaks are all `"\r"` returns `"\n"`.
  - Formatting an all-`\r` file writes `\n` for every line break.
  - Build the string in the test: FC-017/FC-033 harnesses normalize line endings. I took this from the code; I did not run it.
  - The existing FMT-PIPE-007 tests stay valid: none uses an all-`\r` input.
- **SHARED-FIX-008 (clarified):** RH5405 on `if (b) M(b);` in an all-`\r` document gives `if (b)⏎{⏎    M(b);⏎}␍`. The three created breaks are `\n`, and the kept one behind `}` stays `\r`. RH5417 on `void M() { }` creates `\n`. Source: #1363 Q6 output; I did not re-run it, because `apply-fix.sh` normalizes fixtures. The test must build the `\r` document itself and assert both the created and the kept breaks. The existing CRLF and LF tagged tests still hold.
- **FMT-STRUCT-024 (narrowed):** tag only refusal shapes that meet its three conditions:
  - the FMT-STRUCT-007 refusals in `ExpressionBodyDirectiveGuardTests`:
    - `…WithDirectiveBeforeExpressionIsNotConverted` for all six kinds
    - `MethodWithPragmaBeforeExpressionIsNotConverted`
    - `MethodWithNullableDirectiveBeforeExpressionIsNotConverted`
    - `MethodWithLineDirectiveBeforeExpressionIsNotConverted`
    - `MethodWithRegionDirectiveBeforeExpressionIsNotConverted`
    - `MethodWithRegionAfterExpressionIsNotConverted`
    - `MethodWithRegionBeforeArrowIsNotConverted`

    Not the two "IsStillConverted" tests.
  - the comment refusals tagged FMT-STRUCT-012:
    - `OwnLineCommentBeforeStatementKeepsBlock`
    - `CommentBeforeClosingBraceKeepsBlock`
    - `CommentAfterOpeningBraceKeepsBlock`
    - `CommentAfterAccessorKeywordKeepsBlock`
    - `CommentAfterReturnKeywordKeepsBlock`
    - `CommentsAfterStatementAndClosingBraceKeepBlock`
    - `LineCommentBeforeSemicolonKeepsBlock`
    - `DocumentationCommentInsideBodyKeepsBlock`
    - `CommentBeforeOpeningBraceKeepsBlock`
    - `CommentBeforeReturnedExpressionKeepsBlock`
    - `CommentBeforeStatementSemicolonKeepsBlock`
  - the spec example.

  Do **not** tag FMT-STRUCT-024 on the directive or disabled-text refusals of FMT-STRUCT-003, -013, -018, -019 and -023, or on the pragma refusals of FMT-STRUCT-012. Those refuse under FMT-PIPE-002/-003, which the 02 handover's list did not separate out. The non-conforming shapes are unchanged: #1384, #963, #1390, #1395.
- **FMT-BLANK-016:** spec example 1 (unchanged), and the new example 2: `if (a > 0)⏎{⏎}⏎#pragma warning disable CS0168⏎// why⏎a++;` → blank line between `#pragma` and `// why`. Verified with `scripts/trace.sh`: only BlankLinePhase changes it, and it is stable after pass 2. Example 2 also exercises the clarified FMT-BLANK-005 and may carry both tags. The non-conforming shapes are unchanged (#1385).
- **SHARED-COMMENT-011:**
  - the RH5204 example
  - the RH5201, RH5301 and RH5303 shapes from the 02 Premise table
  - **new:** the negative RH5112 example from the spec (no diagnostic, LF and CRLF, verified), which may also carry SHARED-CHAIN-004

  Do not tag the seven non-conforming rules from the 02 table (#1147).
- **SHARED-COMMENT-012:**
  - RH5203 spec example 2 (`P(1,⏎          /* a⏎          b */ 2);`, reported, verified). It must be an analyzer-only assertion: the fix ends `no-progress (2 iteration(s))` under LF and CRLF (re-verified; FC-007).
  - formatter example 1, which is unchanged.
  - **Non-conforming (verified):** the same RH5203 call with `     b */ 2);`, which puts `2` in the alignment column, is `no-diagnostic` under LF and CRLF. The clarified text requires a report whatever the column.
- **FMT-PIPE-014:** the spec example. The Q2 shapes are now conforming under their own requirements; they may carry FMT-PIPE-014 only in a test that asserts the family clause:
  - `delegate*<int,⏎int, void>` joined
  - `int[] b = { 1, 2 };` kept
  - `int P⏎=> 1;` joined
  - a switch arm `1⏎=> 2,` kept

  Indexer lists, `+=`/`??=` wraps, the comment before `]`, attribute-target colon spacing and arrow spacing stay non-conforming (#1383, #958).
- **FMT-PIPE-013:** unchanged from 01/02.

Tagged tests that contradict the amended wording: none found.
- For SHARED-FIX-008, FMT-PIPE-007, FMT-BLANK-005 and RH5203-001, I read their tagged tests.
- None uses an all-`\r` document.
- None has a comment between a directive and the statement after a closing brace.
- None has a multi-line comment tail in front of an argument.

## Further instances for existing follow-ups

- **thoenissen/Reihitsu#1382:** `P(1,⏎          /* a⏎     b */ 2);`, where `2` lands in the alignment column behind the tail, is not reported by RH5203 (LF and CRLF). Under SHARED-COMMENT-012 it must be. Today's report on the spec-example shape comes from the column, not from P33 ("by accident", as #1382 says).

## Open questions to file

_None._

## Decisions needed

_None._ The lead should confirm the FMT-PIPE-007 scope note above. If the clarification has to leave this run, SHARED-FIX-008's first sentence has to stop attributing the lone-`\r` rule to FMT-PIPE-007, and that contradicts the decided P35 text.
