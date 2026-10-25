Role: test implementer
Commit: b826c6b
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. This round follows the maintainer's answer to escalations 5 and 12 (`00-lead.md`, last section; `squad/decisions/2026-10-25-phase-prediction-removal-node-level-shift.md`, option A).

What the round does:
- derives the tests for RH5303-006 and RH5304-005 from their spec examples, their qualifier, and the guard scope `09-spec-author.md` states;
- replaces the four tests that `08-implementer.md` reported failing. They assert node-level output that the decided spec no longer requires.

"Fails" means the test fails on b826c6b plus these files, for the reason given. "Passes" means it passes there and must keep passing. Columns are 0-based.

## Tests

### Replaced

| Replaced test | Why | Replacement |
|---|---|---|
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed` | It asserted the fixed layout 32/28. RH5303-006 now withholds the fix for this shape. | `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered` (below) |
| `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyComplexElementBehindTheLastLineOfAMultiLineCommentMeasuredFromTheCommentIsDetectedAndFixed` | Same, for RH5304-005. | `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered` (below) |
| `CommentLedFirstTokenAnchorTests.FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineComment` | No requirement specifies node-level output for this shape any more. The change doc keeps the node-level step unchanged and records the gap. | Deleted. The document-level output of the same input stays covered by `ComplexElementAlignsToItsBraceBehindTheLastLineOfAMultiLineComment`. |
| `CommentLedFirstTokenAnchorTests.FormatNodeInDocumentAlignsComplexElementToItsBraceBehindTheLastLineOfAMultiLineCommentWhenTheInitializerBraceMoves` | Same. | Its document-level half (`AssertRuleResult`) still follows FMT-INDENT-030 and FMT-INDENT-023. It is kept as `ComplexElementAlignsToItsBraceBehindTheLastLineOfAMultiLineCommentWhenTheInitializerBraceMoves`: the input is unchanged, the node-level assertion is removed, and it is tagged FMT-INDENT-030 and FMT-INDENT-023. It passes. |

`FormatNodeInDocumentAlignsComplexElementToItsBraceBehindCommentThatLeadsTheLine` stays: one-line comment, statement and equals-value targets. Nothing in the decision touches it, and it passes.

### New: `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests`

"No fix" tests use `VerifyNoCodeFixIsOffered`. It checks the diagnostics in the markup and asserts that the provider registers no action for any reported diagnostic. "Fixed" tests use `Verify(source, fixedSource, diagnostics)`.

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`: RH5303-006 example 1, verbatim | RH5303-006, SHARED-INIT-002, RH5303-003, RH5303-004 | fails: one action is offered |
| `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed`: RH5303-006 example 2. The comment ends above the brace, and `2` in 22 is reported and fixed to 24. This is the qualifier's other side. | RH5303-006, RH5303-003, RH5303-004, RH5303-005 | passes |
| `VerifyMisplacedBracesAroundComplexElementBehindTheLastLineOfAMultiLineCommentAreDetectedAndNoCodeFixIsOffered`: guard scope. The outer `{` is in 14, which RH5303-001 reports on the creation. The complex element behind `b */ {` is laid out correctly (32/28), so it is not the reported part. | RH5303-006, RH5303-001, RH5303-004; FC-001 | fails: one action is offered |
| `VerifyMisplacedBracesAroundComplexElementBehindLeadingCommentAreDetectedAndFixed`: the same with `/* e */ {`. The fix is offered, and the result is outer brace 16, 32/28. | RH5303-006, RH5303-001, RH5303-004, RH5303-005; FC-003 | passes |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInAssignmentStatementIsDetectedAndNoCodeFixIsOffered`: `d = new …` as a statement. The formatted target starts at the start of its line, and the guard still applies (`09-spec-author.md`). | RH5303-006, SHARED-INIT-002, RH5303-003 | fails: one action is offered |
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentInExpressionBodyIsDetectedAndNoCodeFixIsOffered`: `P => new …`, the arrow-expression target | RH5303-006, SHARED-INIT-002, RH5303-003 | fails: one action is offered |
| `VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`: two declarations. The first holds the shape, and its diagnostic stays in the fixed code as `{|RH5303:…|}`. The second (`/* f */ {`, 24/20) is fixed to 32/28. The test uses one incremental and one Fix All iteration. | RH5303-006, RH5303-005; FC-008 | fails: the first element is moved to 38/34 and the second is left unfixed |

