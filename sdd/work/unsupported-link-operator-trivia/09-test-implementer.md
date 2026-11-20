Role: test implementer
Commit: 31c6717 (base of this repair; the tests are in the commit that adds this handover)
Verdict: tests written

Repair round 1. The task was auditor finding #1 in `08-auditor.md`: no test covers FMT-UNSUP-015's overlap with FMT-UNSUP-004. Finding #2 is not mine. My prompt carried no intended fix, suspected cause, or hoped-for verdict. I took the auditor's suggested test only as a pointer and derived the tests again from FMT-UNSUP-015's overlap paragraph and from FMT-UNSUP-004's span:

- FMT-UNSUP-004's span takes in a field's or an event field's attribute lists with their arguments, and no initializer value is involved there.
- So a link operator in such an argument is a place where the two shapes meet "outside a value", and FMT-UNSUP-004's reason and location apply.
- The other side is the same attribute on a member that is not a variable declaration. Only FMT-UNSUP-015 refuses that, with its own reason.
- FMT-UNSUP-010 "excepts no initializer value", so it reaches the attribute argument of a field with several declarators too. The field-directive overlap now has a case outside a value as well as the existing case inside one.

My earlier handover (03) said under *Not covered* that no input exists for this overlap. That was wrong: `nameof(s?.Length)` fails only semantically, it parses, and the precheck reads syntax only. The spec checker (07) found the attribute-argument input.

These tests cover behavior that already exists, so they pass on this head. Before handing over I checked that they bite, with a temporary mutation in `PrecheckPhase.FindUnsupportedSyntax` that was reverted and is not in the commit:

- The mutation inserts FMT-UNSUP-015's candidate at the front of the candidate list.
- Under it, all 24 cases of the new declaration test fail on the reason, and so do both builds of the new directive test. The existing value-side directive test fails as well.
- Every other-side case still passes.

## Tests

File `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedLinkOperatorCommentTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CommentInsideALinkOperatorInAnAttributeArgumentOfAVariableDeclarationGivesTheReasonOfTheVariableDeclaration` (24 cases: 4 gaps (`?`–`.` of `?.`, `!`–`.` of `!.`, `!`–`?` and `?`–`.` of `!?.`) × 2 comment kinds (`/* c */` on one line, `// c` with a line break) × 3 members (field with one declarator, field with several declarators, event field), each as `[Obsolete(nameof(Source? /* c */ .Length))]`; asserts `VariableDeclarationCommentReason` at the comment) | FMT-UNSUP-004, FMT-UNSUP-015; FC-003 | `AssertUnsupportedFor` (LF and CRLF) | green (red under the order mutation) |
| `CommentInsideALinkOperatorInAnAttributeArgumentOfAnotherMemberGivesTheReasonOfTheLinkOperator` (32 cases: the same 4 gaps × 2 comment kinds × 4 members that are no variable declaration (method, property, nested class, local function); asserts `LinkOperatorCommentReason` at the comment) | FMT-UNSUP-004, FMT-UNSUP-015; FC-003 | `AssertUnsupportedFor` | green |
| `DirectiveInsideALinkOperatorInAnAttributeArgumentOfAFieldKeepsTheReasonOfTheFieldDeclaration` (X defined / not defined; `#if X … #else … #endif` between the `?` and the `.` inside the attribute argument; several declarators → `FieldDeclarationDirectiveReason`, one declarator → `LinkOperatorCommentReason`, both at `#if X`) | FMT-UNSUP-010, FMT-UNSUP-015 | `AssertUnsupportedFor` | green (red under the order mutation) |

Existing tests I changed: none. I added these to the class: the fields `_attributeGapTrivia`, `_attributedVariableDeclarations`, and `_attributedOtherDeclarations`; the properties `AttributedVariableDeclarationCases` and `AttributedOtherDeclarationCases`; and the private helper `CreateAttributeCases`. Full class on this head: 263 of 263 pass (205 before, plus 58 new). Architecture tests: 198 of 198 pass.

## Not covered, and why

- A link operator in the bracketed size of a fixed-size buffer (`fixed char _b[…]`) also falls in FMT-UNSUP-004's span outside a value. It takes the same path: one `VariableDeclarationSyntax` comment candidate against the link-operator candidate, in the same candidate list. The attribute cases already pin that order, so I added no row for this exotic input. (`rg -n "candidates" src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs`: one list, which `CreateFirstUnsupportedSyntax` orders.)
- Local declarations, `for` initializers, and `using`/`fixed` statements have no attributes. Every link operator in their span is inside an initializer value, which FMT-UNSUP-004 excepts. The existing tests (`LinkOperatorInAMemberIsUnsupported`, the local-initializer row of `EveryLinkOperatorIsCheckedWithItsOwnTokens`, `CommentInsideALinkOperatorInAFieldWithSeveralDeclaratorsGivesTheReasonOfTheLinkOperator`) cover the value side.
- Documentation comments in the attribute gap: the documentation-mode matrix already covers the shape. The overlap is decided by candidate order, not by comment kind, so the two comment kinds suffice.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedLinkOperatorCommentTests"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
