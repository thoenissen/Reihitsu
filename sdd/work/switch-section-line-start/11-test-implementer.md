Role: test implementer
Commit: 6c5c3f3
Verdict: tests written

Repair round 1, findings F2 and F3 of `09-auditor.md` as `00-lead.md` decision 13 routes them, on maintainer answers 8 and 9 and the spec author's examples in `10-spec-author.md`. My prompt named the shapes to cover but carried no intended fix, suspected cause, or hoped-for verdict. No production code touched, and no existing test changed.

All nine new tests are in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/SwitchSectionLineStartTests.cs` and use `AssertRuleResult`. That helper checks the exact output under LF and CRLF, runs a second pass that must leave the output unchanged, and runs the per-phase stability check.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentRunFromFirstDelimitedDocumentationCommentMovesWithLabel` (FMT-BREAK-034 new example, verbatim: `case 1: /* c */ /** d */ case 2:`, `case 3: /** a */ /** b */ case 4:`) | FMT-BREAK-033, FMT-BREAK-034; FC-002, FC-021 | AssertRuleResult | Passes on 6c5c3f3; fails on base `324fcd5` |
| `CommentRunFromFirstDelimitedDocumentationCommentMovesWithFirstStatement` (FMT-BREAK-035 new example, verbatim: `case 1: /* c */ /** d */ A();`, `case 2: /** d */ /* c */ A();`, multi-line `case 3: /** a⏎b */ A();`) | FMT-BREAK-033, FMT-BREAK-035; FC-002, FC-021 | AssertRuleResult | Passes; fails on base |
| `OrdinaryCommentStaysOnLabelLineWhileCommentsFromDocumentationCommentMove` (change-doc entry 6, verbatim, including `case 6: /* c */ /** d */ /* e */ A();`) | FMT-BREAK-033, FMT-BREAK-034, FMT-BREAK-035; FC-002 | AssertRuleResult | Passes; fails on base |
| `BracedSectionEndingInBreakKeepsTrailingCommentAndGetsOneBlankLine` (`break; /* c */ case 2:`, braced) | FMT-BREAK-033, FMT-BLANK-004; FC-019, FC-003 | AssertRuleResult | Passes; fails on base |
| `BracedSectionEndingInBreakKeepsTrailingMultiLineCommentAndGetsOneBlankLine` (`break; /* a⏎b */ case 2:`, braced) | FMT-BREAK-033, FMT-BLANK-004; FC-019, FC-003 | AssertRuleResult | Passes; also passes on base |
| `BracedSectionEndingInReturnKeepsTrailingCommentAndGetsNoBlankLine` (`return 1; /* c */ case 2:`, braced) | FMT-BREAK-033, FMT-BLANK-004; FC-019, FC-003 | AssertRuleResult | Passes; fails on base |
| `BracedSectionEndingInReturnKeepsTrailingMultiLineCommentAndGetsNoBlankLine` (`return 1; /* a⏎b */ case 2:`, braced) | FMT-BREAK-033, FMT-BLANK-004; FC-019, FC-003 | AssertRuleResult | Passes; also passes on base |
| `BracedSectionEndingInBreakKeepsTrailingCommentInFrontOfClosingBrace` (`break; /* c */ }`, braced) | FMT-BLANK-004; FC-019, FC-003, FC-010 | AssertRuleResult | Passes; fails on base |
| `BracedSectionEndingInBreakKeepsTrailingMultiLineCommentInFrontOfClosingBrace` (`break; /* a⏎b */ }`, braced) | FMT-BLANK-004; FC-019, FC-003, FC-010 | AssertRuleResult | Passes; also passes on base |

Where the expected values come from:

- **F2 tests.** The expected outputs are the spec and change-doc examples, copied verbatim from `sdd/specs/formatter/line-breaks.md` (FMT-BREAK-034 and FMT-BREAK-035) and from `sdd/changes/2026-11/switch-section-line-start.md` (entry 6).
- **F3 tests.** No spec example exists for these shapes, so I derived the expected outputs from the requirements:
  - A comment that trails the section's last token stays on that token's line (FMT-BREAK-001, as FMT-BREAK-033 cites it).
  - Exactly one blank line goes in front of a section behind `break`, and none behind `return` (FMT-BLANK-004).
  - No blank line goes in front of the switch's `}` (decision 6).
  - The inner line of a multi-line comment keeps its text and column. The spec examples for FMT-BREAK-033 and FMT-BREAK-035 show the same.
  - The braced layout is the one the existing `BracedSectionEndingIn*` tests pin.
- **Base results.** I took them by copying the test file into a temporary worktree of base `324fcd5` (since removed) and running the same filter. Eight tests failed there, the three multi-line-comment F3 tests passed, and the two earlier `BracedSectionEndingIn*` tests failed as before. I did not diff the base failures one by one. On head they serve as regression tests, not reproductions.

## Scenarios (bug fix)

_N/A_. This is a behavior change.

## Not covered, and why

- **A label behind a multi-line `/** a⏎b */` comment.** This is `case 3: /** a⏎b */ case 4:`. FMT-BREAK-034 states no example for it, and the multi-line arm is decided by the same rule in FMT-BREAK-033 that the first-statement test covers. Per `09-auditor.md` hint 1, the comment's inner line also meets the pre-existing RH5204 no-progress follow-up. I did not add it.
- **A section (FMT-BREAK-033) with a comment run behind a statement.** This is `return 1; /** d */ /* c */ case 2:`. The spec says FMT-DOC-001 relocates a `/** */` comment in front of a section's first label before this phase runs, and the existing `DelimitedDocumentationCommentInFrontOfSectionStaysInFrontOfTheLabel` pins that path. No new example was specified, so I did not add a test.
- **Nested braced sections with a trailing comment.** The guard reads only the last token's trailing trivia and the next token's leading trivia, and every F3 test exercises that through the same `SwitchCaseBraceRewriter.AddBraces` call. One test per gap shape (next section or `}`) and per comment kind (single-line or multi-line) covers both sides of the guard.
- **Failure classes not listed above.** They are unchanged from `04-test-implementer.md`. This round adds only comment variants of shapes already covered.

`scripts/format.sh` reformatted none of the 1 file. `scripts/test.sh --project architecture`: 92 passed. The requirement tags resolve.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~SwitchSectionLineStartTests.CommentRun|FullyQualifiedName~SwitchSectionLineStartTests.OrdinaryCommentStays|FullyQualifiedName~SwitchSectionLineStartTests.BracedSectionEndingIn"
```

On 6c5c3f3 this filter runs 11 tests (the 9 new ones and the 2 existing `BracedSectionEndingIn*` tests), and all 11 pass.
