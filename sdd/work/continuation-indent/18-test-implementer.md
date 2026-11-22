Role: test implementer
Commit: 037d450
Verdict: tests written

Repair round 1, spec checker 16, finding 2 (check 8). The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test file changed.

Decision: keep the tag. FMT-UNSUP-013's first condition states both sides: the `=>` of a switch-expression arm belongs to no expression body, and `1 /* c */ => "one"` does not refuse the input under FMT-UNSUP-013; FMT-UNSUP-017 refuses it. The test asserts exactly that other side: the input is refused with `SwitchArmArrowCommentReason` ("a comment or directive in front of the => of a switch arm"), not with `ExpressionBodyArrowLeadingCommentReason` (FMT-UNSUP-013's reason), at the span `/* c */`. It is derived from a condition FMT-UNSUP-013 states, so it carries `[Requirement("FMT-UNSUP-013")]` next to `[Requirement("FMT-UNSUP-017")]`. Handover 07 named the test only in prose (its line 50); this table records it.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedExpressionBodyArrowLeadingCommentTests.CommentInFrontOfTheArrowOfASwitchArmGivesTheReasonOfTheArm` | FMT-UNSUP-013, FMT-UNSUP-017 | `AssertUnsupportedFor` | passes (ran green on 037d450) |

## Scenarios (bug fix)
_None._ (repair round; no reproduction)

## Not covered, and why
_None._ The finding asks only for the table row.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedExpressionBodyArrowLeadingCommentTests.CommentInFrontOfTheArrowOfASwitchArmGivesTheReasonOfTheArm"
```

## Follow-up candidates
_None._