### New: `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests`

| Test | Requirement / failure class | Expected now |
|---|---|---|
| `VerifyComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`: the RH5304-005 example, verbatim in shape (`D = {` / `/* a` / `b */ {`, 32/28) | RH5304-005, SHARED-INIT-002, RH5304-002, RH5304-003 | fails: one action is offered |
| `VerifySplitAssignmentNextToComplexElementBehindTheLastLineOfAMultiLineCommentIsDetectedAndNoCodeFixIsOffered`: guard scope. `E =` / `{ 1 }` is split and reported under RH5304-001. `D`'s element behind `b */ {` is laid out correctly (40/36) in the same creation. | RH5304-005, RH5304-001, RH5304-003; FC-001 | fails: one action is offered |
| `VerifySplitAssignmentNextToComplexElementBehindLeadingCommentIsDetectedAndFixed`: the same with `/* e */ {`, fixed to `E = {` / `1` in 28 / `}` in 24 | RH5304-005, RH5304-001, RH5304-003, RH5304-004; FC-003 | passes |
| `VerifyComplexElementBelowAMultiLineCommentThatEndsAboveItsBraceIsFixed`: the qualifier's other side. `{` sits on its own line in 28 below `/* a` / `b */`, and `2` in 30 is fixed to 32. | RH5304-005, RH5304-002, RH5304-003, RH5304-004 | passes |
| `VerifyFixAllFixesOnlyTheCreationWithoutComplexElementBehindTheLastLineOfAMultiLineComment`: `var a` holds the shape and keeps `{|RH5304:…|}`. `var b` (`/* e */ {`, 32/28) is fixed to 40/36. | RH5304-005, RH5304-004; FC-008 | fails: `var a`'s element is moved to 46/42 |

Every expected fixed text was checked to be the output of today's document formatter on the same code. The fixtures that are reported but not fixed are the spec examples. The two fixes have separate format-target functions (`TryGetFormatTarget` in each provider), so every dimension is tested on both rules. The statement and expression-body targets are tested on RH5303 only; see below.

## Not covered, and why

- **The statement and arrow-expression targets on RH5304**: not added. RH5304-005 names the same three targets as RH5303-006. The equals-value target is covered on both rules, and the other two on RH5303. Whether the RH5304 guard inspects each target the same way depends on an implementation that does not exist yet. The auditor can ask for the two RH5304 cells if the guard turns out to depend on the target kind.
- **A one-line comment variant of each "no fix" test**: `VerifyComplexElementAlignedToTheLeadingCommentIsDetectedAndFixed` (both rules, from round 07) and the two `…BehindLeadingCommentAreDetectedAndFixed` / `…IsDetectedAndFixed` tests above are that side, and they pass.
- **A `/** … */` multi-line documentation comment in front of the brace**: not added. RH5303-006 covers "a multi-line comment" without a kind. A delimited documentation comment that spans lines ends in its own last line the same way.
- **LF/CRLF**: the analyzer tests run one line ending, as all of them do. The guard decides on line structure, not on line-ending width.
- **Failure classes**:
  - FC-009: these tests assert that no action is registered, not that an action without effect is offered.
  - FC-007: the "fixed" tests converge in one pass through the verifier.
  - FC-004, FC-005, FC-019–FC-021: no join, and no gap the fix edits differently.
  - FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite, literal, rename, or member kind.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: for the auditor.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5303|FullyQualifiedName~RH5304"
scripts/test.sh --project formatter --filter "FullyQualifiedName~CommentLedFirstTokenAnchorTests"
scripts/test.sh --project architecture
```

Observed on b826c6b plus these files:
- `analyzer --filter RH5303|RH5304`: `Failed: 8, Passed: 96, Total: 104`. The 8 failures are the tests marked "fails" above, each because an action is offered.
- `formatter --filter CommentLedFirstTokenAnchorTests`: `Passed: 21`.
- full `formatter` project: `Passed: 2972, Failed: 0`.
- full `analyzer` project: `Failed: 8, Passed: 6204, Total: 6212`. The failures are the same 8 tests.
- `architecture`: `Passed: 84`. This includes `EveryRequirementOfAnEnforcedSpecIsCovered`, which `09-spec-author.md` saw failing for RH5303-006 and RH5304-005.
