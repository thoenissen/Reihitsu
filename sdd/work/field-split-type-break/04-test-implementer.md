Role: test implementer
Commit: b91639c
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: tests (specifying today's output, maintainer answer A), so every test must pass on the unchanged code, and all do. Expected values come from the FMT-STRUCT-021 example (b91639c) and from the requirement's normative text; the per-shape outputs not in the example were measured with `scripts/format.sh` on b91639c and agree with the requirement (the column of a declarator or modifier line is today's indentation-phase output, not decided by FMT-STRUCT-021, and may change under thoenissen/Reihitsu#953).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests.WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` (existing, tag added) | FMT-STRUCT-021 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | pass |
| `FieldDeclarationSplitTests.LineBreaksInsideAndBehindTheHeadAreRepeatedInEveryField` — the spec example verbatim | FMT-STRUCT-021; FC-002, FC-010 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBehindTheTypeIsRepeatedInEveryField` — the issue's input `private int⏎        _g, _h;` | FMT-STRUCT-021; FC-034 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBehindAGenericTypeIsRepeatedInEveryField` | FMT-STRUCT-021; FC-016 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBehindTheTypeIsRepeatedForDeclaratorsWithInitializers` — `_g = 1, _h = 2, _i` | FMT-STRUCT-021 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBetweenTwoModifiersIsRepeatedInEveryField` | FMT-STRUCT-021; FC-016 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBetweenTheLastModifierAndTheTypeIsRepeatedInEveryField` | FMT-STRUCT-021; FC-016 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBehindACommaStandsInNoField` — with an attribute list on its own line | FMT-STRUCT-021; FC-002, FC-003 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.LineBreakBehindTheTypeIsRepeatedAndLineBreakBehindACommaIsNot` — both sides in one declaration | FMT-STRUCT-021; FC-003 | `AssertRuleResult` | pass |
| `FieldDeclarationSplitTests.SingleDeclaratorWithLineBreakBehindTheTypeIsLaidOutAsASplitField` — untagged characterization: a lone field gives the same lines as each split field | — (characterization) | `AssertRuleResult` | pass |
| `Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests.FixRepeatsLineBreaksInsideAndBehindTheHeadInEveryField` (DataRow LF, CRLF) — spec example, three diagnostics in one document; the verifier also runs Fix All and checks one-pass convergence | FMT-STRUCT-021, RH7101-004; FC-007, FC-008 | `Verify` with `ConvertEndOfLine` | pass |
| `RH7101DoNotCombineFieldsAnalyzerTests.FixRepeatsALineBreakBehindTheTypeInEveryField` (DataRow LF, CRLF) — issue's input, raw fix text compared line endings included | FMT-STRUCT-021, RH7101-004; FC-033 | `VerifyCodeFixWithEndOfLine` | pass |
| `Reihitsu.Analyzer.Test.Formatter.Formatting.RH7101DoNotCombineFieldsFormatterTests.VerifyFormatterRepeatsLineBreaksInsideAndBehindTheHeadInEveryField` — spec example, analyzer clean after formatting, LF and CRLF, second pass | FMT-STRUCT-021; FC-013 | `VerifyFormatter` | pass |

Existing tests asserting a changed output: none — no output changes in this lane. Searched the test projects for the split with a head line break (`rg -n "private int$" src/*Test*`): `FieldDeclarationSplitTests.WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged` (tagged) and `RH7101DoNotCombineFieldsAnalyzerTests.VerifyFixLaysOutAFirstDeclaratorWithoutACommentAsTheFormatterLaysItOut` (already tagged RH7101-004; left as is, it covers the same output on the fix surface at the member column).

## Not covered, and why
- FC-019/FC-020/FC-021/FC-005 (comment or directive in the head gap): refused by FMT-UNSUP-004 and FMT-UNSUP-010 inside a multi-declarator field; existing tests cover the refusal (e.g. `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorIsUnsupported`, `DisabledTextBeforeFirstDeclaratorIsUnsupported`). No production change, so no guard to test on either side.
- FC-001, FC-011, FC-012, FC-015, FC-044, FC-038 to FC-043, FC-031, FC-037: no production code, guard, or declared shape changes in this run.
- FC-028 (detached `FormatNode`): no caller formats a detached split field; the RH7101 fix goes through `FormatNodeInDocumentWithContextAsync`, covered by the two fix tests.
- FC-006, FC-022 to FC-027, FC-029, FC-036, FC-018, FC-009, FC-030, FC-014: not reached by the field split's head copy (no rename, no literal, no expression-bodied member; the fix's scope is already tested by `VerifyFixDoesNotReformatUnrelatedMembers`).
- Event fields and locals with a head line break (not split): the arm is the `Splits` parent/kind check, not the head copy; existing tests already pin that they are not split, and the skeptic measured `E1, E2` and locals staying whole with a break. Not added.
- Other declaring types (struct, record, interface, union): same `FieldDeclarationSplitTransform` copy path for every parent `Splits` admits; covered for the plain case by existing tagged tests. Not added per type.
- A break between an attribute list and modifiers written on one line: FMT-BREAK-027's decision, not FMT-STRUCT-021's.
- FC-035: expected values are from the spec example and the requirement, and the code is unchanged.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFields"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"
```
Results on the worktree: formatter 45/45, analyzer 161/161, architecture Sdd 67/67 passed.

## Follow-up candidates
_None._ beyond the two in `00-lead.md`.
