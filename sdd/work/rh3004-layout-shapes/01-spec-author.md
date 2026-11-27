Role: spec author
Commit: 8d8e5f9
Verdict: spec written

No change doc: every statement below writes down behavior the code has today (a clarification, `sdd/README.md`, *When a change doc is required*). The prompt carried no intended fix or suspected cause. The issue's goals are proposals (`agent-filed`); the outcomes per item differ from them as stated below.

## Changed requirements

- RH3004-011 — A comment over several lines in the parameter list keeps its later lines — added (clarification; issue item 1). `Uses` gains FMT-INDENT-007. Ledger updated.
- RH3004-008 — A string or a comment that spans lines withholds the fix — clarified: one new example paragraph (issue item 2) for a delimited comment opened behind `{` whose closing `*/` stands in front of the first statement, on the `Foo();` path and on the single-`return` path. Normative text unchanged.
- RH3004-006 and RH3004-009 — unchanged (issue items 2 and 3, see below).

Rule page `documentation/rules/RH3004.md` unchanged: nothing reported or fixed changes for the user in the normal case.

## Findings per issue item

How established: `scripts/apply-fix.sh RH3004` over fixtures at 8d8e5f9 (LF and CRLF), and `scripts/format.sh` over fix results and directive samples.

1. **Expression-body path, parameter-list comment over several lines without a `#` line.** The fix is offered (RH3004-008, *Path*). Result: the parameter list moves to the column `delegate` stood in (behind `async ` for an `async` method), `/* first` moves with it, every later line of the comment keeps its column, and `) => x;` follows on the comment's last line. This holds for the issue's example (`second` in column 4) and for a later line aligned under `first` (column 46, no longer under `first` afterwards). Whole-document formatting of the result changes nothing apart from the final newline in the file, so no second diff. FMT-INDENT-007 (formatter keeps a delimited comment's later lines) together with SHARED-FIX-007 already decides this output, so it is no open question. It is now RH3004-011. On the block-body path the same comment withholds the fix (RH3004-008, second condition; already tested by `FixWithheldForADelimitedCommentInTheParameterListThatSpansLinesOnTheBlockBodyPath`).
2. **RH3004-006 shift measured on a comment continuation.** No caller can reach this. `{ /* a⏎ b */ Foo();` is on the block-body path, and so is `{ /* a⏎ b */ return x;`, because a comment in the block rules out the expression body (RH3004-003). Both withhold the fix by RH3004-008, second condition: apply-fix reports `no-fix-offered` for both. RH3004-006 already says that a rewritten block holds no delimited comment that spans lines, so the shift is never measured on a comment continuation. No requirement needs to name "the statement itself", and RH3004-006 is unchanged. I added the shape as an example to RH3004-008 so a test reads the result. No existing test covers a comment opened behind `{` (searched the three `RH3004*Tests.cs` files).
3. **Inactive column-0 directives.** RH3004 cannot reach this. RH3004-009 withholds the fix for any directive or disabled text in the anonymous method, and SHARED-FIX-007 (*Moved code*) already says that a directive line does not arise in RH3004-006. A fixture with `#if NEVER` / `#if INNER` / `#endif` / `#endif` inside the block reports `no-fix-offered`. The directive branch of `SyntaxIndentationUtilities.KeepsItsColumn` keeps every non-region directive, active or not, and `IndentationRewriter.IsNonRegionDirectiveTrivia` applies its column-0 rule to active directives only. Even so, the formatter left inactive nested directives as written (in column 0, and in columns 12 and 2) in a whole-document run, so their visible results agree in the shapes tried. No spec change and no production change. Whether an RH3004 test for an inactive nested directive adds anything beyond the existing RH3004-009 tests (which already contain disabled branches) is for the skeptic and the test implementer to decide (*Coverage first*).

## Examples for the tests

- RH3004-011 — the example block (fix offered: `a` with a comment over two lines, `c` with a comment on one line). The issue's own input (`second` in column 4) gives the same kind of result: `(int x /* first⏎    second */) => x;`.
- RH3004-008 — the new example after "is reported, and no code fix is offered for any of the four." (two anonymous methods, no fix offered).

`scripts/test.sh --project architecture` currently fails only on `EveryRequirementOfAnEnforcedSpecIsCovered`: `RH3004-011: no test covers it`. The test implementer's tagged test clears it. All other spec-structure tests pass (Uses, ledger, index).

## Output questions

_None._ Every output above is decided by an unchanged requirement: FMT-INDENT-007 with SHARED-FIX-007 and RH3004-008 for item 1, RH3004-008 for item 2, and RH3004-009 for item 3.

## Internal choices

- Item 1 is written as a new requirement, RH3004-011, and not as a sentence added to RH3004-006. The alternative was a paragraph in RH3004-006. My reason: it is a separate decision with its own reason (comment text is content, FMT-INDENT-007), and RH3004-006 deals with the block-body shift.
- Item 2 is recorded as an example under RH3004-008 and not as wording in RH3004-006 about the measured character. The alternative was to reword RH3004-006. My reason: RH3004-006 already excludes the case, and the issue's question has no input the fix rewrites.

## Follow-up candidates

_None._ Item 3's predicate difference showed no visible disagreement in the shapes I tried. RH7004 and `OrderingMoveSafety` also use `KeepsItsColumn`, but I did not sweep them; that is outside this run.
