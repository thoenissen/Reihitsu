Role: test implementer
Commit: d15c8a3
Verdict: tests written

Repair round 1, audit finding #1 (`12-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-BREAK-037 as rewritten in 97d59bf (its new example `M`, `N`, `O`, `P`, `S`, and its normative "line break" paragraph) and change-doc entry 16. No existing assertion was changed. All six tests are in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ConstraintClauseLineBreakTests.cs` and use `AssertRuleResult`, which runs LF and CRLF, a second pass, and `PhaseStabilityCheck`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ColonBehindMultiLineCommentClosingLineDoesNotWrapClause` (example `M`, audit (a)) | FMT-BREAK-037 | `AssertRuleResult(input)` | fails: head splits `new()` onto its own line |
| `FirstConstraintBehindMultiLineCommentClosingLineDoesNotWrapClause` (example `N`, audit (b)) | FMT-BREAK-037 | `AssertRuleResult(input)` | fails: head splits `new()` onto its own line |
| `CommaBehindMultiLineCommentClosingLineDoesNotWrapClause` (example `O`, audit (c)) | FMT-BREAK-037 | `AssertRuleResult(input)` | passes (head already leaves it unchanged; guards the other side) |
| `ConstraintBehindMultiLineCommentClosingLineDoesNotWrapClause` (example `P`, audit (d)) | FMT-BREAK-037 | `AssertRuleResult(input)` | fails: head moves `IDisposable` to its own line, `new()` stays behind the comment |
| `ConstraintBehindMultiLineCommentClosingLineStartsLineInWrappedClause` (example `S`, entry 16) | FMT-BREAK-037, FMT-INDENT-032 | `AssertRuleResult(input, expected)` | fails: head leaves `new()` behind `b */` instead of starting a line at column 18 |
| `TypeArgumentBehindMultiLineCommentClosingLineDoesNotWrapClause` (audit (e), several constraints) | FMT-BREAK-037 | `AssertRuleResult(input)` | fails: head wraps the clause one constraint per line — see the note below |

Each failing test was checked against `scripts/format.sh` on the head: the difference is exactly the split described, nothing unrelated.

### Note for the lead: case (e) expected value

`13-spec-author.md` lists (e) `IFoo</* a⏎ b */ T>` as "unchanged by this round: … the constraint spans two lines and wraps the clause (Q5)". FMT-BREAK-037's normative text as committed in 97d59bf says the opposite: "two tokens that only such a comment puts on different lines of the text are not separated by a line break, for the joins, for whether the clause is wrapped …", and the wrapped condition is "a line break separates two tokens of its constraints … inside one constraint". In `where T : class, IEquatable</* a⏎ b */T>, new()` only the multi-line comment puts `<` and `T` on different lines, so by the requirement text the clause is not wrapped and stays as written. I wrote the test from the requirement text (unchanged). The head wraps it (`class,⏎ IEquatable</* a⏎ b */T>,⏎ new()`), as it did when the auditor recorded (e) as "holds" under the old wording. The handover's row and the requirement disagree; if the lead takes the handover's reading, this test's expected value changes and FMT-BREAK-037's wording needs an exception for type argument lists. The input writes `*/T` without a space to stay clear of the pre-existing spacing quirk the auditor noted (`*/ T` loses its space on `main`).

## Not covered, and why

- Single-line-comment variants of each gap: already covered by existing tests (`12-auditor.md`: "Every single-line-comment cell holds and is tested").
- Directive or disabled text in these gaps: unchanged by this round; existing tests cover them.
- RH5110 code-fix path: the fix delegates to the formatter (RH5110-003); `S` reaches it only through the same rewriter, and the existing `VerifyFix*` tests already check fix-equals-formatter. Not repeated per case.
- Parameter-list counterpart (`void N(int a, /* x⏎ y */ int b, int c)`): not this run's code; cited in the spec's `Why` only.
- Finding #2 (shared join loop): internal refactor with no output change; existing tests that exercise both rewriters cover it.
- Wrapped side of (e) with a single-line comment: covered by the existing Q5 tests (`IDictionary<int, // c⏎ string>`).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ConstraintClauseLineBreakTests"
```

Result on d15c8a3 with these tests: 45 total, 40 passed, 5 failed (the five listed as failing above).
