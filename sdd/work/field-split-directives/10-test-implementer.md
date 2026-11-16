Role: test implementer
Commit: ec05997
Verdict: tests written

Repair round 1, auditor finding #1 (#1728): the reason a refused input gives when FMT-UNSUP-010 starts at the same directive as another shape had no covering test. The prompt carried no intended fix, suspected cause, or hoped-for verdict; the auditor's suggested change was treated as a suggestion.

Expected values come from the spec, `sdd/specs/formatter/unsupported-syntax.md`:
- FMT-UNSUP-010, last paragraph before the reason: at a declarator comma FMT-UNSUP-009's reason and location win; "Every other directive in the declaration gives this requirement's reason" — so at a non-declarator comma (FMT-UNSUP-009) and behind `?`/`:` (FMT-UNSUP-008), FMT-UNSUP-010's reason and location.
- The other side of the condition "more than one declarator": in a field with one declarator FMT-UNSUP-010 does not apply, so the same directive gives FMT-UNSUP-009's or FMT-UNSUP-008's reason.

## Tests
All in `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedFieldDeclarationDirectiveTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DirectiveInFrontOfAnotherCommaOnALaterLineGivesTheReasonOfThisRequirement` (rows: attribute argument list, tuple type, call in a value, conditional group in a value) | FMT-UNSUP-010, FMT-UNSUP-009 | `AssertUnsupportedFor` (LF and CRLF, unchanged text, reason, span) | pass |
| `DirectiveBetweenAConditionalOperatorAndItsBranchGivesTheReasonOfThisRequirement` (rows: behind `?`, behind `:`, conditional group behind `?`) | FMT-UNSUP-010, FMT-UNSUP-008 | `AssertUnsupportedFor` | pass |
| `DirectiveAtTheSamePlaceOfAFieldWithOneDeclaratorGivesTheReasonOfTheOtherShape` (rows: attribute argument list and call in a value → FMT-UNSUP-009; behind `?` and behind `:` → FMT-UNSUP-008) | FMT-UNSUP-010, FMT-UNSUP-009, FMT-UNSUP-008 | `AssertUnsupportedFor` | pass |

The class summary now names the ties with FMT-UNSUP-009 and FMT-UNSUP-008.

Fail check: with the non-declarator candidate list in `PrecheckPhase.cs` temporarily reordered so that `firstFieldDirective` comes last (local mutation, reverted with `git checkout`, never committed), all seven rows of the first two tests failed at `Assert.AreEqual(reason, result.Unsupported.Reason)`; the one-declarator rows stayed green, as they should. After the revert, all 328 precheck tests passed.

## Scenarios (bug fix)
Not a bug fix.

## Not covered, and why
- A tie of FMT-UNSUP-010 with FMT-UNSUP-004: none exists. FMT-UNSUP-004 refuses comments only, and FMT-UNSUP-010 refuses directives and disabled text only, so they never start at the same trivia. `FirstOfACommentAndADirectiveInTheDeclarationIsReported` already covers their order by position.
- The declarator-comma tie (FMT-UNSUP-009 wins) is covered by the existing tests `DirectiveInFrontOfADeclaratorCommaOnALaterLineKeepsTheReasonOfAListSeparator`, `ThirdDeclaratorInAConditionalGroupIsUnsupportedInEveryBuild`, and `DeclaratorInDisabledTextDoesNotCountAndAtTheCommaTheReasonOfAListSeparatorWins`.
- The CLI report line for a tie: the CLI prints the reason `FindUnsupportedSyntax` and the precheck return. Its tests already pin the line for FMT-UNSUP-010; the reason is decided in the precheck and tested there.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck.UnsupportedFieldDeclarationDirectiveTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"
```

## Follow-up candidates
_None._
