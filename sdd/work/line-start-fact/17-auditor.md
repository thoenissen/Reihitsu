Role: auditor
Commit: de63fcabc4c7e1088deb3bc5e13c0647a86bde5e
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from these sources:
- `00-lead.md` and the earlier audits of the addition, `05-auditor.md` and `12-auditor.md`;
- the repair handovers `13-spec-author.md` to `16-spec-checker.md`;
- the change doc `sdd/changes/2026-10/fixed-keyword-spacing.md`, FMT-SPACE-002, FMT-SPACE-012, and FMT-SPACE-013;
- the diff 3815e6e..de63fca.

## Scope

- **Head:** de63fca, checked out, clean tree.
- **Base:** `origin/main` = 802afbf (the SHA in my prompt). It is an ancestor of the head.
- **What I audited:** the repair delta against 3815e6e, which answers finding #8. It is the second audit of the `fixed` addition.
- **What the delta changes:**
  - the change doc (*What*, and the decision *No exception for the fixed-size buffer*);
  - one new example in FMT-SPACE-012;
  - three new tests in `HorizontalSpacingRequirementTests`;
  - the handovers and one line in `00-lead.md`.
- **What it leaves alone:** production code. `git diff --stat 3815e6e..HEAD` lists no file under `src/` other than the test class.
- **Scope growth:** none. The repair stayed on finding #8's surface.
- **Diff against the handovers:** it matches `13-` to `16-`.
- **Results I trusted:** the cleaner's build (0 warnings) and formatter run (3270 passed, the 3267 of `12-auditor.md` plus the three new tests). I also trusted the test implementer's mutation run, which removed `FixedKeyword` from `IsSpacedKeyword`.
- **Reused evidence:** the formatter code is byte-identical to 3815e6e. So the probes of `12-auditor.md` still hold for the head:
  - the e6c896f build kept `fixed/* c */int` and `fixed /* c */int`;
  - the 3815e6e build spaces both behind the comment.

## Previous findings

- **#8** (`12-auditor.md`): **closed**. It named two failing members, and both are now recorded and tested:
  - **Buffer `fixed/* c */int`.** The change doc's *What* and its buffer decision state the new output `fixed/* c */ int`. FMT-SPACE-012 carries it as an example. `OneSpaceBehindCommentGluedToFixedSizeBufferType` pins it, and that test is red without `FixedKeyword` (`14-test-implementer.md`).
  - **Buffer `fixed /* c */int`.** *What* states it. `OneSpaceBehindSpacedCommentGluedToFixedSizeBufferType` pins it, and that test is also red without `FixedKeyword`.
  - **The false claim.** The "same output … today" claim is gone. The decision now separates two cases:
    - a whitespace-only gap, whose output is unchanged; `public fixed  int _c[4];` serves as the example;
    - a comment glued to the type, whose output changes.

    Both statements match the before and after outputs that `12-auditor.md` probed.
  - **The holding members of #8's enumeration.** The repair moved none of their code, so they are unchanged.
- **#7:** closed in `12-auditor.md`. The delta does not touch it.
- **#1 to #6:** closed in earlier audits. The delta does not touch them.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption. The guard-scope rows of `12-auditor.md` (`KeywordSpacingRule.IsSpacedKeyword` against RH6001, and `TrimGapBeforeToken`) are byte-identical and still hold._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| The output for a fixed-size buffer's gap after `fixed` | Code: `KeywordSpacingRule.IsSpacedKeyword` only, unchanged since 3815e6e. Specs: FMT-SPACE-012 (requirement and new example) and FMT-SPACE-013. Record: the change doc. The rule pages carry no spacing statement about `fixed` (`rg fixed documentation`). | One owner. The spec example, the change doc, and the code agree. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `HorizontalSpacingRequirementTests.OneSpaceBehindCommentGluedToFixedSizeBufferType` | The FMT-SPACE-012 buffer example: `public fixed/* c */int _b[4];` becomes `public fixed/* c */ int _b[4];`. The comment stays, and the one space goes behind it (FMT-SPACE-013). | Comment kept glued to `int`. Space placed in front of the comment. Comment moved. | `AssertRuleResult(input, expected)`: exact output under LF and CRLF, then a second pass and `PhaseStabilityCheck`. | Adequate. It was red without `FixedKeyword`, so it falsifies the decision that #8 found untested. |
| `….OneSpaceBehindSpacedCommentGluedToFixedSizeBufferType` | `public fixed /* c */int _c[4];` becomes `public fixed /* c */ int _c[4];`. The space in front of the comment stays (FMT-SPACE-013), and one space goes behind it. | The space in front is removed or widened. No space goes behind. | `AssertRuleResult` | Adequate. It was red without `FixedKeyword`. |
| `….SeveralSpacesBehindCommentInFixedSizeBufferBecomeOne` | `public fixed /* d */  int _d[4];` becomes `public fixed /* d */ int _d[4];`. | Any width other than one behind the comment. | `AssertRuleResult` | Adequate as the whitespace-side boundary pin of the change doc's split. It is green without `FixedKeyword`, because FMT-SPACE-002's collapse already gives this output. `14-test-implementer.md` states that it pins output and cannot catch a removal. That matches the change doc's "unchanged" claim for whitespace. |

## Failure classes dismissed

- **FC-001, FC-002:** no guard changed.
- **FC-003, FC-017:** closed with #8. The changed buffer output is recorded, and two tests that fail without the change pin it.
- **FC-004, FC-005, FC-019 to FC-021:** no code moved. The new tests show that the comment keeps its place in all three shapes.
- **FC-006 to FC-009, FC-018, FC-022 to FC-025, FC-029, FC-030, FC-036:** no production code and no code fix changed.
- **FC-010 to FC-012:** each new test checks the second pass and per-phase stability under LF and CRLF.
- **FC-013, FC-014:** unchanged from `12-auditor.md`. No RH rule reports the buffer gap. RH6001 needs a `(`.
- **FC-015, FC-016:** see *Policy ownership*. #8's enumeration is fully recorded.
- **FC-026, FC-027:** the rule is token-local. Nesting was covered in `12-auditor.md`.
- **FC-028:** no entry point changed.
- **FC-031:** the three new summaries describe their bodies. The change doc and the spec example describe the code at head.
- **FC-032:** the only compiled change is the test class. The cleaner ran the formatter project on 24fc86f, and the later commits are handovers.
- **FC-033 to FC-035:** the expected values come from the spec example and the spec author's re-derived table. Two of the three tests are red without the implementation.
- **FC-037:** no text-only claim is made.
- **Tab-width matrix:** unchanged from `12-auditor.md`. The preparation phase replaces the tab first.

## Architecture findings

_None._ The repair corrects a record and adds tests for a decision that one owner makes. It repeats no earlier finding's class.

## Hints

_None new._ The two hints of `12-auditor.md` still stand: the copied `VerifyFormatter` steps, and the unused `int[] others` fixture parameters.
