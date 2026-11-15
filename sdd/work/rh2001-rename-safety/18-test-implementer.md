Role: test implementer
Commit: (this commit, on 3593829)
Verdict: tests written

Repair round 2, for auditor findings #4 and #5 (`16-auditor.md`; `00-lead.md` decisions 19 and 20). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All 10 tests are new and sit in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, right before `#endregion // Tests`. Each is tagged `RH2001-011` only. I changed no existing test. No existing test asserts an offered fix for g2, g5, or q1, and none asserts a withheld fix for t1–t6. I checked with `rg` for `class X<`, `var _value`, `Deconstruct`, `) = (`, `(int, int)`, and `other.Value` in the test class.

"`main`" is 4838e9a (`origin/main`). I ran it in a temporary worktree, which has since been removed. Fixtures: `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r18`. `main` outputs from `scripts/apply-fix.sh RH2001`: `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r18-main.txt`. All 10 fixtures were fixed on `main`, with identical results under LF and CRLF. I also ran the new tests against `main`'s production code.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoFixIsOfferedWhenThePropertyIsReadThroughAnotherConstructionOfItsGenericType` (g2) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` (offers `other._value + _value`) |
| `VerifyNoFixIsOfferedWhenThePropertyIsAssignedInAnObjectInitializerOfAnotherConstructionOfItsGenericType` (g5) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` (offers `{ _value = _value }`) |
| `VerifyNoFixIsOfferedWhenThePropertyReadThroughAnotherConstructionOfItsGenericTypeGivesAnAnonymousObjectMemberItsInferredName` (g1) | RH2001-011; FC-006, FC-036 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` (offers `new { other._value }`, then `pair.Value` is CS1061) |
| `VerifyNoFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheFieldNextToALocalNamedLikeTheField` (q1) | RH2001-011; FC-016 | `VerifyNoCodeFixIsOffered` | green on head; red on `main` (offers `this._value._value + _value`) |
| `VerifyFixIsOfferedWhenAConstructorDeconstructsItsParametersIntoTheProperties` (t1, two diagnostics, get-only, so `readonly` fields) | RH2001-011; FC-001, FC-003 | `Verify` | red on head (no action); green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleReturnedAsAnUnnamedTupleType` (t2) | RH2001-011; FC-003 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleAssignedToAnExplicitlyTypedTupleLocal` (t3) | RH2001-011; FC-003 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATuplePassedAsAnUnnamedTupleArgument` (t4) | RH2001-011; FC-003 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsADeconstructionTarget` (t5) | RH2001-011; FC-003 | `Verify` | red on head; green on `main` |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfTheSourceOfADeconstruction` (t6) | RH2001-011; FC-003 | `Verify` | red on head; green on `main` |

Head 3593829 with these tests: 241 tests, 6 red, namely t1–t6. Each fails in "Iterative code fix application" because `Test0.cs` stays unchanged. That is the expected difference: no action is registered. The four withheld tests are green on head. Decision 19 keeps them withheld, so the implementer must keep them green. They are red on `main`.

The `main` outputs I assert are these lines, taken verbatim from `r18-main.txt`:
- t1: `internal Sample(int value, string name) => (_value, _name) = (value, name);`, `private readonly int _value;`, `private readonly string _name;`, and `return _name + _value;`
- t2: `return (_value, 1);`
- t3: `(int First, int Second) pair = (_value, 1);`
- t4: `return Use((_value, 1));`
- t5: `(_value, other) = pair;` and `return _value;`
- t6: `(value, count) = (_value, 1);`

`var t = (Value, 1);` read as `t.Value` is already covered. `VerifyNoFixIsOfferedWhenAPropertyReferenceGivesATupleElementItsInferredName` (12, e) uses `var pair = (Value, 1); return pair.Value;`. It is green on head and red on `main`. I added no duplicate. It is the withheld side of the t2–t6 boundary (FC-003).

g1 goes beyond the worklist. The auditor named it as the dependent member that a narrowing of the comparison alone would expose (`16-auditor.md` #4). It is pinned so that a later `OriginalDefinition` change cannot ship CS1061 silently.

## Not covered, and why

- **g3 `(other.Value, 1)` with `Sample<int> other`.** This is the tuple twin of g1 and takes the same inferred-name arm. One generic-construction member of that arm is enough. The existing (e) covers the natural-type tuple.
- **`foreach` deconstruction target.** `foreach ((Value, var x) in …)` does not compile: a `foreach` loop must declare its iteration variables (CS8186). I took this from the compiler rule and did not run it. So no caller reaches the arm the auditor traced.
- **Nested deconstruction targets, such as `((Value, a), b) = …`.** These are not on the worklist. If the implementer's narrowing handles nesting with a separate condition, then nesting is one more side of the guard, and a test for it is worth adding then (FC-003).
- **The reserved tuple name `(Rest, 1)` (n6).** The auditor lists it as a hint, and decision 20 does not include it.
- **CRLF variants.** This is a binding check with no layout involved. apply-fix gave identical LF and CRLF results for every fixture.
- **Fix All.** The guard is evaluated per diagnostic. t1 already runs the batch fix over two diagnostics in one document through `Verify`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AnotherConstructionOfItsGenericType|FullyQualifiedName~NextToALocalNamedLikeTheField|FullyQualifiedName~DeconstructsItsParameters|FullyQualifiedName~UnnamedTuple|FullyQualifiedName~ExplicitlyTypedTupleLocal|FullyQualifiedName~DeconstructionTarget|FullyQualifiedName~SourceOfADeconstruction|FullyQualifiedName~GivesATupleElementItsInferredName"
```
