Role: test implementer
Commit: a55d07e
Verdict: tests written

Repair round 2, audit finding #3 (`18-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values are derived from FMT-BREAK-037 (the multi-line-comment paragraph: a line break inside a `/* … */` text separates no tokens; a wrapped clause becomes one constraint per line, the comma at the end of the line of the constraint in front of it) and FMT-INDENT-032 (later constraints align to the first constraint's column). The auditor's observed head outputs agree with them; they were not the source. No existing assertion was changed. The three tests are in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConstraintClauseLineBreakTests.cs` and use `AssertRuleResult(input, expected)`, which runs LF and CRLF, a second pass, and the phase stability check.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ColonBehindMultiLineCommentClosingLineInWrappedClauseAlignsUnderFirstConstraint` (wrapped `M`: `where T /* a⏎ b */ : class,⏎ new()`) | FMT-BREAK-037, FMT-INDENT-032 | `AssertRuleResult(input, expected)` | passes: colon stays behind `b */`, `new()` at column 19 under `class` |
| `FirstConstraintBehindMultiLineCommentClosingLineInWrappedClauseAlignsLaterConstraints` (wrapped `N`: `where T : /* a⏎ b */ class,⏎ new()`) | FMT-BREAK-037, FMT-INDENT-032 | `AssertRuleResult(input, expected)` | passes: `class` stays behind `b */`, `new()` at column 17 |
| `CommaBehindMultiLineCommentClosingLineInWrappedClauseIsFollowedByLineBreak` (wrapped `O`: `where T : class /* a⏎ b */, new(),⏎ IDisposable`) | FMT-BREAK-037, FMT-INDENT-032 | `AssertRuleResult(input, expected)` | passes: `b */,⏎ new(),⏎ IDisposable`, both at column 18 |

In each input the later constraint starts at column 12 (the block column), so the alignment is exercised, not only kept.

### Watched fail

These tests cover behavior the head already implements, so they pass on it. To show the wrapped-`O` test pins the guard the auditor named, I reverted only the second condition of `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` (line 232) from `SyntaxTriviaUtilities.AreSeparatedByEndOfLine(...)` to `TokenGapUtilities.HasLineBreakBetween(...)` locally and ran the class: exactly `CommaBehindMultiLineCommentClosingLineInWrappedClauseIsFollowedByLineBreak` failed (47/48 passed). The production file was restored from a copy afterwards; `git diff src/Reihitsu.Formatter` is empty and the commit touches no production code.

The wrapped `M` and `N` tests pin the alignment under a first constraint that stands on a comment's closing line (FMT-INDENT-032); no single-condition revert of the rewriter changes their output (the clause is wrapped by the line break behind the comma either way), so no mutation was run for them.

## Not covered, and why

- Optional FMT-BREAK-037 example for wrapped `O` (auditor's suggestion): a spec change, not this role's.
- RH5110 code-fix path: the fix delegates to the formatter (RH5110-003); the auditor ran `apply-fix.sh RH5110` over `S`, `P`, `N`, `M` and the existing `VerifyFix*` tests check fix-equals-formatter.
- "Behind two commas", "inside a constraint outside `<>`", "comment and line break behind a comma": the auditor's table records them as covered by the `S` path or the existing comment-gap tests.
- Directive and single-line-comment variants: covered by existing tests (`14-test-implementer.md`).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ConstraintClauseLineBreakTests"
```

Result with these tests: 48 total, 48 passed.
