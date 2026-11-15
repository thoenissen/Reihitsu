Role: test implementer
Commit: 4f66948 (on top of b223706, the spec author's repair commit)
Verdict: tests written

Repair round 1, for auditor findings #1–#3 (`10-auditor.md`; `00-lead.md` decisions 15–17). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All 18 tests are new and live in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, right before `#endregion // Tests`. I changed no existing test.

All tests are tagged `RH2001-011`. The spec author's b223706 states all three classes there: the cref in paragraph 1, inferred names in paragraph 3 (under SHARED-FIX-003, like the block-body lambda paragraph, whose tests carry only `RH2001-011`), and the receiver `Value._value` in paragraph 4. The tests cover every new example in that commit: `pair = new { Value }`, `pair = (Value, 1)`, the expression-bodied `Describe() => new { Value }.ToString()`, the combined explicit-name example, `Value._value`, and the cref example verbatim.

"`main`" is 4838e9a, run in a temporary worktree that has since been removed. I measured outputs with `scripts/apply-fix.sh RH2001` under LF and CRLF, and both gave identical results. Fixtures are in `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r12`. I also ran the test class against `main`'s production code.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName` (a) | RH2001-011; FC-006, FC-036 | `VerifyNoCodeFixIsOffered` | red on head (action registered); red on `main` |
| `VerifyNoFixIsOfferedWhenAThisQualifiedPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName` (b) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyNoFixIsOfferedWhenAPropertyReferenceThroughAnotherInstanceGivesAnAnonymousObjectMemberItsInferredName` (c) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAMemberOfAnAnonymousObjectInAQueryProjectionItsInferredName` (d) | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesATupleElementItsInferredName` (e) | RH2001-011 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredNameThatToStringPrints` (f, block body, no read by name) | RH2001-011; FC-036 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyNoFixIsOfferedWhenAPropertyReferenceInAnExpressionBodyGivesAnAnonymousObjectMemberItsInferredName` (f, spec example, expression body) | RH2001-011; FC-027 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedAnonymousObjectMember` | RH2001-011; FC-003 | `Verify` | green on head and `main` |
| `VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedTupleElement` | RH2001-011; FC-003 | `Verify` | green on head and `main` |
| `VerifyFixIsOfferedWhenPropertyReferencesAreTheValuesOfAnExplicitlyNamedAnonymousObjectMemberAndTupleElement` (spec example) | RH2001-011; FC-003 | `Verify` | green on head and `main` |
| `VerifyFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheField` (spec example) | RH2001-011; FC-001, FC-002 | `Verify` | red on head (no action, so the expected text is not produced); green on `main` |
| `VerifyFixIsOfferedWhenTheThisQualifiedPropertyIsTheReceiverOfAMemberNamedLikeTheField` (r2) | RH2001-011; FC-016 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenTheNullForgivingPropertyIsTheReceiverOfAMemberNamedLikeTheField` (r1) | RH2001-011; FC-016 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenTheCastPropertyIsTheReceiverOfAMemberNamedLikeTheField` (r4) | RH2001-011; FC-016 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsTheConditionalReceiverOfAMemberNamedLikeTheField` (`Value?._value`) | RH2001-011; FC-003 | `Verify` | green on head and `main` |
| `VerifyNoFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheField` (spec example) | RH2001-011; FC-001 | `VerifyNoCodeFixIsOffered` | red on head; red on `main` |
| `VerifyFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheFieldThroughItsType` (`cref="Base._value"`) | RH2001-011; FC-003 | `Verify` | green on head and `main` |
| `VerifyFixRenamesADocumentationCommentReferenceToTheProperty` (`cref="Value"` → `cref="_value"`) | RH2001-011; FC-003 | `Verify` | green on head and `main` |

Run on head 68612fc + b223706: 231 tests, 12 red. The red ones are 7 withheld tests for #1, 4 receiver tests for #2, and 1 cref test for #3. All 12 fail on the expected difference:
- each withheld test fails at `Assert.IsEmpty(actions)`;
- each receiver test fails because the iterative fix leaves `Test0.cs` unchanged.

Run on `main`: the 8 new withheld tests fail, because `main` offers the fix, and the 10 new offered tests pass. The failure on `main` is confirmed for every withheld test.

## Input and `main` output

| Test | Input (changed line) | `main` output |
|---|---|---|
| a | `var pair = new { Value }; return pair.Value;` | `new { _value }`; `pair.Value` is CS1061 |
| b | `new { this.Value }` | `new { this._value }` |
| c | `new { other.Value }` (`Get(Sample other)`) | `new { other._value }` |
| d | `items.Select(item => new { Value, item }).Select(pair => pair.Value + pair.item)` | `new { _value, item }` |
| e | `var pair = (Value, 1); return pair.Value;` | `(_value, 1)` |
| f | `return new { Value }.ToString();` / `string Describe() => new { Value }.ToString();` | `new { _value }.ToString()` |
| explicit names | `new { Value = Value }`, `(Value: Value, 1)` | `new { Value = _value }`, `(Value: _value, 1)` (asserted) |
| receiver | nested `Inner` in `Outer` (`private int _value;`): `return Value._value;` / `this.Value._value` / `Value!._value` (`#nullable enable`, `Outer?`) / `((Outer)Value)._value` (`object`) / `Value?._value` (`int?`) | `_value._value` / `this._value._value` / `_value!._value` / `((Outer)_value)._value` / `_value?._value` (asserted) |
| cref, inherited | `/// Reads <see cref="_value"/>.` with `Base._value` | cref left as written; it then names `Sample._value` |
| cref, qualified | `<see cref="Base._value"/>` | cref unchanged, `return _value;` (asserted) |
| cref, property | `<see cref="Value"/>` | `<see cref="_value"/>` (asserted) |

## Not covered, and why

- **r3 `Value._value()` calling an outer method named `_value`, and r5 an `internal int _value` of another type.** These reach the same receiver description as the tested forms. r3 names a method in `_camelCase`, and r5 breaks RH4108, so both are rare by `AGENTS.md`. The four receiver shapes I tested already cover the qualifier switch's member-access, null-forgiving, and parenthesized/cast receivers, plus `this.`.
- **Fix All with an inferred-name projection next to fixable properties.** The guard is evaluated per diagnostic, and the existing `VerifyFixAllLeavesAPropertyWhoseFieldNameIsBoundAndReplacesTheOthers` already pins the withheld-in-Fix-All path for RH2001-011.
- **A named argument `F(Value: Value)` or a `with` expression.** These have no inferred member name, so the rename changes nothing beyond the reference. They are outside the condition RH2001-011 paragraph 3 states.
- **CRLF variants.** The guard is a binding and syntax check with no layout involved, and apply-fix gave identical LF and CRLF results for every fixture.
- **Static property in an inferred projection** (`new { Value }` in a static method): the reference is spelled the same way and goes through the same declarator. One shape is enough, and the `this.` and `other.` forms already vary the reference expression.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ItsInferredName|FullyQualifiedName~ExplicitlyNamed|FullyQualifiedName~ReceiverOfAMemberNamedLikeTheField|FullyQualifiedName~DocumentationCommentReference"
```
