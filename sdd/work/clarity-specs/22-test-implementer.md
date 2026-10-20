Role: test implementer
Commit: e1c9e52012ef624efeee147a679e968065b54553
Verdict: tests written

Second repair round (`squad/decisions/2026-10-20-997-second-repair-round.md`). It covers findings 5 and 7 of `20-auditor.md` and the characterization tests listed in `21-spec-author.md`. No production file and no spec changed. No existing assertion changed. The prompt carried the task only: no intended fix, suspected cause, or hoped-for verdict.

Every new test passes on the unchanged code. No spec statement turned out false.

## Tests

All files are under `src/Reihitsu.Analyzer.Test/Clarity/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3002…Tests.ParenthesesAroundAnArgumentOfAnObjectCreationAreReported` | RH3002-001, RH3002-007 (finding 5) | `Verify(source, fixed)` | `new C({\|RH3002:(a)\|})` → `new C(a)`: pass |
| `RH3002…Tests.ParenthesesAroundAnArgumentOfAnElementAccessAreReported` | RH3002-001, RH3002-007 (finding 5) | `Verify(source, fixed)` | `x[{\|RH3002:(a)\|}]` → `x[a]`: pass |
| `RH3002…Tests.ParenthesesAroundTheValueOfAMemberDeclarationAreReported` | RH3002-001, RH3002-007 (finding 5) | `Verify(source, fixed)` | `int _f = (1);` → `int _f = 1;` and `int P { get; } = (2);` → `= 2;`: pass |
| `RH3103…Tests.DottedNullableGenericAsNameofArgumentIsReportedOnTheGenericName` | untagged, Q-RH3103 input 1 (thoenissen/Reihitsu#1231) | analyzer-only `Verify(source)` | `nameof(System.{\|RH3103:Nullable<int>\|})` is reported on `Nullable<int>` alone. The fix is not applied. Pass |
| `RH3103…Tests.GlobalDottedNullableGenericAsNameofArgumentIsReportedOnTheGenericName` | untagged, Q-RH3103 input 2 | analyzer-only `Verify(source)` | `nameof(global::System.{\|RH3103:Nullable<int>\|})`, same as above: pass |
| `RH3005…Tests.FixOfConstantLeftArgumentBeforeAGreaterThanArgumentWritesAGenericReading` | untagged, Q-RH3005 input 1 (thoenissen/Reihitsu#1227) | `ApplyCodeFixAsync` + exact text | `M(Max > G, B > (7));` → `M(G < Max, B > (7));`: pass |
| `RH3005…Tests.FixOfStaticReadonlyLeftElementBeforeAGreaterThanElementWritesAGenericReading` | untagged, Q-RH3005 input 2 | `ApplyCodeFixAsync` | `new[] { Min > G, B > (7) }` → `new[] { G < Min, B > (7) }`: pass |
| `RH3005…Tests.FixOfLiteralLeftArgumentAfterALessThanArgumentWritesAGenericReading` | untagged, Q-RH3005 input 3 | `ApplyCodeFixAsync` | `M(G < A, (7) < B);` → `M(G < A, B > (7));`: pass |
| `RH3001…Tests.FixOfNegationAfterGreaterThanInAnArgumentWritesAGenericReading` | untagged, Q-RH3001-1 input 1 (thoenissen/Reihitsu#1245) | `ApplyCodeFixAsync` | `M(G < A, B > !x);` → `M(G < A, B > (x == false));`: pass |
| `RH3001…Tests.FixOfNegationAfterGreaterThanInAComparisonChainWritesAGenericReading` | untagged, Q-RH3001-1 input 2 | `ApplyCodeFixAsync` | `return G < A > !x;` → `return G < A > (x == false);`: pass |
| `RH3001…Tests.NegationAsAWholeArgumentIsReportedWithoutACodeFix` | untagged, Q-RH3001-2 | `VerifyNoCodeFixIsOffered` | `M({\|RH3001:!\|}x)` is reported, and no action is offered: pass |
| `RH3001…Tests.NegationAsAWholeTupleElementIsReportedWithoutACodeFix` | untagged, Q-RH3001-2 | `VerifyNoCodeFixIsOffered` | `({\|RH3001:!\|}x, y)` is reported, and no action is offered: pass |
| `RH3105…Tests.FixOfQualifiedOperandAfterLessThanBeforeAGreaterThanArgumentWritesAGenericReading` | untagged, Q-RH3105 input 1 (thoenissen/Reihitsu#1233) | `ApplyCodeFixAsync` | `M(A < this.B, D > (7));` → `M(A < B, D > (7));`: pass |
| `RH3105…Tests.FixOfQualifiedOperandBeforeGreaterThanAfterALessThanArgumentWritesAGenericReading` | untagged, Q-RH3105 input 2 | `ApplyCodeFixAsync` | `M(A < B, this.D > (7));` → `M(A < B, D > (7));`: pass |
| `RH3105…Tests.FixOfQualifiedArgumentBetweenLessThanAndGreaterThanArgumentsWritesAGenericReading` | untagged, Q-RH3105 input 3 | `ApplyCodeFixAsync` | `M(A < B, this.X, D > (7));` → `M(A < B, X, D > (7));`: pass |
| `RH3105…Tests.FixOfQualifiedOperandInAComparisonChainWritesAGenericReading` | untagged, Q-RH3105 input 4 | `ApplyCodeFixAsync` | `return A < this.B > (7);` → `return A < B > (7);`: pass |
| `RH3101…Tests.FixOfBaseOperandAfterLessThanBeforeAGreaterThanArgumentWritesAGenericReading` | untagged, Q-RH3101 input 1 (thoenissen/Reihitsu#1229) | `ApplyCodeFixAsync` | `M(A < base.B, D > (7));` → `M(A < B, D > (7));`: pass |
| `RH3101…Tests.FixOfBaseArgumentBetweenLessThanAndGreaterThanArgumentsWritesAGenericReading` | untagged, Q-RH3101 input 2 | `ApplyCodeFixAsync` | `M(A < B, base.X, D > (7));` → `M(A < B, X, D > (7));`: pass |

**Finding 7.** The summary of `RH3103…Tests.NullableGenericInEveryListedTypePositionIsReportedOverTheWholeName` now reads "unqualified `Nullable<T>`, and `Nullable<T>` qualified with dots (also behind `global::`)". "Alias-qualified" is gone. The tags (RH3103-001, RH3103-003) and the assertion are unchanged.

**Finding 5.** The existing `ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition` is unchanged. It stays a copy of the spec example, and together with the three new tests every position RH3002-001 lists is covered.

**Inputs and expected text.** The inputs are the spec author's one-line fixtures laid out over several lines. The expected changed line is the "Fixed text" quoted in `21-spec-author.md`. The rest of each document is expected to stay byte-identical, and it does.

**Mutation check.** Each assertion was mutated once to confirm it can fail, then restored:

- The RH3103 markup was widened to `{|RH3103:System.Nullable<int>|}`.
- The RH3005 expected line was set back to `M(Max > G, B > (7));`.

Both runs failed on the mutated assertion.

### Existing tags against the narrowed requirements

I extracted each tagged method body by script and checked it against the narrowed wording:

- **RH3001-004 (5 tests).** No negation is a whole argument, a tuple element, or the right operand of `>`. The inputs are `return !…`, `var x = !a && b`, `b == !a`, and `!(a && b)`. Fits.
- **RH3005-004 (12 tests).** Every expression holds a single comparison. Fits.
- **RH3101-004 (7 tests) and RH3105-004 (8 tests).** No `<` or `>` operator appears in any tagged body. Fits.
- **RH3103-002 (2 tests).** Both use the unqualified `nameof(Nullable<int>)`. Fits.
- **RH3103-003 (12 tests).**
  - None uses a name qualified by an alias alone.
  - The only `::` is `global::System.Nullable<long>`, which is a dotted qualified name.
  - The only `nameof` is a type argument inside `nameof(List<…>)`, a position of RH3103-001.
  - Fits.
- **`AliasQualifiedNullableGenericInTypeofIsReportedAndFixed`.** It is tagged RH3103-001 and -004 only, not -003. Both requirements name the alias-only form. Fits.

## Not covered, and why

- **Q-RH3001-2's other inputs** (`new C(!y)`, `this[!y]`, `[A(!true)]`). The spec author asked for "at least" `M(!x)` and `(!x, y)`. `21-spec-author.md` attributes all five inputs to the same `FindNode` tie, which I did not re-derive. The other three are left out as further instances of it.
- **The RH3103 dotted `nameof` fix.** It is deliberately not applied: it throws `InvalidCastException`, per `21-spec-author.md`. The tests are analyzer-only.
- **CRLF variants.** No test here pins layout. The fixes splice tokens inside one line, and the assertions compare whole-document text that the fix leaves otherwise untouched. Line-ending handling of these fixes is outside the questions being characterized.
- **RH3002 argument positions.** All three argument kinds reach one arm, `ArgumentSyntax => true`, in `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.IsUnnecessary`. Both member `=` values reach `EqualsValueClauseSyntax => true`. A test per position was still added, because the finding asks for the listed clauses to be pinned.
- **Enum member values** (`A = (1)`) were not added. RH3002-001 says "member declaration", and that clause is covered by the field and property cases. An enum member also reaches the same `EqualsValueClauseSyntax` arm.
- **Failure classes.** No production code or rewrite changed. The catalog entries that can reach a fix (FC-006, FC-016, FC-027: a rewrite creating a generic reading) are exactly what these characterization tests pin. The others are as `20-auditor.md` dismissed them.

## Commands

```shell
scripts/format.sh src/Reihitsu.Analyzer.Test/Clarity/RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3005UseReadableConditionsAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3103UseShorthandForNullableTypesAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests.cs
# Formatted 0 of 6 file(s).

scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperator|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3002StatementMust|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3005UseReadable|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3101DoNotPrefix|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3103UseShorthand|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3105DoNotPrefix"
# Passed: 362, Failed: 0

scripts/test.sh --project analyzer --filter "Name~WritesAGenericReading|Name~AsNameofArgumentIsReportedOnTheGenericName|Name=NegationAsAWholeArgumentIsReportedWithoutACodeFix|Name=NegationAsAWholeTupleElementIsReportedWithoutACodeFix|Name=ParenthesesAroundAnArgumentOfAnObjectCreationAreReported|Name=ParenthesesAroundAnArgumentOfAnElementAccessAreReported|Name=ParenthesesAroundTheValueOfAMemberDeclarationAreReported"
# Passed: 18, Failed: 0 (the new tests only)

scripts/test.sh --project architecture
# Passed: 72, Failed: 0
```
