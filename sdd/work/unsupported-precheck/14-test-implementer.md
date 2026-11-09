Role: test implementer
Commit: 941a5ecc
Verdict: tests written

Repair round 1, Decision 16, finding 2 of `12-spec-checker.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test file is changed: all three tags are correct and kept.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact` | FMT-STRUCT-023 | `AssertRuleResult(input)` | pass |
| `FieldDeclarationSplitTests.DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact` | FMT-STRUCT-023 | `AssertRuleResult(input)` | pass |
| `FieldDeclarationSplitTests.CombinedStaticInterfaceFieldsAreSplit` | FMT-STRUCT-021 | `AssertRuleResult(input, expected)` | pass |

## Verification of the tags
- **Origin.** The three methods, attributes included, are byte-identical at `158e8fc` and at HEAD (each method extracted from both revisions and compared with `diff`). The diff did not add these tags; the line diff of `FieldDeclarationSplitTests.cs` (140 insertions, 2273 deletions) shows them as `+` lines only because the deletions around them realign the hunks. They are listed here so the table names them, as the checker asks.
- **FMT-STRUCT-023** ("MUST NOT split a field declaration that contains a preprocessor directive or disabled text anywhere in its trivia"). Both inputs are `private int` / `#if DEBUG` (resp. `#if false`) / `/* c */` / `#endif` / `_first, _second;` in a class, and assert the input comes back unchanged under LF and CRLF. The fixtures parse with default options, which define no `DEBUG`, so in both inputs `/* c */` is disabled text, not comment trivia: FMT-UNSUP-004 does not refuse the input, and the unchanged output is the split withholding itself for a declaration with directives and disabled text. That is the requirement's normative sentence; the tag stays.
- **FMT-STRUCT-021** ("MUST split a field declaration with several declarators in a class, struct, record, or interface into one field declaration per declarator, in source order, each repeating the original ... modifiers, and type"). Input `static int _a, _b;` in an interface; expected `static int _a;` and `static int _b;`. That is the interface arm of the requirement with the modifier repeated; the tag stays.

## Not covered, and why
Nothing new is in scope: the round asks to verify existing tags and change no assertion.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests.DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact|FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests.CombinedStaticInterfaceFieldsAreSplit"
```
Result on 941a5ecc: `Passed!  - Failed: 0, Passed: 3, Skipped: 0, Total: 3`.
