Role: test implementer
Commit: 7c223e0
Verdict: tests written

Follow-up round for `13-spec-author.md`, *Examples for the tests* (RH2001-009's new example, property `Third`). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests.VerifyNoFixIsOfferedForADirectiveBetweenTheLastModifierAndTheTypeWithoutReadonly` (LF, CRLF) | RH2001-009 | `VerifyNoCodeFixIsOffered` | Pass on this head |

Input is the example's `Third`: `private static` / `#pragma warning disable CS0169` / `int Third { get; set; }` — no reorder, no `readonly` added. The test passes on this head, where the implementer's change already withholds the fix; per `13-spec-author.md`, `origin/main` offered a fix giving `int _value;` for this shape, so the test pins the changed behavior. I did not re-run it on `origin/main`.

## Scenarios (bug fix)
_Not a bug-fix round._

## Not covered, and why
- `First` and `Second` of the same example: already pinned by `VerifyNoFixIsOfferedForADirectiveThatAReorderedModifierWouldCross` and `VerifyNoFixIsOfferedForADirectiveBetweenModifiersThatKeepTheirOrder` (13-spec-author.md lines 1167 and 1262 of the test file).
- No existing test pinned `Third`: `rg '#pragma|#nullable|#region|#line|#warning'` over the RH2001 test files finds directives only above the property, between modifiers, between type and name, behind the name, and before the semicolon.
- FMT-BLANK-018: already pinned by `GluedCommentBlankLineTests.DecidesBlankLinesAboveGluedCommentAsDirectlyBeforeItsToken` (13-spec-author.md).

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001"
```
Result: 289 passed, 0 failed.

## Follow-up candidates
_None._
