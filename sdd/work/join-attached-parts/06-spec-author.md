Role: spec author
Commit: 83d7a44
Verdict: spec written

This round carries out the maintainer's batch-2 answers recorded in `00-lead.md`: 1A (a comment on the same line in the gap is refused) and 2A (the gap between `?` and `[` is declared too, in this run). It also covers decision 7 (RH5112 in change 7) and the wording suggestion from 05-skeptic. Besides those answers, the prompt carried no intended fix, suspected cause, or hoped-for verdict.

I checked the deletions for 2A against the code before moving them to *Deleted*:

- **The `?`–`[` refusal.**
  - On `main`, `ChainLineBreakRewriter.CloseInnerGaps(attachedPart.Tokens, …)` joins that gap. It is called in `NormalizeChain` and in `JoinConditionalElementAccesses`, and it refuses through `CloseGap`'s `WouldJoinAcrossUnjoinableTrivia` term.
  - A conditional element access's tokens are `!`?, `?`, `[` (`FluentChain.GetConditionalTokens`). Every gap between them is now in the shape.
  - Only the refusal the new FMT-BREAK-040 owner would need is deleted. `CloseGap` and `CloseInnerGaps` stay, because link operators and member names still use them.
- **`MethodChainAlignmentContributor.Contribute`: both `AlignRestOfRun(attachedPart.Tokens, model)` calls.** One is in the chain-without-links branch and one is in the attached-part loop.
  - The only attached part with more than one token is a conditional element access.
  - A later token of it starts a line only when the trivia in the gap blocks the join, and FMT-UNSUP-014 now refuses every input that has such trivia there.
  - Every node-level input that holds the `?` also holds the `[` (and the `!` in front of it).
  - `AlignRestOfRun` stays for `link.OperatorTokens`.

## Changed requirements

- **FMT-UNSUP-014 — changed (still Draft with the change doc).**
  - First sentence: the skeptic's wording ("of the kinds whose gap FMT-BREAK-039 or FMT-BREAK-040 joins"). It now names two gaps as bullets: the gap in front of the part's first token, and the gap between the `?` and the `[` of a conditional element access.
  - The bullet "In front of the part" became "In front of the part, and between `?` and `[`". `n? /* c */ [0]`, `n? // c⏎[0]` and `x!? /* c */ [0]` are now refused. `n?[/* c */ 0]` is not.
  - "On any line" names `[with /* size */ (4)]` (FMT-SPACE-028) and `n? /* c */ [0]`.
  - *Why* replaces the carve-out sentence with the reason for 2A.
  - The FMT-UNSUP-005 sentence now says "either gap".
  - Refused example: added `var e = n? // c5⏎[0];`. The location is unchanged (`// c1`, 7,21).
  - "Other side" example: `n? /* c6 */ [0]` became `n?[/* c6 */ 0]`.
  - Uses row: added FMT-SPACE-028.
- **FMT-BREAK-040 — changed.** *Why* drops the carve-out that sent the `?`–`[` gap to FMT-BREAK-001. Neither the join nor the line-break removal ever meets the shape.
- **FMT-SPACE-028 — changed.** The sentence "A comment in the gap keeps its place … (FMT-SPACE-013, FMT-SPACE-014)" is replaced by: the gap never holds a comment, a directive, or disabled text, and an input that holds one is refused (FMT-UNSUP-014), for example `[with /* size */ (4)]`. The example drops the `d` line in both the input and the output. The `horizontal-spacing.md` Uses row gains FMT-UNSUP-014.
- **RH2001-007 and RH7101-007 — changed (wording).** Both also name the gap between the `?` and the `[` of a conditional element access.
- **`AGENTS.md` PrecheckPhase row.** The eighth shape now names the `?`–`[` gap.
- **Change doc `sdd/changes/2026-11/attached-part-below-element.md`.**
  - *Affected requirements*: adds FMT-SPACE-028.
  - *What*: states the `?`–`[` gap and the FMT-SPACE-028 consequence.
  - Change 6 gains two examples. Both *before* outputs come from `scripts/format.sh` on the current code:
    - `[with /* size */ (4)]`: before → `[with /* size */(4)]`; CLI line `Error: <file>(7,32): …`
    - `return n? // first⏎[0];`: before → `[0]` under the `?`; CLI line `Error: <file>(5,19): …`
  - Change 7 gains RH5112. Its *before* comes from `scripts/apply-fix.sh RH5112`, LF and CRLF: `return items // element⏎               ?[0].ToString();`.
  - *Decisions*: the batch-2 answers are added to the summary paragraph. "The gaps of the part, on any line" now records 1A and the FMT-SPACE-028 consequence, and a new section "The gap between `?` and `[` is declared too" records 2A.
  - *Special cases*: four entries added to *Deleted*:
    - the `?`–`[` refusal;
    - both `AlignRestOfRun(attachedPart.Tokens, …)` calls;
    - the FMT-BREAK-040 text;
    - the FMT-SPACE-028 text.

    In *Checked and kept*, the old `?`–`[`/`AlignRestOfRun` item is replaced by one that keeps `CloseInnerGaps` and `AlignRestOfRun` as methods, for link operators.
  - *Alternatives*: the old "include the gap" item is replaced by two items the maintainer rejected (leave the gap out; refuse only a gap with a line break).

`scripts/test.sh --project architecture`: 197 passed, 1 failed. `EveryRequirementOfAnEnforcedSpecIsCovered` fails because FMT-BREAK-040 and FMT-UNSUP-014 have no covering test yet, as expected. The Uses failure that showed up along the way (FMT-SPACE-028) is fixed.

## Examples for the tests

- **FMT-UNSUP-014**
  - The three examples. The refused one now includes `n? // c5⏎[0]`.
  - Refused: `n? /* c */ [0]`, `x!? /* c */ [0]`, `[with /* size */ (4)]`.
  - Not refused: `n?[/* c */ 0]`.
  - Node level: an input that holds only the element binding `[0]` does not hold the `?`–`[` gap.
- **Change doc, change 6:** the two new examples with their CLI lines.
- **Change doc, change 7:** RH5112 is withheld (fixture above). The existing analyzer-only test does not lock this.
- **FMT-SPACE-028:** the amended example (three lines).
- **Existing tests whose expected value becomes "refused".** These are in addition to 05-skeptic's list:
  - the two `[with /* size */ (4)]` cases in `WithElementSpacingTests` (FMT-SPACE-028);
  - `ChainLinkLayoutTests.CommentInsideConditionalElementAccessPutsBracketUnderQuestionMark`.
- Change doc changes 6 and 7: the CLI lines and the four withheld fixes.

## Output questions

_None._ Both batch-2 questions are answered.

## Internal choices

- **The CLI reason text is unchanged** (`a comment or directive in front of an argument list, an element access, or a null-forgiving operator`), also for the `?`–`[` gap, where the comment stands in front of the element binding's `[`. The alternative was to extend the reason and the requirement title. That would change every CLI line already shown in the change doc, for a member that is rarer still.
- **FMT-SPACE-028 points to FMT-UNSUP-014 instead of being silent.** The alternative was to only drop the sentence and the example line, which would leave readers asking what happens to a comment there.
- **RH2001-007 and RH7101-007 name the `?`–`[` gap explicitly**, so their lists of shapes match FMT-UNSUP-014. The alternative was to leave them as they were, since they reference FMT-UNSUP-014 in any case.
- **The RH5112 example is placed before change 7's closing sentence** ("The same holds for every other fix …"), so that sentence still ends the list.

## Follow-up candidates

_None_ new.
