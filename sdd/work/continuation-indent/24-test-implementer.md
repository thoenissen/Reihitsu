Role: test implementer
Commit: f466f5c
Verdict: tests written

Repair round 2, tests for decisions 29 and 30 (`00-lead.md`), auditor findings 1 and 2 (`22-auditor.md`), derived from the spec examples added in `45c0e2c` (FMT-BREAK-017, FMT-BREAK-022, FMT-BREAK-041, FMT-UNSUP-019) and the list in `23-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PatternChainLayoutTests.SpecExampleBraceOfAPropertyPatternInACaseLabelJoinsItsCombinator` | FMT-BREAK-017 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | **fails**: `case P` / `and` / `{` (and `or` / `{`), brace not joined onto the combinator; the `is` part already matches |
| `PatternChainLayoutTests.SpecExampleBraceWrittenBehindItsCombinatorGivesTheSameOutput` | FMT-BREAK-017 | `AssertRuleResult` | passes (characterization of the other break position; same expected text as the row above) |
| `ContinuationJoinTests.SpecExampleBraceOfAPropertyPatternBehindNotInACaseLabelJoinsTheNot` | FMT-BREAK-041 | `AssertRuleResult` | **fails**: `case not` / `{` and `or not` / `{`; the `o is not` / `{` part already matches |
| `ContinuationJoinTests.SpecExampleBraceWrittenBehindNotGivesTheSameOutput` | FMT-BREAK-041 | `AssertRuleResult` | passes (same expected text as the row above) |
| `ContinuationJoinTests.BraceBehindNotOfAPatternTheCaseLabelDoesNotIntroduceKeepsItsOwnLine` | FMT-BREAK-041, FMT-BREAK-022 | `AssertRuleResult` | passes; other side of the case-label condition with the line break behind `not`: `when p.B is not⏎{` and subpattern `B: not⏎{` give FMT-BREAK-022's example output |
| `LineBreakRequirementTests.SpecExampleCaseLabelIntroducesItsPatternThroughCombinatorsAndNot` | FMT-BREAK-022 | `AssertRuleResult` | passes (characterization; must stay green when the predicate is unified) |
| `ReihitsuFormatterNodeColumnTests.CaseLabelNotPatternTargetJoinsTheBraceOfItsPropertyPatternOntoNot` | FMT-PIPE-024, FMT-BREAK-041 | `AssertFormatsTarget` (`FormatNodeInDocumentAsync`, LF/CRLF, second pass) + `AssertRuleResult` | **fails**: node-level target `not⏎{…}` of a `case` label keeps `case not` / `{` |
| `ReihitsuFormatterNodeColumnTests.CaseLabelNotPatternTargetKeepsTheBraceWrittenBehindNot` | FMT-PIPE-024, FMT-BREAK-022 | as above | passes (node-level control: same expected text as the row above) |
| `UnsupportedAssignmentInArmCommentTests.SpecExampleCommentsOutsideTheShapeAreSupported` (updated to the spec example: record `R`, `c5`–`c7`) | FMT-UNSUP-019 | `AssertSupported` | passes |
| `UnsupportedAssignmentInArmCommentTests.SpecExampleInitializerMembersInsideAnArmOrAMemberAreUnsupported` | FMT-UNSUP-019 | `AssertUnsupportedFor` + location (line 14, column 38) | passes |
| `UnsupportedAssignmentInArmCommentTests.EachInitializerMemberInsideAnArmOrAMemberIsUnsupported` (6 rows: object/`with`/index initializer × arm/anonymous member) | FMT-UNSUP-019 | `AssertUnsupportedFor` | passes |
| `UnsupportedAssignmentInArmCommentTests.InitializerMemberOutsideAnArmOrAMemberIsSupported` (3 rows) | FMT-UNSUP-019 | `AssertSupported` | passes |

Whole formatter project on `f466f5c`: 7130 tests, 3 failures, exactly the three marked **fails**, each on the brace not being joined (the issue's expected-versus-actual difference). Architecture tests: 198/198.

## Not covered, and why
- **Node-level `not` pattern behind `is`** (other side of the case-label fact at node level): written and dropped. It failed on today's code for a reason unrelated to the run — the target `not⏎{…}` of `o is not` lays the brace out at the column of `return` + 4 instead of under `is` (`{` at column 12, document-level 17), because the `is` anchor lies outside the target. No caller formats a bare pattern node: the only code fix formatting a pattern is RH5306, which targets `ListPatternSyntax` (`rg -l PatternSyntax src/Reihitsu.Analyzer.CodeFixes | xargs grep -l FormatNode`). Under *Supported inputs* that is neither a defect nor a follow-up. The same argument makes the two case-label node-level tests caller-unreachable; they are kept only because decision 29 makes the node-level fact part of the one predicate, and they pin it. The lead may drop them.
- **Parenthesized pattern inside a case label** (`case (1 or⏎{…}):`): not specified by any example; its current `case⏎(` layout is already on `00-lead.md`'s follow-up list.
- **Existing tests asserting changed output**: `rg` for a combinator or `not` at the end of a line in test sources found only `is` contexts (`RecursivePatternAlignmentTests` lines 320/520/524, `PatternChainLayoutTests`, `ContinuationJoinTests` line 50), whose output does not change, and `PatternChainLayoutTests`' `case 1 or⏎2:` (no property pattern). No analyzer test holds the case-label shape. None changed.
- **Comment/directive in the gap** (FC for token gaps): a comment between a combinator or `not` and its pattern is FMT-UNSUP-017's refusal, already covered by its tests; the run does not change it.
- **CRLF and idempotency**: covered by `AssertRuleResult`/`AssertFormatsTarget` (both line endings, second pass) in every layout test.
- **Code-fix convergence / Fix All**: no code fix changes in this repair.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PatternChainLayoutTests|FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~LineBreakRequirementTests|FullyQualifiedName~UnsupportedAssignmentInArmCommentTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
```

## Follow-up candidates
_None._ (The node-level `is not` column difference concerns an input no caller produces.)
