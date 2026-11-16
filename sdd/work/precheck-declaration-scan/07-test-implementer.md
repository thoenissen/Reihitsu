Role: test implementer
Commit: 69d677b (tests run on this head with the test change below; production code untouched)
Verdict: tests written

Repair round 1, auditor finding #1. The prompt carried no intended fix, suspected cause, or hoped-for verdict; the auditor's suggested input was re-derived rather than taken as given (below).

## Tests

In `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedVariableDeclarationTests.cs`; the helper runs LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentOnItsOwnLineInFrontOfTheFirstTokenOfALeftNestedValueIsUnsupported` (renamed from `CommentInFrontOfTheFirstTokenOfALeftNestedValueIsUnsupported`) | FMT-UNSUP-004 | `AssertUnsupported(input, "// c")` | pass |

Re-derivation: the old input `string s = /* c */ "x" + ...;` put the comment in the trailing trivia of `=`, a token outside every value, so the verdict never depended on the value's first token. For the comment to be the leading trivia of `"x"` (the deep-left first token of `(("x" + "p0") + "p1") + "p2"`), the line break must come behind `=`, which ends `=`'s trailing trivia; the comment then starts the next line:

```csharp
string s =
    // c
    "x" + "p0" + "p1" + "p2";
```

FMT-UNSUP-004 demands the refusal ("a comment in front of the value, also on its own line"), at `// c`. The doc comment now states that the comment is the leading trivia of the value's first token.

Sensitivity, checked by a temporary edit of `PrecheckPhase.cs` (`FirstToken: default(SyntaxToken)`), reverted with `git checkout` (not committed): the rewritten test fails, together with `CommentOnItsOwnLineInFrontOfAValueIsUnsupported` and `CommentOnItsOwnLineInFrontOfALaterValueOfSeveralTokensIsUnsupported` — 3 of 82 in the class. The old version passed under the same mutation (auditor).

Existing tests asserting a changed output: none; no output changes.

## Scenarios (bug fix)

_None._ Time-only change.

## Not covered, and why

- The auditor's hint on `CommentInFrontOfTheValueOfADeclarationInsideALaterValueIsUnsupported` (comment in the inner `=`'s trailing trivia): left unchanged. Its stated invariant is FC-039 (the inner declaration is checked with its own span), which it falsifies; the first-token side is pinned by the rewritten test and the two own-line tests above, all through the same value loop.
- Everything else as in `02-test-implementer.md`.

## Commands

```shell
scripts/test.sh --no-install --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"
```

Result: `Passed! - Failed: 0, Passed: 249`.

## Follow-up candidates

_None._
